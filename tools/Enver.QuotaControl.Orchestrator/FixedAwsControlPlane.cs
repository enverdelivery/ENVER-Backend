using System.Buffers;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Enver.QuotaControl.Orchestrator;

// Fixed regional AWS Query API. No AWS CLI, SDK credential chain, redirects or retries.
// This adapter is NOT exercised against AWS by the local suite.
public sealed class FixedAwsControlPlane : IControlPlane, IDisposable
{
    private readonly HttpClient client;
    private readonly string access, token;
    private readonly byte[] key;
    private readonly DateTimeOffset expiry;
    private bool expired;

    internal FixedAwsControlPlane(HttpMessageHandler handler, DateTimeOffset credentialExpiry)
    {
        expiry = credentialExpiry; access = "SYNTHETIC_ACCESS"; token = "SYNTHETIC_SESSION_TOKEN";
        key = Encoding.ASCII.GetBytes("AWS4SYNTHETIC_SIGNING_SECRET");
        client = new(handler) { Timeout = TimeSpan.FromSeconds(15) };
    }

    public FixedAwsControlPlane(DateTimeOffset credentialExpiry)
    {
        expiry = credentialExpiry;
        if (expiry - DateTimeOffset.UtcNow < TimeSpan.FromMinutes(20)) throw new ControlFailure(Failure.Expired);
        // Existing manually provisioned profile ONLY; do not evaluate config/process/SSO/role fallbacks.
        var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aws", "credentials");
        var selected = false; var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in File.ReadLines(file))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith('[')) { selected = trimmed == "[" + FixedResources.Profile + "]"; continue; }
            if (!selected || trimmed.StartsWith('#') || trimmed.StartsWith(';') || !trimmed.Contains('=')) continue;
            var split = trimmed.IndexOf('='); var name = trimmed[..split].Trim();
            if (name is "aws_access_key_id" or "aws_secret_access_key" or "aws_session_token")
                if (!entries.TryAdd(name, trimmed[(split + 1)..].Trim())) throw new ControlFailure(Failure.Precheck);
        }
        if (!entries.TryGetValue("aws_access_key_id", out access!) || !entries.TryGetValue("aws_session_token", out token!) ||
            !entries.TryGetValue("aws_secret_access_key", out var secret) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(secret))
            throw new ControlFailure(Failure.Precheck);
        key = Encoding.UTF8.GetBytes("AWS4" + secret);
        client = new(new SocketsHttpHandler { AllowAutoRedirect = false, UseProxy = false, AutomaticDecompression = DecompressionMethods.None })
            { Timeout = TimeSpan.FromSeconds(15) };
    }

    private static string Value(XContainer doc, string name) => doc.Descendants().FirstOrDefault(x => x.Name.LocalName == name)?.Value ?? "";
    private static string Canonical(XElement element) => element.ToString(SaveOptions.DisableFormatting);
    private static XElement? Element(XContainer doc, string name) => doc.Descendants().FirstOrDefault(x => x.Name.LocalName == name);
    private static XElement? SingleRecord(XContainer doc, string list, bool allowEmpty = false)
    {
        if (!string.IsNullOrEmpty(Value(doc, "Marker"))) throw new ControlFailure(Failure.Precheck);
        var records = Element(doc, list)?.Elements().ToArray() ?? [];
        if (records.Length == 0 && allowEmpty) return null;
        if (records.Length != 1) throw new ControlFailure(Failure.Precheck);
        return records[0];
    }
    private static string[] Strings(XContainer doc, string name) => Element(doc, name)?.Elements().Select(x => x.Value).Order().ToArray() ?? [];
    private static bool Empty(XContainer doc, string name) => Element(doc, name) is not { } p || !p.Elements().Any();
    private static string Hash(ReadOnlySpan<byte> data) => Convert.ToHexStringLower(SHA256.HashData(data));
    private static byte[] Hmac(byte[] signingKey, string value) => HMACSHA256.HashData(signingKey, Encoding.UTF8.GetBytes(value));

    private async Task<XDocument> QueryAsync(string service, string action, (string Key, string Value)[] fields,
        ReadOnlyMemory<byte> password, CancellationToken ct)
    {
        if (expired || DateTimeOffset.UtcNow >= expiry) { expired = true; throw new ControlFailure(Failure.Expired); }
        var body = new ArrayBufferWriter<byte>();
        void Add(string name, ReadOnlySpan<byte> value)
        {
            if (body.WrittenCount > 0) { body.GetSpan(1)[0] = (byte)'&'; body.Advance(1); }
            var prefix = Encoding.UTF8.GetBytes(Uri.EscapeDataString(name) + "="); body.Write(prefix);
            foreach (var b in value)
            {
                if (b is >= (byte)'A' and <= (byte)'Z' or >= (byte)'a' and <= (byte)'z' or >= (byte)'0' and <= (byte)'9' or (byte)'-' or (byte)'_' or (byte)'.' or (byte)'~')
                { body.GetSpan(1)[0] = b; body.Advance(1); }
                else { var encoded = Encoding.ASCII.GetBytes("%" + b.ToString("X2", CultureInfo.InvariantCulture)); body.Write(encoded); }
            }
        }
        void Text(string name, string value) => Add(name, Encoding.UTF8.GetBytes(value));
        Text("Action", action); Text("Version", service == "sts" ? "2011-06-15" : "2015-02-02");
        foreach (var field in fields) Text(field.Key, field.Value);
        if (!password.IsEmpty) Add("AuthenticationMode.Passwords.member.1", password.Span);
        byte[]? requestBytes = null, responseBytes = null, dateKey = null, regionKey = null, serviceKey = null, signingKey = null;
        var mutating = action is "CreateUser" or "ModifyUserGroup" or "DeleteUser";
        var sent = false;
        try
        {
            requestBytes = body.WrittenSpan.ToArray();
            var host = service + "." + FixedResources.Region + ".amazonaws.com";
            var now = DateTimeOffset.UtcNow;
            var date = now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var amzDate = now.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
            const string contentType = "application/x-www-form-urlencoded; charset=utf-8";
            const string signed = "content-type;host;x-amz-date;x-amz-security-token";
            var headers = "content-type:" + contentType + "\nhost:" + host + "\nx-amz-date:" + amzDate + "\nx-amz-security-token:" + token + "\n";
            var canonical = "POST\n/\n\n" + headers + "\n" + signed + "\n" + Hash(requestBytes);
            var scope = date + "/" + FixedResources.Region + "/" + service + "/aws4_request";
            dateKey = Hmac(key, date); regionKey = Hmac(dateKey, FixedResources.Region); serviceKey = Hmac(regionKey, service); signingKey = Hmac(serviceKey, "aws4_request");
            var signature = Convert.ToHexStringLower(Hmac(signingKey, "AWS4-HMAC-SHA256\n" + amzDate + "\n" + scope + "\n" + Hash(Encoding.UTF8.GetBytes(canonical))));
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://" + host + "/");
            request.Headers.TryAddWithoutValidation("X-Amz-Date", amzDate);
            request.Headers.TryAddWithoutValidation("X-Amz-Security-Token", token);
            request.Headers.TryAddWithoutValidation("Authorization", "AWS4-HMAC-SHA256 Credential=" + access + "/" + scope + ", SignedHeaders=" + signed + ", Signature=" + signature);
            request.Content = new ByteArrayContent(requestBytes);
            request.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
            sent = true;
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            responseBytes = new byte[131073]; var used = 0;
            while (used < responseBytes.Length)
            {
                var count = await stream.ReadAsync(responseBytes.AsMemory(used), ct).ConfigureAwait(false);
                if (count == 0) break; used += count;
            }
            if (used > 131072) throw new ControlFailure(mutating ? Failure.AmbiguousMutation : Failure.Transport);
            using var reader = XmlReader.Create(new MemoryStream(responseBytes, 0, used), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 131072 });
            var xml = XDocument.Load(reader);
            if (!response.IsSuccessStatusCode)
            {
                var code = Value(xml, "Code");
                if (code is "ExpiredToken" or "ExpiredTokenException" or "RequestExpired") { expired = true; throw new ControlFailure(Failure.Expired); }
                if (action == "DescribeUsers" && code == "UserNotFound") return new(new XElement("Absent"));
                throw new ControlFailure(response.StatusCode >= HttpStatusCode.InternalServerError && mutating ? Failure.AmbiguousMutation : Failure.Rejected);
            }
            return xml;
        }
        catch (ControlFailure) { throw; }
        catch { throw new ControlFailure(sent && mutating ? Failure.AmbiguousMutation : Failure.Transport); }
        finally
        {
            // Includes the exact used region of ArrayBufferWriter; its capacity can contain earlier copies.
            body.Clear();
            foreach (var buffer in new[] { requestBytes, responseBytes, dateKey, regionKey, serviceKey, signingKey })
                if (buffer is not null) CryptographicOperations.ZeroMemory(buffer);
        }
    }

    private Task<XDocument> CacheAsync(string action, CancellationToken token, params (string, string)[] fields) => QueryAsync("elasticache", action, fields, default, token);
    public async Task<Snapshot> ReadAsync(CancellationToken ct)
    {
        var identity = await QueryAsync("sts", "GetCallerIdentity", [], default, ct).ConfigureAwait(false);
        if (Value(identity, "Account") != FixedResources.Account || Value(identity, "Arn") != FixedResources.Principal) throw new ControlFailure(Failure.Precheck);
        var runtime = await CacheAsync("DescribeUsers", ct, ("UserId", FixedResources.Runtime)).ConfigureAwait(false);
        var group = await CacheAsync("DescribeUserGroups", ct, ("UserGroupId", FixedResources.Group)).ConfigureAwait(false);
        var rg = await CacheAsync("DescribeReplicationGroups", ct, ("ReplicationGroupId", FixedResources.Group)).ConfigureAwait(false);
        var cluster = await CacheAsync("DescribeCacheClusters", ct, ("CacheClusterId", FixedResources.Group + "-001")).ConfigureAwait(false);
        var control = await CacheAsync("DescribeUsers", ct, ("UserId", FixedResources.Control)).ConfigureAwait(false);
        var ru = SingleRecord(runtime, "Users")!;
        var gr = SingleRecord(group, "UserGroups")!;
        var cache = SingleRecord(rg, "ReplicationGroups")!;
        var node = SingleRecord(cluster, "CacheClusters")!;
        if (Value(ru, "UserId") != FixedResources.Runtime || Value(ru, "Status") != "active" || Value(ru, "Engine") != "valkey" ||
            !Strings(ru, "UserGroupIds").SequenceEqual(new[] { FixedResources.Group }) || Value(gr, "UserGroupId") != FixedResources.Group ||
            !Strings(gr, "ReplicationGroups").SequenceEqual(new[] { FixedResources.Group }) || Value(cache, "ReplicationGroupId") != FixedResources.Group ||
            !Strings(cache, "UserGroupIds").SequenceEqual(new[] { FixedResources.Group }) || Value(cache, "TransitEncryptionEnabled") != "true" || Value(cache, "ClusterEnabled") != "false" ||
            Value(Element(cache, "PrimaryEndpoint") ?? new XElement("Missing"), "Address") != FixedResources.Endpoint ||
            Value(Element(cache, "PrimaryEndpoint") ?? new XElement("Missing"), "Port") != "6379" ||
            Value(node, "CacheClusterId") != FixedResources.Group + "-001" || Value(node, "ReplicationGroupId") != FixedResources.Group ||
            Value(node, "CacheClusterStatus") != "available" || Value(node, "TransitEncryptionEnabled") != "true" || Value(node, "Engine") != "valkey") throw new ControlFailure(Failure.Drift);
        var current = SingleRecord(control, "Users", allowEmpty: true);
        var presence = current is null ? ControlPresence.Absent :
            Value(current, "UserId") == FixedResources.Control && Value(current, "UserName") == FixedResources.Control && Value(current, "Engine") == "valkey" &&
            Strings(current, "UserGroupIds").All(id => id == FixedResources.Group) &&
            Value(current, "AccessString") == FixedResources.Acl && Value(current, "Type") == "password" && Value(current, "PasswordCount") == "1"
                ? Value(current, "Status") == "active" ? ControlPresence.Expected : Value(current, "Status") == "creating" ? ControlPresence.Pending : ControlPresence.Unexpected
                : ControlPresence.Unexpected;
        // Select stable security/topology fields; never payloads/passwords, not volatile counters/dates.
        var runtimeCanonical = string.Join("\n", new[] { "UserId", "UserName", "ARN", "Engine", "Status", "AccessString", "Type", "PasswordCount" }.Select(n => n + "=" + Value(ru, n)));
        var resourceCanonical = string.Join("\n", new[] { "ReplicationGroupId", "ARN", "TransitEncryptionEnabled", "TransitEncryptionMode", "AtRestEncryptionEnabled", "ClusterEnabled", "AutomaticFailover", "MultiAZ" }.Select(n => n + "=" + Value(cache, n)))
            + "\n" + Canonical(Element(cache, "NodeGroups") ?? new XElement("Missing")) + "\n" + Canonical(Element(node, "SecurityGroups") ?? new XElement("Missing"))
            + "\nGroup=" + Value(gr, "UserGroupId") + "\nGroupArn=" + Value(gr, "ARN") + "\nGroupEngine=" + Value(gr, "Engine");
        var stable = Value(gr, "Status") == "active" && Value(cache, "Status") == "available" && Empty(gr, "PendingChanges") && Empty(cache, "PendingModifiedValues");
        return new(runtimeCanonical, resourceCanonical, Strings(gr, "UserIds"), stable, presence);
    }
    private async Task AwaitAsync(Func<Snapshot, bool> condition, TimeSpan limit, CancellationToken ct)
    {
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct); budget.CancelAfter(limit);
        while (true)
        {
            if (condition(await ReadAsync(budget.Token).ConfigureAwait(false))) return;
            await Task.Delay(TimeSpan.FromSeconds(2), budget.Token).ConfigureAwait(false);
        }
    }
    public async Task CreateAsync(ReadOnlyMemory<byte> password, CancellationToken ct)
    {
        await QueryAsync("elasticache", "CreateUser", [("UserId", FixedResources.Control), ("UserName", FixedResources.Control), ("Engine", "valkey"),
            ("AccessString", FixedResources.Acl), ("AuthenticationMode.Type", "password")], password, ct).ConfigureAwait(false);
        // Create acknowledgement establishes ownership even if subsequent propagation checks fail in core.
    }
    public async Task AssociateAsync(CancellationToken ct)
    {
        await CacheAsync("ModifyUserGroup", ct, ("UserGroupId", FixedResources.Group), ("UserIdsToAdd.member.1", FixedResources.Control)).ConfigureAwait(false);
        await AwaitAsync(s => s.Stable && s.Members.Contains(FixedResources.Control) && s.Control == ControlPresence.Expected, TimeSpan.FromSeconds(180), ct).ConfigureAwait(false);
    }
    public async Task RevokeAsync(CancellationToken ct)
    {
        await CacheAsync("ModifyUserGroup", ct, ("UserGroupId", FixedResources.Group), ("UserIdsToRemove.member.1", FixedResources.Control)).ConfigureAwait(false);
        await AwaitAsync(s => s.Stable && !s.Members.Contains(FixedResources.Control), TimeSpan.FromSeconds(215), ct).ConfigureAwait(false);
    }
    public async Task DeleteAsync(CancellationToken ct)
    {
        await CacheAsync("DeleteUser", ct, ("UserId", FixedResources.Control)).ConfigureAwait(false);
        await AwaitAsync(s => s.Control == ControlPresence.Absent, TimeSpan.FromSeconds(180), ct).ConfigureAwait(false);
    }
    public void Dispose() { client.Dispose(); CryptographicOperations.ZeroMemory(key); }
}
