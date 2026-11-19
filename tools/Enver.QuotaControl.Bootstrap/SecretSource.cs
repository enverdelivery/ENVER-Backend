using System.Text.Json;

namespace Enver.QuotaControl.Bootstrap;

internal interface ISecretSource
{
    ValueTask<RedisCredential> ReadAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Lee el JSON de credencial desde stdin redirigido. No acepta password por argv,
/// entorno, archivo, consola interactiva ni Secrets Manager dentro de este binario.
/// </summary>
internal sealed class StdinSecretSource(TextReader reader, bool isRedirected) : ISecretSource
{
    public async ValueTask<RedisCredential> ReadAsync(CancellationToken cancellationToken)
    {
        if (!isRedirected)
        {
            throw new BootstrapValidationException("secret-stdin-required");
        }

        string text;
        try
        {
            text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw new BootstrapValidationException("secret-input-cancelled");
        }
        catch
        {
            throw new BootstrapValidationException("secret-input-invalid");
        }

        if (string.IsNullOrWhiteSpace(text) || text.Length > 65536)
        {
            throw new BootstrapValidationException("secret-input-invalid");
        }

        try
        {
            using var document = JsonDocument.Parse(text);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("username", out var usernameElement) ||
                !document.RootElement.TryGetProperty("password", out var passwordElement) ||
                usernameElement.ValueKind != JsonValueKind.String ||
                passwordElement.ValueKind != JsonValueKind.String)
            {
                throw new BootstrapValidationException("secret-input-invalid");
            }

            return new RedisCredential(usernameElement.GetString() ?? string.Empty, passwordElement.GetString() ?? string.Empty);
        }
        catch (BootstrapValidationException)
        {
            throw;
        }
        catch
        {
            // No propagar el texto de entrada ni detalles del parser.
            throw new BootstrapValidationException("secret-input-invalid");
        }
    }
}
