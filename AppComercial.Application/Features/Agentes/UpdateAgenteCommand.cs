using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Application.Features.Agentes;

/// <summary>
/// Comando para actualizar los datos de un Agente existente en CONTPAQi Comercial.
/// El código del agente se proporciona en la URL (ruta) del endpoint PUT.
/// </summary>
public class UpdateAgenteCommand : IRequest<int>
{
    /// <summary>
    /// Código del agente a actualizar. Se asigna automáticamente desde la URL.
    /// No es necesario incluirlo en el cuerpo del request.
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nuevo nombre del agente. Máximo 60 caracteres. Opcional.
    /// Si se proporciona, reemplaza el nombre actual.
    /// Ejemplo: "María García Sánchez"
    /// </summary>
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>
    /// Nuevo tipo de agente. Valores válidos: 1 = Vendedor, 2 = Cobrador. Opcional.
    /// </summary>
    [Range(1, 2, ErrorMessage = "El tipo de agente debe ser 1 (Vendedor) o 2 (Cobrador).")]
    public int? TipoAgente { get; set; }
}

public class UpdateAgenteCommandHandler : IRequestHandler<UpdateAgenteCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateAgenteCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateAgenteCommand request, CancellationToken cancellationToken)
    {
        // Construimos el diccionario solo con los campos que se proporcionaron
        var datos = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.Nombre))
            datos["CNOMBREAGENTE"] = request.Nombre;

        if (request.TipoAgente.HasValue)
            datos["CTIPOAGENTE"] = request.TipoAgente.Value.ToString();

        if (datos.Count == 0)
            throw new ArgumentException("Se debe proporcionar al menos un campo para actualizar: Nombre o TipoAgente.");

        return await _sdk.ActualizarAgenteAsync(request.Codigo, datos);
    }
}
