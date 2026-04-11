using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Application.Features.Agentes;

/// <summary>
/// Comando para dar de alta un nuevo Agente en CONTPAQi Comercial.
/// </summary>
public class CreateAgenteCommand : IRequest<int>
{
    /// <summary>
    /// Código único del agente. Máximo 30 caracteres. Requerido.
    /// Ejemplo: "AGT001"
    /// </summary>
    [Required(ErrorMessage = "El código del agente es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código debe tener entre 1 y 30 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del agente. Máximo 60 caracteres. Requerido.
    /// Ejemplo: "Juan Pérez López"
    /// </summary>
    [Required(ErrorMessage = "El nombre del agente es requerido.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de agente. Valores válidos: 1 = Vendedor, 2 = Cobrador. Por defecto: 1.
    /// </summary>
    [Range(1, 2, ErrorMessage = "El tipo de agente debe ser 1 (Vendedor) o 2 (Cobrador).")]
    public int TipoAgente { get; set; } = 1;
}

public class CreateAgenteCommandHandler : IRequestHandler<CreateAgenteCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateAgenteCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateAgenteCommand request, CancellationToken cancellationToken)
    {
        // Construimos el diccionario de campos SDK a partir de las propiedades tipadas
        var datos = new Dictionary<string, string>
        {
            ["CCODIGOAGENTE"]  = request.Codigo,
            ["CNOMBREAGENTE"]  = request.Nombre,
            ["CTIPOAGENTE"]    = request.TipoAgente.ToString()
        };

        return await _sdk.CrearAgenteAsync(datos);
    }
}
