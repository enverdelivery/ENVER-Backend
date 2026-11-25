using Enver.API.Http;

namespace Enver.API.Features.Users.Application;

public enum GrantStatus { Enabled, Revoked }
public sealed record RescuerGrant(Guid Id, GrantStatus Status, long Revision, DateTime UpdatedAt, string RulesVersion, DateTime RulesAcceptedAt);

public enum GrantReadStatus { Unavailable, Missing, Confirmed }
public sealed record GrantRead(GrantReadStatus Status, RescuerGrant? Grant = null);
public enum GrantWriteStatus { Unavailable, Conflict, Reconciled, Confirmed }
public sealed record EnrollmentDecision(Guid OperationId, Guid UserId, string RulesVersion, string PolicyVersion, DateTime Now);

public interface IRescuerEnrollmentStore
{
    Task<GrantRead> ReadAsync(Guid id, RequestBudget budget);
    Task<GrantWriteStatus> InsertAsync(EnrollmentDecision decision, RequestBudget budget);
}
