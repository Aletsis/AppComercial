using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace AppComercial.Api.Gateway.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
}

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IConfiguration _configuration;
    private const string ApiKeyHeaderName = "X-Api-Key";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration) : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Extraer el encabezado X-Api-Key
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKeyValues))
        {
            return AuthenticateResult.NoResult();
        }

        var extractedApiKey = extractedApiKeyValues.ToString();
        if (string.IsNullOrWhiteSpace(extractedApiKey))
        {
            return AuthenticateResult.Fail("El encabezado X-Api-Key está vacío.");
        }

        // 2. Obtener la clave configurada en el servidor
        var configuredApiKey = _configuration["ApiSettings:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            return AuthenticateResult.Fail("La API Key no está configurada en el servidor.");
        }

        // 3. Comparar llaves
        if (configuredApiKey != extractedApiKey)
        {
            return AuthenticateResult.Fail("API Key inválida.");
        }

        // 4. Autenticación exitosa - Crear identidad del cliente
        var userName = "ApiKeyClient";
        if (Request.Headers.TryGetValue("X-Usuario", out var userHeader) && !string.IsNullOrWhiteSpace(userHeader))
        {
            userName = userHeader.ToString().Trim();
        }
        else if (Request.Headers.TryGetValue("X-User-Name", out var userHeader2) && !string.IsNullOrWhiteSpace(userHeader2))
        {
            userName = userHeader2.ToString().Trim();
        }
        else if (Request.Headers.TryGetValue("X-User", out var userHeader3) && !string.IsNullOrWhiteSpace(userHeader3))
        {
            userName = userHeader3.ToString().Trim();
        }

        var claims = new[] 
        { 
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, "InternalClient")
        };
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
