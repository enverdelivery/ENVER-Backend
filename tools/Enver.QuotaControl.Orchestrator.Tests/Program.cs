using System.Text;
using System.Text.Json;
using System.Net;
using System.Xml.Linq;
using Enver.QuotaControl.Orchestrator;

var passed = 0;
void Check(bool value) { if (!value) throw new Exception("ASSERTION_FAILED"); }
async Task Test(string name, Func<Task> action)
{
    try { await action(); passed++; Console.WriteLine("PASS " + name); }
    catch { Console.WriteLine("FAIL " + name); Environment.ExitCode = 1; }
}
async Task<ControlSession> Sequence(FakePlane plane, FakeInventory? inventory = null, SyntheticSecret? secret = null)
{
    var session = new ControlSession(plane, inventory ?? new(), secret ?? new(), TimeSpan.FromSeconds(900));
    foreach (var command in new[] { "precheck", "create-control", "associate-control", "run-inventory" })
    {
        await session.ExecuteAsync(command);
        if (session.State is SessionState.Incident or SessionState.Stop or SessionState.Restored) break;
    }
    return session;
}
await Test("complete simulated success and mandatory revocation", async () =>
{
    var p = new FakePlane(); await using var s = await Sequence(p);
    Check(s.State == SessionState.Restored && !p.Exists && !p.Member && p.Runtime == "baseline-runtime");
    Check(p.Actions.SequenceEqual(new[] { "create", "associate", "revoke", "delete" }));
});
await Test("CreateUser definite fail", async () =>
{
    var p = new FakePlane { Fail = "create" }; await using var s = await Sequence(p);
    Check(s.State == SessionState.Stop && !p.Exists && p.Actions.SequenceEqual(new[] { "create" }));
});
await Test("association fail requires cleanup", async () =>
{
    var p = new FakePlane { Fail = "associate" }; await using var s = await Sequence(p);
    Check(s.State == SessionState.Stop && !p.Exists && p.Actions.Contains("delete"));
});
await Test("inventory STOP requires revoke/delete", async () =>
{
    var p = new FakePlane(); await using var s = await Sequence(p, new() { Complete = false });
    Check(s.State == SessionState.Stop && !p.Exists && !p.Member && p.Actions.Contains("revoke"));
});
await Test("session expired stops without renewal", async () =>
{
    var p = new FakePlane { Fail = "create", Expired = true }; await using var s = await Sequence(p);
    Check(s.State == SessionState.Stop && s.Receipts.Any(x => x.Failure == Failure.Expired) && !p.Exists);
});
await Test("revoke failure is INCIDENT, never presumed revoked", async () =>
{
    var p = new FakePlane { Fail = "revoke" }; await using var s = await Sequence(p);
    Check(s.State == SessionState.Incident && p.Member && p.Exists && !p.Actions.Contains("delete"));
});
await Test("delete failure is INCIDENT", async () =>
{
    var p = new FakePlane { Fail = "delete" }; await using var s = await Sequence(p);
    Check(s.State == SessionState.Incident && !p.Member && p.Exists);
});
await Test("baseline mismatch cannot be silently repaired", async () =>
{
    var p = new FakePlane { DriftAfterDelete = true }; await using var s = await Sequence(p);
    Check(s.State == SessionState.Incident && p.Runtime == "external-drift" && !p.Exists);
});
await Test("partial cleanup receipt does not claim restored", async () =>
{
    var p = new FakePlane { Fail = "delete" }; await using var s = await Sequence(p);
    Check(s.Receipts.Any(x => x.Event == Event.Revoked) && !s.Receipts.Any(x => x.Event == Event.BaselineRestored));
});
await Test("synthetic secret absent from receipts argv ENV and zeroed", async () =>
{
    var secret = new SyntheticSecret(); var p = new FakePlane(); var inv = new FakeInventory();
    await using var s = await Sequence(p, inv, secret);
    Check(p.PasswordMatched && inv.PasswordMatched && secret.Buffer!.All(x => x == 0));
    var output = JsonSerializer.Serialize(s.Receipts) + string.Join(" ", Environment.GetCommandLineArgs()) +
        JsonSerializer.Serialize(Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>().Select(x => new { k = x.Key.ToString(), v = x.Value?.ToString() }));
    Check(!output.Contains(SyntheticSecret.Value, StringComparison.Ordinal));
});
await Test("ambiguous Create never adopts or repeats", async () =>
{
    var p = new FakePlane { AmbiguousCreate = true }; await using var s = await Sequence(p);
    Check(s.State == SessionState.Incident && p.Actions.SequenceEqual(new[] { "create" }));
});
await Test("preexisting CONTROL collision never deleted", async () =>
{
    var p = new FakePlane { Exists = true }; await using var s = await Sequence(p);
    Check(s.State == SessionState.Stop && p.Exists && p.Actions.Count == 0);
});
await Test("disposal after create cleans without association", async () =>
{
    var p = new FakePlane(); var s = new ControlSession(p, new FakeInventory(), new SyntheticSecret(), TimeSpan.FromSeconds(900));
    await s.ExecuteAsync("precheck"); await s.ExecuteAsync("create-control"); await s.DisposeAsync();
    Check(s.State == SessionState.Stop && !p.Exists && p.Actions.SequenceEqual(new[] { "create", "delete" }));
});
await Test("idempotent sequence no duplicate mutations", async () =>
{
    var p = new FakePlane(); await using var s = new ControlSession(p, new FakeInventory(), new SyntheticSecret(), TimeSpan.FromSeconds(900));
    foreach (var c in new[] { "precheck", "precheck", "create-control", "create-control", "associate-control", "associate-control", "run-inventory", "revoke-control", "delete-control", "verify-baseline" }) await s.ExecuteAsync(c);
    Check(p.Actions.SequenceEqual(new[] { "create", "associate", "revoke", "delete" }) && s.State == SessionState.Restored);
});
await Test("unknown command fails before generation", async () =>
{
    var p = new FakePlane(); var sec = new SyntheticSecret(); await using var s = new ControlSession(p, new FakeInventory(), sec, TimeSpan.FromSeconds(900));
    await s.ExecuteAsync("arbitrary-resource"); Check(sec.Buffer is null && p.Actions.Count == 0 && s.State == SessionState.Stop);
});
await Test("zero deadline cannot generate", async () =>
{
    var p = new FakePlane(); var sec = new SyntheticSecret(); await using var s = new ControlSession(p, new FakeInventory(), sec, TimeSpan.Zero);
    await s.ExecuteAsync("precheck"); Check(s.State == SessionState.Stop && sec.Buffer is null);
});
await Test("inventory exception revokes", async () =>
{
    var p = new FakePlane(); await using var s = await Sequence(p, new() { Throw = true });
    Check(s.State == SessionState.Stop && !p.Exists && !p.Member);
});
await Test("fixed AWS wire uses RAM body, no secret URL/header", async () =>
{
    var wire = new SyntheticWire(); using var adapter = new FixedAwsControlPlane(wire, DateTimeOffset.UtcNow.AddHours(1));
    await adapter.CreateAsync(Encoding.ASCII.GetBytes(SyntheticSecret.Value), default);
    Check(wire.Calls == 1 && wire.CreateBodyCorrect && !wire.SecretInHeadersOrUrl && wire.Signed);
});
await Test("AWS expired response stops adapter calls without renewal", async () =>
{
    var wire = new SyntheticWire { Expire = true }; using var adapter = new FixedAwsControlPlane(wire, DateTimeOffset.UtcNow.AddHours(1));
    for (var n = 0; n < 2; n++)
    {
        try { await adapter.ReadAsync(default); Check(false); } catch (ControlFailure f) { Check(f.Code == Failure.Expired); }
    }
    Check(wire.Calls == 1);
});
await Test("AWS XML membership endpoint baseline exact", async () =>
{
    var wire = new SyntheticWire(); using var adapter = new FixedAwsControlPlane(wire, DateTimeOffset.UtcNow.AddHours(1));
    var snapshot = await adapter.ReadAsync(default);
    Check(snapshot.Stable && snapshot.Control == ControlPresence.Absent && snapshot.Members.SequenceEqual(new[] { FixedResources.Runtime }));
    Check(wire.Calls == 6);
});
await Test("AWS XML extra member is not accepted baseline", async () =>
{
    var wire = new SyntheticWire { ExtraMember = true }; using var adapter = new FixedAwsControlPlane(wire, DateTimeOffset.UtcNow.AddHours(1));
    await using var session = new ControlSession(adapter, new FakeInventory(), new SyntheticSecret(), TimeSpan.FromSeconds(900));
    await session.ExecuteAsync("precheck"); Check(session.State == SessionState.Stop && !wire.CreateBodyCorrect);
});
await Test("AWS XML wrong endpoint fails closed", async () =>
{
    var wire = new SyntheticWire { WrongEndpoint = true }; using var adapter = new FixedAwsControlPlane(wire, DateTimeOffset.UtcNow.AddHours(1));
    try { await adapter.ReadAsync(default); Check(false); } catch (ControlFailure f) { Check(f.Code == Failure.Drift); }
});
await Test("verify-baseline performs a fresh read after cleanup", async () =>
{
    var p = new FakePlane(); await using var s = await Sequence(p); p.Runtime = "post-cleanup-drift";
    await s.ExecuteAsync("verify-baseline"); Check(s.State == SessionState.Incident);
});
await Test("ambiguous association cleans even if first snapshot omits member", async () =>
{
    var p = new FakePlane { AmbiguousAssociate = true, OmitMemberOnce = true }; await using var s = await Sequence(p);
    Check(s.State == SessionState.Stop && !p.Exists && !p.Member && p.Actions.Contains("revoke"));
});
await Test("delete ambiguity remains INCIDENT even if remotely absent", async () =>
{
    var p = new FakePlane { AmbiguousDelete = true }; await using var s = await Sequence(p);
    Check(s.State == SessionState.Incident && !p.Exists && !s.Receipts.Any(x => x.Event == Event.BaselineRestored));
});
await Test("deadline overrun remains INCIDENT after successful cleanup", async () =>
{
    var t = new SyntheticTime(); var p = new FakePlane { AfterDelete = () => t.Advance(TimeSpan.FromSeconds(901)) };
    await using var s = new ControlSession(p, new FakeInventory(), new SyntheticSecret(), TimeSpan.FromSeconds(900), t);
    foreach (var c in new[] { "precheck", "create-control", "associate-control", "run-inventory" }) await s.ExecuteAsync(c);
    Check(s.State == SessionState.Incident && !p.Exists && !p.Member && s.Receipts.Any(x => x.Failure == Failure.Deadline));
});
await Test("executable default is STOP before any profile or network access", async () =>
{
    var output = Console.Out; using var capture = new StringWriter();
    try { Console.SetOut(capture); Check(await Enver.QuotaControl.Orchestrator.Program.Main([]) == 2); }
    finally { Console.SetOut(output); }
    Check(capture.ToString().Contains("no action performed", StringComparison.Ordinal));
});
await Test("local PipeProducer CONTROL descriptor hashes checked without child", () =>
{
    new PipeInventory().Precheck(); return Task.CompletedTask;
});
Console.WriteLine($"RESULT {passed}/28 PASS; simulated only; AWS=0 Redis=0 PipeProducer child=0 real passwords=0");

