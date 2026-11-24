using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Infrastructure.Redis;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

// Opt-in de un único test. Sin mensajes de excepción, argumentos, claves Redis ni payloads.
// El camino sano no agrega comandos Redis, serialización ni escritura de logs.
internal sealed class DistributedQuotaDiagnostics
{
    private static readonly AsyncLocal<DistributedQuotaDiagnostics?> Current = new();
    [ThreadStatic] private static bool observing;
    private readonly object sync = new();
    private readonly List<object> events = new(16);
    private readonly List<object> exceptions = new(8);
    private readonly string partition;
    private string phase = "setup";
    private int round;
    private bool evalReached, evalSubmitted, evalReturned, luaUnavailable, truncated, observerIncomplete;
    private string final = "NOT_RETURNED";
    private RedisResult? response;
    private AuthenticationQuotaConsumeRequest? request;

    internal DistributedQuotaDiagnostics(string metadataKey) =>
        partition = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(metadataKey)))[..16];

    internal IDisposable Observe()
    {
        var previous = Current.Value;
        Current.Value = this;
        AppDomain.CurrentDomain.FirstChanceException += OnException;
        return new Subscription(this, previous);
    }

    private void OnException(object? sender, FirstChanceExceptionEventArgs args)
    {
        if (Current.Value != this || observing) return;
        observing = true;
        try
        {
            lock (sync)
            {
                if (exceptions.Count == 6) { truncated = true; return; }
                // Solo métodos del producto, sin nombres de archivos, líneas, mensajes ni Exception.Data.
                var methods = new StackTrace(1, false).GetFrames()
                    .Select(frame => frame.GetMethod()).OfType<MethodBase>()
                    .Where(method => method.DeclaringType?.Namespace?.StartsWith("Enver.API.", StringComparison.Ordinal) == true)
                    .Select(method => method.DeclaringType!.Name + "." + method.Name)
                    .Select(method => method.Length <= 96 ? method : method[..96]).Take(2).ToArray();
                exceptions.Add(new { phase, round, beforeEvalSubmission = !evalSubmitted,
                    type = ExceptionCategory(args.Exception), methods,
                    budgetCause = args.Exception is Enver.API.Http.BudgetCanceledException canceled ? canceled.Cause.ToString() : "NOT_APPLICABLE" });
            }
        }
        catch { observerIncomplete = true; } // Fallo del observador, nunca reemplaza la excepción observada.
        finally { observing = false; }
    }

    private static string ExceptionCategory(Exception exception) => exception switch
    {
        RedisTimeoutException => "RedisTimeoutException",
        RedisConnectionException => "RedisConnectionException",
        RedisServerException => "RedisServerException",
        Enver.API.Http.BudgetCanceledException => "BudgetCanceledException",
        OperationCanceledException => "OperationCanceledException",
        TimeoutException => "TimeoutException",
        ArgumentException => "ArgumentException",
        ObjectDisposedException => "ObjectDisposedException",
        InvalidOperationException => "InvalidOperationException",
        _ => "OTHER_EXCEPTION"
    };

    private void Add(object row)
    {
        lock (sync)
        {
            if (events.Count < 16) events.Add(row);
            else truncated = true;
        }
    }

    internal void Phase(string name, int attempt)
    {
        phase = name; round = attempt;
        evalReached = evalSubmitted = evalReturned = luaUnavailable = false;
        response = null;
        final = "NOT_RETURNED";
    }

    internal void Seed(AuthenticationQuotaScopeType scope, long redisTime, long timestamp, long horizon) =>
        Add(new { phase, round, kind = "seed", scope = scope.ToString(), redisTimeUs = redisTime,
            timestampUs = timestamp, boundaryUs = horizon, expectedExpiryMs = (horizon + 999) / 1000 });

    internal void Retry(long maximumHorizon, long previousLast, long now, long retry) =>
        Add(new { phase, round, kind = "retry", maximumHorizonUs = maximumHorizon, previousLastUs = previousLast,
            logicalNowUs = now, retrySeconds = retry, calculatedRetrySeconds = (maximumHorizon - now + 999999) / 1000000 });

    internal void Request(AuthenticationQuotaConsumeRequest value) => request = value;
    internal void Connectivity(string host, RedisConnectionSignal signal) =>
        Add(new { phase, round, kind = "connectivity", host, signal = signal.ToString() });
    internal void Reached() => evalReached = true;
    internal void Submitted() => evalSubmitted = true; // Invocar el cliente NO prueba ejecución remota.
    internal void Reply(RedisResult value)
    {
        response = value;
        evalReturned = true;
    }
    internal void Decision(AuthenticationQuotaConsumeResult result)
    {
        // Categorizar SOLO después de que el adapter decidió; no consume su plazo de EVAL.
        if (response is not null)
        {
            var reply = RedisAuthenticationQuotaScriptProtocol.Parse(response);
            luaUnavailable = reply.IsSemantic && reply.Status == "unavailable";
            Add(new { phase, round, kind = "eval-return", semantic = reply.IsSemantic, status = reply.Status,
                category = reply.Category, logicalNowUs = reply.NowMicroseconds, retrySeconds = reply.RetryAfterSeconds });
        }
        final = result.Status.ToString();
        Add(new { phase, round, kind = "decision", final, cause = result.CancellationCause.ToString(),
            retryMs = result.RetryAfter?.TotalMilliseconds, evalReached, evalSubmitted, evalReturned });
    }

    internal async Task<Exception> FailureAsync(DistributedQuotaTestScope scope, Exception original)
    {
        // Lecturas SOLO después del fallo y antes del cleanup existente. No snapshot atómico,
        // no replay de Lua, retry ni inferencia del guard. Plazo exclusivamente diagnóstico.
        object snapshot;
        try
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var token = deadline.Token;
            var db = scope.Db;
            var time = (RedisResult[])(await db.ExecuteAsync("TIME").WaitAsync(token))!;
            var redisTime = checked((long)time[0] * 1000000 + (long)time[1]);
            var keys = new List<(string Kind, RedisKey Key)> { ("metadata", scope.Config.Keys.Metadata), ("registry", scope.Config.Keys.Registry) };
            if (request is not null)
                keys.AddRange(request.Scopes.Select(item => (item.ScopeType.ToString(), (RedisKey)scope.Data.Key(request, item))));
            var rows = await Task.WhenAll(keys.Select(async item =>
            {
                var exists = await db.KeyExistsAsync(item.Key).WaitAsync(token);
                var pttl = (long)await db.ExecuteAsync("PTTL", item.Key).WaitAsync(token);
                var expiry = (long)await db.ExecuteAsync("PEXPIRETIME", item.Key).WaitAsync(token);
                return new { kind = item.Kind, exists, pttlMs = pttl, expireAtMs = expiry };
            })).WaitAsync(token);
            RedisValue[] fields = ["count", "horizon", "last", "dirty", "version"];
            var values = await db.HashGetAsync(scope.Config.Keys.Metadata, fields).WaitAsync(token);
            var metadata = fields.Select((field, i) => new { field = field.ToString(),
                value = long.TryParse(values[i].ToString(), out var number) ? (long?)number : null }).ToArray();
            snapshot = new { nonAtomic = true, redisTimeUs = redisTime, keys = rows, metadata };
        }
        catch (Exception failure) { snapshot = new { unavailable = true, type = ExceptionCategory(failure) }; }
        string classification = final == "Unavailable" && luaUnavailable ? "LUA_UNAVAILABLE_CONFIRMED"
            : final == "Unavailable" && !evalSubmitted ? "PRE_EVAL_UNAVAILABLE"
            : "UNRESOLVED_OR_DIFFERENT_FAILURE";
        object[] eventRows, exceptionRows;
        lock (sync) { eventRows = events.ToArray(); exceptionRows = exceptions.ToArray(); }
        var json = JsonSerializer.Serialize(new { stage = 1, timing = scope.Timing?.Evidence(), classification, phase, round, partition,
            final, evalReached, evalSubmitted, evalReturned, luaGuard = "NOT_OBSERVABLE",
            truncated, observerIncomplete, events = eventRows, exceptions = exceptionRows, beforeCleanup = snapshot });
        // El colector CI conserva ErrorInfo, no StdOut. Original como InnerException con su stack.
        // Omitir arrays enteros si fuera necesario; nunca cortar un JSON ni un valor sensible.
        if (json.Length > 4400)
            json = JsonSerializer.Serialize(new { stage = 1, timing = scope.Timing?.Evidence(), classification, phase, round, partition,
                final, evalReached, evalSubmitted, evalReturned, luaGuard = "NOT_OBSERVABLE",
                truncated = true, observerIncomplete, exceptions = exceptionRows, beforeCleanup = snapshot });
        if (json.Length > 4400)
            json = JsonSerializer.Serialize(new { stage = 1, timing = scope.Timing?.Evidence(), classification, phase, round, partition,
                final, evalReached, evalSubmitted, evalReturned, luaGuard = "NOT_OBSERVABLE", truncated = true, observerIncomplete,
                omitted = "EXCEPTION_EVENTS_AND_POSTFAILURE_SNAPSHOT" });
        return new InvalidOperationException("DISTRIBUTED_QUOTA_DIAGNOSTIC " + json, original);
    }

    private sealed class Subscription(DistributedQuotaDiagnostics owner, DistributedQuotaDiagnostics? previous) : IDisposable
    {
        public void Dispose()
        {
            AppDomain.CurrentDomain.FirstChanceException -= owner.OnException;
            Current.Value = previous;
        }
    }
}
