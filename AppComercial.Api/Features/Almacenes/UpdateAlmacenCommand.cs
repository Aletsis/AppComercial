using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.Almacenes;

/// <summary>
/// Comando para actualizar los datos de un Almacén existente en CONTPAQi Comercial.
/// El código del almacén se proporciona en la URL (ruta) del endpoint PUT.
/// Al menos uno de los campos opcionales debe ser enviado.
/// </summary>
public class UpdateAlmacenCommand : IRequest<int>
{
    /// <summary>
    /// Código del almacén a actualizar. Se asigna automáticamente desde la URL.
    /// No es necesario incluirlo en el cuerpo del request.
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nuevo nombre descriptivo del almacén. Máximo 60 caracteres. Opcional.
    /// Si se proporciona, reemplaza el nombre actual.
    /// Ejemplo: "Almacén Norte"
    /// </summary>
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>
    /// Nuevo segmento contable del almacén. Máximo 60 caracteres. Opcional.
    /// </summary>
    [StringLength(60, ErrorMessage = "El segmento contable no puede exceder 60 caracteres.")]
    public string? SegmentoContable { get; set; }

    /// <summary>
    /// Indica si el almacén tiene un domicilio asociado. Opcional.
    /// Valores válidos: 0 = Sin domicilio, 1 = Con domicilio.
    /// </summary>
    [Range(0, 1, ErrorMessage = "BanDomicilio debe ser 0 (Sin domicilio) o 1 (Con domicilio).")]
    public int? BanDomicilio { get; set; }

    /// <summary>
    /// Texto extra 1 del almacén. Opcional. Máximo 50 caracteres.
    /// </summary>
    [StringLength(50, ErrorMessage = "TextoExtra1 no puede exceder 50 caracteres.")]
    public string? TextoExtra1 { get; set; }

    /// <summary>
    /// Texto extra 2 del almacén. Opcional. Máximo 50 caracteres.
    /// </summary>
    [StringLength(50, ErrorMessage = "TextoExtra2 no puede exceder 50 caracteres.")]
    public string? TextoExtra2 { get; set; }

    /// <summary>
    /// Texto extra 3 del almacén. Opcional. Máximo 50 caracteres.
    /// </summary>
    [StringLength(50, ErrorMessage = "TextoExtra3 no puede exceder 50 caracteres.")]
    public string? TextoExtra3 { get; set; }
}

public class UpdateAlmacenCommandHandler : IRequestHandler<UpdateAlmacenCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateAlmacenCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateAlmacenCommand request, CancellationToken cancellationToken)
    {
        // Construimos el diccionario solo con los campos que se proporcionaron
        var datos = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.Nombre))
            datos["CNOMBREALMACEN"] = request.Nombre;

        if (!string.IsNullOrWhiteSpace(request.SegmentoContable))
            datos["CSEGCONTALMACEN"] = request.SegmentoContable;

        if (request.BanDomicilio.HasValue)
            datos["CBANDOMICILIO"] = request.BanDomicilio.Value.ToString();

        if (!string.IsNullOrWhiteSpace(request.TextoExtra1))
            datos["CTEXTOEXTRA1"] = request.TextoExtra1;

        if (!string.IsNullOrWhiteSpace(request.TextoExtra2))
            datos["CTEXTOEXTRA2"] = request.TextoExtra2;

        if (!string.IsNullOrWhiteSpace(request.TextoExtra3))
            datos["CTEXTOEXTRA3"] = request.TextoExtra3;

        if (datos.Count == 0)
            throw new ArgumentException("Se debe proporcionar al menos un campo para actualizar: Nombre, SegmentoContable, BanDomicilio, TextoExtra1, TextoExtra2 o TextoExtra3.");

        return await _sdk.ActualizarAlmacenAsync(request.Codigo, datos);
    }
}
