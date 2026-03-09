using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.Cotizaciones;

/// <summary>
/// Comando para actualizar campos de una Cotización existente en CONTPAQi Comercial.
/// El concepto, serie y folio se proporcionan en la URL (ruta) del endpoint PUT.
/// Al menos uno de los campos opcionales debe ser enviado.
/// </summary>
public class UpdateCotizacionCommand : IRequest<int>
{
    /// <summary>Código del concepto. Se asigna automáticamente desde la URL.</summary>
    public string CodigoConcepto { get; set; } = string.Empty;

    /// <summary>Serie de la cotización. Se asigna automáticamente desde la URL.</summary>
    public string Serie { get; set; } = string.Empty;

    /// <summary>Folio de la cotización. Se asigna automáticamente desde la URL.</summary>
    public string Folio { get; set; } = string.Empty;

    /// <summary>
    /// Nuevo código del cliente al que se cotiza. Opcional. Máximo 30 caracteres.
    /// Ejemplo: "CLI001"
    /// </summary>
    [StringLength(30, ErrorMessage = "El código del cliente no puede exceder 30 caracteres.")]
    public string? CodigoCliente { get; set; }

    /// <summary>
    /// Nueva fecha de la cotización. Opcional.
    /// Formato esperado: "MM/dd/yyyy". Ejemplo: "03/15/2024"
    /// </summary>
    public DateTime? Fecha { get; set; }

    /// <summary>
    /// Nueva referencia (ej. número de solicitud del cliente). Opcional. Máximo 30 caracteres.
    /// </summary>
    [StringLength(30, ErrorMessage = "La referencia no puede exceder 30 caracteres.")]
    public string? Referencia { get; set; }

    /// <summary>Observaciones generales. Opcional. Máximo 500 caracteres.</summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres.")]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Nuevo código del agente de venta. Opcional. Máximo 30 caracteres.
    /// </summary>
    [StringLength(30, ErrorMessage = "El código del agente no puede exceder 30 caracteres.")]
    public string? CodigoAgente { get; set; }

    /// <summary>Texto extra 1. Opcional. Máximo 50 caracteres.</summary>
    [StringLength(50, ErrorMessage = "TextoExtra1 no puede exceder 50 caracteres.")]
    public string? TextoExtra1 { get; set; }

    /// <summary>Texto extra 2. Opcional. Máximo 50 caracteres.</summary>
    [StringLength(50, ErrorMessage = "TextoExtra2 no puede exceder 50 caracteres.")]
    public string? TextoExtra2 { get; set; }

    /// <summary>Texto extra 3. Opcional. Máximo 50 caracteres.</summary>
    [StringLength(50, ErrorMessage = "TextoExtra3 no puede exceder 50 caracteres.")]
    public string? TextoExtra3 { get; set; }
}

public class UpdateCotizacionCommandHandler : IRequestHandler<UpdateCotizacionCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateCotizacionCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateCotizacionCommand request, CancellationToken cancellationToken)
    {
        var datos = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.CodigoCliente))
            datos["CCODIGOCTEPROV"] = request.CodigoCliente;

        if (request.Fecha.HasValue)
            datos["CFECHA"] = request.Fecha.Value.ToString("MM/dd/yyyy");

        if (!string.IsNullOrWhiteSpace(request.Referencia))
            datos["CREFERENCIA"] = request.Referencia;

        if (!string.IsNullOrWhiteSpace(request.Observaciones))
            datos["COBSERVACIONES"] = request.Observaciones;

        if (!string.IsNullOrWhiteSpace(request.CodigoAgente))
            datos["CCODIGOAGENTE"] = request.CodigoAgente;

        if (!string.IsNullOrWhiteSpace(request.TextoExtra1))
            datos["CTEXTOEXTRA1"] = request.TextoExtra1;

        if (!string.IsNullOrWhiteSpace(request.TextoExtra2))
            datos["CTEXTOEXTRA2"] = request.TextoExtra2;

        if (!string.IsNullOrWhiteSpace(request.TextoExtra3))
            datos["CTEXTOEXTRA3"] = request.TextoExtra3;

        if (datos.Count == 0)
            throw new ArgumentException(
                "Se debe proporcionar al menos un campo para actualizar: " +
                "CodigoCliente, Fecha, Referencia, Observaciones, " +
                "CodigoAgente, TextoExtra1, TextoExtra2 o TextoExtra3.");

        return await _sdk.ActualizarDocumentoAsync(request.CodigoConcepto, request.Serie, request.Folio, datos);
    }
}
