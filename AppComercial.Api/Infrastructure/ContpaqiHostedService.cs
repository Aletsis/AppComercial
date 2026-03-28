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
            var usuarioGlobal = _config["Contpaqi:Usuario"] ?? "SUPERVISOR";
            var passwordGlobal = _config["Contpaqi:Contrasena"] ?? "";

            // Inicia la sesión global con el SDK. Requiere que COM interop cargue MGW10008.dll
            await _sdk.IniciarSesionAsync(usuarioGlobal, passwordGlobal);
            _logger.LogInformation("Sesión iniciada correctamente con usuario: {Usuario}", usuarioGlobal);

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
            
            // HACK CONTPAQi: Incluso sin llamar a TerminarSesion, el Recolector de Basura 
            // intentará desasignar la memoria no administrada de COM, detonando el error C0000005 
            // y sacando una ventana de advertencia de Windows en el servidor (lo cual pausa todo).
            // La mejor solución heredada para esto en .NET es matar el proceso al instante, dejando 
            // que Windows Kernel limpie los recursos para evitar el enganche en DLL_PROCESS_DETACH.
            _logger.LogWarning("Forzando apagado del proceso para evitar choque de memoria con MGW000.dll...");
            
            // Da 500ms para que se escriban los últimos logs antes del "suicidio"
            await Task.Delay(500, cancellationToken);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error al cerrar el SDK local.");
        }
    }
}
