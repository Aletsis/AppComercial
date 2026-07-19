using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AppComercial.Application.Features.Auth;
using Microsoft.Extensions.Configuration;

namespace AppComercial.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public class AuthTokensController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public AuthTokensController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    /// <summary>
    /// Endpoint central para iniciar sesión con un usuario de CONTPAQi.
    /// Valida credenciales contra la tabla CAC10000 de RepositorioAdminPAQ y devuelve un Token JWT.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginCommand command)
    {
        try
        {
            // ── Validar API Key si está configurado como obligatorio para el Login ─────
            var requireApiKey = _configuration.GetValue<bool>("ApiSettings:RequireApiKeyForLogin");
            if (requireApiKey)
            {
                if (!Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKeyValues))
                {
                    return Unauthorized(new { error = "Se requiere el encabezado 'X-Api-Key' para iniciar sesión." });
                }

                var configuredApiKey = _configuration["ApiSettings:ApiKey"];
                if (string.IsNullOrWhiteSpace(configuredApiKey) || configuredApiKey != extractedApiKeyValues.ToString())
                {
                    return Unauthorized(new { error = "API Key inválida para iniciar sesión." });
                }
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ocurrió un error en el motor de autenticación.", details = ex.Message });
        }
    }
}
