using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using AppComercial.Application.DTOs;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace AppComercial.Application.Features.Documentos;

public class EmitirDocumentoCommand : IRequest<TimbradoResult>
{
    [Required(ErrorMessage = "El código de concepto es requerido.")]
    public string CodigoConcepto { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "La serie no puede exceder los 10 caracteres.")]
    public string Serie { get; set; } = string.Empty;

    [Range(0.000001, double.MaxValue, ErrorMessage = "El folio debe ser mayor a 0.")]
    public double Folio { get; set; }

    public string PasswordContpaqi { get; set; } = string.Empty;

    public string CorreoOArchivoAdicional { get; set; } = string.Empty; // En Comercial puede separar correos por ; o ruta de visor PFD
}

public class EmitirDocumentoCommandHandler : IRequestHandler<EmitirDocumentoCommand, TimbradoResult>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IConfiguration _configuration;

    public EmitirDocumentoCommandHandler(IContpaqiSdk sdk, IConfiguration configuration)
    {
        _sdk = sdk;
        _configuration = configuration;
    }

    public async Task<TimbradoResult> Handle(EmitirDocumentoCommand request, CancellationToken cancellationToken)
    {
        var password = request.PasswordContpaqi;
        if (string.IsNullOrWhiteSpace(password))
        {
            password = _configuration["Contpaqi:CsdPassword"];
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("La contraseña del CSD es requerida para timbrar (no provista en la petición ni en la configuración).");
        }

        var camposALeer = new[] { "CUUID", "CCADENAORIGINAL", "CSELLOEMISOR", "CSATSELLO", "CCERTIFICADOEMISOR", "CCERTIFICADOSAT", "CFECHA", "CHORA" };

        var datosLeidos = await _sdk.EmitirDocumentoYLeerDatosAsync(
            request.CodigoConcepto,
            request.Serie,
            request.Folio,
            password,
            request.CorreoOArchivoAdicional,
            camposALeer);

        var result = new TimbradoResult
        {
            UUID = datosLeidos.GetValueOrDefault("CUUID", string.Empty),
            CadenaOriginal = datosLeidos.GetValueOrDefault("CCADENAORIGINAL", string.Empty),
            SelloDigitalEmisor = datosLeidos.GetValueOrDefault("CSELLOEMISOR", string.Empty),
            SelloDigitalSAT = datosLeidos.GetValueOrDefault("CSATSELLO", string.Empty),
            NoCertificadoEmisor = datosLeidos.GetValueOrDefault("CCERTIFICADOEMISOR", string.Empty),
            NoCertificadoSAT = datosLeidos.GetValueOrDefault("CCERTIFICADOSAT", string.Empty)
        };

        var fecha = datosLeidos.GetValueOrDefault("CFECHA", string.Empty);
        var hora = datosLeidos.GetValueOrDefault("CHORA", string.Empty);
        if (!string.IsNullOrWhiteSpace(fecha) || !string.IsNullOrWhiteSpace(hora))
        {
            result.FechaTimbrado = $"{fecha} {hora}".Trim();
        }

        return result;
    }
}
