using System.Globalization;

namespace Enver.QuotaControl;

// Lista cerrada: no API para comandos arbitrarios, credenciales o connection strings.
public enum RedisVerb { TYPE, PTTL, PEXPIRETIME, HGETALL, ZCARD, ZRANGE, SCAN, TIME, EVAL }

public sealed class RedisCommand
{
    public RedisVerb Verb { get; }
    public IReadOnlyList<string> Arguments { get; }
    private RedisCommand(RedisVerb verb, params string[] arguments)
    {
        Verb = verb;
        Arguments = Array.AsReadOnly(arguments);
    }

    public static RedisCommand ReadKey(RedisVerb verb, string key)
    {
        if (verb is not (RedisVerb.TYPE or RedisVerb.PTTL or RedisVerb.PEXPIRETIME or RedisVerb.HGETALL or RedisVerb.ZCARD) ||
            !AllowedKey(key) || (verb == RedisVerb.HGETALL && key != Contract.Metadata))
            throw new ControlException(Failure.InvalidArguments);
        return new(verb, key);
    }

    public static RedisCommand Range(string key, int first, int last)
    {
        if (!AllowedKey(key) || first < 0 || last < first || last - first >= 100)
            throw new ControlException(Failure.InvalidArguments);
        return new(RedisVerb.ZRANGE, key, Contract.Decimal(first), Contract.Decimal(last), "WITHSCORES");
    }

    public static RedisCommand Scan(string cursor)
    {
        ValidateCursor(cursor);
        return new(RedisVerb.SCAN, cursor, "MATCH", Contract.BucketPattern, "COUNT", "100");
    }

    public static RedisCommand Time() => new(RedisVerb.TIME);

    public static RedisCommand Recover(ApprovedDescriptor descriptor, string generation)
    {
        descriptor.Validate();
        Contract.ValidateGeneration(generation);
        return new(RedisVerb.EVAL, RecoveryScript.Text, "2", Contract.Metadata, Contract.Registry,
            descriptor.Snapshot(), generation);
    }

    public static void ValidateCursor(string cursor)
    {
        if (cursor.Length == 0 || (cursor.Length > 1 && cursor[0] == '0') ||
            cursor.Any(c => c is < '0' or > '9') ||
            !ulong.TryParse(cursor, NumberStyles.None, CultureInfo.InvariantCulture, out _))
            throw new ControlException(Failure.UnexpectedResponse);
    }

    private static bool AllowedKey(string key) => key == Contract.Metadata || key == Contract.Registry ||
        (key.Length <= 512 && key.StartsWith(Contract.BucketPrefix, StringComparison.Ordinal));

    // Nunca volcar argumentos (incluyen nombres opacos de buckets).
    public override string ToString() => Verb.ToString();
}

public enum ReplyKind { Text, SimpleString, Integer, Array, Null, Error }
public enum ReplyError { Unknown, Authentication, NoPermission, Topology }

public sealed class RedisReply
{
    public ReplyKind Kind { get; }
    public ReplyError ErrorCategory { get; private init; }
    private readonly string? text;
    private readonly long number;
    private readonly IReadOnlyList<RedisReply>? items;
    private RedisReply(ReplyKind kind, string? text = null, long number = 0, RedisReply[]? items = null)
    {
        Kind = kind;
        this.text = text;
        this.number = number;
        this.items = items is null ? null : System.Array.AsReadOnly((RedisReply[])items.Clone());
    }
    public static RedisReply String(string value) => new(ReplyKind.Text, text: value);
    public static RedisReply Simple(string value) => new(ReplyKind.SimpleString, text: value);
    public static RedisReply Integer(long value) => new(ReplyKind.Integer, number: value);
    public static RedisReply Array(params RedisReply[] values) => new(ReplyKind.Array, items: values);
    public static RedisReply Null() => new(ReplyKind.Null);
    public static RedisReply Error(ReplyError category = ReplyError.Unknown) => new(ReplyKind.Error) { ErrorCategory = category };
    public string Text => Kind is ReplyKind.Text or ReplyKind.SimpleString ? text! : throw new ControlException(Failure.UnexpectedResponse);
    public long Number => Kind == ReplyKind.Integer ? number : throw new ControlException(Failure.UnexpectedResponse);
    public IReadOnlyList<RedisReply> Items => Kind == ReplyKind.Array ? items! : throw new ControlException(Failure.UnexpectedResponse);
    public override string ToString() => Kind.ToString();
}

public interface IRedisCommandClient
{
    // Un transporte futuro debe fijar DB0/primary y cambiar este token ante reconexión/topología.
    object SessionIdentity { get; }
    bool IsLocalSimulation { get; }
    ValueTask<RedisReply> ExecuteAsync(RedisCommand command, CancellationToken cancellationToken);
}

public sealed class DisabledRedisClient : IRedisCommandClient
{
    public object SessionIdentity { get; } = new();
    public bool IsLocalSimulation => false;
    public ValueTask<RedisReply> ExecuteAsync(RedisCommand command, CancellationToken cancellationToken) =>
        ValueTask.FromException<RedisReply>(new ControlException(Failure.RemoteTransportDisabled));
}

public static class RecoveryScript
{
    public static string Text { get; } = Load();
    private static string Load()
    {
        using var stream = typeof(RecoveryScript).Assembly.GetManifestResourceStream("Enver.QuotaControl.RecoverClosed.lua")
            ?? throw new ControlException(Failure.ApplyDisabled);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
