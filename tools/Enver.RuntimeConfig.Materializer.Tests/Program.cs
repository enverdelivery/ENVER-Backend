using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Enver.RuntimeConfig.Materializer;
using Enver.API.Composition;
using Microsoft.Extensions.Configuration;

var count = 0;
void Assert(bool condition) { if (!condition) throw new Exception("Test assertion failed (no payload)."); count++; }
const string canary = "LOCAL-CANARY-never-a-secret";
var good = new Dictionary<string, string>
{
    ["redis"] = JsonSerializer.Serialize(new { username = "local-user", password = canary }),
    ["mongodb"] = JsonSerializer.Serialize(new { username = "local-user", password = canary, host = "canary.mongodb.net", database = "enver_development" }),
    ["hmac"] = JsonSerializer.Serialize(new { keyBase64 = Convert.ToBase64String(Enumerable.Repeat((byte)1, 32).ToArray()) }),
    ["jwt"] = JsonSerializer.Serialize(new { activeKid = "local", keys = new[] { new { kid = "local", keyBase64 = Convert.ToBase64String(Enumerable.Repeat((byte)2, 32).ToArray()) } } })
};
var runtime = await Enver.RuntimeConfig.Materializer.Program.MapAndValidate(new FakeReader(good), default);
Assert(runtime.Count == 6);
Assert(runtime["RateLimiting"]!["MaxActivePartitions"]!.GetValue<int>() == 5000);
Assert(runtime["AuthenticationComposition"]!["QuotaGeneration"]!.GetValue<string>() == RuntimeMapping.Generation);
Assert(runtime["Redis"]!["NamespacePrefix"]!.GetValue<string>() == "enver:development");
Assert(runtime["MongoDb"]!["DatabaseName"]!.GetValue<string>() == "enver_development");
// Fake earlier providers cannot override the final runtime provider (same API extension).
using (var memory = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(runtime)))
{
    var config = BindingDiagnostic.Defaults().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Redis:NamespacePrefix"] = "earlier-provider", ["Jwt:ActiveKid"] = "earlier-provider" }).AddJsonStream(memory).Build();
    try { BindingDiagnostic.Validate(config); Assert(config["Redis:NamespacePrefix"] == "enver:development"); }
    finally { (config as IDisposable)?.Dispose(); }
}
var secretValues = new[] { canary, JsonNode.Parse(good["hmac"])!["keyBase64"]!.GetValue<string>(), JsonNode.Parse(good["jwt"])!["keys"]![0]!["keyBase64"]!.GetValue<string>() };
async Task MustFail(ISecretReader reader)
{
    try { await Enver.RuntimeConfig.Materializer.Program.MapAndValidate(reader, default); throw new Exception("Expected failure."); }
    catch (DeliveryFailure e) { Assert(secretValues.All(s => !e.ToString().Contains(s, StringComparison.Ordinal))); Assert(e.InnerException is null); }
}
foreach (var key in good.Keys)
{
    var bad = new Dictionary<string, string>(good) { [key] = "{invalid:" + canary };
    await MustFail(new FakeReader(bad));
    bad[key] = JsonSerializer.Serialize(new { unexpected = canary }); await MustFail(new FakeReader(bad));
}
await MustFail(new FakeReader(good, true));
foreach (var jwt in new object[] {
    new { activeKid = "missing", keys = new[] { new { kid = "local", keyBase64 = Convert.ToBase64String(new byte[32]) } } },
    new { activeKid = "local", keys = Array.Empty<object>() },
    new { activeKid = "local", keys = new[] { new { kid = "local", keyBase64 = "invalid-" + canary } } },
    new { activeKid = "local", keys = new[] { new { kid = "local", keyBase64 = JsonNode.Parse(good["hmac"])!["keyBase64"]!.GetValue<string>() } } }
}) await MustFail(new FakeReader(new Dictionary<string, string>(good) { ["jwt"] = JsonSerializer.Serialize(jwt) }));
await MustFail(new FakeReader(new Dictionary<string, string>(good) { ["mongodb"] = JsonSerializer.Serialize(new { username = "u", password = canary, host = "invalid/host", database = "enver_development" }) }));
await MustFail(new FakeReader(new Dictionary<string, string>(good) { ["redis"] = JsonSerializer.Serialize(new { username = "u", password = canary + ",allowAdmin=true" }) }));
using (var fake = new FakeSecretsClient(good))
{
    var mapped = await Enver.RuntimeConfig.Materializer.Program.MapAndValidate(new PinnedSecretReader(fake), default);
    Assert(fake.Calls.SequenceEqual(Pins.All));
    Assert(mapped.Count == 6);
    fake.BadVersion = true;
    await MustFail(new PinnedSecretReader(fake));
    fake.BadVersion = false; fake.Throw = true;
    await MustFail(new PinnedSecretReader(fake));
}
var old = Console.Out;
using var captured = new StringWriter();
Console.SetOut(captured);
try
{
    Assert(await Enver.RuntimeConfig.Materializer.Program.Run(new[] { canary }, new LocalOnlyRuntime(), captured, new()) == 2);
    Assert(await Enver.RuntimeConfig.Materializer.Program.Run(new[] { "--materialize", canary }, new LocalOnlyRuntime(), captured, new()) == 2);
    Assert(await Enver.RuntimeConfig.Materializer.Program.Run(new[] { "--validate-runtime" }, new LocalOnlyRuntime(), captured, new()) != 0);
    Assert(await Enver.RuntimeConfig.Materializer.Program.Run(new[] { "--materialize" }, new LocalOnlyRuntime(), captured, new()) != 0);
}
finally { Console.SetOut(old); }
Assert(secretValues.All(s => !captured.ToString().Contains(s, StringComparison.Ordinal)));
// The production CLI accepts two fixed switches, never payload argv. Fakes bypass CLI only in-process.
if (count != 47) throw new Exception("Existing assertion count changed.");
Console.WriteLine($"Existing assertions: {count}/{count} PASS; canary leak checks PASS; AWS/network/API/Redis/Dynamo calls=0.");
await ObservabilityTests.Run(good, secretValues);
await ConfigParseRegressionTests.Run(good, secretValues);
await IdentityContractTests.Run();

