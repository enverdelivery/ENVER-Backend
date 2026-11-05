using Microsoft.AspNetCore.Diagnostics;

namespace Enver.API.Http;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (context.RequestAborted.IsCancellationRequested || exception is BudgetCanceledException { Cause: BudgetCancellationCause.Client }) return true;
        if (context.Response.HasStarted) { context.Abort(); return true; }
        return await ApiProblemDetails.TryWriteAsync(context, exception is OperationCanceledException or TimeoutException ? 503 : 500);
    }
}
