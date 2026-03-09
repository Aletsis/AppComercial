using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.Monedas;

/// <summary>
/// Comando para dar de alta una nueva Moneda en CONTPAQi Comercial.
/// </summary>
public class CreateMonedaCommand : IRequest<int>
{
    /// <summary>
    /// Nombre completo de la moneda. Requerido. Máximo 60 caracteres.
    /// Ejemplo: "Dólar Americano", "Euro"
    /// </summary>
    [Required(ErrorMessage = "El nombre de la moneda es requerido.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Símbolo de la moneda. Requerido. Máximo 10 caracteres.
    /// Ejemplo: "USD", "€", "MXN"
    /// </summary>
    [Required(ErrorMessage = "El símbolo de la moneda es requerido.")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "El símbolo debe tener entre 1 y 10 caracteres.")]
    public string Simbolo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre en plural de la moneda. Requerido. Máximo 60 caracteres.
    /// Se utiliza en la impresión de documentos (p. ej. "DÓLARES").
    /// Ejemplo: "Dólares", "Euros"
    /// </summary>
    [Required(ErrorMessage = "El plural de la moneda es requerido.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El plural debe tener entre 1 y 60 caracteres.")]
    public string Plural { get; set; } = string.Empty;

    /// <summary>
    /// Nombre en singular de la moneda. Requerido. Máximo 60 caracteres.
    /// Se utiliza en la impresión de documentos (p. ej. "DÓLAR").
    /// Ejemplo: "Dólar", "Euro"
    /// </summary>
    [Required(ErrorMessage = "El singular de la moneda es requerido.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El singular debe tener entre 1 y 60 caracteres.")]
    public string Singular { get; set; } = string.Empty;
}

public class CreateMonedaCommandHandler : IRequestHandler<CreateMonedaCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateMonedaCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateMonedaCommand request, CancellationToken cancellationToken)
    {
        // Construimos el diccionario de campos SDK a partir de las propiedades tipadas
        var datos = new Dictionary<string, string>
        {
            ["CNOMBREMONEDA"]  = request.Nombre,
            ["CSIMBOLOMONEDA"] = request.Simbolo,
            ["CPLURAL"]        = request.Plural,
            ["CSINGULAR"]      = request.Singular
        };

        return await _sdk.CrearMonedaAsync(datos);
    }
}
