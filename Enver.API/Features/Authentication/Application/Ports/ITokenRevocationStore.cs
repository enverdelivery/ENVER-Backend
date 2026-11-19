using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Ports;

/// <summary>Persistencia de marcas de revocación de tokens ya autenticados.</summary>
public interface ITokenRevocationStore
{
    ValueTask<TokenRevocationWriteResult> RevokeAsync(
        TokenRevocationWriteRequest request,
        RequestBudget requestBudget);

    ValueTask<TokenRevocationReadResult> CheckAsync(
        TokenRevocationReadRequest request,
        RequestBudget requestBudget);
}

public readonly record struct TokenRevocationWriteRequest(
    string Issuer,
    string Jti,
    DateTimeOffset ExpiresAtUtc,
    TimeSpan EffectiveClockSkew,
    TimeSpan OperationalMargin);

public readonly record struct TokenRevocationReadRequest(string Issuer, string Jti);

public enum TokenRevocationWriteStatus
{
    Confirmed,
    NoOperation,
    Unavailable,
    Uncertain
}

public enum TokenRevocationReadStatus
{
    Revoked,
    NotRevoked,
    Unavailable,
    Uncertain
}

public readonly record struct TokenRevocationWriteResult(
    TokenRevocationWriteStatus Status,
    BudgetCancellationCause CancellationCause = BudgetCancellationCause.None)
{
    public bool ResultUncertain => Status == TokenRevocationWriteStatus.Uncertain;
}

public readonly record struct TokenRevocationReadResult(
    TokenRevocationReadStatus Status,
    BudgetCancellationCause CancellationCause = BudgetCancellationCause.None)
{
    public bool ResultUncertain => Status == TokenRevocationReadStatus.Uncertain;
}
