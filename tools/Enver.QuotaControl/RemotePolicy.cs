using System.Globalization;

namespace Enver.QuotaControl;

public static class RemotePolicy
{
    // AUTH es exclusivo del handshake; no existe comando AUTH arbitrario en RedisCommand.
    public static void EnsureAllowed(string operation, string verb)
    {
        if (operation != "inventory" || verb is not ("AUTH" or "SCAN" or "TYPE" or "PTTL" or "PEXPIRETIME" or
            "HGETALL" or "ZCARD" or "ZRANGE" or "TIME")) throw new ControlException(Failure.PolicyFailure);
    }

    public static void Validate(string operation, RedisCommand command)
    {
        EnsureAllowed(operation, command.Verb.ToString());
        var a = command.Arguments;
        var valid = command.Verb switch
        {
            RedisVerb.TIME => a.Count == 0,
            RedisVerb.SCAN => a.Count == 5 && Cursor(a[0]) && a[1] == "MATCH" && a[2] == Contract.BucketPattern && a[3] == "COUNT" && a[4] == "100",
            RedisVerb.HGETALL => a.Count == 1 && a[0] == Contract.Metadata,
            RedisVerb.TYPE or RedisVerb.PTTL or RedisVerb.PEXPIRETIME or RedisVerb.ZCARD => a.Count == 1 && Key(a[0]),
            RedisVerb.ZRANGE => a.Count == 4 && Key(a[0]) && Rank(a[1], out var start) && Rank(a[2], out var end) &&
                end >= start && end - start < 100 && a[3] == "WITHSCORES",
            _ => false
        };
        if (!valid) throw new ControlException(Failure.PolicyFailure);
    }

    private static bool Cursor(string value)
    {
        try { RedisCommand.ValidateCursor(value); return true; }
        catch { return false; }
    }
    private static bool Rank(string value, out int rank) => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out rank)
        && rank >= 0 && Contract.Decimal(rank) == value;
    private static bool Key(string key) => key == Contract.Metadata || key == Contract.Registry ||
        (key.Length <= 512 && key.StartsWith(Contract.BucketPrefix, StringComparison.Ordinal));

    public static void CheckError(RedisReply reply, bool authenticating = false)
    {
        if (reply.Kind != ReplyKind.Error) return;
        throw new ControlException(reply.ErrorCategory switch
        {
            ReplyError.Authentication => Failure.AuthTlsFailure,
            ReplyError.NoPermission => Failure.PolicyFailure,
            ReplyError.Topology => Failure.InventoryIncomplete,
            _ => authenticating ? Failure.AuthTlsFailure : Failure.ProtocolFailure
        });
    }

    public static void ValidateReply(RedisCommand command, RedisReply reply)
    {
        CheckError(reply);
        try
        {
            switch (command.Verb)
            {
                case RedisVerb.TYPE:
                    if (reply.Kind != ReplyKind.SimpleString || reply.Text is not ("none" or "hash" or "zset" or "string" or "set" or "list" or "stream")) Bad();
                    break;
                case RedisVerb.PTTL:
                case RedisVerb.PEXPIRETIME:
                    if (reply.Kind != ReplyKind.Integer || reply.Number < -2) Bad();
                    break;
                case RedisVerb.ZCARD:
                    if (reply.Kind != ReplyKind.Integer || reply.Number < 0) Bad();
                    break;
                case RedisVerb.HGETALL:
                    Flat(reply, 512);
                    break;
                case RedisVerb.ZRANGE:
                    Flat(reply, 200);
                    for (var i = 1; i < reply.Items.Count; i += 2) Contract.ParseUnsigned(reply.Items[i].Text);
                    break;
                case RedisVerb.SCAN:
                    if (reply.Items.Count != 2 || reply.Items[0].Kind != ReplyKind.Text) Bad();
                    RedisCommand.ValidateCursor(reply.Items[0].Text);
                    var keys = reply.Items[1].Items;
                    if (keys.Count > 10000 || keys.Any(k => k.Kind != ReplyKind.Text || k.Text.Length > 512)) Bad();
                    break;
                case RedisVerb.TIME:
                    if (reply.Items.Count != 2 || reply.Items.Any(p => p.Kind != ReplyKind.Text)) Bad();
                    InventoryReader.ParseTime(reply);
                    break;
                default: throw new ControlException(Failure.PolicyFailure);
            }
        }
        catch (ControlException exception) when (exception.Failure != Failure.PolicyFailure)
        { throw new ControlException(Failure.ProtocolFailure); }
    }

    private static void Flat(RedisReply reply, int maximum)
    {
        if (reply.Items.Count > maximum || reply.Items.Count % 2 != 0 || reply.Items.Any(p => p.Kind != ReplyKind.Text)) Bad();
    }
    private static void Bad() => throw new ControlException(Failure.ProtocolFailure);
}
