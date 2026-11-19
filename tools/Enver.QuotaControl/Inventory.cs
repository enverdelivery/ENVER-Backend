using System.Collections.ObjectModel;

namespace Enver.QuotaControl;

public sealed record ScoredMember(string Member, long Score)
{
    public override string ToString() => nameof(ScoredMember);
}

// Estas observaciones internas NO son el formato de salida; pueden contener identificadores opacos.
public sealed record KeyObservation(string Type, long Pttl, long ExpireTime,
    IReadOnlyDictionary<string, string>? Fields, IReadOnlyList<ScoredMember>? Members)
{
    public override string ToString() => nameof(KeyObservation);
}

public sealed record InventoryResult(KeyObservation Metadata, KeyObservation Registry,
    int BucketCount, int RegistryMemberCount, int Discrepancies, long ServerTimeMicroseconds)
{
    public bool IsEmpty => Metadata.Type == "none" && Registry.Type == "none" && BucketCount == 0 && Discrepancies == 0;
    public InventorySummary Sanitize() => new(Metadata.Type != "none", Registry.Type != "none",
        BucketCount, RegistryMemberCount, Discrepancies, true);
    public override string ToString() => nameof(InventoryResult);
}

public sealed record InventorySummary(bool MetadataPresent, bool RegistryPresent,
    int BucketCount, int RegistryMemberCount, int Discrepancies, bool IterationCompleted);

public sealed class InventoryReader(IRedisCommandClient client, InventoryTrace? trace = null)
{
    private readonly object session = client.SessionIdentity;

    private async ValueTask<RedisReply> Send(RedisCommand command, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        CheckSession();
        RedisReply response;
        try { response = await client.ExecuteAsync(command, token).ConfigureAwait(false); }
        catch (ControlException) { throw; }
        catch (OperationCanceledException) { throw; }
        catch { throw new ControlException(Failure.InventoryIncomplete); }
        token.ThrowIfCancellationRequested();
        CheckSession();
        return response;
    }

    private void CheckSession()
    {
        if (!ReferenceEquals(session, client.SessionIdentity)) throw new ControlException(Failure.InventoryIncomplete);
    }

    public async Task<InventoryResult> ReadAsync(CancellationToken token)
    {
        var metadata = await Observe(Contract.Metadata, token).ConfigureAwait(false);
        var registry = await Observe(Contract.Registry, token).ConfigureAwait(false);
        var discrepancies = Consistency(metadata) + Consistency(registry);
        if (metadata.Type is not ("none" or "hash") || registry.Type is not ("none" or "zset")) discrepancies++;
        var found = new HashSet<string>(StringComparer.Ordinal);
        var physicalMembers = new HashSet<string>(StringComparer.Ordinal);
        var buckets = new Dictionary<string, KeyObservation>(StringComparer.Ordinal);
        var cursor = "0";
        var cursors = new HashSet<string>(StringComparer.Ordinal);
        var pages = 0;
        if (trace is not null) trace.ScanStartedUtc = DateTimeOffset.UtcNow;
        do
        {
            if (++pages > 10000) throw new ControlException(Failure.InventoryIncomplete);
            var page = (await Send(RedisCommand.Scan(cursor), token).ConfigureAwait(false)).Items;
            if (page.Count != 2) throw new ControlException(Failure.UnexpectedResponse);
            cursor = page[0].Text;
            RedisCommand.ValidateCursor(cursor);
            if (trace is not null) { trace.Pages = pages; trace.FinalCursor = cursor; }
            if (cursor != "0" && !cursors.Add(cursor)) throw new ControlException(Failure.InventoryIncomplete);
            var keys = page[1].Items;
            if (keys.Count > 10000) throw new ControlException(Failure.InventoryIncomplete);
            foreach (var item in keys)
            {
                var key = item.Text;
                if (key.Length > 512 || !key.StartsWith(Contract.BucketPrefix, StringComparison.Ordinal))
                    throw new ControlException(Failure.UnexpectedResponse);
                if (!found.Add(key))
                {
                    if (trace is not null) trace.DuplicateKeys++;
                    continue;
                }
                if (trace is not null) trace.UniqueKeys = found.Count;
                if (found.Count > 5000) throw new ControlException(Failure.InventoryIncomplete);
                var observation = await Observe(key, token).ConfigureAwait(false);
                var member = key[Contract.BucketPrefix.Length..];
                if (!Contract.ValidMember(member)) discrepancies++;
                physicalMembers.Add(member);
                buckets.Add(member, observation);
                discrepancies += Consistency(observation);
                if (observation.Type != "zset") discrepancies++; // Incluso desaparecido: nueva pasada requerida.
                if (observation.Members is not null)
                    foreach (var attempt in observation.Members)
                    {
                        try { Contract.ValidateGeneration(attempt.Member); }
                        catch (ControlException) { discrepancies++; }
                    }
            }
        } while (cursor != "0");
        if (trace is not null) trace.ScanFinishedUtc = DateTimeOffset.UtcNow;

        var registered = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in registry.Members ?? [])
        {
            if (!Contract.ValidMember(member.Member) || !registered.Add(member.Member)) discrepancies++;
            if (!physicalMembers.Contains(member.Member)) discrepancies++;
        }
        foreach (var member in physicalMembers)
            if (!registered.Contains(member)) discrepancies++;

