using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.UnidadesMedida;

/// <summary>
/// Comando para actualizar los datos de una Unidad de Medida existente en CONTPAQi Comercial.
/// El nombre de la unidad se proporciona en la URL (ruta) del endpoint PUT.
/// Al menos uno de los campos opcionales debe ser enviado.
/// </summary>
public class UpdateUnidadMedidaCommand : IRequest<int>
{
    /// <summary>
    /// Nombre actual de la unidad a actualizar. Se asigna automáticamente desde la URL.
    /// No es necesario incluirlo en el cuerpo del request.
    /// Ejemplo: "PIEZA", "KILOGRAMO"
    /// </summary>
    public string NombreUnidad { get; set; } = string.Empty;

    /// <summary>
    /// Nueva abreviatura de la unidad. Opcional. Máximo 3 caracteres (límite del SDK).
    /// Ejemplo: "PZA", "KG"
    /// </summary>
    [StringLength(3, MinimumLength = 1, ErrorMessage = "La abreviatura debe tener entre 1 y 3 caracteres (límite del SDK).")]
    public string? Abreviatura { get; set; }

    /// <summary>
    /// Nuevo texto de despliegue en reportes. Opcional. Máximo 20 caracteres.
    /// Ejemplo: "Pza.", "Kg."
    /// </summary>
    [StringLength(20, MinimumLength = 1, ErrorMessage = "El despliegue debe tener entre 1 y 20 caracteres.")]
    public string? Despliegue { get; set; }

    /// <summary>
    /// Clave SAT de la unidad. Opcional. (CCLAVEINT)
    /// </summary>
    public string? ClaveInt { get; set; }

    /// <summary>
    /// Clave de comercio exterior de la unidad. Opcional. (CCLAVESAT)
    /// </summary>
    public string? ClaveSat { get; set; }
}

public class UpdateUnidadMedidaCommandHandler : IRequestHandler<UpdateUnidadMedidaCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateUnidadMedidaCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateUnidadMedidaCommand request, CancellationToken cancellationToken)
    {
        var datos = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.Abreviatura))
            datos["CABREVIATURA"] = request.Abreviatura;

        if (!string.IsNullOrWhiteSpace(request.Despliegue))
            datos["CDESPLIEGUE"] = request.Despliegue;

        if (!string.IsNullOrWhiteSpace(request.ClaveInt))
            datos["CCLAVEINT"] = request.ClaveInt;

        if (!string.IsNullOrWhiteSpace(request.ClaveSat))
            datos["CCLAVESAT"] = request.ClaveSat;

        if (datos.Count == 0)
            throw new ArgumentException(
                "Se debe proporcionar al menos un campo para actualizar: Abreviatura, Despliegue, ClaveInt o ClaveSat.");

        return await _sdk.ActualizarUnidadMedidaAsync(request.NombreUnidad, datos);
    }
}
