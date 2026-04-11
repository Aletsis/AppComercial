using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Application.Features.Almacenes;

/// <summary>
/// Comando para dar de alta un nuevo Almacén en CONTPAQi Comercial.
/// </summary>
public class CreateAlmacenCommand : IRequest<int>
{
    /// <summary>
    /// Código único del almacén. Máximo 30 caracteres. Requerido.
    /// Ejemplo: "ALM01"
    /// </summary>
    [Required(ErrorMessage = "El código del almacén es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código debe tener entre 1 y 30 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo del almacén. Máximo 60 caracteres. Requerido.
    /// Ejemplo: "Almacén Central"
    /// </summary>
    [Required(ErrorMessage = "El nombre del almacén es requerido.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Segmento contable asociado al almacén. Opcional.
    /// Ejemplo: "1-1100-0000-0000"
    /// </summary>
    [StringLength(60, ErrorMessage = "El segmento contable no puede exceder 60 caracteres.")]
    public string? SegmentoContable { get; set; }

    /// <summary>
    /// Indica si el almacén tiene un domicilio asociado. Por defecto: 0 (Sin domicilio).
    /// Valores válidos: 0 = Sin domicilio, 1 = Con domicilio.
    /// </summary>
    [Range(0, 1, ErrorMessage = "BanDomicilio debe ser 0 (Sin domicilio) o 1 (Con domicilio).")]
    public int BanDomicilio { get; set; } = 0;

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

public class CreateAlmacenCommandHandler : IRequestHandler<CreateAlmacenCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateAlmacenCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateAlmacenCommand request, CancellationToken cancellationToken)
    {
        // Construimos el diccionario de campos SDK a partir de las propiedades tipadas
        var datos = new Dictionary<string, string>
        {
            ["CCODIGOALMACEN"] = request.Codigo,
            ["CNOMBREALMACEN"] = request.Nombre,
            ["CBANDOMICILIO"]  = request.BanDomicilio.ToString()
        };

        if (!string.IsNullOrWhiteSpace(request.SegmentoContable))
            datos["CSEGCONTALMACEN"] = request.SegmentoContable;

        if (!string.IsNullOrWhiteSpace(request.TextoExtra1))
            datos["CTEXTOEXTRA1"] = request.TextoExtra1;

        if (!string.IsNullOrWhiteSpace(request.TextoExtra2))
            datos["CTEXTOEXTRA2"] = request.TextoExtra2;

        if (!string.IsNullOrWhiteSpace(request.TextoExtra3))
            datos["CTEXTOEXTRA3"] = request.TextoExtra3;

        return await _sdk.CrearAlmacenAsync(datos);
    }
}