sealed class SyntheticSecret : ISecretFactory
{
    public const string Value = "TEST_ONLY_NOT_A_REAL_PASSWORD_0123456789abc";
    public byte[]? Buffer;
    public byte[] Generate() => Buffer = Encoding.ASCII.GetBytes(Value);
}
sealed class FakeInventory : IInventory
{
    public bool Complete = true, Throw, PasswordMatched;
    public void Precheck() { }
    public Task<InventoryResult> RunAsync(ReadOnlyMemory<byte> password, CancellationToken token)
    {
        PasswordMatched = password.Span.SequenceEqual(Encoding.ASCII.GetBytes(SyntheticSecret.Value));
        if (Throw) throw new ControlFailure(Failure.InventoryStop);
        return Task.FromResult(new InventoryResult(Complete, Complete ? 0 : 2));
    }
}
sealed class FakePlane : IControlPlane
{
    public bool Exists, Member, Expired, AmbiguousCreate, AmbiguousAssociate, AmbiguousDelete, OmitMemberOnce, DriftAfterDelete, PasswordMatched;
    public Action? AfterDelete;
    public string Runtime = "baseline-runtime", Fail = "";
    public List<string> Actions = [];
    public Task<Snapshot> ReadAsync(CancellationToken token)
    {
        var visible = Member;
        if (Member && OmitMemberOnce) { visible = false; OmitMemberOnce = false; }
        return Task.FromResult(new Snapshot(Runtime, "fixed-resource", visible ? [FixedResources.Runtime, FixedResources.Control] : [FixedResources.Runtime], true, Exists ? ControlPresence.Expected : ControlPresence.Absent));
    }
    void Action(string action)
    {
        Actions.Add(action);
        if (Fail == action) throw new ControlFailure(Expired ? Failure.Expired : Failure.Rejected);
    }
    public Task CreateAsync(ReadOnlyMemory<byte> password, CancellationToken token)
    {
        Action("create"); Exists = true; PasswordMatched = password.Span.SequenceEqual(Encoding.ASCII.GetBytes(SyntheticSecret.Value));
        if (AmbiguousCreate) throw new ControlFailure(Failure.AmbiguousMutation);
        return Task.CompletedTask;
    }
    public Task AssociateAsync(CancellationToken token) { Action("associate"); Member = true; if (AmbiguousAssociate) throw new ControlFailure(Failure.AmbiguousMutation); return Task.CompletedTask; }
    public Task RevokeAsync(CancellationToken token) { Action("revoke"); Member = false; return Task.CompletedTask; }
    public Task DeleteAsync(CancellationToken token) { Action("delete"); Exists = false; if (DriftAfterDelete) Runtime = "external-drift"; AfterDelete?.Invoke(); if (AmbiguousDelete) throw new ControlFailure(Failure.AmbiguousMutation); return Task.CompletedTask; }
}

