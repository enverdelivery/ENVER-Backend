using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Ports;

/// <summary>
/// Puerto futuro de 021; 052 no proporciona implementación ni registro DI.
/// Exige AuthenticationQuotaEvaluationContract y tiempo Redis compartido, no cuotas locales.
/// Unavailable/Uncertain se traducirán a AuthenticationQuotaStatus.Unavailable (503) en la frontera existente;
/// Rejected a 429 y Admitted a continuación D012, sin exponer incertidumbre públicamente.
/// </summary>
public interface IAuthenticationQuotaStore
{
    ValueTask<AuthenticationQuotaConsumeResult> EvaluateAndConsumeAsync(
        AuthenticationQuotaConsumeRequest request, RequestBudget requestBudget);
}
