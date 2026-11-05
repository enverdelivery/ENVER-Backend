using Enver.API.Features.Authentication.Validation;

namespace Enver.API.Http;

public interface IAuthenticationAdmissionChecks { ValueTask<bool> IsVerifiableAsync(HttpContext context, AuthenticationOperation operation, CancellationToken token); }
public interface IAuthenticationInputAssessment { ValueTask<AuthenticationInputAssessment> AssessAsync(HttpContext context, AuthenticationOperation operation, CancellationToken token); }
public interface IAuthenticationQuotaAdmission { ValueTask<AuthenticationQuotaResult> EvaluateAndConsumeAsync(HttpContext context, AuthenticationOperation operation, string internalAttemptId, CancellationToken token); }

/// <summary>D012: una admisión por request; sin Redis, usuarios o normalizador funcional.</summary>
public sealed class AuthenticationValidationCoordinator(IAuthenticationAdmissionChecks admission, IAuthenticationInputAssessment inputs, IAuthenticationQuotaAdmission quotas) : IAuthenticationBoundaryContinuation
{
    private static readonly object StateKey = new();
    private sealed record State(AuthenticationOperation Operation, Lazy<Task<AuthenticationCoordinationResult>> Work);

    public async Task ContinueAsync(HttpContext context, AuthenticationBoundaryResult result, RequestDelegate next)
    {
        if (result.ErrorStatus is null) { await next(context); return; }
        var decision = await CoordinateAsync(context, result);
        if (decision.ClientCanceled) context.Abort();
        else await ApiProblemDetails.TryWriteAsync(context, decision.HttpStatus ?? 400, decision.InvalidFields.Select(ToApi));
    }

    public async Task<AuthenticationCoordinationResult> CoordinateAsync(HttpContext context, AuthenticationBoundaryResult boundary, IEnumerable<AuthenticationValidationField>? mvcErrors = null)
    {
        State state;
        lock (context.Items)
        {
            if (!context.Items.TryGetValue(StateKey, out var existing))
            {
                existing = new State(boundary.Operation, new(() => EvaluateAsync(context, boundary), LazyThreadSafetyMode.ExecutionAndPublication));
                context.Items[StateKey] = existing;
            }
            state = (State)existing!;
        }
        if (state.Operation != boundary.Operation) return new(503, []);
        var evaluated = await state.Work.Value;
        var invalid = evaluated.InvalidFields.Concat(boundary.InvalidFields.Select(ToValidation)).Concat(mvcErrors ?? []).Distinct().ToArray();
        if (evaluated.ClientCanceled || evaluated.HttpStatus is not null) return evaluated with { InvalidFields = invalid };
        return evaluated with { HttpStatus = invalid.Length > 0 || boundary.ErrorStatus == 400 ? 400 : null, InvalidFields = invalid };
    }

    private async Task<AuthenticationCoordinationResult> EvaluateAsync(HttpContext context, AuthenticationBoundaryResult boundary)
    {
        var token = context.Features.Get<RequestBudget>()?.Token ?? context.RequestAborted;
        try
        {
            if (!await admission.IsVerifiableAsync(context, boundary.Operation, token)) return new(503, []);
            if (boundary.ErrorStatus == 400 && !boundary.JsonReadable) return new(400, [AuthenticationValidationField.Body]);
            if (boundary.Operation == AuthenticationOperation.Logout && context.User.Identity?.IsAuthenticated != true) return new(401, []);
            var assessment = await inputs.AssessAsync(context, boundary.Operation, token);
            if (!assessment.HasQuotaIdentity) return new(400, assessment.InvalidFields.Count > 0 ? assessment.InvalidFields : [boundary.Operation == AuthenticationOperation.Logout ? AuthenticationValidationField.Authorization : AuthenticationValidationField.Email]);
            var quota = await quotas.EvaluateAndConsumeAsync(context, boundary.Operation, Guid.NewGuid().ToString("N"), token);
            return quota.Status switch
            {
                AuthenticationQuotaStatus.Admitted => new(null, assessment.InvalidFields),
                AuthenticationQuotaStatus.Rejected => new(429, [], quota.RetryAfter),
                _ => new(503, [])
            };
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested || context.Features.Get<RequestBudget>()?.Cause == BudgetCancellationCause.Client) { return new(null, [], ClientCanceled: true); }
        catch (Exception) { return new(503, []); }
    }

    internal static ApiErrorField ToApi(AuthenticationValidationField field) => field switch
    {
        AuthenticationValidationField.FullName => ApiErrorField.FullName, AuthenticationValidationField.Email => ApiErrorField.Email, AuthenticationValidationField.Password => ApiErrorField.Password, AuthenticationValidationField.Authorization => ApiErrorField.Authorization, _ => ApiErrorField.Body
    };
    private static AuthenticationValidationField ToValidation(ApiErrorField field) => field switch
    {
        ApiErrorField.FullName => AuthenticationValidationField.FullName, ApiErrorField.Email => AuthenticationValidationField.Email, ApiErrorField.Password => AuthenticationValidationField.Password, ApiErrorField.Authorization => AuthenticationValidationField.Authorization, _ => AuthenticationValidationField.Body
    };
}
