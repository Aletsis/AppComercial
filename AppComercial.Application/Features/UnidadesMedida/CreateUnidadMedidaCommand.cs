using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Application.Features.UnidadesMedida;

/// <summary>
/// Comando para dar de alta una nueva Unidad de Medida en CONTPAQi Comercial.
/// </summary>
public class CreateUnidadMedidaCommand : IRequest<int>
{
    /// <summary>
    /// Nombre completo de la unidad de medida. Máximo 60 caracteres. Requerido.
    /// Este nombre es el identificador principal en CONTPAQi.
    /// Ejemplo: "PIEZA", "KILOGRAMO", "LITRO", "METRO"
    /// </summary>
    [Required(ErrorMessage = "El nombre de la unidad es requerido.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string NombreUnidad { get; set; } = string.Empty;

    /// <summary>
    /// Abreviatura de la unidad de medida. Máximo 3 caracteres. Requerido.
    /// Ejemplo: "PZA", "KG", "LT", "MT"
    /// </summary>
    [Required(ErrorMessage = "La abreviatura es requerida.")]
    [StringLength(3, MinimumLength = 1, ErrorMessage = "La abreviatura debe tener entre 1 y 3 caracteres (límite del SDK).")]
    public string Abreviatura { get; set; } = string.Empty;

    /// <summary>
    /// Texto de despliegue o descripción corta que aparece en reportes. Máximo 20 caracteres. Requerido.
    /// Ejemplo: "Pza.", "Kg.", "Lt.", "Mt."
    /// </summary>
    [Required(ErrorMessage = "El despliegue es requerido.")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "El despliegue debe tener entre 1 y 20 caracteres.")]
    public string Despliegue { get; set; } = string.Empty;

    /// <summary>
    /// Clave SAT de la unidad. Opcional. (CCLAVEINT)
    /// </summary>
    public string? ClaveInt { get; set; }

    /// <summary>
    /// Clave de comercio exterior de la unidad. Opcional. (CCLAVESAT)
    /// </summary>
    public string? ClaveSat { get; set; }
}

public class CreateUnidadMedidaCommandHandler : IRequestHandler<CreateUnidadMedidaCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateUnidadMedidaCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateUnidadMedidaCommand request, CancellationToken cancellationToken)
    {
        var nuevaUnidad = new tUnidad
        {
            cNombreUnidad = request.NombreUnidad,
            cAbreviatura  = request.Abreviatura,
            cDespliegue   = request.Despliegue
        };

        var idUnidad = await _sdk.CrearUnidadMedidaAsync(nuevaUnidad);

        var datosExtra = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.ClaveInt))
            datosExtra["CCLAVEINT"] = request.ClaveInt;

        if (!string.IsNullOrWhiteSpace(request.ClaveSat))
            datosExtra["CCLAVESAT"] = request.ClaveSat;

        if (datosExtra.Count > 0)
        {
            await _sdk.ActualizarUnidadMedidaAsync(request.NombreUnidad, datosExtra);
        }

        return idUnidad;
    }
}