        // Detecta cambios de M/R durante la pasada; la exclusion externa sigue siendo imprescindible.
        var finalMetadata = await Observe(Contract.Metadata, token).ConfigureAwait(false);
        var finalRegistry = await Observe(Contract.Registry, token).ConfigureAwait(false);
        discrepancies += Consistency(finalMetadata) + Consistency(finalRegistry);
        if (!Same(metadata, finalMetadata) || !Same(registry, finalRegistry)) discrepancies++;
        var time = ParseTime(await Send(RedisCommand.Time(), token).ConfigureAwait(false));
        if (trace is not null) trace.ServerTimeMicroseconds = time;
        foreach (var (member, bucket) in buckets)
        {
            if (!Contract.ValidMember(member) || bucket.Type != "zset" || bucket.Members is not { Count: > 0 }) continue;
            var parts = member.Split(':');
            var window = ApprovedDescriptor.Baseline.Rules.Single(r => r.Name == parts[0] + "/" + parts[1]).WindowMicroseconds;
            var last = bucket.Members.Max(m => m.Score);
            if (last > time || last > Contract.MaximumExactInteger - window) { discrepancies++; continue; }
            var horizon = last + window;
            var entry = registry.Members?.SingleOrDefault(m => m.Member == member);
            if (entry is null || entry.Score != horizon || bucket.ExpireTime != (horizon + 999) / 1000) discrepancies++;
        }
        if (trace is not null) trace.Completed = true;
        // No ocultar una presencia observada solo en la relectura final.
        return new(metadata.Type != "none" ? metadata : finalMetadata, registry.Type != "none" ? registry : finalRegistry,
            found.Count, Math.Max(registered.Count, finalRegistry.Members?.Count ?? 0), discrepancies, time);
    }

    internal async Task<KeyObservation> Observe(string key, CancellationToken token)
    {
        var type = (await Send(RedisCommand.ReadKey(RedisVerb.TYPE, key), token).ConfigureAwait(false)).Text;
        if (type is not ("none" or "hash" or "zset" or "string" or "list" or "set" or "stream"))
            throw new ControlException(Failure.UnexpectedResponse);
        var ttl = (await Send(RedisCommand.ReadKey(RedisVerb.PTTL, key), token).ConfigureAwait(false)).Number;
        var expiration = (await Send(RedisCommand.ReadKey(RedisVerb.PEXPIRETIME, key), token).ConfigureAwait(false)).Number;
        if (ttl < -2 || expiration < -2) throw new ControlException(Failure.UnexpectedResponse);
        IReadOnlyDictionary<string, string>? fields = null;
        IReadOnlyList<ScoredMember>? members = null;
        if (key == Contract.Metadata && type == "hash")
        {
            var flat = (await Send(RedisCommand.ReadKey(RedisVerb.HGETALL, key), token).ConfigureAwait(false)).Items;
            if (flat.Count % 2 != 0 || flat.Count > 512) throw new ControlException(Failure.UnexpectedResponse);
            var dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var i = 0; i < flat.Count; i += 2)
            {
                var name = flat[i].Text;
                var value = flat[i + 1].Text;
                if (name.Length > 128 || value.Length > 4096 || !dictionary.TryAdd(name, value))
                    throw new ControlException(Failure.UnexpectedResponse);
            }
            fields = new ReadOnlyDictionary<string, string>(dictionary);
        }
        if (type == "zset")
        {
            var size = (await Send(RedisCommand.ReadKey(RedisVerb.ZCARD, key), token).ConfigureAwait(false)).Number;
            var maximum = key == Contract.Registry ? 5000 : 300;
            if (size < 0 || size > maximum) throw new ControlException(Failure.InventoryIncomplete);
            var list = new List<ScoredMember>();
            for (var rank = 0; rank < size; rank += 100)
            {
                var last = (int)Math.Min(rank + 99, size - 1);
                var values = (await Send(RedisCommand.Range(key, rank, last), token).ConfigureAwait(false)).Items;
                if (values.Count != 2 * (last - rank + 1)) throw new ControlException(Failure.InventoryIncomplete);
                for (var i = 0; i < values.Count; i += 2)
                {
                    var member = values[i].Text;
                    if (member.Length > 512) throw new ControlException(Failure.UnexpectedResponse);
                    list.Add(new(member, Contract.ParseUnsigned(values[i + 1].Text)));
                }
            }
            var finalSize = (await Send(RedisCommand.ReadKey(RedisVerb.ZCARD, key), token).ConfigureAwait(false)).Number;
            if (finalSize != size || list.Select(m => m.Member).Distinct(StringComparer.Ordinal).Count() != size)
                throw new ControlException(Failure.InventoryIncomplete);
            members = list.AsReadOnly();
        }
        return new(type, ttl, expiration, fields, members);
    }

    public static long ParseTime(RedisReply reply)
    {
        var values = reply.Items;
        if (values.Count != 2) throw new ControlException(Failure.UnexpectedResponse);
        var seconds = Contract.ParseUnsigned(values[0].Text, Contract.MaximumExactInteger / 1000000);
        var micros = Contract.ParseUnsigned(values[1].Text, 999999);
        var time = checked(seconds * 1000000 + micros);
        Contract.ValidateTime(time);
        return time;
    }

    private static int Consistency(KeyObservation key) => key.Type == "none"
        ? (key.Pttl == -2 && key.ExpireTime == -2 ? 0 : 1)
        : (key.Pttl >= -1 && key.ExpireTime >= -1 && ((key.Pttl == -1) == (key.ExpireTime == -1)) ? 0 : 1);

    private static bool Same(KeyObservation a, KeyObservation b) => a.Type == b.Type && a.ExpireTime == b.ExpireTime &&
        ((a.Fields is null && b.Fields is null) || (a.Fields is not null && b.Fields is not null &&
        a.Fields.Count == b.Fields.Count && a.Fields.All(p => b.Fields.TryGetValue(p.Key, out var v) && p.Value == v))) &&
        ((a.Members is null && b.Members is null) || (a.Members is not null && b.Members is not null && a.Members.SequenceEqual(b.Members)));
}
