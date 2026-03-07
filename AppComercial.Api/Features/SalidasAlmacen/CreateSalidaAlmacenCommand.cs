using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.SalidasAlmacen;

/// <summary>
/// Crea un documento de Salida de Almacén. El código de concepto debe ser uno
/// de naturaleza 5 (Salida de Almacén) configurado en CONTPAQi Comercial.
/// </summary>
public class CreateSalidaAlmacenCommand : IRequest<int>
{
    /// <summary>Código del Concepto de tipo Salida de Almacén (ej. "SA").</summary>
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    /// <summary>Código del almacén origen del que salen los productos.</summary>
    public string CodigoAlmacen { get; set; } = string.Empty;
    /// <summary>Lista de partidas/productos que salen del almacén.</summary>
    public List<SalidaPartida> Partidas { get; set; } = new();
}

public class SalidaPartida
{
    public string CodigoProducto { get; set; } = string.Empty;
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
        // 1. Crear la cabecera del documento
        var documento = new tDocumento
        {
            aCodConcepto     = request.CodigoConcepto,
            aSerie           = request.Serie,
            aReferencia      = request.Referencia,
            aFecha           = DateTime.Now.ToString("MM/dd/yyyy"),
            aCodigoCteProv   = string.Empty,
            aNumMoneda       = 1,
            aTipoCambio      = 1.0
        };

        var idDocumento = await _sdk.CrearDocumentoAsync(documento);

        // 2. Agregar cada partida
        foreach (var partida in request.Partidas)
        {
            var movimiento = new tMovimiento
            {
                aConsecutivo         = 0,
                aCodProdSer          = partida.CodigoProducto,
                aUnidades            = partida.Unidades,
                aCosto               = 0,
                aPrecio              = 0,
                aCodAlmacen          = request.CodigoAlmacen,
                aReferencia          = string.Empty,
                aCodClasificacion    = string.Empty
            };
            await _sdk.CrearMovimientoAsync(idDocumento, movimiento);
        }

        return idDocumento;
    }
}
