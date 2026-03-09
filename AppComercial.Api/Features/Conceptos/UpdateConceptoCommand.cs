using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.Conceptos;

/// <summary>
/// Comando para actualizar los datos de un Concepto existente en CONTPAQi Comercial.
/// El código del concepto se proporciona en la URL (ruta) del endpoint PUT.
/// Al menos uno de los campos opcionales debe ser enviado.
/// </summary>
public class UpdateConceptoCommand : IRequest<int>
{
    /// <summary>
    /// Código del concepto a actualizar. Se asigna automáticamente desde la URL.
    /// No es necesario incluirlo en el cuerpo del request.
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nuevo nombre descriptivo del concepto. Máximo 60 caracteres. Opcional.
    /// Si se proporciona, reemplaza el nombre actual.
    /// Ejemplo: "Factura de Venta Contado"
    /// </summary>
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>
    /// Nueva serie por omisión del concepto. Máximo 10 caracteres. Opcional.
    /// </summary>
    [StringLength(10, ErrorMessage = "La serie por omisión no puede exceder 10 caracteres.")]
    public string? SeriePorOmision { get; set; }

    /// <summary>
    /// Nuevo segmento contable del concepto. Máximo 60 caracteres. Opcional.
    /// </summary>
    [StringLength(60, ErrorMessage = "El segmento contable no puede exceder 60 caracteres.")]
    public string? SegmentoContable { get; set; }

    /// <summary>
    /// Indica si el concepto genera CFDI. Opcional.
    /// Valores válidos: 0 = No, 1 = Sí.
    /// </summary>
    [Range(0, 1, ErrorMessage = "EsCFD debe ser 0 (No) o 1 (Sí).")]
    public int? EsCFD { get; set; }
}

public class UpdateConceptoCommandHandler : IRequestHandler<UpdateConceptoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateConceptoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateConceptoCommand request, CancellationToken cancellationToken)
    {
        // Construimos el diccionario solo con los campos que se proporcionaron
        var datos = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.Nombre))
            datos["CNOMBRECONCEPTO"] = request.Nombre;

        if (!string.IsNullOrWhiteSpace(request.SeriePorOmision))
            datos["CSERIEPOROMISION"] = request.SeriePorOmision;

        if (!string.IsNullOrWhiteSpace(request.SegmentoContable))
            datos["CSEGCONTCONCEPTO"] = request.SegmentoContable;

        if (request.EsCFD.HasValue)
            datos["CESCFD"] = request.EsCFD.Value.ToString();

        if (datos.Count == 0)
            throw new ArgumentException("Se debe proporcionar al menos un campo para actualizar: Nombre, SeriePorOmision, SegmentoContable o EsCFD.");

        return await _sdk.ActualizarConceptoAsync(request.Codigo, datos);
    }
}
