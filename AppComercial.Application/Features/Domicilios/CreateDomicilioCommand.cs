using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Application.Features.Domicilios;

/// <summary>
/// Comando para crear un nuevo Domicilio asociado a un Cliente o Proveedor en CONTPAQi Comercial.
/// </summary>
public class CreateDomicilioCommand : IRequest<int>
{
    /// <summary>
    /// Código del cliente o proveedor al que se asocia este domicilio. Requerido.
    /// Ejemplo: "CLI001", "PROV005"
    /// </summary>
    [Required(ErrorMessage = "El código del catálogo (cliente/proveedor) es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código debe tener entre 1 y 30 caracteres.")]
    public string CodigoCatalogo { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de catálogo al que pertenece el domicilio. Requerido.
    /// Valores válidos: 1 = Cliente, 2 = Proveedor.
    /// </summary>
    [Required(ErrorMessage = "El tipo de catálogo es requerido.")]
    [Range(1, 2, ErrorMessage = "TipoCatalogo debe ser 1 (Cliente) o 2 (Proveedor).")]
    public int TipoCatalogo { get; set; }

    /// <summary>
    /// Tipo de dirección. Requerido.
    /// Valores válidos: 0 = Fiscal, 1 = Envío.
    /// </summary>
    [Range(0, 1, ErrorMessage = "TipoDireccion debe ser 0 (Fiscal) o 1 (Envío).")]
    public int TipoDireccion { get; set; } = 0;

    /// <summary>
    /// Nombre de la calle. Requerido. Máximo 60 caracteres.
    /// Ejemplo: "Av. Insurgentes Sur"
    /// </summary>
    [Required(ErrorMessage = "La calle es requerida.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "La calle debe tener entre 1 y 60 caracteres.")]
    public string Calle { get; set; } = string.Empty;

    /// <summary>
    /// Número exterior. Opcional. Máximo 60 caracteres.
    /// Ejemplo: "1234"
    /// </summary>
    [StringLength(60, ErrorMessage = "El número exterior no puede exceder 60 caracteres.")]
    public string NumeroExterior { get; set; } = string.Empty;

    /// <summary>
    /// Número interior. Opcional. Máximo 60 caracteres.
    /// Ejemplo: "Piso 3, Ofic. 301"
    /// </summary>
    [StringLength(60, ErrorMessage = "El número interior no puede exceder 60 caracteres.")]
    public string NumeroInterior { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la colonia. Opcional. Máximo 60 caracteres.
    /// Ejemplo: "Del Valle"
    /// </summary>
    [StringLength(60, ErrorMessage = "La colonia no puede exceder 60 caracteres.")]
    public string Colonia { get; set; } = string.Empty;

    /// <summary>
    /// Código postal. Opcional. Máximo 30 caracteres.
    /// Ejemplo: "03100"
    /// </summary>
    [StringLength(30, ErrorMessage = "El código postal no puede exceder 30 caracteres.")]
    public string CodigoPostal { get; set; } = string.Empty;

    /// <summary>
    /// Ciudad. Opcional. Máximo 60 caracteres.
    /// Ejemplo: "Ciudad de México"
    /// </summary>
    [StringLength(60, ErrorMessage = "La ciudad no puede exceder 60 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>
    /// Estado o provincia. Opcional. Máximo 60 caracteres.
    /// Ejemplo: "CDMX", "Jalisco"
    /// </summary>
    [StringLength(60, ErrorMessage = "El estado no puede exceder 60 caracteres.")]
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// País. Por defecto: "México". Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "El país no puede exceder 60 caracteres.")]
    public string Pais { get; set; } = "México";

    /// <summary>
    /// Teléfono principal. Opcional. Máximo 60 caracteres.
    /// Ejemplo: "55 1234 5678"
    /// </summary>
    [StringLength(60, ErrorMessage = "El teléfono no puede exceder 60 caracteres.")]
    public string Telefono1 { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono secundario. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "El teléfono 2 no puede exceder 60 caracteres.")]
    public string Telefono2 { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico de contacto. Opcional. Máximo 60 caracteres.
    /// Ejemplo: "contacto@empresa.com"
    /// </summary>
    [StringLength(60, ErrorMessage = "El email no puede exceder 60 caracteres.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Dirección de sitio web. Opcional. Máximo 60 caracteres.
    /// Ejemplo: "www.empresa.com"
    /// </summary>
    [StringLength(60, ErrorMessage = "La dirección web no puede exceder 60 caracteres.")]
    public string DireccionWeb { get; set; } = string.Empty;
}

public class CreateDomicilioCommandHandler : IRequestHandler<CreateDomicilioCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateDomicilioCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateDomicilioCommand request, CancellationToken cancellationToken)
    {
        var nuevaDireccion = new tDireccion
        {
            cCodCatalogo    = request.CodigoCatalogo,
            cTipoCatalogo   = request.TipoCatalogo,
            cTipoDireccion  = request.TipoDireccion,
            cNombreCalle    = request.Calle,
            cNumeroExterior = request.NumeroExterior,
            cNumeroInterior = request.NumeroInterior,
            cColonia        = request.Colonia,
            cCodigoPostal   = request.CodigoPostal,
            cCiudad         = request.Ciudad,
            cEstado         = request.Estado,
            cPais           = request.Pais,
            cTelefono1      = request.Telefono1,
            cTelefono2      = request.Telefono2,
            cTelefono3      = string.Empty,
            cTelefono4      = string.Empty,
            cEmail          = request.Email,
            cDireccionWeb   = request.DireccionWeb,
            cTextoExtra     = string.Empty
        };

        return await _sdk.CrearDireccionAsync(nuevaDireccion);
    }
}
