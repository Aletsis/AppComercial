using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.EntradasAlmacen;

/// <summary>
/// Crea un documento de Entrada de Almacén. El código de concepto debe ser uno
/// de naturaleza 4 (Entrada de Almacén) configurado en CONTPAQi Comercial.
/// </summary>
public class CreateEntradaAlmacenCommand : IRequest<int>
{
    /// <summary>Código del Concepto de tipo Entrada de Almacén (ej. "EA").</summary>
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    /// <summary>Lista de partidas/productos que entran al almacén.</summary>
    public List<EntradaPartida> Partidas { get; set; } = new();
}

public class EntradaPartida
{
    public string CodigoProducto { get; set; } = string.Empty;
    /// <summary>Código del almacén destino donde entran los productos.</summary>
    public string CodigoAlmacen { get; set; } = string.Empty;
    public double Unidades { get; set; }
    /// <summary>Costo unitario del producto para esta entrada.</summary>
    public double Costo { get; set; }
}

public class CreateEntradaAlmacenCommandHandler : IRequestHandler<CreateEntradaAlmacenCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateEntradaAlmacenCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateEntradaAlmacenCommand request, CancellationToken cancellationToken)
    {
        // 1. Crear la cabecera del documento
        var documento = new tDocumento
        {
            aCodConcepto     = request.CodigoConcepto,
            aSerie           = request.Serie,
            aReferencia      = request.Referencia,
            aFecha           = DateTime.Now.ToString("MM/dd/yyyy"),
            aCodigoCteProv   = string.Empty, // Entradas de almacén suelen no llevar cliente
            aNumMoneda       = 1,
            aTipoCambio      = 1.0
        };

        var idDocumento = await _sdk.CrearDocumentoAsync(documento);

        // 2. Agregar cada partida (movimiento de inventario)
        foreach (var partida in request.Partidas)
        {
            var movimiento = new tMovimiento
            {
                aConsecutivo         = 0,
                aCodProdSer          = partida.CodigoProducto,
                aUnidades            = partida.Unidades,
                aCosto               = partida.Costo,
                aPrecio              = partida.Costo,   // En entradas, precio = costo
                aCodAlmacen          = partida.CodigoAlmacen,
                aReferencia          = string.Empty,
                aCodClasificacion    = string.Empty
            };
            await _sdk.CrearMovimientoAsync(idDocumento, movimiento);
        }

        return idDocumento;
    }
}
