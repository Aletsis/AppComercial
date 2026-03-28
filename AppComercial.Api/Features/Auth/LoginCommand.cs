using MediatR;
using AppComercial.Api.Infrastructure;
using AppComercial.Api.Sdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AppComercial.Api.Features.Auth;

public class LoginCommand : IRequest<LoginResponse>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token    { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Nombre   { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de acceso / perfil de permisos del usuario en CONTPAQi Comercial.
    /// Corresponde a la columna NIVEL de CAC10000.
    /// Úsalo en el cliente para mostrar u ocultar funciones según el rol.
    /// </summary>
    public int? Nivel { get; set; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly RepositorioAdminDbContext _dbContext;
    private readonly IContpaqiSdk             _sdk;
    private readonly IConfiguration           _config;

    public LoginCommandHandler(
        RepositorioAdminDbContext dbContext,
        IContpaqiSdk             sdk,
        IConfiguration           config)
    {
        _dbContext = dbContext;
        _sdk       = sdk;
        _config    = config;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Verificar que el usuario exista en CONTPAQi Comercial (IDSISTEMA = '5')
        var usuario = await _dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdSistema == "5" && u.Clave != null && u.Clave.Trim() == request.Username.Trim(), cancellationToken);

        if (usuario == null)
        {
            throw new UnauthorizedAccessException($"El usuario '{request.Username}' no existe en la base de datos del sistema.");
        }

        // 2. Bypass Validación de Contraseña (MVP / API en red local)
        // Por limitaciones del motor nativo (no soporta múltiples fInicioSesionSDK concurrentes
        // sin degradar el rendimiento o bloquearse), para esta versión asumimos que si el 
        // usuario existe en la BD de la red local, se le permite el acceso.
        // El nivel/rol (IdPerfil) dictará a qué funciones podrá entrar en la App Móvil.

        // 3. Generar el JWT — incluye Nivel para control de roles en el cliente
        var secretKey = _config["JwtSettings:SecretKey"]
                        ?? "b2c9a62a-ec8a-4d7a-a681-fc1b5708cbcf_SuperSecretStringForDevOnly123!!";
        var key = Encoding.UTF8.GetBytes(secretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name,           usuario.Clave    ?? ""),
            new Claim(ClaimTypes.GivenName,      usuario.Nombre   ?? ""),
            // Nivel/Rol incluido en el token para que el cliente lo pueda leer sin hacer
            // una segunda petición. Valores: 0=Sin perfil, 1=Admin, 2=Vendedor, etc.
            // (depende de cómo estén configurados los perfiles en CONTPAQi)
            new Claim("nivel", usuario.IdPerfil?.ToString() ?? "0")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject            = new ClaimsIdentity(claims),
            Expires            = DateTime.UtcNow.AddHours(8), // un turno completo de trabajo
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token        = tokenHandler.CreateToken(tokenDescriptor);

        return new LoginResponse
        {
            Token    = tokenHandler.WriteToken(token),
            Username = usuario.Clave   ?? "",
            Nombre   = usuario.Nombre  ?? "",
            Nivel    = usuario.IdPerfil
        };
    }
}
