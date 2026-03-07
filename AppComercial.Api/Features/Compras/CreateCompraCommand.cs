using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Compras;

/// <summary>
/// Crea un documento de Compra completo con sus partidas. El código de concepto
/// debe ser uno de naturaleza 2 (Compra) configurado en CONTPAQi Comercial.
/// </summary>
public class CreateCompraCommand : IRequest<int>
{
    /// <summary>Código del Concepto de tipo Compra (ej. "COMP").</summary>
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    /// <summary>Código del proveedor al que se le compra.</summary>
    public string CodigoProveedor { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    /// <summary>Código del almacén destino donde entran las mercancías compradas.</summary>
    public string CodigoAlmacen { get; set; } = string.Empty;
    public int NumeroMoneda { get; set; } = 1;
    public double TipoCambio { get; set; } = 1.0;
    /// <summary>Lista de partidas de la compra.</summary>
    public List<CompraPartida> Partidas { get; set; } = new();
}

public class CompraPartida
{
    public string CodigoProducto { get; set; } = string.Empty;
    public double Unidades { get; set; }
    /// <summary>Precio/costo unitario de compra.</summary>
    public double PrecioUnitario { get; set; }
}

public class CreateCompraCommandHandler : IRequestHandler<CreateCompraCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateCompraCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateCompraCommand request, CancellationToken cancellationToken)
    {
        // 1. Crear la cabecera del documento de compra
        var documento = new tDocumento
        {
            aCodConcepto     = request.CodigoConcepto,
            aSerie           = request.Serie,
            aCodigoCteProv   = request.CodigoProveedor,
            aReferencia      = request.Referencia,
            aFecha           = DateTime.Now.ToString("MM/dd/yyyy"),
            aNumMoneda       = request.NumeroMoneda,
            aTipoCambio      = request.TipoCambio
        };

        var idDocumento = await _sdk.CrearDocumentoAsync(documento);

        // 2. Agregar las partidas de compra (movimientos)
        foreach (var partida in request.Partidas)
        {
            var movimiento = new tMovimiento
            {
                aConsecutivo         = 0,
                aCodProdSer          = partida.CodigoProducto,
                aUnidades            = partida.Unidades,
                aPrecio              = partida.PrecioUnitario,
                aCosto               = partida.PrecioUnitario,
                aCodAlmacen          = request.CodigoAlmacen,
                aReferencia          = string.Empty,
                aCodClasificacion    = string.Empty
            };
            await _sdk.CrearMovimientoAsync(idDocumento, movimiento);
        }

        return idDocumento;
    }
}
