using AppComercial.Domain.Interfaces.SdkModels;
using AppComercial.Domain.Interfaces;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace AppComercial.Infrastructure.Sdk;

[SupportedOSPlatform("windows")]
public class ContpaqiSdk : IContpaqiSdk
{
    // Semaphore to enforce single-threaded access to the native COM SDK
    private static readonly SemaphoreSlim _sdkSemaphore = new SemaphoreSlim(1, 1);

    // ─────────────────────────────────────────────────────────────────
    //  DllImport — CONTPAQi Comercial Premium (MGWServicios.dll)
    //  Ubicación: C:\Program Files (x86)\Compac\COMERCIAL
    // ─────────────────────────────────────────────────────────────────
    [DllImport("MGWServicios.dll", EntryPoint = "fInicializaSDK",   CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int  C_fInicializaSDK();

    [DllImport("MGWServicios.dll", EntryPoint = "fSetNombrePAQ",    CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int  C_fSetNombrePAQ(string aNombrePAQ);

    [DllImport("MGWServicios.dll", EntryPoint = "fInicioSesionSDK", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void C_fInicioSesionSDK(string aUsuario, string aContrasenia);

    [DllImport("MGWServicios.dll", EntryPoint = "fTerminaSDK",      CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void C_fTerminaSDK();

    [DllImport("MGWServicios.dll", EntryPoint = "fAbreEmpresa",     CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int  C_fAbreEmpresa(string aDirectorioEmpresa);

    [DllImport("MGWServicios.dll", EntryPoint = "fCierraEmpresa",   CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void C_fCierraEmpresa();

    // ─────────────────────────────────────────────────────────────────
    //  DllImport — CONTPAQi Factura Electrónica (MGW_SDK.dll)
    //  Ubicación: C:\Archivos de programa\Compacw\Facturacion
    // ─────────────────────────────────────────────────────────────────
    [DllImport("MGW_SDK.dll", EntryPoint = "fSetNombrePAQ",    CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int  F_fSetNombrePAQ(string aNombrePAQ);

    [DllImport("MGW_SDK.dll", EntryPoint = "fInicioSesionSDK", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void F_fInicioSesionSDK(string aUsuario, string aContrasenia);

    [DllImport("MGW_SDK.dll", EntryPoint = "fTerminaSDK",      CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void F_fTerminaSDK();

    [DllImport("MGW_SDK.dll", EntryPoint = "fAbreEmpresa",     CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int  F_fAbreEmpresa(string aDirectorioEmpresa);

    [DllImport("MGW_SDK.dll", EntryPoint = "fCierraEmpresa",   CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void F_fCierraEmpresa();

    [DllImport("MGW_SDK.dll", EntryPoint = "fEmitirDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int  F_fEmitirDocumento(string aCodConcepto, string aSerie, double aFolio, string aPassword, string aArchivoAdicional);

    [DllImport("MGW_SDK.dll", EntryPoint = "fBuscarDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int  F_fBuscarDocumento(string aCodConcepto, string aSerie, string aFolio);

    [DllImport("MGW_SDK.dll", EntryPoint = "fLeeDatoDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int  F_fLeeDatoDocumento(string aCampo, StringBuilder aValor, int aLongitud);

    [DllImport("MGW_SDK.dll", EntryPoint = "fLeeDatoConceptoDocto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int  F_fLeeDatoConceptoDocto(string aCampo, StringBuilder aValor, int aLongitud);

    [DllImport("MGW_SDK.dll", EntryPoint = "fBuscaProducto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int  F_fBuscaProducto(string aCodigo);

    [DllImport("MGW_SDK.dll", EntryPoint = "fError", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void F_fError(int aNumeroError, StringBuilder aMensaje, int aLongitud);

    // ─────────────────────────────────────────────────────────────────
    //  Despacho unificado — llama a la DLL correcta según _sistemaActual
    // ─────────────────────────────────────────────────────────────────
    private int  Sdk_fAbreEmpresa(string ruta)    => _sistemaActual == SistemaContpaqi.FacturaElectronica ? F_fAbreEmpresa(ruta)    : C_fAbreEmpresa(ruta);
    private void Sdk_fCierraEmpresa()             { if (_sistemaActual == SistemaContpaqi.FacturaElectronica) F_fCierraEmpresa();    else C_fCierraEmpresa(); }
    private void Sdk_fTerminaSDK()                { if (_sistemaActual == SistemaContpaqi.FacturaElectronica) F_fTerminaSDK();       else C_fTerminaSDK();    }
    private void Sdk_fInicioSesionSDK(string u, string p) { if (_sistemaActual == SistemaContpaqi.FacturaElectronica) F_fInicioSesionSDK(u, p); else C_fInicioSesionSDK(u, p); }

    private int Sdk_fEmitirDocumento(string c, string s, double f, string p, string a)
        => _sistemaActual == SistemaContpaqi.FacturaElectronica ? F_fEmitirDocumento(c, s, f, p, a) : fEmitirDocumento(c, s, f, p, a);

    private int Sdk_fBuscarDocumento(string c, string s, string f)
        => _sistemaActual == SistemaContpaqi.FacturaElectronica ? F_fBuscarDocumento(c, s, f) : fBuscarDocumento(c, s, f);

    private int Sdk_fLeeDatoDocumento(string aCampo, StringBuilder aValor, int aLongitud)
        => _sistemaActual == SistemaContpaqi.FacturaElectronica ? F_fLeeDatoDocumento(aCampo, aValor, aLongitud) : fLeeDatoDocumento(aCampo, aValor, aLongitud);

    private int Sdk_fLeeDatoConceptoDocto(string aCampo, StringBuilder aValor, int aLongitud)
        => _sistemaActual == SistemaContpaqi.FacturaElectronica ? F_fLeeDatoConceptoDocto(aCampo, aValor, aLongitud) : fLeeDatoConceptoDocto(aCampo, aValor, aLongitud);

    private void Sdk_fError(int e, StringBuilder m, int l)
    {
        if (_sistemaActual == SistemaContpaqi.FacturaElectronica) F_fError(e, m, l); else fError(e, m, l);
    }

    // ─────────────────────────────────────────────────────────────────
    //  Resolución del directorio de binarios según sistema y Registro
    // ─────────────────────────────────────────────────────────────────
    private static void SetDirectorioBinariosSDK(SistemaContpaqi sistema)
    {
        string? directorioBase = null;

        // CONTPAQi registra su DirectorioBase bajo distintos nombres según la versión.
        // Intentar todas las variantes conocidas antes de usar el fallback.
        var candidatosRegistro = sistema == SistemaContpaqi.FacturaElectronica
            ? new[]
            {
                @"SOFTWARE\Computación en Acción, SA CV\CONTPAQ I FACTURACION",
                @"SOFTWARE\Computacion en Accion, SA CV\CONTPAQ I FACTURACION",
                @"SOFTWARE\CONTPAQ i®\CONTPAQ I FACTURACION",
                @"SOFTWARE\CONTPAQ i(R)\CONTPAQ I FACTURACION",
            }
            : new[]
            {
                @"SOFTWARE\Computación en Acción, SA CV\CONTPAQ I COMERCIAL",
                @"SOFTWARE\Computacion en Accion, SA CV\CONTPAQ I COMERCIAL",
                @"SOFTWARE\CONTPAQ i®\CONTPAQ I COMERCIAL",
                @"SOFTWARE\CONTPAQ i(R)\CONTPAQ I COMERCIAL",
                @"SOFTWARE\Computación en Acción, SA CV\CONTPAQ i® Comercial",
                @"SOFTWARE\Computacion en Accion, SA CV\CONTPAQ i Comercial",
            };

        foreach (var subKey in candidatosRegistro)
        {
            try
            {
                using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                    .OpenSubKey(subKey, false);
                var val = key?.GetValue("DirectorioBase")?.ToString();
                if (!string.IsNullOrEmpty(val))
                {
                    directorioBase = val;
                    break;
                }
            }
            catch { /* continuar con la siguiente variante */ }
        }

        if (string.IsNullOrEmpty(directorioBase))
        {
            // Fallbacks documentados en el Manual de Referencia del SDK
            directorioBase = sistema == SistemaContpaqi.FacturaElectronica
                ? @"C:\Archivos de programa\Compacw\Facturacion"
                : @"C:\Program Files (x86)\Compac\Comercial";
        }

        if (!Directory.Exists(directorioBase))
            throw new DirectoryNotFoundException(
                $"El directorio de binarios del SDK ({sistema}) no existe: '{directorioBase}'. " +
                "Verifique que CONTPAQi esté instalado correctamente.");

        _directorioBinariosActual = directorioBase;
        Directory.SetCurrentDirectory(directorioBase);
    }

    private static bool _seInicializoSdkEnEsteProceso = false;
    private static SistemaContpaqi _sistemaActual = SistemaContpaqi.Comercial;
    private static string? _directorioBinariosActual = null;

    public static void AsegurarDirectorioBinarios()
    {
        if (!string.IsNullOrEmpty(_directorioBinariosActual))
        {
            Directory.SetCurrentDirectory(_directorioBinariosActual);
        }
    }

    public async Task IniciarSesionAsync(string nombreUsuario = "SUPERVISOR", string contrasena = "", SistemaContpaqi sistema = SistemaContpaqi.Comercial)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            if (_seInicializoSdkEnEsteProceso)
            {
                // Re-inicio: SDK ya cargado en memoria, solo re-autenticamos.
                Sdk_fInicioSesionSDK(nombreUsuario, contrasena);
                return;
            }

            // 1. W.Dir = carpeta de instalación correcta según sistema (lee Registry)
            SetDirectorioBinariosSDK(sistema);

            // 2. Inicializar según el sistema (Manual SDK):
            //    Comercial Premium   → fInicializaSDK()  en MGWServicios.dll
            //    Factura Electrónica → fSetNombrePAQ("CONTPAQ I Facturacion") en MGW_SDK.dll
            if (sistema == SistemaContpaqi.FacturaElectronica)
            {
                var paqResult = F_fSetNombrePAQ("CONTPAQ I Facturacion");
                if (paqResult != 0)
                    throw new Exception($"fSetNombrePAQ (Factura Electrónica) falló: {paqResult}");
            }
            else
            {
                var initResult = C_fInicializaSDK();
                if (initResult != 0)
                    throw new Exception($"fInicializaSDK (Comercial) falló: {initResult}");
            }

            // 3. Autenticar usuario
            Sdk_fInicioSesionSDK(nombreUsuario, contrasena);

            _sistemaActual = sistema;
            _seInicializoSdkEnEsteProceso = true;
        }
        finally { _sdkSemaphore.Release(); }
    }

    /// <summary>
    /// Valida las credenciales de un usuario usando el motor nativo de CONTPAQi.
    /// Estrategia: intenta iniciar sesión con las credenciales dadas. Si fSetNombrePAQ devuelve 0,
    /// las credenciales son válidas. Siempre restaura la sesión de SUPERVISOR al terminar.
    /// </summary>
    public async Task<bool> ValidarCredencialesAsync(string usuario, string contrasena, string rutaEmpresa)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            Sdk_fInicioSesionSDK(usuario, contrasena);
            int result = Sdk_fAbreEmpresa(rutaEmpresa);
            bool credencialesValidas = (result == 0);

            // Restaurar sesión de SUPERVISOR
            Sdk_fInicioSesionSDK("SUPERVISOR", "");
            Sdk_fAbreEmpresa(rutaEmpresa);

            return credencialesValidas;
        }
        finally { _sdkSemaphore.Release(); }
    }

    public async Task TerminarSesionAsync()
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            if (!_seInicializoSdkEnEsteProceso) return;
            Sdk_fTerminaSDK();
            _seInicializoSdkEnEsteProceso = false;
        }
        finally { _sdkSemaphore.Release(); }
    }

    public async Task AbrirEmpresaAsync(string rutaEmpresa)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            var result = Sdk_fAbreEmpresa(rutaEmpresa);
            LanzarExcepcionErrorSDK(result, "Error al abrir la empresa");
        }
        finally { _sdkSemaphore.Release(); }
    }

    public async Task CerrarEmpresaAsync()
    {
        await _sdkSemaphore.WaitAsync();
        try { Sdk_fCierraEmpresa(); }
        finally { _sdkSemaphore.Release(); }
    }

    [DllImport("MGWServicios.dll", EntryPoint = "fLlenaRegistroCteProv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fLlenaRegistroCteProv(ref tCteProv astCteProv, int aEsAlta);

    [DllImport("MGWServicios.dll", EntryPoint = "fAltaCteProv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fAltaCteProv(ref int aIdCteProv, ref tCteProv astCteProv);

    public async Task<int> CrearClienteAsync(tCteProv cliente)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            int nuevoId = 0;

            // Asignar valores por defecto y asegurar que ningún string sea nulo (evita SEHException en P/Invoke)
            cliente.cCodigoCliente = (cliente.cCodigoCliente ?? "").Trim();
            if (cliente.cCodigoCliente.Length > SdkConstantes.kLongCodigo - 1)
                cliente.cCodigoCliente = cliente.cCodigoCliente.Substring(0, SdkConstantes.kLongCodigo - 1);

            cliente.cRazonSocial = (cliente.cRazonSocial ?? "").Trim();
            if (cliente.cRazonSocial.Length > SdkConstantes.kLongNombre - 1)
                cliente.cRazonSocial = cliente.cRazonSocial.Substring(0, SdkConstantes.kLongNombre - 1);

            var rfc = (cliente.cRFC ?? "").Trim().ToUpperInvariant();
            cliente.cRFC = string.IsNullOrWhiteSpace(rfc) ? "XAXX010101000" : rfc;
            if (cliente.cRFC.Length > SdkConstantes.kLongRFC - 1)
                cliente.cRFC = cliente.cRFC.Substring(0, SdkConstantes.kLongRFC - 1);

            cliente.cFechaAlta = string.IsNullOrWhiteSpace(cliente.cFechaAlta) ? DateTime.Now.ToString("MM/dd/yyyy") : cliente.cFechaAlta;
            cliente.cCURP = cliente.cCURP ?? "";
            cliente.cDenComercial = cliente.cDenComercial ?? "";
            cliente.cRepLegal = cliente.cRepLegal ?? "";
            cliente.cNombreMoneda = string.IsNullOrWhiteSpace(cliente.cNombreMoneda) ? "Peso Mexicano" : cliente.cNombreMoneda;
            cliente.cCodigoValorClasificacionCliente1 = cliente.cCodigoValorClasificacionCliente1 ?? "";
            cliente.cCodigoValorClasificacionCliente2 = cliente.cCodigoValorClasificacionCliente2 ?? "";
            cliente.cCodigoValorClasificacionCliente3 = cliente.cCodigoValorClasificacionCliente3 ?? "";
            cliente.cCodigoValorClasificacionCliente4 = cliente.cCodigoValorClasificacionCliente4 ?? "";
            cliente.cCodigoValorClasificacionCliente5 = cliente.cCodigoValorClasificacionCliente5 ?? "";
            cliente.cCodigoValorClasificacionCliente6 = cliente.cCodigoValorClasificacionCliente6 ?? "";
            cliente.cFechaBaja = cliente.cFechaBaja ?? "";
            cliente.cFechaUltimaRevision = cliente.cFechaUltimaRevision ?? "";
            cliente.cMensajeria = cliente.cMensajeria ?? "";
            cliente.cCuentaMensajeria = cliente.cCuentaMensajeria ?? "";
            cliente.cCodigoAlmacen = cliente.cCodigoAlmacen ?? "";
            cliente.cCodigoAgenteVenta = cliente.cCodigoAgenteVenta ?? "";
            cliente.cCodigoAgenteCobro = cliente.cCodigoAgenteCobro ?? "";
            cliente.cCodigoValorClasificacionProveedor1 = cliente.cCodigoValorClasificacionProveedor1 ?? "";
            cliente.cCodigoValorClasificacionProveedor2 = cliente.cCodigoValorClasificacionProveedor2 ?? "";
            cliente.cCodigoValorClasificacionProveedor3 = cliente.cCodigoValorClasificacionProveedor3 ?? "";
            cliente.cCodigoValorClasificacionProveedor4 = cliente.cCodigoValorClasificacionProveedor4 ?? "";
            cliente.cCodigoValorClasificacionProveedor5 = cliente.cCodigoValorClasificacionProveedor5 ?? "";
            cliente.cCodigoValorClasificacionProveedor6 = cliente.cCodigoValorClasificacionProveedor6 ?? "";
            cliente.cTextoExtra1 = cliente.cTextoExtra1 ?? "";
            cliente.cTextoExtra2 = cliente.cTextoExtra2 ?? "";
            cliente.cTextoExtra3 = cliente.cTextoExtra3 ?? "";

            if (cliente.cTipoCliente == 0) cliente.cTipoCliente = 1;
            if (cliente.cBanVentaCredito == 0) cliente.cBanVentaCredito = 1;
            if (cliente.cEstatus == 0) cliente.cEstatus = 1;

            var result = fAltaCteProv(ref nuevoId, ref cliente);
            LanzarExcepcionErrorSDK(result, "Error al crear cliente en SDK.");

            return nuevoId;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    [DllImport("MGWServicios.dll", EntryPoint = "fBuscaCteProv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fBuscaCteProv(string aCodigo);

    private int Sdk_fBuscaCteProv(string aCodigo) => fBuscaCteProv(aCodigo ?? "");
    private int Sdk_fBuscaProducto(string aCodigo) => fBuscaProducto(aCodigo ?? "");

    [DllImport("MGWServicios.dll", EntryPoint = "fEditaCteProv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEditaCteProv();

    [DllImport("MGWServicios.dll", EntryPoint = "fSetDatoCteProv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSetDatoCteProv(string aCampo, string aValor);

    [DllImport("MGWServicios.dll", EntryPoint = "fGuardaCteProv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fGuardaCteProv();

    [DllImport("MGWServicios.dll", EntryPoint = "fCancelarModificacionCteProv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fCancelarModificacionCteProv();

    public Task<int> ActualizarClienteAsync(string codigo, Dictionary<string, string> datos) =>
        EjecutarOperacionDictionaryAsync(() => fBuscaCteProv(codigo), fEditaCteProv, fSetDatoCteProv, fGuardaCteProv, datos, "Cliente", () => fCancelarModificacionCteProv());

    [DllImport("MGWServicios.dll", EntryPoint = "fAltaProducto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fAltaProducto(ref int aIdProducto, ref tProducto astProducto);

    [DllImport("MGWServicios.dll", EntryPoint = "fCancelarModificacionProducto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fCancelarModificacionProducto();

    [DllImport("MGWServicios.dll", EntryPoint = "fError", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void fError(int aNumeroError, StringBuilder aMensaje, int aLongitud);

    public async Task<int> CrearProductoAsync(tProducto producto)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            int nuevoId = 0;

            // Asignamos valores default requeridos por el SDK interno si vienen vacíos
            producto.cFechaAltaProducto = string.IsNullOrEmpty(producto.cFechaAltaProducto) ? DateTime.Now.ToString("MM/dd/yyyy") : producto.cFechaAltaProducto;
            if (producto.cTipoProducto == 0) producto.cTipoProducto = 1; // 1 = Producto
            if (producto.cStatusProducto == 0) producto.cStatusProducto = 1; // 1 = Alta
            if (string.IsNullOrEmpty(producto.cCodigoUnidadBase)) producto.cCodigoUnidadBase = "PIEZA"; // Requerido por CONTPAQi
            
            var result = fAltaProducto(ref nuevoId, ref producto);
            LanzarExcepcionErrorSDK(result, "Error SDK");

            return nuevoId;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    [DllImport("MGWServicios.dll", EntryPoint = "fBuscaProducto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fBuscaProducto(string aCodigo);

    [DllImport("MGWServicios.dll", EntryPoint = "fEditaProducto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEditaProducto();

    [DllImport("MGWServicios.dll", EntryPoint = "fSetDatoProducto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSetDatoProducto(string aCampo, string aValor);

    [DllImport("MGWServicios.dll", EntryPoint = "fGuardaProducto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fGuardaProducto();

    public async Task<int> ActualizarProductoAsync(string codigo, Dictionary<string, string> datos)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            var result = fBuscaProducto(codigo);
            if (result != 0) throw new Exception($"Producto no encontrado. Código error: {result}");

            result = fEditaProducto();
            if (result != 0)
            {
                StringBuilder sb = new StringBuilder(512);
                fError(result, sb, 512);
                fCancelarModificacionProducto(); // En caso de que se haya quedado tomado
                throw new Exception($"Error al iniciar edición del producto. Código error: {result} - {sb.ToString()}");
            }

            try
            {
                foreach (var dato in datos)
                {
                    result = fSetDatoProducto(dato.Key, dato.Value);
                    if (result != 0) throw new Exception($"Error al setear campo {dato.Key}. Código error: {result}");
                }

                result = fGuardaProducto();
                LanzarExcepcionErrorSDK(result, "Error SDK al actualizar producto");
            }
            catch
            {
                fCancelarModificacionProducto();
                throw;
            }

            return 1;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    [DllImport("MGWServicios.dll", EntryPoint = "fEntregEnDiscoXML", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEntregEnDiscoXML(string aCodConcepto, string aSerie, double aFolio, int aTipoArchivo, string aRutaPlantilla);

    [DllImport("MGWServicios.dll", EntryPoint = "fAltaDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fAltaDocumento(ref int aIdDocumento, ref tDocumento astDocumento);

    public async Task<string> GenerarPdfAsync(string codigoConcepto, string serie, double folio, string rutaEmpresa)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            // tipoArchivo = 1 → PDF (0 = XML)
            var result = fEntregEnDiscoXML(codigoConcepto, serie ?? string.Empty, folio, 1, string.Empty);
            if (result != 0)
            {
                StringBuilder sb = new StringBuilder(512);
                fError(result, sb, 512);
                throw new Exception($"Error al generar PDF en SDK. Código: {result} - {sb}");
            }

            // CONTPAQi generates: [Concept][Serie][Folio].pdf
            // However, fEntregEnDiscoXML uses various padding conventions.
            string dirArchivoDigital = Path.Combine(rutaEmpresa, "XML_SDK");
            string sConcepto = (codigoConcepto ?? "").Trim();
            string sSerie = (serie ?? "").Trim();
            string sFolio = folio.ToString("0");

            // Look for files ending with [Folio].pdf that contain nuestra Serie o Concepto
            string? rutaArchivo = Directory.GetFiles(dirArchivoDigital, $"*{sFolio}.pdf")
                .Where(f => {
                    var name = Path.GetFileName(f);
                    bool matchConcepto = !string.IsNullOrEmpty(sConcepto) && name.StartsWith(sConcepto);
                    bool matchSerie = !string.IsNullOrEmpty(sSerie) && name.Contains(sSerie);
                    // Si no pedimos serie, al menos que coincida el concepto
                    return matchConcepto || matchSerie;
                })
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .FirstOrDefault();

            if (rutaArchivo == null || !File.Exists(rutaArchivo))
                throw new FileNotFoundException($"El PDF fue generado por el SDK pero no se pudo encontrar en {dirArchivoDigital}. Patrón: *{sFolio}.pdf");

            return rutaArchivo;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    public async Task<int> CrearDocumentoAsync(tDocumento documento)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            int nuevoId = 0;
            var result = fAltaDocumento(ref nuevoId, ref documento);
            LanzarExcepcionErrorSDK(result, "Error al crear documento en SDK.");
            return nuevoId;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    [DllImport("MGWServicios.dll", EntryPoint = "fLeeDatoProducto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fLeeDatoProducto(string aCampo, StringBuilder aValor, int aLongitud);

    [DllImport("MGWServicios.dll", EntryPoint = "fAltaMovimiento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fAltaMovimiento(int aIdDocumento, ref int aIdMovimiento, ref tMovimiento astMovimiento);

    [DllImport("MGWServicios.dll", EntryPoint = "fBuscarIdDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fBuscarIdDocumento(int aIdDocumento);

    public async Task<int> CrearMovimientoAsync(int idDocumento, tMovimiento movimiento)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            string originalProductCode = movimiento.aCodProdSer;
            string? idProductoStr = null;

            // 1. OBTENER ID PRODUCTO PRIMERO
            // Hacemos esto antes de manipular el Documento para no romper el apuntador interno (causa de 0xC0000005)
            // Esto mueve el "cursor" del SDK a catálogo de Productos momentáneamente.
            if (fBuscaProducto(originalProductCode) == 0)
            {
                StringBuilder sbId = new StringBuilder(50);
                if (fLeeDatoProducto("CIDPRODUCTO", sbId, 50) == 0)
                {
                    idProductoStr = sbId.ToString();
                }
            }

            // 2. ENFOCAR EL DOCUMENTO
            // Cambiamos el "cursor" al documento y lo ponemos en edición para que los totales puedan afectarse.
            var resultDoc = fBuscarIdDocumento(idDocumento);
            if (resultDoc != 0) throw new Exception($"Documento con ID {idDocumento} no encontrado en SDK. Código: {resultDoc}");

            resultDoc = fEditarDocumento();
            if (resultDoc != 0) throw new Exception($"No se pudo iniciar la edición del Documento {idDocumento}. Código: {resultDoc}");

            // 3. AGREGAR MOVIMIENTO BASE
            int nuevoId = 0;
            var result = fAltaMovimiento(idDocumento, ref nuevoId, ref movimiento);
            if (result != 0)
            {
                fCancelarModificacionDocumento();
                fDesbloqueaDocumento();
                StringBuilder sb = new StringBuilder(512);
                fError(result, sb, 512);
                throw new Exception($"Error al agregar movimiento en SDK. Código: {result} - {sb.ToString()}");
            }

            // 4. EDITAR CON IDENTIFICADOR SEGURO (Bypass a la separación de guiones en PR-0015)
            if (nuevoId > 0 && !string.IsNullOrEmpty(idProductoStr))
            {
                // Focalizamos el cursor en el nuevo movimiento específico.
                if (fBuscarIdMovimiento(nuevoId) == 0)
                {
                    var resultEditaMov = fEditarMovimiento();
                    if (resultEditaMov == 0)
                    {
                        // En lugar del código con guion, mandamos el ID interno intacto.
                        fSetDatoMovimiento("CIDPRODUCTO", idProductoStr);

                        if (!string.IsNullOrEmpty(movimiento.aCodAlmacen))
                        {
                            fSetDatoMovimiento("ALMACEN", movimiento.aCodAlmacen);
                        }

                        // Reinscribir precio y cantidades para que el motor asimile el redibujado de la partida
                        fSetDatoMovimiento("PRECIO", movimiento.aPrecio.ToString());
                        fSetDatoMovimiento("UNIDADES", movimiento.aUnidades.ToString());

                        var resGuardaMov = fGuardaMovimiento();
                        if (resGuardaMov != 0)
                        {
                            fCancelaCambiosMovimiento();
                        }
                    }
                }
            }

            // 5. GUARDAR DOCUMENTO Y DISPARAR TRIGGERS DE TOTALES
            // Este paso consolida todo: Cuadra los impuestos y recalcula el total neto para la Base de Datos.
            resultDoc = fGuardaDocumento();
            if (resultDoc != 0)
            {
                fDesbloqueaDocumento();
                StringBuilder sb = new StringBuilder(512);
                fError(resultDoc, sb, 512);
                throw new Exception($"Movimiento agregado, pero error al actualizar totales del documento. SDK: {resultDoc} - {sb.ToString()}");
            }

            fDesbloqueaDocumento();

            return nuevoId;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    [DllImport("MGWServicios.dll", EntryPoint = "fAltaDireccion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fAltaDireccion(ref int aIdDireccion, ref tDireccion astDireccion);

    public async Task<int> CrearDireccionAsync(tDireccion direccion)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            int nuevoId = 0;

            // Sanitizar campos no nulos y truncar según límites del SDK para evitar corrupción de memoria en P/Invoke
            direccion.cCodCatalogo = (direccion.cCodCatalogo ?? "").Trim();
            if (direccion.cCodCatalogo.Length > SdkConstantes.kLongCodigo - 1)
                direccion.cCodCatalogo = direccion.cCodCatalogo.Substring(0, SdkConstantes.kLongCodigo - 1);

            direccion.cNombreCalle = string.IsNullOrWhiteSpace(direccion.cNombreCalle) ? "Conocido" : direccion.cNombreCalle.Trim();
            if (direccion.cNombreCalle.Length > SdkConstantes.kLongDescripcion - 1)
                direccion.cNombreCalle = direccion.cNombreCalle.Substring(0, SdkConstantes.kLongDescripcion - 1);

            direccion.cNumeroExterior = (direccion.cNumeroExterior ?? "").Trim();
            if (direccion.cNumeroExterior.Length > SdkConstantes.kLongNumeroExtInt - 1)
                direccion.cNumeroExterior = direccion.cNumeroExterior.Substring(0, SdkConstantes.kLongNumeroExtInt - 1);

            direccion.cNumeroInterior = (direccion.cNumeroInterior ?? "").Trim();
            if (direccion.cNumeroInterior.Length > SdkConstantes.kLongNumeroExtInt - 1)
                direccion.cNumeroInterior = direccion.cNumeroInterior.Substring(0, SdkConstantes.kLongNumeroExtInt - 1);

            direccion.cColonia = (direccion.cColonia ?? "").Trim();
            if (direccion.cColonia.Length > SdkConstantes.kLongDescripcion - 1)
                direccion.cColonia = direccion.cColonia.Substring(0, SdkConstantes.kLongDescripcion - 1);

            direccion.cCodigoPostal = (direccion.cCodigoPostal ?? "").Trim();
            if (direccion.cCodigoPostal.Length > SdkConstantes.kLongCodigoPostal - 1)
                direccion.cCodigoPostal = direccion.cCodigoPostal.Substring(0, SdkConstantes.kLongCodigoPostal - 1);

            direccion.cTelefono1 = (direccion.cTelefono1 ?? "").Trim();
            if (direccion.cTelefono1.Length > SdkConstantes.kLongTelefono - 1)
                direccion.cTelefono1 = direccion.cTelefono1.Substring(0, SdkConstantes.kLongTelefono - 1);

            direccion.cTelefono2 = (direccion.cTelefono2 ?? "").Trim();
            if (direccion.cTelefono2.Length > SdkConstantes.kLongTelefono - 1)
                direccion.cTelefono2 = direccion.cTelefono2.Substring(0, SdkConstantes.kLongTelefono - 1);

            direccion.cTelefono3 = (direccion.cTelefono3 ?? "").Trim();
            if (direccion.cTelefono3.Length > SdkConstantes.kLongTelefono - 1)
                direccion.cTelefono3 = direccion.cTelefono3.Substring(0, SdkConstantes.kLongTelefono - 1);

            direccion.cTelefono4 = (direccion.cTelefono4 ?? "").Trim();
            if (direccion.cTelefono4.Length > SdkConstantes.kLongTelefono - 1)
                direccion.cTelefono4 = direccion.cTelefono4.Substring(0, SdkConstantes.kLongTelefono - 1);

            direccion.cEmail = (direccion.cEmail ?? "").Trim();
            if (direccion.cEmail.Length > SdkConstantes.kLongEmailWeb - 1)
                direccion.cEmail = direccion.cEmail.Substring(0, SdkConstantes.kLongEmailWeb - 1);

            direccion.cDireccionWeb = (direccion.cDireccionWeb ?? "").Trim();
            if (direccion.cDireccionWeb.Length > SdkConstantes.kLongEmailWeb - 1)
                direccion.cDireccionWeb = direccion.cDireccionWeb.Substring(0, SdkConstantes.kLongEmailWeb - 1);

            direccion.cCiudad = (direccion.cCiudad ?? "").Trim();
            if (direccion.cCiudad.Length > SdkConstantes.kLongDescripcion - 1)
                direccion.cCiudad = direccion.cCiudad.Substring(0, SdkConstantes.kLongDescripcion - 1);

            direccion.cEstado = (direccion.cEstado ?? "").Trim();
            if (direccion.cEstado.Length > SdkConstantes.kLongDescripcion - 1)
                direccion.cEstado = direccion.cEstado.Substring(0, SdkConstantes.kLongDescripcion - 1);

            direccion.cPais = string.IsNullOrWhiteSpace(direccion.cPais) ? "México" : direccion.cPais.Trim();
            if (direccion.cPais.Length > SdkConstantes.kLongDescripcion - 1)
                direccion.cPais = direccion.cPais.Substring(0, SdkConstantes.kLongDescripcion - 1);

            direccion.cTextoExtra = (direccion.cTextoExtra ?? "").Trim();
            if (direccion.cTextoExtra.Length > SdkConstantes.kLongTextoExtra - 1)
                direccion.cTextoExtra = direccion.cTextoExtra.Substring(0, SdkConstantes.kLongTextoExtra - 1);

            var result = fAltaDireccion(ref nuevoId, ref direccion);
            LanzarExcepcionErrorSDK(result, "Error al crear dirección en SDK.");
            return nuevoId;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    [DllImport("MGWServicios.dll", EntryPoint = "fAltaUnidad", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fAltaUnidad(ref int aIdUnidad, ref tUnidad astUnidad);

    public async Task<int> CrearUnidadMedidaAsync(tUnidad unidad)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            int nuevoId = 0;
            var result = fAltaUnidad(ref nuevoId, ref unidad);
            LanzarExcepcionErrorSDK(result, "Error al crear unidad de medida en SDK.");
            return nuevoId;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    [DllImport("MGWServicios.dll", EntryPoint = "fEmitirDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEmitirDocumento(string aCodConcepto, string aSerie, double aFolio, string aPassword, string aArchivoAdicional);

    public async Task<int> EmitirDocumentoAsync(string codigoConcepto, string serie, double folio, string pass, string email)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            // Indispensable: las DLLs de firmado (SelloDigital.dll) se cargan dinámicamente
            // y requieren que el CurrentDirectory sea la carpeta de binarios de CONTPAQi.
            AsegurarDirectorioBinarios();

            // 1. Asegurar que el "cursor" del SDK esté en el documento correcto. 
            // Muchos problemas de 'External component' ocurren porque el SDK pierde el foco interno.
            var searchResult = Sdk_fBuscarDocumento(codigoConcepto ?? "", serie ?? "", folio.ToString("0"));
            if (searchResult != 0)
            {
                 LanzarExcepcionErrorSDK(searchResult, $"No se pudo localizar el documento para timbrar (Concepto: {codigoConcepto}, Serie: {serie}, Folio: {folio})");
            }

            // 2. Intentar la emisión.
            var result = Sdk_fEmitirDocumento(
                codigoConcepto ?? "", 
                serie ?? "", 
                folio, 
                pass ?? "", 
                email ?? "");

            LanzarExcepcionErrorSDK(result, "Error al timbrar/emitir documento en SDK.");
            return 1;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    public async Task<Dictionary<string, string>> EmitirDocumentoYLeerDatosAsync(string codigoConcepto, string serie, double folio, string pass, string email, IEnumerable<string> campos)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            AsegurarDirectorioBinarios();

            // 1. Asegurar que el "cursor" del SDK esté en el documento correcto. 
            var searchResult = Sdk_fBuscarDocumento(codigoConcepto ?? "", serie ?? "", folio.ToString("0"));
            if (searchResult != 0)
            {
                 LanzarExcepcionErrorSDK(searchResult, $"No se pudo localizar el documento para timbrar (Concepto: {codigoConcepto}, Serie: {serie}, Folio: {folio})");
            }

            // 2. Intentar la emisión.
            var result = Sdk_fEmitirDocumento(
                codigoConcepto ?? "", 
                serie ?? "", 
                folio, 
                pass ?? "", 
                email ?? "");

            LanzarExcepcionErrorSDK(result, "Error al timbrar/emitir documento en SDK.");

            // 3. Leer los campos fiscales y otros solicitados inmediatamente bajo el mismo semáforo
            var resultados = new Dictionary<string, string>();
            foreach (var campo in campos)
            {
                if (string.IsNullOrWhiteSpace(campo)) continue;
                try
                {
                    StringBuilder sb = new StringBuilder(5000);
                    int readResult = Sdk_fLeeDatoDocumento(campo, sb, 5000);
                    if (readResult == 0)
                    {
                        resultados[campo] = sb.ToString();
                    }
                    else
                    {
                        resultados[campo] = string.Empty;
                    }
                }
                catch
                {
                    resultados[campo] = string.Empty;
                }
            }

            return resultados;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    [DllImport("MGWServicios.dll", EntryPoint = "fCancelaDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fCancelaDocumento(string aCodConcepto, string aSerie, double aFolio, string aPassword);

    public async Task<int> CancelarDocumentoAsync(string codigoConcepto, string serie, double folio, string pass)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            var result = fCancelaDocumento(codigoConcepto, serie, folio, pass);
            LanzarExcepcionErrorSDK(result, "Error al cancelar documento en SDK.");
            return 1;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    [DllImport("MGWServicios.dll", EntryPoint = "fSaldarDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSaldarDocumento(string aCodConcepto_Pagar, string aSerie_Pagar, double aFolio_Pagar, string aCodConcepto_Pago, string aSerie_Pago, double aFolio_Pago);

    public async Task<int> SaldarDocumentoAsync(string codigoConceptoDoc, string serieDoc, double folioDoc, string codigoConceptoPago, string seriePago, double folioPago)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            var result = fSaldarDocumento(codigoConceptoDoc, serieDoc, folioDoc, codigoConceptoPago, seriePago, folioPago);
            LanzarExcepcionErrorSDK(result, "Error al saldar documento en SDK.");
            return 1;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    [DllImport("MGWServicios.dll", EntryPoint = "fAgregaRelacionCFDI2", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fAgregaRelacionCFDI2(string aUUID, string aTipoRelacion);

    [DllImport("MGWServicios.dll", EntryPoint = "fAgregaRelacionCFDI", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fAgregaRelacionCFDI(string aSelloDigital, string aUUIDRelacionado, string aTipoRelacion);

    public async Task<int> AgregarRelacionCfdiAsync(string uuid, string tipoRelacion)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            int result;
            try 
            {
                result = fAgregaRelacionCFDI2(uuid, tipoRelacion);
            }
            catch (EntryPointNotFoundException)
            {
                result = fAgregaRelacionCFDI("", uuid, tipoRelacion); 
            }
            
            LanzarExcepcionErrorSDK(result, "Error al agregar relacion CFDI.");
            return 1;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    // =========================================================================================
    // IMPLEMENTACIONES FALTANTES: Creación y Edición vía fInserta / fBusca / fEdita / fGuarda
    // =========================================================================================

    private async Task<int> EjecutarOperacionDictionaryAsync(Func<int> fnBuscaOInserta, Func<int>? fnEdita, Func<string, string, int> fnSetDato, Func<int> fnGuarda, Dictionary<string, string> datos, string entidad, Action? fnCancela = null)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            var result = fnBuscaOInserta();
            if (result != 0)
            {
                StringBuilder sb = new StringBuilder(512);
                fError(result, sb, 512);
                throw new Exception($"Error al {(fnEdita == null ? "insertar" : "buscar")} {entidad}. Código error: {result} - {sb.ToString()}");
            }

            if (fnEdita != null)
            {
                result = fnEdita();
                LanzarExcepcionErrorSDK(result, $"Error al iniciar edición de {entidad}. Código error:");
            }

            try
            {
                foreach (var dato in datos)
                {
                    result = fnSetDato(dato.Key, dato.Value ?? "");
                    LanzarExcepcionErrorSDK(result, $"Error al setear campo '{dato.Key}' en {entidad}. Código error:");
                }

                result = fnGuarda();
                LanzarExcepcionErrorSDK(result, "Error al guardar {entidad}.");
            }
            finally
            {
                // Unconditionally cancel the modification state to release the SQL lock and free memory
                if (fnEdita != null && fnCancela != null)
                {
                    fnCancela(); // Liberar el recurso en CONTPAQi
                }

                if (entidad == "Documento")
                {
                     fDesbloqueaDocumento();
                }
            }

            return 1;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    // --- AGENTES ---
    [DllImport("MGWServicios.dll", EntryPoint = "fBuscaAgente", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fBuscaAgente(string aCodigoAgente);
    [DllImport("MGWServicios.dll", EntryPoint = "fInsertaAgente", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fInsertaAgente();
    [DllImport("MGWServicios.dll", EntryPoint = "fEditaAgente", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEditaAgente();
    [DllImport("MGWServicios.dll", EntryPoint = "fSetDatoAgente", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSetDatoAgente(string aCampo, string aValor);
    [DllImport("MGWServicios.dll", EntryPoint = "fGuardaAgente", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fGuardaAgente();
    [DllImport("MGWServicios.dll", EntryPoint = "fCancelarModificacionAgente", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fCancelarModificacionAgente();

    public Task<int> CrearAgenteAsync(Dictionary<string, string> datos) =>
        EjecutarOperacionDictionaryAsync(fInsertaAgente, null, fSetDatoAgente, fGuardaAgente, datos, "Agente");

    public Task<int> ActualizarAgenteAsync(string codigo, Dictionary<string, string> datos) =>
        EjecutarOperacionDictionaryAsync(() => fBuscaAgente(codigo), fEditaAgente, fSetDatoAgente, fGuardaAgente, datos, "Agente", () => fCancelarModificacionAgente());

    // --- ALMACENES ---
    [DllImport("MGWServicios.dll", EntryPoint = "fBuscaAlmacen", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fBuscaAlmacen(string aCodigoAlmacen);
    [DllImport("MGWServicios.dll", EntryPoint = "fInsertaAlmacen", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fInsertaAlmacen();
    [DllImport("MGWServicios.dll", EntryPoint = "fEditaAlmacen", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEditaAlmacen();
    [DllImport("MGWServicios.dll", EntryPoint = "fSetDatoAlmacen", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSetDatoAlmacen(string aCampo, string aValor);
    [DllImport("MGWServicios.dll", EntryPoint = "fGuardaAlmacen", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fGuardaAlmacen();
    [DllImport("MGWServicios.dll", EntryPoint = "fCancelarModificacionAlmacen", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fCancelarModificacionAlmacen();

    public Task<int> CrearAlmacenAsync(Dictionary<string, string> datos) =>
        EjecutarOperacionDictionaryAsync(fInsertaAlmacen, null, fSetDatoAlmacen, fGuardaAlmacen, datos, "Almacen");

    public Task<int> ActualizarAlmacenAsync(string codigo, Dictionary<string, string> datos) =>
        EjecutarOperacionDictionaryAsync(() => fBuscaAlmacen(codigo), fEditaAlmacen, fSetDatoAlmacen, fGuardaAlmacen, datos, "Almacen", () => fCancelarModificacionAlmacen());

    [DllImport("MGWServicios.dll", EntryPoint = "fBuscaConceptoDocto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fBuscaConceptoDocto(string aCodigoConcepto);
    [DllImport("MGWServicios.dll", EntryPoint = "fEditaConceptoDocto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEditaConceptoDocto();
    [DllImport("MGWServicios.dll", EntryPoint = "fSetDatoConceptoDocto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSetDatoConceptoDocto(string aCampo, string aValor);
    [DllImport("MGWServicios.dll", EntryPoint = "fGuardaConceptoDocto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fGuardaConceptoDocto();

    public Task<int> CrearConceptoAsync(Dictionary<string, string> datos)
    {
        // La librería MGWServicios.dll no exporta ninguna función para insertar conceptos
        // (no existe fInsertaConceptoDocto, fAltaConceptoDocto, etc.).
        throw new NotSupportedException("El SDK de CONTPAQi no soporta la creación de Conceptos de Documento. MGWServicios.dll no cuenta con una función fInserta... para Conceptos. Solo se soporta la lectura y edición.");
    }

    public Task<int> ActualizarConceptoAsync(string codigo, Dictionary<string, string> datos) =>
        EjecutarOperacionDictionaryAsync(() => fBuscaConceptoDocto(codigo), fEditaConceptoDocto, fSetDatoConceptoDocto, fGuardaConceptoDocto, datos, "Concepto");

    // --- DOCUMENTOS ---
    [DllImport("MGWServicios.dll", EntryPoint = "fBuscarDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fBuscarDocumento(string aCodConcepto, string aSerie, string aFolio);
    [DllImport("MGWServicios.dll", EntryPoint = "fEditarDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEditarDocumento();
    [DllImport("MGWServicios.dll", EntryPoint = "fSetDatoDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSetDatoDocumento(string aCampo, string aValor);
    [DllImport("MGWServicios.dll", EntryPoint = "fGuardaDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fGuardaDocumento();
    [DllImport("MGWServicios.dll", EntryPoint = "fCancelarModificacionDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fCancelarModificacionDocumento();
    [DllImport("MGWServicios.dll", EntryPoint = "fDesbloqueaDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fDesbloqueaDocumento();

    [DllImport("MGWServicios.dll", EntryPoint = "fLeeDatoDocumento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fLeeDatoDocumento(string aCampo, StringBuilder aValor, int aLongitud);

    [DllImport("MGWServicios.dll", EntryPoint = "fLeeDatoConceptoDocto", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fLeeDatoConceptoDocto(string aCampo, StringBuilder aValor, int aLongitud);

    public async Task<string> LeerDatoDocumentoAsync(string campo)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            StringBuilder sb = new StringBuilder(5000); // Algunos campos como CadenaOriginal son largos
            int result = Sdk_fLeeDatoDocumento(campo ?? "", sb, 5000);
            LanzarExcepcionErrorSDK(result, $"Error al leer campo {campo} del documento");
            return sb.ToString();
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    public Task<int> ActualizarDocumentoAsync(string codigoConcepto, string serie, string folio, Dictionary<string, string> datos) =>
        EjecutarOperacionDictionaryAsync(() => fBuscarDocumento(codigoConcepto, serie, folio), fEditarDocumento, fSetDatoDocumento, fGuardaDocumento, datos, "Documento", () => fCancelarModificacionDocumento());

    public Task<int> ActualizarDocumentoPorIdAsync(int idDocumento, Dictionary<string, string> datos) =>
        EjecutarOperacionDictionaryAsync(() => fBuscarIdDocumento(idDocumento), fEditarDocumento, fSetDatoDocumento, fGuardaDocumento, datos, "Documento", () => fCancelarModificacionDocumento());

    // --- DIRECCIONES/DOMICILIOS ---
    [DllImport("MGWServicios.dll", EntryPoint = "fBuscaIdDireccion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fBuscaIdDireccion(int aIdDireccion);
    [DllImport("MGWServicios.dll", EntryPoint = "fEditaDireccion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEditaDireccion();
    [DllImport("MGWServicios.dll", EntryPoint = "fSetDatoDireccion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSetDatoDireccion(string aCampo, string aValor);
    [DllImport("MGWServicios.dll", EntryPoint = "fGuardaDireccion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fGuardaDireccion();
    [DllImport("MGWServicios.dll", EntryPoint = "fCancelarModificacionDireccion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fCancelarModificacionDireccion();

    public Task<int> ActualizarDireccionAsync(int idDireccion, Dictionary<string, string> datos) =>
        EjecutarOperacionDictionaryAsync(() => fBuscaIdDireccion(idDireccion), fEditaDireccion, fSetDatoDireccion, fGuardaDireccion, datos, "Direccion", () => fCancelarModificacionDireccion());

    // --- MONEDAS ---
    [DllImport("MGWServicios.dll", EntryPoint = "fBuscaIdMoneda", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fBuscaIdMoneda(int aIdMoneda);
    [DllImport("MGWServicios.dll", EntryPoint = "fInsertaMoneda", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fInsertaMoneda();
    [DllImport("MGWServicios.dll", EntryPoint = "fEditaMoneda", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEditaMoneda();
    [DllImport("MGWServicios.dll", EntryPoint = "fSetDatoMoneda", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSetDatoMoneda(string aCampo, string aValor);
    [DllImport("MGWServicios.dll", EntryPoint = "fGuardaMoneda", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fGuardaMoneda();

    public Task<int> CrearMonedaAsync(Dictionary<string, string> datos) =>
        EjecutarOperacionDictionaryAsync(fInsertaMoneda, null, fSetDatoMoneda, fGuardaMoneda, datos, "Moneda");

    public Task<int> ActualizarMonedaAsync(int idMoneda, Dictionary<string, string> datos) =>
        EjecutarOperacionDictionaryAsync(() => fBuscaIdMoneda(idMoneda), fEditaMoneda, fSetDatoMoneda, fGuardaMoneda, datos, "Moneda");

    // --- MOVIMIENTOS (PARTIDAS) ---
    [DllImport("MGWServicios.dll", EntryPoint = "fBuscarIdMovimiento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fBuscarIdMovimiento(int aIdMovimiento);
    [DllImport("MGWServicios.dll", EntryPoint = "fEditarMovimiento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEditarMovimiento();
    [DllImport("MGWServicios.dll", EntryPoint = "fSetDatoMovimiento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSetDatoMovimiento(string aCampo, string aValor);
    [DllImport("MGWServicios.dll", EntryPoint = "fGuardaMovimiento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fGuardaMovimiento();
    [DllImport("MGWServicios.dll", EntryPoint = "fCancelaCambiosMovimiento", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fCancelaCambiosMovimiento();

    public async Task<int> ActualizarMovimientoAsync(int idDocumento, int idMovimiento, Dictionary<string, string> datos)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            string? idProductoStr = null;
            if (datos.TryGetValue("PRODUCTO", out string? originalProductCode))
            {
                // Buscar el ID exacto antes de enfocar documento para evitar 0xC0000005
                if (fBuscaProducto(originalProductCode) == 0)
                {
                    StringBuilder sbId = new StringBuilder(50);
                    if (fLeeDatoProducto("CIDPRODUCTO", sbId, 50) == 0)
                    {
                        idProductoStr = sbId.ToString();
                        datos["CIDPRODUCTO"] = idProductoStr;
                    }
                }
                // Remover el key PRODUCTO para no sobreescribir la resolución nativa
                datos.Remove("PRODUCTO");
            }

            var resultDoc = fBuscarIdDocumento(idDocumento);
            if (resultDoc != 0) throw new Exception($"Documento padre con ID {idDocumento} no encontrado en SDK. Código: {resultDoc}");

            resultDoc = fEditarDocumento();
            if (resultDoc != 0) throw new Exception($"No se pudo iniciar la edición del Documento {idDocumento}. Código: {resultDoc}");

            var resultMov = fBuscarIdMovimiento(idMovimiento);
            if (resultMov != 0)
            {
                fCancelarModificacionDocumento();
                fDesbloqueaDocumento();
                throw new Exception($"Movimiento con ID {idMovimiento} no encontrado. Código SDK: {resultMov}");
            }

            resultMov = fEditarMovimiento();
            if (resultMov != 0)
            {
                fCancelarModificacionDocumento();
                fDesbloqueaDocumento();
                throw new Exception($"No se pudo iniciar la edición del Movimiento {idMovimiento}. Código SDK: {resultMov}");
            }

            foreach (var kvp in datos)
            {
                fSetDatoMovimiento(kvp.Key, kvp.Value);
            }

            resultMov = fGuardaMovimiento();
            if (resultMov != 0)
            {
                fCancelaCambiosMovimiento();
                fCancelarModificacionDocumento();
                fDesbloqueaDocumento();
                StringBuilder sb = new StringBuilder(512);
                fError(resultMov, sb, 512);
                throw new Exception($"Error al guardar el movimiento. Código SDK: {resultMov} - {sb.ToString()}");
            }

            resultDoc = fGuardaDocumento();
            if (resultDoc != 0)
            {
                fDesbloqueaDocumento();
                StringBuilder sb = new StringBuilder(512);
                fError(resultDoc, sb, 512);
                throw new Exception($"Movimiento modificado, pero error al actualizar totales del documento. SDK: {resultDoc} - {sb.ToString()}");
            }

            fDesbloqueaDocumento();
            return idMovimiento;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    // --- UNIDADES MEDIDA ---
    [DllImport("MGWServicios.dll", EntryPoint = "fBuscaUnidad", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fBuscaUnidad(string aNombreUnidad);
    [DllImport("MGWServicios.dll", EntryPoint = "fEditaUnidad", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEditaUnidad();
    [DllImport("MGWServicios.dll", EntryPoint = "fSetDatoUnidad", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSetDatoUnidad(string aCampo, string aValor);
    [DllImport("MGWServicios.dll", EntryPoint = "fGuardaUnidad", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fGuardaUnidad();
    [DllImport("MGWServicios.dll", EntryPoint = "fCancelarModificacionUnidad", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fCancelarModificacionUnidad();

    public Task<int> ActualizarUnidadMedidaAsync(string nombreUnidad, Dictionary<string, string> datos) =>
        EjecutarOperacionDictionaryAsync(() => fBuscaUnidad(nombreUnidad), fEditaUnidad, fSetDatoUnidad, fGuardaUnidad, datos, "Unidad", () => fCancelarModificacionUnidad());
    #region Auxiliares
    private void LanzarExcepcionErrorSDK(int codigoError, string contexto)
    {
        if (codigoError == 0) return;

        StringBuilder sb = new StringBuilder(512);
        Sdk_fError(codigoError, sb, 512);
        var mensaje = sb.ToString();
        
        // Agregar traducciones comunes si no hay mensaje
        if (string.IsNullOrWhiteSpace(mensaje))
        {
            mensaje = codigoError switch
            {
                1 => "La empresa no está abierta o no se ha iniciado el SDK.",
                2 => "La empresa no existe o la ruta es incorrecta.",
                3 => "El SDK ya ha sido inicializado en otra instancia.",
                10 => "Error en los parámetros de la función.",
                11 => "Error fatal del SDK de CONTPAQi.",
                _ => "Mensaje de error no disponible"
            };
        }

        throw new Exception($"{contexto}. Error SDK: {codigoError} - {mensaje}");
    }
    #endregion
}
