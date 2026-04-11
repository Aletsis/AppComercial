using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Application.Features.Monedas;

/// <summary>
/// Comando para actualizar los datos de una Moneda existente en CONTPAQi Comercial.
/// El ID de la moneda se proporciona en la URL (ruta) del endpoint PUT.
/// Al menos uno de los campos opcionales debe ser enviado.
/// </summary>
public class UpdateMonedaCommand : IRequest<int>
{
    /// <summary>
    /// ID interno de la moneda a actualizar. Se asigna automáticamente desde la URL.
    /// No es necesario incluirlo en el cuerpo del request.
    /// </summary>
    public int IdMoneda { get; set; }

    /// <summary>
    /// Nuevo nombre completo de la moneda. Opcional. Máximo 60 caracteres.
    /// Ejemplo: "Dólar Americano"
    /// </summary>
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>
    /// Nuevo símbolo de la moneda. Opcional. Máximo 10 caracteres.
    /// Ejemplo: "USD", "€"
    /// </summary>
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El símbolo debe tener entre 1 y 10 caracteres.")]
    public string? Simbolo { get; set; }

    /// <summary>
    /// Nuevo nombre en plural. Opcional. Máximo 60 caracteres.
    /// Se utiliza en la impresión literal de montos en documentos.
    /// Ejemplo: "Dólares"
    /// </summary>
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El plural debe tener entre 1 y 60 caracteres.")]
    public string? Plural { get; set; }

    /// <summary>
    /// Nuevo nombre en singular. Opcional. Máximo 60 caracteres.
    /// Se utiliza en la impresión literal de montos en documentos.
    /// Ejemplo: "Dólar"
    /// </summary>
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El singular debe tener entre 1 y 60 caracteres.")]
    public string? Singular { get; set; }
}

public class UpdateMonedaCommandHandler : IRequestHandler<UpdateMonedaCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateMonedaCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateMonedaCommand request, CancellationToken cancellationToken)
    {
        // Construimos el diccionario solo con los campos que se proporcionaron
        var datos = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.Nombre))
            datos["CNOMBREMONEDA"] = request.Nombre;

        if (!string.IsNullOrWhiteSpace(request.Simbolo))
            datos["CSIMBOLOMONEDA"] = request.Simbolo;

        if (!string.IsNullOrWhiteSpace(request.Plural))
            datos["CPLURAL"] = request.Plural;

        if (!string.IsNullOrWhiteSpace(request.Singular))
            datos["CSINGULAR"] = request.Singular;

        if (datos.Count == 0)
            throw new ArgumentException(
                "Se debe proporcionar al menos un campo para actualizar: " +
                "Nombre, Simbolo, Plural o Singular.");

        return await _sdk.ActualizarMonedaAsync(request.IdMoneda, datos);
    }
}
