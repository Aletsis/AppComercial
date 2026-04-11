using AppComercial.Domain.Interfaces.SdkModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppComercial.Domain.Interfaces;

/// <summary>
/// Sistema CONTPAQi al que conectará el SDK.
/// Determina la función de inicialización correcta según el Manual de Referencia del SDK.
/// </summary>
public enum SistemaContpaqi
{
    /// <summary>
    /// CONTPAQi Comercial Premium — usa fInicializaSDK() para iniciar.
    /// </summary>
    Comercial,

    /// <summary>
    /// CONTPAQi Factura Electrónica — usa fSetNombrePAQ("CONTPAQ I Facturacion") en lugar de fInicializaSDK().
    /// </summary>
    FacturaElectronica
}

public interface IContpaqiSdk
{
    /// <param name="sistema">Sistema al que conectar. Por defecto Comercial.</param>
    Task IniciarSesionAsync(string nombreUsuario = "SUPERVISOR", string contrasena = "", SistemaContpaqi sistema = SistemaContpaqi.Comercial);
    Task TerminarSesionAsync();
    Task AbrirEmpresaAsync(string rutaEmpresa);
    Task CerrarEmpresaAsync();
    Task<int> CrearClienteAsync(tCteProv cliente);
    Task<int> ActualizarClienteAsync(string codigo, Dictionary<string, string> datos);
    Task<int> CrearProductoAsync(tProducto producto);
    Task<int> ActualizarProductoAsync(string codigo, Dictionary<string, string> datos);
    Task<int> CrearDocumentoAsync(tDocumento documento);
    Task<int> CrearMovimientoAsync(int idDocumento, tMovimiento movimiento);
    Task<int> CrearDireccionAsync(tDireccion direccion);
    Task<int> CrearUnidadMedidaAsync(tUnidad unidad);
    Task<int> EmitirDocumentoAsync(string codigoConcepto, string serie, double folio, string pass, string email);
    Task<int> CancelarDocumentoAsync(string codigoConcepto, string serie, double folio, string pass);
    Task<int> SaldarDocumentoAsync(string codigoConceptoDoc, string serieDoc, double folioDoc, string codigoConceptoPago, string seriePago, double folioPago);
    Task<int> AgregarRelacionCfdiAsync(string uuid, string tipoRelacion);

    // Nuevas funcionalidades agregadas
    Task<int> CrearAgenteAsync(Dictionary<string, string> datos);
    Task<int> ActualizarAgenteAsync(string codigo, Dictionary<string, string> datos);

    Task<int> CrearAlmacenAsync(Dictionary<string, string> datos);
    Task<int> ActualizarAlmacenAsync(string codigo, Dictionary<string, string> datos);

    Task<int> CrearConceptoAsync(Dictionary<string, string> datos);
    Task<int> ActualizarConceptoAsync(string codigo, Dictionary<string, string> datos);

    Task<int> ActualizarDocumentoAsync(string codigoConcepto, string serie, string folio, Dictionary<string, string> datos);
    Task<int> ActualizarDocumentoPorIdAsync(int idDocumento, Dictionary<string, string> datos);

    Task<int> ActualizarDireccionAsync(int idDireccion, Dictionary<string, string> datos);

    Task<int> CrearMonedaAsync(Dictionary<string, string> datos);
    Task<int> ActualizarMonedaAsync(int idMoneda, Dictionary<string, string> datos);

    Task<int> ActualizarMovimientoAsync(int idDocumento, int idMovimiento, Dictionary<string, string> datos);

    Task<int> ActualizarUnidadMedidaAsync(string nombreUnidad, Dictionary<string, string> datos);

    /// <summary>
    /// Valida las credenciales de un usuario contra el motor nativo de CONTPAQi Comercial.
    /// Usa fInicioSesionSDK internamente y restaura la sesión de SUPERVISOR si falla.
    /// Devuelve true si el usuario y contraseña son válidos.
    /// </summary>
    Task<bool> ValidarCredencialesAsync(string usuario, string contrasena, string rutaEmpresa);

    /// <summary>
    /// Genera un PDF del documento usando la forma impresa configurada en el concepto de CONTPAQi.
    /// El PDF se guarda en [DirectorioEmpresa]/XML_SDK/{serie}{folio}.pdf
    /// </summary>
    /// <returns>Ruta completa del archivo PDF generado en el servidor.</returns>
    Task<string> GenerarPdfAsync(string codigoConcepto, string serie, double folio, string rutaEmpresa);
}
