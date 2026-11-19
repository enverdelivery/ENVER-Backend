using Enver.QuotaControl;

namespace Enver.QuotaControl.Tests;

// Doble en memoria. No implementa sockets, TLS, AWS, Redis real ni un interprete Lua.
internal sealed class FakeKey(string type)
{
    public string Type { get; } = type;
    public long Pttl { get; set; } = -1;
    public long ExpireTime { get; set; } = -1;
    public Dictionary<string, string> Fields { get; } = new(StringComparer.Ordinal);
    public List<ScoredMember> Members { get; } = [];
}

internal sealed class FakeRedis : IRedisCommandClient
{
    public object SessionIdentity { get; set; } = new();
    public bool IsLocalSimulation { get; set; } = true;
    public Dictionary<string, FakeKey> Keys { get; } = new(StringComparer.Ordinal);
    public List<RedisCommand> Commands { get; } = [];
    public Queue<(string Cursor, string[] Keys)> ScanPages { get; } = new();
    public Action<RedisCommand>? BeforeCommand { get; set; }
    public Func<RedisCommand, RedisReply?>? Override { get; set; }
    public long Now { get; set; } = 1700000000000000; // Tiempo fijo de prueba, no observado.
    public bool LoseApplyResponse { get; set; }
    public int HashWrites { get; private set; }

    public ValueTask<RedisReply> ExecuteAsync(RedisCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Commands.Add(command);
        BeforeCommand?.Invoke(command);
        var overridden = Override?.Invoke(command);
        if (overridden is not null) return ValueTask.FromResult(overridden);
        var args = command.Arguments;
        Keys.TryGetValue(args.Count == 0 ? "" : args[0], out var key);
        var result = command.Verb switch
        {
            RedisVerb.TYPE => RedisReply.String(key?.Type ?? "none"),
            RedisVerb.PTTL => RedisReply.Integer(key?.Pttl ?? -2),
            RedisVerb.PEXPIRETIME => RedisReply.Integer(key?.ExpireTime ?? -2),
            RedisVerb.HGETALL => RedisReply.Array((key?.Fields ?? []).SelectMany(p => new[] { RedisReply.String(p.Key), RedisReply.String(p.Value) }).ToArray()),
            RedisVerb.ZCARD => RedisReply.Integer(key?.Members.Count ?? 0),
            RedisVerb.ZRANGE => RedisReply.Array((key?.Members ?? []).Skip(int.Parse(args[1])).Take(int.Parse(args[2]) - int.Parse(args[1]) + 1)
                .SelectMany(p => new[] { RedisReply.String(p.Member), RedisReply.String(Contract.Decimal(p.Score)) }).ToArray()),
            RedisVerb.SCAN => Scan(command),
            RedisVerb.TIME => RedisReply.Array(RedisReply.String(Contract.Decimal(Now / 1000000)), RedisReply.String(Contract.Decimal(Now % 1000000))),
            RedisVerb.EVAL => Evaluate(command),
            _ => throw new InvalidOperationException("Unexpected test command")
        };
        return ValueTask.FromResult(result);
    }

    private RedisReply Scan(RedisCommand command)
    {
        Assert.Equal(5, command.Arguments.Count);
        Assert.Equal("MATCH", command.Arguments[1]);
        Assert.Equal(Contract.BucketPattern, command.Arguments[2]);
        Assert.Equal("COUNT", command.Arguments[3]);
        var page = ScanPages.Count == 0
            ? ("0", Keys.Keys.Where(k => k.StartsWith(Contract.BucketPrefix, StringComparison.Ordinal)).ToArray())
            : ScanPages.Dequeue();
        return RedisReply.Array(RedisReply.String(page.Item1), RedisReply.Array(page.Item2.Select(RedisReply.String).ToArray()));
    }

    private RedisReply Evaluate(RedisCommand command)
    {
        // Modelo independiente del CAS: se audita el Lua por separado; esto no prueba su ejecucion.
        Assert.Equal(RecoveryScript.Text, command.Arguments[0]);
        Assert.Equal("2", command.Arguments[1]);
        Assert.Equal(Contract.Metadata, command.Arguments[2]);
        Assert.Equal(Contract.Registry, command.Arguments[3]);
        Assert.Equal(ApprovedDescriptor.Baseline.Snapshot(), command.Arguments[4]);
        if (Keys.ContainsKey(Contract.Metadata)) return ApplyReply("aborted", "metadata_exists", 0);
        if (Keys.ContainsKey(Contract.Registry)) return ApplyReply("aborted", "registry_exists", 0);
        var plan = ClosedPlan.Build(ApprovedDescriptor.Baseline, command.Arguments[5], Now);
        var metadata = new FakeKey("hash");
        foreach (var pair in plan.Fields) metadata.Fields.Add(pair.Key, pair.Value);
        Keys.Add(Contract.Metadata, metadata);
        HashWrites++;
        if (LoseApplyResponse) throw new IOException("SIMULATED_RESPONSE_LOSS_NOT_A_SECRET");
        return ApplyReply("created_closed", "ok", Now);
    }

    public static RedisReply ApplyReply(string status, string reason, long time) => RedisReply.Array(
        RedisReply.String("enver.auth.quota.recovery-closed"), RedisReply.String("1"),
        RedisReply.String(status), RedisReply.String(reason), RedisReply.String(Contract.Decimal(time)));

    public void PutClosed(string generation)
    {
        var metadata = new FakeKey("hash");
        foreach (var pair in ClosedPlan.Build(ApprovedDescriptor.Baseline, generation, Now).Fields)
            metadata.Fields.Add(pair.Key, pair.Value);
        Keys.Add(Contract.Metadata, metadata);
    }
}
