using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.SalidasAlmacen;

/// <summary>
/// Crea un documento de Salida de Almacén completo (cabecera + partidas) en CONTPAQi Comercial.
/// El código de concepto debe ser uno configurado con naturaleza Salida de Almacén (CNATURALEZA = 5).
/// </summary>
public class CreateSalidaAlmacenCommand : IRequest<int>
{
    /// <summary>
    /// Código del Concepto de tipo Salida de Almacén configurado en CONTPAQi. Requerido.
    /// Ejemplo: "SA", "SALMAC"
    /// </summary>
    [Required(ErrorMessage = "El código del concepto es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del concepto debe tener entre 1 y 30 caracteres.")]
    public string CodigoConcepto { get; set; } = string.Empty;

    /// <summary>
    /// Serie del documento. Puede ser vacío si el concepto no maneja series.
    /// Ejemplo: "SA"
    /// </summary>
    [StringLength(10, ErrorMessage = "La serie no puede exceder 10 caracteres.")]
    public string Serie { get; set; } = string.Empty;

    /// <summary>
    /// Referencia o motivo de la salida. Opcional. Máximo 30 caracteres.
    /// Ejemplo: "Ajuste por merma", "Transferencia sucursal norte"
    /// </summary>
    [StringLength(30, ErrorMessage = "La referencia no puede exceder 30 caracteres.")]
    public string Referencia { get; set; } = string.Empty;

    /// <summary>
    /// Código del almacén origen del que salen las mercancías. Requerido.
    /// Ejemplo: "ALM01"
    /// </summary>
    [Required(ErrorMessage = "El código del almacén es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del almacén debe tener entre 1 y 30 caracteres.")]
    public string CodigoAlmacen { get; set; } = string.Empty;

    /// <summary>
    /// Lista de productos que salen del almacén. Se requiere al menos una partida.
    /// </summary>
    [MinLength(1, ErrorMessage = "La salida de almacén debe tener al menos una partida.")]
    public List<SalidaPartida> Partidas { get; set; } = new();
}

/// <summary>
/// Representa una línea de producto dentro de un documento de Salida de Almacén.
/// </summary>
public class SalidaPartida
{
    /// <summary>
    /// Código del producto que sale del almacén. Requerido. Máximo 30 caracteres.
    /// Ejemplo: "PROD-001"
    /// </summary>
    [Required(ErrorMessage = "El código del producto es requerido en cada partida.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del producto debe tener entre 1 y 30 caracteres.")]
    public string CodigoProducto { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de unidades que salen. Debe ser mayor a 0.
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Las unidades deben ser mayores a 0.")]
    public double Unidades { get; set; }
}

public class CreateSalidaAlmacenCommandHandler : IRequestHandler<CreateSalidaAlmacenCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateSalidaAlmacenCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateSalidaAlmacenCommand request, CancellationToken cancellationToken)
    {
        // 1. Crear la cabecera del documento de salida
        var documento = new tDocumento
        {
            aCodConcepto   = request.CodigoConcepto,
            aSerie         = request.Serie,
            aReferencia    = request.Referencia,
            aFecha         = DateTime.Now.ToString("MM/dd/yyyy"),
            aCodigoCteProv = string.Empty,
            aNumMoneda     = 1,
            aTipoCambio    = 1.0
        };

        var idDocumento = await _sdk.CrearDocumentoAsync(documento);

        // 2. Agregar cada partida al documento
        foreach (var partida in request.Partidas)
        {
            var movimiento = new tMovimiento
            {
                aConsecutivo      = 0,
                aCodProdSer       = partida.CodigoProducto,
                aUnidades         = partida.Unidades,
                aCosto            = 0,
                aPrecio           = 0,
                aCodAlmacen       = request.CodigoAlmacen,
                aReferencia       = string.Empty,
                aCodClasificacion = string.Empty
            };
            await _sdk.CrearMovimientoAsync(idDocumento, movimiento);
        }

        return idDocumento;
    }
}
