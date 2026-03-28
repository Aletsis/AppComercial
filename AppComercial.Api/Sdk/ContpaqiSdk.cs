using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Api.Sdk;

public interface IContpaqiSdk
{
    Task IniciarSesionAsync(string nombreUsuario = "SUPERVISOR", string contrasena = "");
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

public class ContpaqiSdk : IContpaqiSdk
{
    // Semaphore to enforce single-threaded access to the native COM SDK
    private static readonly SemaphoreSlim _sdkSemaphore = new SemaphoreSlim(1, 1);

    // Carga de MGWServicios.dll que es la libreria core del SDK de Comercial
    // CallingConvention.StdCall es vital para evitar desbordamientos de pila en DLLs de C/C++
    [DllImport("MGWServicios.dll", EntryPoint = "fSetNombrePAQ", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSetNombrePAQ(string aNombrePAQ);

    [DllImport("MGWServicios.dll", EntryPoint = "fInicioSesionSDK", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void fInicioSesionSDK(string aUsuario, string aContrasenia);

    [DllImport("MGWServicios.dll", EntryPoint = "fTerminaSDK", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void fTerminaSDK();

    [DllImport("MGWServicios.dll", EntryPoint = "fAbreEmpresa", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fAbreEmpresa(string aDirectorioEmpresa);

    [DllImport("MGWServicios.dll", EntryPoint = "fCierraEmpresa", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void fCierraEmpresa();

    private void SetDirectorioBinariosSDK()
    {
        string? directorioBase = null;

        try
        {
            // Leer la ruta de instalación de CONTPAQi Comercial desde el registro de Windows (Vista 32 bits porque es x86)
            using RegistryKey? key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                .OpenSubKey(@"SOFTWARE\Computación en Acción, SA CV\CONTPAQ I COMERCIAL", false);

            if (key != null)
            {
                directorioBase = key.GetValue("DirectorioBase")?.ToString();
            }
        }
        catch 
        {
            // Ignorar errores de acceso al registro y usar fallback
        }

        if (string.IsNullOrEmpty(directorioBase))
        {
            // Fallback común si no se encontró en el registro
            directorioBase = @"C:\Program Files (x86)\Compac\Comercial";
        }

        if (Directory.Exists(directorioBase))
        {
            Directory.SetCurrentDirectory(directorioBase);
        }
        else
        {
            throw new DirectoryNotFoundException($"El directorio de binarios de Comercial '{directorioBase}' no existe.");
        }
    }

    public async Task IniciarSesionAsync(string nombreUsuario = "SUPERVISOR", string contrasena = "")
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            // 1. ANTES de interactuar con el SDK, el Working Directory debe ser la carpeta de instalación del sistema.
            SetDirectorioBinariosSDK();

            // En comercial usamos fInicioSesionSDK si hay credenciales, y luego fSetNombrePAQ
            fInicioSesionSDK(nombreUsuario, contrasena);
            
            var result = fSetNombrePAQ("CONTPAQ I COMERCIAL");
            if (result != 0)
            {
                throw new Exception($"Error al iniciar el SDK nativo de Comercial. Código de error: {result}");
            }
        }
        finally
        {
            _sdkSemaphore.Release();
        }
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
            SetDirectorioBinariosSDK();

            // Intentar sesión con las credenciales del usuario
            fInicioSesionSDK(usuario, contrasena);
            int result = fSetNombrePAQ("CONTPAQ I COMERCIAL");
            bool credencialesValidas = (result == 0);

            if (credencialesValidas)
            {
                // Abrir empresa para dejar el SDK en estado consistente
                fAbreEmpresa(rutaEmpresa);
            }

            // SIEMPRE restaurar la sesión de SUPERVISOR para no interrumpir otras operaciones
            fInicioSesionSDK("SUPERVISOR", "");
            fSetNombrePAQ("CONTPAQ I COMERCIAL");
            fAbreEmpresa(rutaEmpresa);

            return credencialesValidas;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    public async Task TerminarSesionAsync()
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            fTerminaSDK();
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    public async Task AbrirEmpresaAsync(string rutaEmpresa)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            var result = fAbreEmpresa(rutaEmpresa);
            if (result != 0)
            {
                throw new Exception($"Error al abrir la empresa en SDK. Código de error: {result}");
            }
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    public async Task CerrarEmpresaAsync()
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            fCierraEmpresa();
        }
        finally
        {
            _sdkSemaphore.Release();
        }
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

            // Asignamos valores default requeridos por el SDK interno si vienen vacíos
            cliente.cFechaAlta = string.IsNullOrEmpty(cliente.cFechaAlta) ? DateTime.Now.ToString("MM/dd/yyyy") : cliente.cFechaAlta;
            if (string.IsNullOrEmpty(cliente.cNombreMoneda)) cliente.cNombreMoneda = "Peso Mexicano";
            if (cliente.cBanVentaCredito == 0) cliente.cBanVentaCredito = 1;
            if (cliente.cEstatus == 0) cliente.cEstatus = 1;

            var result = fAltaCteProv(ref nuevoId, ref cliente);
            if (result != 0)
            {
                throw new Exception($"Error al crear cliente en SDK. Código: {result}");
            }

            return nuevoId;
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

    [DllImport("MGWServicios.dll", EntryPoint = "fBuscaCteProv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fBuscaCteProv(string aCodigo);

    [DllImport("MGWServicios.dll", EntryPoint = "fEditaCteProv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fEditaCteProv();

    [DllImport("MGWServicios.dll", EntryPoint = "fSetDatoCteProv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fSetDatoCteProv(string aCampo, string aValor);

    [DllImport("MGWServicios.dll", EntryPoint = "fGuardaCteProv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern int fGuardaCteProv();

    public async Task<int> ActualizarClienteAsync(string codigo, Dictionary<string, string> datos)
    {
        await _sdkSemaphore.WaitAsync();
        try
        {
            var result = fBuscaCteProv(codigo);
            if (result != 0) throw new Exception($"Cliente no encontrado para actualizar. Código error: {result}");

            result = fEditaCteProv();
            if (result != 0) throw new Exception($"Error al iniciar edición del cliente. Código error: {result}");

            foreach (var dato in datos)
            {
                result = fSetDatoCteProv(dato.Key, dato.Value);
                if (result != 0) throw new Exception($"Error al setear campo {dato.Key}. Código error: {result}");
            }

            result = fGuardaCteProv();
            if (result != 0)
            {
                StringBuilder sb = new StringBuilder(512);
                fError(result, sb, 512);
                throw new Exception($"Error al guardar cambios de cliente/proveedor. Código: {result} - {sb.ToString()}");
            }

            return 1; // Success
        }
        finally
        {
            _sdkSemaphore.Release();
        }
    }

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
            if (result != 0)
            {
                StringBuilder sb = new StringBuilder(512);
                fError(result, sb, 512);
                throw new Exception($"Error SDK {result}: {sb.ToString()}");
            }

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
                if (result != 0)
                {
                    StringBuilder sb = new StringBuilder(512);
                    fError(result, sb, 512);
                    throw new Exception($"Error SDK al actualizar producto {result}: {sb.ToString()}");
                }
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
            string rutaArchivo = Directory.GetFiles(dirArchivoDigital, $"*{sFolio}.pdf")
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
            if (result != 0)
            {
                StringBuilder sb = new StringBuilder(512);
                fError(result, sb, 512);
                throw new Exception($"Error al crear documento en SDK. Código: {result} - {sb.ToString()}");
            }
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
            string idProductoStr = null;

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
            var result = fAltaDireccion(ref nuevoId, ref direccion);
            if (result != 0)
            {
                StringBuilder sb = new StringBuilder(512);
                fError(result, sb, 512);
                throw new Exception($"Error al crear dirección en SDK. Código: {result} - {sb.ToString()}");
            }
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
            if (result != 0)
            {
                StringBuilder sb = new StringBuilder(512);
                fError(result, sb, 512);
                throw new Exception($"Error al crear unidad de medida en SDK. Código: {result} - {sb.ToString()}");
            }
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
            var result = fEmitirDocumento(codigoConcepto, serie, folio, pass, email);
            if (result != 0)
            {
                StringBuilder sb = new StringBuilder(512);
                fError(result, sb, 512);
                throw new Exception($"Error al timbrar/emitir documento en SDK. Código: {result} - {sb.ToString()}");
            }
            return 1;
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
            if (result != 0)
            {
                StringBuilder sb = new StringBuilder(512);
                fError(result, sb, 512);
                throw new Exception($"Error al cancelar documento en SDK. Código: {result} - {sb.ToString()}");
            }
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
            if (result != 0)
            {
                StringBuilder sb = new StringBuilder(512);
                fError(result, sb, 512);
                throw new Exception($"Error al saldar documento en SDK. Código: {result} - {sb.ToString()}");
            }
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
                if (result != 0)
                {
                    StringBuilder sb = new StringBuilder(512);
                    fError(result, sb, 512);
                    throw new Exception($"Error al iniciar edición de {entidad}. Código error: {result} - {sb.ToString()}");
                }
            }

            try
            {
                foreach (var dato in datos)
                {
                    result = fnSetDato(dato.Key, dato.Value);
                    if (result != 0)
                    {
                        StringBuilder sb = new StringBuilder(512);
                        fError(result, sb, 512);
                        throw new Exception($"Error al setear campo '{dato.Key}' en {entidad}. Código error: {result} - {sb.ToString()}");
                    }
                }

                result = fnGuarda();
                if (result != 0)
                {
                    StringBuilder sb = new StringBuilder(512);
                    fError(result, sb, 512);
                    throw new Exception($"Error al guardar {entidad}. Código: {result} - {sb.ToString()}");
                }
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
            string idProductoStr = null;
            if (datos.TryGetValue("PRODUCTO", out string originalProductCode))
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
}