sealed class SyntheticTime : TimeProvider
{
    private long ticks;
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    public override long GetTimestamp() => ticks;
    public void Advance(TimeSpan duration) => ticks += duration.Ticks;
}

sealed class SyntheticWire : HttpMessageHandler
{
    public int Calls;
    public bool Expire, ExtraMember, WrongEndpoint, CreateBodyCorrect, SecretInHeadersOrUrl, Signed;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        Calls++;
        var text = await request.Content!.ReadAsStringAsync(token);
        var form = text.Split('&').Select(x => x.Split('=', 2)).ToDictionary(x => Uri.UnescapeDataString(x[0]), x => Uri.UnescapeDataString(x[1]));
        SecretInHeadersOrUrl |= (request.RequestUri!.ToString() + request.Headers).Contains(SyntheticSecret.Value, StringComparison.Ordinal);
        Signed |= request.Headers.Contains("Authorization") && request.Headers.Contains("X-Amz-Security-Token");
        if (request.RequestUri.Host != (form["Action"] == "GetCallerIdentity" ? "sts" : "elasticache") + ".us-east-1.amazonaws.com") throw new Exception("FIXED_HOST");
        if (Expire) return new(HttpStatusCode.Forbidden) { Content = new StringContent("<ErrorResponse><Error><Code>ExpiredToken</Code></Error></ErrorResponse>") };
        if (form["Action"] == "CreateUser")
        {
            CreateBodyCorrect = form["AuthenticationMode.Passwords.member.1"] == SyntheticSecret.Value && form["UserId"] == FixedResources.Control &&
                form["UserName"] == FixedResources.Control && form["AccessString"] == FixedResources.Acl && form.Count == 8;
            return Ok("<CreateUserResponse><CreateUserResult/></CreateUserResponse>");
        }
        string xml = form["Action"] switch
        {
            "GetCallerIdentity" => $"<Identity><Account>{FixedResources.Account}</Account><Arn>{FixedResources.Principal}</Arn></Identity>",
            "DescribeUsers" when form["UserId"] == FixedResources.Runtime => $"<Users><member><UserId>{FixedResources.Runtime}</UserId><UserName>{FixedResources.Runtime}</UserName><Status>active</Status><Engine>valkey</Engine><AccessString>unchanged-runtime</AccessString><Authentication><Type>password</Type><PasswordCount>1</PasswordCount></Authentication><UserGroupIds><member>{FixedResources.Group}</member></UserGroupIds></member></Users>",
            "DescribeUsers" => "<Users/>",
            "DescribeUserGroups" => $"<UserGroups><member><UserGroupId>{FixedResources.Group}</UserGroupId><Status>active</Status><Engine>valkey</Engine><UserIds><member>{FixedResources.Runtime}</member>{(ExtraMember ? "<member>unexpected</member>" : "")}</UserIds><ReplicationGroups><member>{FixedResources.Group}</member></ReplicationGroups><PendingChanges/></member></UserGroups>",
            "DescribeReplicationGroups" => $"<ReplicationGroups><ReplicationGroup><ReplicationGroupId>{FixedResources.Group}</ReplicationGroupId><Status>available</Status><TransitEncryptionEnabled>true</TransitEncryptionEnabled><ClusterEnabled>false</ClusterEnabled><UserGroupIds><member>{FixedResources.Group}</member></UserGroupIds><NodeGroups><NodeGroup><PrimaryEndpoint><Address>{(WrongEndpoint ? "wrong.example" : FixedResources.Endpoint)}</Address><Port>6379</Port></PrimaryEndpoint></NodeGroup></NodeGroups><PendingModifiedValues/></ReplicationGroup></ReplicationGroups>",
            "DescribeCacheClusters" => $"<CacheClusters><CacheCluster><CacheClusterId>{FixedResources.Group}-001</CacheClusterId><ReplicationGroupId>{FixedResources.Group}</ReplicationGroupId><CacheClusterStatus>available</CacheClusterStatus><TransitEncryptionEnabled>true</TransitEncryptionEnabled><Engine>valkey</Engine><SecurityGroups/></CacheCluster></CacheClusters>",
            _ => throw new Exception("NO_ARBITRARY_ACTION")
        };
        return Ok("<Response>" + xml + "</Response>");
    }
    private static HttpResponseMessage Ok(string xml) => new(HttpStatusCode.OK) { Content = new StringContent(xml) };
}
