using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AppComercial.Infrastructure;

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
        _logger.LogInformation("Inicializando SDK de CONTPAQi...");
        try
        {
            var rutaEmpresa    = _config["Contpaqi:DirectorioEmpresa"] ?? @"C:\Compac\Empresas\adEMPRESA_DE_PRUEBA";
            var usuarioGlobal  = _config["Contpaqi:Usuario"] ?? "SUPERVISOR";
            var passwordGlobal = _config["Contpaqi:Contrasena"] ?? "";

            // Leer el sistema seleccionado. Valores aceptados: "Comercial" | "FacturaElectronica"
            var sistemaStr = _config["Contpaqi:Sistema"] ?? "Comercial";
            var sistema = Enum.TryParse<SistemaContpaqi>(sistemaStr, ignoreCase: true, out var parsed)
                ? parsed
                : SistemaContpaqi.Comercial;

            _logger.LogInformation("Sistema seleccionado: {Sistema}", sistema);

            await _sdk.IniciarSesionAsync(usuarioGlobal, passwordGlobal, sistema);
            _logger.LogInformation("Sesión iniciada con usuario: {Usuario}", usuarioGlobal);

            await _sdk.AbrirEmpresaAsync(rutaEmpresa);
            _logger.LogInformation("Empresa abierta: {Directorio}", rutaEmpresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error al iniciar el SDK.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cerrando empresa en el SDK...");
        try
        {
            // Solo cerramos la empresa. NO llamamos fTerminaSDK() aquí.
            //
            // Razón: fTerminaSDK() descarga las DLLs nativas de la memoria del proceso.
            // Si el proceso (WPF) sigue vivo, el GC de .NET intentará limpiar los handles
            // P/Invoke que ya no existen → C0000005 (Access Violation).
            //
            // fTerminaSDK() únicamente se invoca desde MainWindow.OnClosed(),
            // seguido de Process.GetCurrentProcess().Kill() para evitar el crash del GC.
            await _sdk.CerrarEmpresaAsync();
            _logger.LogInformation("Empresa cerrada correctamente. SDK permanece inicializado hasta cerrar la aplicación.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error al cerrar la empresa en el SDK.");
        }
    }
}
