using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

namespace Car.Tracker.Presentation.ConsultarPlacaModels;

/// <summary>
/// Adiciona Authorization Basic (usuário = Email, senha = ApiKey) em cada requisição ao provedor.
/// </summary>
public sealed class ConsultarPlacaAuthHandler(IOptions<ConsultarPlacaOptions> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var o = options.Value;
        if (!string.IsNullOrWhiteSpace(o.Email))
        {
            var raw = $"{o.Email}:{o.ApiKey}";
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
