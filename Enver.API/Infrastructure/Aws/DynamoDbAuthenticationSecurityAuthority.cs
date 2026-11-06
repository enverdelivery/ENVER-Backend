using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;

namespace Enver.API.Infrastructure.Aws;

/// <summary>DynamoDB es autoridad operacional compartida, no almacén de revocaciones individuales.</summary>
public sealed class DynamoDbAuthenticationSecurityAuthority : IAuthenticationSecurityAuthority
{
    private const string AuthorityId = "authentication";
    private const string Id = "Id";
    private const string State = "State";
    private const string Generation = "Generation";
    private const string Revision = "Revision";
    private const string LastPossibleIssuanceUtc = "LastPossibleIssuanceUtc";
    private const string QuarantineUntilUtc = "QuarantineUntilUtc";
    private const string CapturedLifetimeSeconds = "CapturedLifetimeSeconds";
    private const string CapturedSkewSeconds = "CapturedSkewSeconds";
    private const string CapturedOperationalMarginSeconds = "CapturedOperationalMarginSeconds";

    private readonly IAmazonDynamoDB client;
    private readonly AuthenticationSecurityAuthorityOptions options;

    public DynamoDbAuthenticationSecurityAuthority(IAmazonDynamoDB client, AuthenticationSecurityAuthorityOptions options)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        if (!ValidTableName(options.TableName) || options.OperationTimeoutMilliseconds is < 1 or > 2000)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Las opciones de autoridad DynamoDB son inválidas.");
        }
    }

    public async ValueTask<AuthenticationSecurityAuthorityReadResult> ReadAsync(RequestBudget requestBudget)
    {
        ArgumentNullException.ThrowIfNull(requestBudget);
        using var dependency = requestBudget.BeginDependency(options.OperationTimeoutMilliseconds);
        var dispatched = false;
        try
        {
            dependency.ThrowIfCancellationRequested();
            dependency.MarkDispatched();
            dispatched = true;
            var response = await client.GetItemAsync(new GetItemRequest
            {
                TableName = options.TableName,
                ConsistentRead = true,
                Key = Key()
            }, dependency.Token).WaitAsync(dependency.Token).ConfigureAwait(false);
            dependency.ThrowIfCancellationRequested();
            if (response.Item is null || response.Item.Count == 0)
            {
                return new AuthenticationSecurityAuthorityReadResult(AuthenticationSecurityAuthorityResultStatus.Available);
            }

            return TryMap(response.Item, out var state)
                ? new AuthenticationSecurityAuthorityReadResult(AuthenticationSecurityAuthorityResultStatus.Available, state)
                : new AuthenticationSecurityAuthorityReadResult(AuthenticationSecurityAuthorityResultStatus.Unavailable);
        }
        catch (BudgetCanceledException exception)
        {
            return ReadCanceled(exception);
        }
        catch (OperationCanceledException)
        {
            return ReadCancellationFromBudget(dependency, dispatched);
        }
        catch (AmazonServiceException exception)
        {
            return new AuthenticationSecurityAuthorityReadResult(IsConfirmedUnavailable(exception)
                ? AuthenticationSecurityAuthorityResultStatus.Unavailable
                : dispatched ? AuthenticationSecurityAuthorityResultStatus.Uncertain : AuthenticationSecurityAuthorityResultStatus.Unavailable);
        }
        catch
        {
            return new AuthenticationSecurityAuthorityReadResult(dispatched
                ? AuthenticationSecurityAuthorityResultStatus.Uncertain : AuthenticationSecurityAuthorityResultStatus.Unavailable);
        }
    }

    public async ValueTask<AuthenticationSecurityAuthorityPotentialIssuanceResult> RegisterPotentialIssuanceAsync(RegisterPotentialIssuanceRequest request, RequestBudget requestBudget)
    {
        var current = await ReadAsync(requestBudget).ConfigureAwait(false);
        if (current.Status != AuthenticationSecurityAuthorityResultStatus.Available || current.State is not { State: AuthenticationSecurityGlobalState.Open } state)
        {
            return PotentialFromRead(current);
        }

        if (state.Generation != request.ExpectedGeneration || state.Revision != request.ExpectedRevision)
        {
            return new AuthenticationSecurityAuthorityPotentialIssuanceResult(AuthenticationSecurityAuthorityResultStatus.Conflict);
        }

        var latest = Max(state.LastPossibleIssuanceUtc, request.LatestPossibleIssuanceUtc);
        var expected = new AuthenticationSecurityAuthorityState(AuthenticationSecurityGlobalState.Open, state.Generation, checked(state.Revision + 1), latest, null, null, null, null);
        var updated = await UpdateAsync(
            "SET #last = :last, #revision = :nextRevision",
            OpenCondition(),
            Values(state, checked(state.Revision + 1), lastPossibleIssuanceUtc: latest),
            expected,
            requestBudget).ConfigureAwait(false);
        return new AuthenticationSecurityAuthorityPotentialIssuanceResult(updated.Status, updated.State, updated.CancellationCause);
    }

    public async ValueTask<AuthenticationSecurityAuthorityCloseResult> MergeClosedAsync(CloseAuthenticationSecurityRequest request, RequestBudget requestBudget)
    {
        var current = await ReadAsync(requestBudget).ConfigureAwait(false);
        if (current.Status != AuthenticationSecurityAuthorityResultStatus.Available || current.State is null)
        {
            return CloseFromRead(current);
        }

        var state = current.State;
        if (state.Generation != request.ExpectedGeneration || state.Revision != request.ExpectedRevision)
        {
            return new AuthenticationSecurityAuthorityCloseResult(AuthenticationSecurityAuthorityResultStatus.Conflict);
        }

        var last = Max(state.LastPossibleIssuanceUtc, request.LastPossibleIssuanceUtc);
        var (lifetime, skew, margin) = SelectHorizon(state, request);
        DateTimeOffset candidate;
        try
        {
            candidate = last.Add(TimeSpan.FromSeconds(checked(lifetime + skew + margin)));
        }
        catch (ArgumentOutOfRangeException)
        {
            return new AuthenticationSecurityAuthorityCloseResult(AuthenticationSecurityAuthorityResultStatus.Unavailable);
        }

        var quarantine = Max(state.QuarantineUntilUtc, candidate);
        var generation = state.State == AuthenticationSecurityGlobalState.Open ? checked(state.Generation + 1) : state.Generation;
        var expected = new AuthenticationSecurityAuthorityState(AuthenticationSecurityGlobalState.Closed, generation, checked(state.Revision + 1), last, quarantine, lifetime, skew, margin);
        var values = Values(state, checked(state.Revision + 1), generation, last, quarantine, lifetime, skew, margin);
        values[":closed"] = new AttributeValue { S = nameof(AuthenticationSecurityGlobalState.Closed) };
        var updated = await UpdateAsync(
            "SET #state = :closed, #generation = :nextGeneration, #revision = :nextRevision, #last = :last, #quarantine = :quarantine, #lifetime = :lifetime, #skew = :skew, #margin = :margin",
            StateCondition(state.State),
            values,
            expected,
            requestBudget).ConfigureAwait(false);
        return new AuthenticationSecurityAuthorityCloseResult(updated.Status, updated.State, updated.CancellationCause);
    }

    public async ValueTask<AuthenticationSecurityAuthorityReopenResult> TryReopenAsync(TryReopenAuthenticationSecurityRequest request, RequestBudget requestBudget)
    {
        var current = await ReadAsync(requestBudget).ConfigureAwait(false);
        if (current.Status != AuthenticationSecurityAuthorityResultStatus.Available || current.State is not { State: AuthenticationSecurityGlobalState.Closed } state)
        {
            return ReopenFromRead(current);
        }

        if (state.Generation != request.ExpectedGeneration || state.Revision != request.ExpectedRevision || request.ObservedNowUtc < state.QuarantineUntilUtc)
        {
            return new AuthenticationSecurityAuthorityReopenResult(AuthenticationSecurityAuthorityResultStatus.Conflict);
        }

        var values = Values(state, checked(state.Revision + 1));
        values[":open"] = new AttributeValue { S = nameof(AuthenticationSecurityGlobalState.Open) };
        values[":now"] = Number(request.ObservedNowUtc.ToUnixTimeMilliseconds());
        var expected = new AuthenticationSecurityAuthorityState(AuthenticationSecurityGlobalState.Open, state.Generation, checked(state.Revision + 1), null, null, null, null, null);
        var updated = await UpdateAsync(
            "SET #state = :open, #revision = :nextRevision REMOVE #last, #quarantine, #lifetime, #skew, #margin",
            $"{StateCondition(AuthenticationSecurityGlobalState.Closed)} AND #quarantine <= :now",
            values,
            expected,
            requestBudget).ConfigureAwait(false);
        return new AuthenticationSecurityAuthorityReopenResult(updated.Status, updated.State, updated.CancellationCause);
    }

    private async ValueTask<UpdateResult> UpdateAsync(string updateExpression, string conditionExpression, Dictionary<string, AttributeValue> values, AuthenticationSecurityAuthorityState expectedState, RequestBudget requestBudget)
    {
        using var dependency = requestBudget.BeginDependency(options.OperationTimeoutMilliseconds);
        var dispatched = false;
        try
        {
            dependency.ThrowIfCancellationRequested();
            dependency.MarkDispatched();
            dispatched = true;
            _ = await client.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = options.TableName,
                Key = Key(),
                UpdateExpression = updateExpression,
                ConditionExpression = conditionExpression,
                ExpressionAttributeNames = Names(updateExpression, conditionExpression),
                ExpressionAttributeValues = values,
                ReturnValues = ReturnValue.ALL_NEW
            }, dependency.Token).WaitAsync(dependency.Token).ConfigureAwait(false);
            dependency.ThrowIfCancellationRequested();
            // El estado esperado se deriva del GetItem consistente y de la mutación CAS confirmada.
            // DynamoDB Local no devuelve Attributes para UpdateItem aun con ALL_NEW en esta configuración.
            return new UpdateResult(AuthenticationSecurityAuthorityResultStatus.Available, expectedState);
        }
        catch (ConditionalCheckFailedException)
        {
            return new UpdateResult(AuthenticationSecurityAuthorityResultStatus.Conflict);
        }
        catch (BudgetCanceledException exception)
        {
            return UpdateCanceled(exception);
        }
        catch (OperationCanceledException)
        {
            return UpdateCancellationFromBudget(dependency, dispatched);
        }
        catch (AmazonServiceException exception)
        {
            return new UpdateResult(IsConfirmedUnavailable(exception)
                ? AuthenticationSecurityAuthorityResultStatus.Unavailable
                : dispatched ? AuthenticationSecurityAuthorityResultStatus.Uncertain : AuthenticationSecurityAuthorityResultStatus.Unavailable);
        }
        catch
        {
            return new UpdateResult(dispatched ? AuthenticationSecurityAuthorityResultStatus.Uncertain : AuthenticationSecurityAuthorityResultStatus.Unavailable);
        }
    }

    private static Dictionary<string, AttributeValue> Key() => new(StringComparer.Ordinal) { [Id] = new AttributeValue { S = AuthorityId } };

    private static Dictionary<string, string> Names(string updateExpression, string conditionExpression)
    {
        var all = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["#state"] = State, ["#generation"] = Generation, ["#revision"] = Revision, ["#last"] = LastPossibleIssuanceUtc,
            ["#quarantine"] = QuarantineUntilUtc, ["#lifetime"] = CapturedLifetimeSeconds, ["#skew"] = CapturedSkewSeconds, ["#margin"] = CapturedOperationalMarginSeconds
        };
        return all.Where(pair => updateExpression.Contains(pair.Key, StringComparison.Ordinal) || conditionExpression.Contains(pair.Key, StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
    }

    private static Dictionary<string, AttributeValue> Values(AuthenticationSecurityAuthorityState state, long nextRevision, long? nextGeneration = null,
        DateTimeOffset? lastPossibleIssuanceUtc = null, DateTimeOffset? quarantineUntilUtc = null, int? lifetime = null, int? skew = null, int? margin = null)
    {
        var values = new Dictionary<string, AttributeValue>(StringComparer.Ordinal)
        {
            [":expectedState"] = new AttributeValue { S = state.State.ToString() },
            [":expectedGeneration"] = Number(state.Generation),
            [":expectedRevision"] = Number(state.Revision),
            [":nextRevision"] = Number(nextRevision)
        };
        if (nextGeneration is not null) values[":nextGeneration"] = Number(nextGeneration.Value);
        if (lastPossibleIssuanceUtc is not null) values[":last"] = Number(lastPossibleIssuanceUtc.Value.ToUnixTimeMilliseconds());
        if (quarantineUntilUtc is not null) values[":quarantine"] = Number(quarantineUntilUtc.Value.ToUnixTimeMilliseconds());
        if (lifetime is not null) values[":lifetime"] = Number(lifetime.Value);
        if (skew is not null) values[":skew"] = Number(skew.Value);
        if (margin is not null) values[":margin"] = Number(margin.Value);
        return values;
    }

    private static string OpenCondition() => StateCondition(AuthenticationSecurityGlobalState.Open);
    private static string StateCondition(AuthenticationSecurityGlobalState state) => "#state = :expectedState AND #generation = :expectedGeneration AND #revision = :expectedRevision";
    private static AttributeValue Number(long value) => new() { N = value.ToString(CultureInfo.InvariantCulture) };

    private static bool TryMap(Dictionary<string, AttributeValue>? item, out AuthenticationSecurityAuthorityState? state, bool requireId = true)
    {
        state = null;
        if (item is null || (requireId && (!TryString(item, Id, out var id) || id != AuthorityId)) || !TryString(item, State, out var rawState) ||
            !Enum.TryParse<AuthenticationSecurityGlobalState>(rawState, false, out var globalState) || !TryLong(item, Generation, out var generation) || !TryLong(item, Revision, out var revision))
        {
            return false;
        }

        try
        {
            if (globalState == AuthenticationSecurityGlobalState.Open)
            {
                if (!TryOptionalUtc(item, LastPossibleIssuanceUtc, out var last) || item.ContainsKey(QuarantineUntilUtc) ||
                    item.ContainsKey(CapturedLifetimeSeconds) || item.ContainsKey(CapturedSkewSeconds) || item.ContainsKey(CapturedOperationalMarginSeconds)) return false;
                state = new AuthenticationSecurityAuthorityState(globalState, generation, revision, last, null, null, null, null);
                return true;
            }

            if (!TryUtc(item, LastPossibleIssuanceUtc, out var closedLast) || !TryUtc(item, QuarantineUntilUtc, out var quarantine) ||
                !TryInt(item, CapturedLifetimeSeconds, out var lifetime) || !TryInt(item, CapturedSkewSeconds, out var skew) || !TryInt(item, CapturedOperationalMarginSeconds, out var margin))
            {
                return false;
            }

            state = new AuthenticationSecurityAuthorityState(globalState, generation, revision, closedLast, quarantine, lifetime, skew, margin);
            return true;
        }
        catch (ArgumentException) { return false; }
        catch (OverflowException) { return false; }
    }

    private static bool TryString(IReadOnlyDictionary<string, AttributeValue> item, string name, out string value)
    {
        value = string.Empty;
        return item.TryGetValue(name, out var attribute) && !string.IsNullOrEmpty(attribute.S) && (value = attribute.S) is not null;
    }

    private static bool TryLong(IReadOnlyDictionary<string, AttributeValue> item, string name, out long value)
    {
        value = default;
        return item.TryGetValue(name, out var attribute) && long.TryParse(attribute.N, NumberStyles.None, CultureInfo.InvariantCulture, out value) && value >= 0;
    }

    private static bool TryInt(IReadOnlyDictionary<string, AttributeValue> item, string name, out int value)
    {
        value = default;
        return item.TryGetValue(name, out var attribute) && int.TryParse(attribute.N, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryUtc(IReadOnlyDictionary<string, AttributeValue> item, string name, out DateTimeOffset value)
    {
        value = default;
        return item.TryGetValue(name, out var attribute) && long.TryParse(attribute.N, NumberStyles.None, CultureInfo.InvariantCulture, out var milliseconds) && TryFromUnixMilliseconds(milliseconds, out value);
    }

    private static bool TryOptionalUtc(IReadOnlyDictionary<string, AttributeValue> item, string name, out DateTimeOffset? value)
    {
        value = null;
        if (!item.ContainsKey(name)) return true;
        if (!TryUtc(item, name, out var utc)) return false;
        value = utc;
        return true;
    }

    private static bool TryFromUnixMilliseconds(long milliseconds, out DateTimeOffset value)
    {
        try { value = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds); return true; }
        catch (ArgumentOutOfRangeException) { value = default; return false; }
    }

    private static bool IsConfirmedUnavailable(AmazonServiceException exception) => exception.ErrorCode is "ResourceNotFoundException" or "AccessDeniedException" or "UnrecognizedClientException" or "ValidationException" or "ProvisionedThroughputExceededException" or "ThrottlingException" or "RequestLimitExceeded";
    private static bool ValidTableName(string? name) => !string.IsNullOrWhiteSpace(name) && name.Length is >= 3 and <= 255 && name.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '-' or '.');
    private static DateTimeOffset Max(DateTimeOffset? first, DateTimeOffset second) => first is null || second > first ? second : first.Value;
    private static DateTimeOffset Max(DateTimeOffset? first, DateTimeOffset? second) => first is null ? second!.Value : second is null || first >= second ? first.Value : second.Value;

    private static (int Lifetime, int Skew, int Margin) SelectHorizon(AuthenticationSecurityAuthorityState current, CloseAuthenticationSecurityRequest request) =>
        current.State == AuthenticationSecurityGlobalState.Closed && current.CapturedHorizon >= request.CapturedHorizon
            ? (current.CapturedLifetimeSeconds!.Value, current.CapturedSkewSeconds!.Value, current.CapturedOperationalMarginSeconds!.Value)
            : (request.CapturedLifetimeSeconds, request.CapturedSkewSeconds, request.CapturedOperationalMarginSeconds);

    private static AuthenticationSecurityAuthorityPotentialIssuanceResult PotentialFromRead(AuthenticationSecurityAuthorityReadResult read) => new(
        read.Status == AuthenticationSecurityAuthorityResultStatus.Available ? AuthenticationSecurityAuthorityResultStatus.Unavailable : read.Status, null, read.CancellationCause);
    private static AuthenticationSecurityAuthorityCloseResult CloseFromRead(AuthenticationSecurityAuthorityReadResult read) => new(
        read.Status == AuthenticationSecurityAuthorityResultStatus.Available ? AuthenticationSecurityAuthorityResultStatus.Unavailable : read.Status, null, read.CancellationCause);
    private static AuthenticationSecurityAuthorityReopenResult ReopenFromRead(AuthenticationSecurityAuthorityReadResult read) => new(
        read.Status == AuthenticationSecurityAuthorityResultStatus.Available ? AuthenticationSecurityAuthorityResultStatus.Unavailable : read.Status, null, read.CancellationCause);
    private static AuthenticationSecurityAuthorityReadResult ReadCanceled(BudgetCanceledException exception) => new(exception.ResultUncertain ? AuthenticationSecurityAuthorityResultStatus.Uncertain : AuthenticationSecurityAuthorityResultStatus.Unavailable, null, exception.Cause);
    private static UpdateResult UpdateCanceled(BudgetCanceledException exception) => new(exception.ResultUncertain ? AuthenticationSecurityAuthorityResultStatus.Uncertain : AuthenticationSecurityAuthorityResultStatus.Unavailable, null, exception.Cause);

    private static AuthenticationSecurityAuthorityReadResult ReadCancellationFromBudget(DependencyBudget dependency, bool dispatched)
    {
        try { dependency.ThrowIfCancellationRequested(); }
        catch (BudgetCanceledException exception) { return ReadCanceled(exception); }
        return new AuthenticationSecurityAuthorityReadResult(dispatched ? AuthenticationSecurityAuthorityResultStatus.Uncertain : AuthenticationSecurityAuthorityResultStatus.Unavailable);
    }

    private static UpdateResult UpdateCancellationFromBudget(DependencyBudget dependency, bool dispatched)
    {
        try { dependency.ThrowIfCancellationRequested(); }
        catch (BudgetCanceledException exception) { return UpdateCanceled(exception); }
        return new UpdateResult(dispatched ? AuthenticationSecurityAuthorityResultStatus.Uncertain : AuthenticationSecurityAuthorityResultStatus.Unavailable);
    }

    private readonly record struct UpdateResult(AuthenticationSecurityAuthorityResultStatus Status, AuthenticationSecurityAuthorityState? State = null, BudgetCancellationCause CancellationCause = BudgetCancellationCause.None);
}
