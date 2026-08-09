using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Application.Features.Domicilios;

/// <summary>
/// Comando para actualizar los datos de un Domicilio existente en CONTPAQi Comercial.
/// El ID del domicilio se proporciona en la URL (ruta) del endpoint PUT.
/// Al menos uno de los campos opcionales debe ser enviado.
/// </summary>
public class UpdateDomicilioCommand : IRequest<int>
{
    /// <summary>
    /// ID interno del domicilio a actualizar. Se asigna automáticamente desde la URL.
    /// No es necesario incluirlo en el cuerpo del request.
    /// </summary>
    public int IdDireccion { get; set; }

    /// <summary>
    /// Nuevo nombre de la calle. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "La calle no puede exceder 60 caracteres.")]
    public string? Calle { get; set; }

    /// <summary>
    /// Nuevo número exterior. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "El número exterior no puede exceder 60 caracteres.")]
    public string? NumeroExterior { get; set; }

    /// <summary>
    /// Nuevo número interior. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "El número interior no puede exceder 60 caracteres.")]
    public string? NumeroInterior { get; set; }

    /// <summary>
    /// Nueva colonia. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "La colonia no puede exceder 60 caracteres.")]
    public string? Colonia { get; set; }

    /// <summary>
    /// Nuevo código postal. Opcional. Máximo 30 caracteres.
    /// </summary>
    [StringLength(30, ErrorMessage = "El código postal no puede exceder 30 caracteres.")]
    public string? CodigoPostal { get; set; }

    /// <summary>
    /// Nueva ciudad. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "La ciudad no puede exceder 60 caracteres.")]
    public string? Ciudad { get; set; }

    /// <summary>
    /// Nuevo estado o provincia. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "El estado no puede exceder 60 caracteres.")]
    public string? Estado { get; set; }

    /// <summary>
    /// Nuevo país. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "El país no puede exceder 60 caracteres.")]
    public string? Pais { get; set; }

    /// <summary>
    /// Nuevo teléfono principal. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "El teléfono no puede exceder 60 caracteres.")]
    public string? Telefono1 { get; set; }

    /// <summary>
    /// Nuevo teléfono secundario. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "El teléfono 2 no puede exceder 60 caracteres.")]
    public string? Telefono2 { get; set; }

    /// <summary>
    /// Nuevo correo electrónico de contacto. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "El email no puede exceder 60 caracteres.")]
    public string? Email { get; set; }

    /// <summary>
    /// Nueva dirección de sitio web. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "La dirección web no puede exceder 60 caracteres.")]
    public string? DireccionWeb { get; set; }
}

public class UpdateDomicilioCommandHandler : IRequestHandler<UpdateDomicilioCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateDomicilioCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateDomicilioCommand request, CancellationToken cancellationToken)
    {
        // Construimos el diccionario solo con los campos que se proporcionaron
        var datos = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.Calle))
            datos["CNOMBRECALLE"] = request.Calle;

        if (!string.IsNullOrWhiteSpace(request.NumeroExterior))
            datos["CNUMEROEXTERIOR"] = request.NumeroExterior;

        if (!string.IsNullOrWhiteSpace(request.NumeroInterior))
            datos["CNUMEROINTERIOR"] = request.NumeroInterior;

        if (!string.IsNullOrWhiteSpace(request.Colonia))
            datos["CCOLONIA"] = request.Colonia;

        if (!string.IsNullOrWhiteSpace(request.CodigoPostal))
            datos["CCODIGOPOSTAL"] = request.CodigoPostal;

        if (!string.IsNullOrWhiteSpace(request.Ciudad))
            datos["CCIUDAD"] = request.Ciudad;

        if (!string.IsNullOrWhiteSpace(request.Estado))
            datos["CESTADO"] = request.Estado;

        if (!string.IsNullOrWhiteSpace(request.Pais))
            datos["CPAIS"] = request.Pais;

        if (!string.IsNullOrWhiteSpace(request.Telefono1))
            datos["CTELEFONO1"] = request.Telefono1;

        if (!string.IsNullOrWhiteSpace(request.Telefono2))
            datos["CTELEFONO2"] = request.Telefono2;

        if (!string.IsNullOrWhiteSpace(request.Email))
            datos["CEMAIL"] = request.Email;

        if (!string.IsNullOrWhiteSpace(request.DireccionWeb))
            datos["CDIRECCIONWEB"] = request.DireccionWeb;

        if (datos.Count == 0)
            throw new ArgumentException(
                "Se debe proporcionar al menos un campo para actualizar: " +
                "Calle, NumeroExterior, NumeroInterior, Colonia, CodigoPostal, " +
                "Ciudad, Estado, Pais, Telefono1, Telefono2, Email o DireccionWeb.");

        return await _sdk.ActualizarDireccionAsync(request.IdDireccion, datos);
    }
}
