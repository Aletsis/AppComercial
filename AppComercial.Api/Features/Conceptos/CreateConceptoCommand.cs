using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.Conceptos;

/// <summary>
/// Comando para dar de alta un nuevo Concepto de Documento en CONTPAQi Comercial.
/// Los conceptos definen el tipo y comportamiento de los documentos (ventas, compras, etc.).
/// </summary>
public class CreateConceptoCommand : IRequest<int>
{
    /// <summary>
    /// Código único del concepto. Máximo 30 caracteres. Requerido.
    /// Ejemplo: "FAC", "COMP", "NOT"
    /// </summary>
    [Required(ErrorMessage = "El código del concepto es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código debe tener entre 1 y 30 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo del concepto. Máximo 60 caracteres. Requerido.
    /// Ejemplo: "Factura de Venta", "Compra a Proveedor"
    /// </summary>
    [Required(ErrorMessage = "El nombre del concepto es requerido.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Naturaleza del concepto. Requerido.
    /// Valores válidos:
    ///   1 = Venta
    ///   2 = Compra
    ///   3 = Devolución sobre venta
    ///   4 = Devolución sobre compra
    ///   5 = Traslado entre almacenes
    ///   6 = Ajuste de inventario
    /// </summary>
    [Required(ErrorMessage = "La naturaleza del concepto es requerida.")]
    [Range(1, 6, ErrorMessage = "La naturaleza debe ser un valor entre 1 y 6.")]
    public int Naturaleza { get; set; }

    /// <summary>
    /// Serie por omisión del concepto. Opcional. Máximo 10 caracteres.
    /// Ejemplo: "A", "FAC"
    /// </summary>
    [StringLength(10, ErrorMessage = "La serie por omisión no puede exceder 10 caracteres.")]
    public string? SeriePorOmision { get; set; }

    /// <summary>
    /// Segmento contable asociado al concepto. Opcional. Máximo 60 caracteres.
    /// Ejemplo: "1-4010-0000-0000"
    /// </summary>
    [StringLength(60, ErrorMessage = "El segmento contable no puede exceder 60 caracteres.")]
    public string? SegmentoContable { get; set; }

    /// <summary>
    /// Indica si este concepto genera un Comprobante Fiscal Digital (CFDI). Por defecto: 0.
    /// Valores válidos: 0 = No, 1 = Sí.
    /// </summary>
    [Range(0, 1, ErrorMessage = "EsCFD debe ser 0 (No) o 1 (Sí).")]
    public int EsCFD { get; set; } = 0;
}

public class CreateConceptoCommandHandler : IRequestHandler<CreateConceptoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateConceptoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateConceptoCommand request, CancellationToken cancellationToken)
    {
        // Construimos el diccionario de campos SDK a partir de las propiedades tipadas
        var datos = new Dictionary<string, string>
        {
            ["CCODIGOCONCEPTO"] = request.Codigo,
            ["CNOMBRECONCEPTO"] = request.Nombre,
            ["CNATURALEZA"]     = request.Naturaleza.ToString(),
            ["CESCFD"]          = request.EsCFD.ToString()
        };

        if (!string.IsNullOrWhiteSpace(request.SeriePorOmision))
            datos["CSERIEPOROMISION"] = request.SeriePorOmision;

        if (!string.IsNullOrWhiteSpace(request.SegmentoContable))
            datos["CSEGCONTCONCEPTO"] = request.SegmentoContable;

        return await _sdk.CrearConceptoAsync(datos);
    }
}
