using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Application.Features.Documentos;

/// <summary>
/// Comando para crear un Documento genérico en CONTPAQi Comercial (cabecera únicamente).
/// Para documentos con partidas, utilice los endpoints especializados:
///   POST /api/Facturas       → Facturas de venta (CNATURALEZA = 1)
///   POST /api/Cotizaciones   → Cotizaciones (CNATURALEZA = 1)
///   POST /api/Pedidos        → Pedidos de venta (CNATURALEZA = 1)
///   POST /api/NotasCredito   → Notas de crédito / devoluciones (CNATURALEZA = 3)
///   POST /api/Compras        → Compras a proveedores (CNATURALEZA = 2)
///   POST /api/EntradasAlmacen → Entradas de almacén (CNATURALEZA = 4)
///   POST /api/SalidasAlmacen  → Salidas de almacén (CNATURALEZA = 5)
/// </summary>
public class CreateDocumentoCommand : IRequest<int>
{
    /// <summary>
    /// Código del Concepto de Documento que define el tipo. Requerido. Máximo 30 caracteres.
    /// Ejemplo: "FAC", "COMP", "EA", "SA"
    /// </summary>
    [Required(ErrorMessage = "El código del concepto es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del concepto debe tener entre 1 y 30 caracteres.")]
    public string Concepto { get; set; } = string.Empty;

    /// <summary>
    /// Serie del documento. Puede ser vacío si el concepto no maneja series.
    /// Máximo 10 caracteres. Ejemplo: "A", "FAC"
    /// </summary>
    [StringLength(10, ErrorMessage = "La serie no puede exceder 10 caracteres.")]
    public string Serie { get; set; } = string.Empty;

    /// <summary>
    /// Código del cliente o proveedor asociado al documento. Máximo 30 caracteres.
    /// Ejemplo: "CLI001", "PROV005"
    /// </summary>
    [StringLength(30, ErrorMessage = "El código del cliente/proveedor no puede exceder 30 caracteres.")]
    public string CodigoClienteProveedor { get; set; } = string.Empty;

    /// <summary>
    /// Referencia o número de documento externo. Opcional. Máximo 30 caracteres.
    /// Ejemplo: "OC-2024-001"
    /// </summary>
    [StringLength(30, ErrorMessage = "La referencia no puede exceder 30 caracteres.")]
    public string Referencia { get; set; } = string.Empty;

    /// <summary>
    /// Número de moneda. Por defecto: 1 (Peso Mexicano).
    /// Consultar catálogo de Monedas para otros valores.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El número de moneda debe ser mayor a 0.")]
    public int NumeroMoneda { get; set; } = 1;

    /// <summary>
    /// Tipo de cambio respecto al peso mexicano. Por defecto: 1.0.
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "El tipo de cambio debe ser mayor a 0.")]
    public double TipoCambio { get; set; } = 1.0;
}

public class CreateDocumentoCommandHandler : IRequestHandler<CreateDocumentoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateDocumentoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateDocumentoCommand request, CancellationToken cancellationToken)
    {
        var nuevoDocumento = new tDocumento
        {
            aCodConcepto   = request.Concepto,
            aSerie         = request.Serie,
            aCodigoCteProv = request.CodigoClienteProveedor,
            aReferencia    = request.Referencia,
            aFecha         = DateTime.Now.ToString("MM/dd/yyyy"),
            aNumMoneda     = request.NumeroMoneda,
            aTipoCambio    = request.TipoCambio
        };

        return await _sdk.CrearDocumentoAsync(nuevoDocumento);
    }
}