sealed class LocalOnlyRuntime : IDeliveryRuntime, IRuntimeFileStore
{
    public IRuntimeFileStore Files => this;
    public void Prepare() => throw new UnauthorizedAccessException("LOCAL-CANARY-never-a-secret");
    public IRuntimeFileAttempt CreateTemporary() => throw new InvalidOperationException();
    public Task<ISecretReader> ResolveCredentials(CancellationToken cancellationToken) => throw new InvalidOperationException();
    public void ValidateRuntime(DeliveryTrace trace) => throw new FileNotFoundException("LOCAL-CANARY-never-a-secret");
    public void Dispose() { }
}

sealed class FakeReader(Dictionary<string, string> payloads, bool fail = false) : ISecretReader
{
    public Task<string> Read(SecretPin pin, CancellationToken cancellationToken) => fail
        ? throw new Exception("LOCAL-CANARY-never-a-secret") : Task.FromResult(payloads[pin.Name]);
}
sealed class FakeSecretsClient(Dictionary<string, string> payloads) : AmazonSecretsManagerClient(new AnonymousAWSCredentials(), RegionEndpoint.USEast1)
{
    internal readonly List<SecretPin> Calls = [];
    internal bool BadVersion, Throw;
    public override Task<GetSecretValueResponse> GetSecretValueAsync(GetSecretValueRequest request, CancellationToken cancellationToken = default)
    {
        var pin = Pins.All.Single(p => p.Arn == request.SecretId);
        if (request.VersionId != pin.Version || request.VersionStage is not null) throw new Exception("Pin test failed.");
        Calls.Add(pin);
        if (Throw) throw new Exception("LOCAL-CANARY-never-a-secret");
        return Task.FromResult(new GetSecretValueResponse { ARN = pin.Arn, VersionId = BadVersion ? "invalid" : pin.Version,
            SecretString = payloads[pin.Name], HttpStatusCode = HttpStatusCode.OK });
    }
}
