using AppComercial.Api.Sdk;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AppComercial.Api.Infrastructure;

public class ContpaqiHostedService : IHostedService
{
    private readonly IContpaqiSdk _sdk;
    private readonly ILogger<ContpaqiHostedService> _logger;
    private readonly IConfiguration _config;

    public ContpaqiHostedService(IContpaqiSdk sdk, ILogger<ContpaqiHostedService> logger, IConfiguration config)
    {
        _sdk = sdk;
        _logger = logger;
        _config = config;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inicializando SDK de CONTPAQi Comercial Open Source...");
        try
        {
            var rutaEmpresa = _config["Contpaqi:DirectorioEmpresa"] ?? @"C:\Compac\Empresas\adCONTPAQi_Comercial";

            // Inicia la sesión global con el SDK. Requiere que COM interop cargue MGW10008.dll
            await _sdk.IniciarSesionAsync("SUPERVISOR");
            _logger.LogInformation("Sesión iniciada correctamente.");

            // Abre la empresa por defecto
            await _sdk.AbrirEmpresaAsync(rutaEmpresa);
            _logger.LogInformation("Empresa abierta correctamente: {Directorio}", rutaEmpresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error al iniciar y abrir la empresa en el SDK local.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cerrando sesión del SDK local...");
        try
        {
            await _sdk.CerrarEmpresaAsync();
            // Evitamos llamar a _sdk.TerminarSesion() porque en ASP.NET Core causa 
            // una excepcion C0000005 (Access Violation) durante el DLL_PROCESS_DETACH de MGW000.DLL.
            // Al cerrarse el proceso, Windows liberará los recursos automáticamente.
            // await _sdk.TerminarSesionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error al cerrar el SDK local.");
        }
    }
}
