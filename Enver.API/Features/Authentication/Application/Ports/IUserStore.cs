using Enver.API.Features.Authentication.Core;
using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Ports;

public interface IUserStore
{
    ValueTask<UserReadResult> FindByNormalizedEmailAsync(string normalizedEmail, RequestBudget budget);
    ValueTask<UserReadResult> FindByIdAsync(Guid id, RequestBudget budget);
    ValueTask<UserInsertResult> InsertAsync(User user, RequestBudget budget);
}

public enum UserReadStatus { Found, NotFound, Unavailable }
public enum UserInsertStatus { Confirmed, Conflict, Unavailable, Uncertain }

/// <summary>Resultado interno: nunca imprimir el usuario ni usarlo como contrato HTTP.</summary>
public sealed class UserReadResult
{
    public UserReadResult(UserReadStatus status, User? user = null,
        BudgetCancellationCause cancellationCause = BudgetCancellationCause.None)
    {
        if (!Enum.IsDefined(status) || !Enum.IsDefined(cancellationCause) ||
            (status == UserReadStatus.Found) != (user is not null) ||
            (status != UserReadStatus.Unavailable && cancellationCause != BudgetCancellationCause.None))
        {
            throw new ArgumentException("Resultado de lectura incompatible.");
        }

        Status = status;
        User = user;
        CancellationCause = cancellationCause;
    }

    public UserReadStatus Status { get; }
    public User? User { get; }
    public BudgetCancellationCause CancellationCause { get; }
    public override string ToString() => $"UserReadResult: {Status}";
}

public sealed class UserInsertResult
{
    public UserInsertResult(UserInsertStatus status, BudgetCancellationCause cancellationCause = BudgetCancellationCause.None)
    {
        if (!Enum.IsDefined(status) || !Enum.IsDefined(cancellationCause) ||
            (status is UserInsertStatus.Confirmed or UserInsertStatus.Conflict && cancellationCause != BudgetCancellationCause.None))
        {
            throw new ArgumentException("Resultado de inserción incompatible.");
        }

        Status = status;
        CancellationCause = cancellationCause;
    }

    public UserInsertStatus Status { get; }
    public BudgetCancellationCause CancellationCause { get; }
    public bool ResultUncertain => Status == UserInsertStatus.Uncertain;
    public override string ToString() => $"UserInsertResult: {Status}";
}
