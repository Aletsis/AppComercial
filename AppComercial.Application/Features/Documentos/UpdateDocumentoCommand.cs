using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Application.Features.Documentos;

/// <summary>
/// Comando para actualizar campos de un Documento existente en CONTPAQi Comercial.
/// El concepto, serie y folio se proporcionan en la URL (ruta) del endpoint PUT.
/// Al menos uno de los campos opcionales debe ser enviado.
/// </summary>
public class UpdateDocumentoCommand : IRequest<int>
{
    /// <summary>
    /// Código del concepto del documento. Se asigna automáticamente desde la URL.
    /// No es necesario incluirlo en el cuerpo del request.
    /// </summary>
    public string CodigoConcepto { get; set; } = string.Empty;

    /// <summary>
    /// Serie del documento. Se asigna automáticamente desde la URL.
    /// No es necesario incluirlo en el cuerpo del request.
    /// </summary>
    public string Serie { get; set; } = string.Empty;

    /// <summary>
    /// Folio del documento. Se asigna automáticamente desde la URL.
    /// No es necesario incluirlo en el cuerpo del request.
    /// </summary>
    public string Folio { get; set; } = string.Empty;

    // ─── Cabecera del documento ────────────────────────────────────────────────

    /// <summary>
    /// Nuevo código del cliente o proveedor asociado al documento. Opcional.
    /// Máximo 30 caracteres.
    /// Ejemplo: "CLI001", "PROV005"
    /// </summary>
    [StringLength(30, ErrorMessage = "El código del cliente/proveedor no puede exceder 30 caracteres.")]
    public string? CodigoClienteProveedor { get; set; }

    /// <summary>
    /// Nueva fecha del documento. Opcional.
    /// Formato esperado: "MM/dd/yyyy". Ejemplo: "03/15/2024"
    /// </summary>
    public DateTime? Fecha { get; set; }

    /// <summary>
    /// Nueva referencia del documento (p. ej. número de factura del proveedor). Opcional.
    /// Máximo 30 caracteres.
    /// </summary>
    [StringLength(30, ErrorMessage = "La referencia no puede exceder 30 caracteres.")]
    public string? Referencia { get; set; }

    /// <summary>
    /// Observaciones generales del documento. Opcional.
    /// Máximo 500 caracteres.
    /// </summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres.")]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Código del agente de venta/compra asociado. Opcional.
    /// Máximo 30 caracteres. Ejemplo: "AGT001"
    /// </summary>
    [StringLength(30, ErrorMessage = "El código del agente no puede exceder 30 caracteres.")]
    public string? CodigoAgente { get; set; }

    /// <summary>
    /// Nuevo tipo de cambio respecto al peso mexicano. Opcional.
    /// Debe ser mayor a 0.
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "El tipo de cambio debe ser mayor a 0.")]
    public double? TipoCambio { get; set; }

    // ─── Campos extra ─────────────────────────────────────────────────────────

    /// <summary>
    /// Texto extra 1 del documento. Opcional. Máximo 50 caracteres.
    /// </summary>
    [StringLength(50, ErrorMessage = "TextoExtra1 no puede exceder 50 caracteres.")]
    public string? TextoExtra1 { get; set; }

    /// <summary>
    /// Texto extra 2 del documento. Opcional. Máximo 50 caracteres.
    /// </summary>
    [StringLength(50, ErrorMessage = "TextoExtra2 no puede exceder 50 caracteres.")]
    public string? TextoExtra2 { get; set; }

    /// <summary>
    /// Texto extra 3 del documento. Opcional. Máximo 50 caracteres.
    /// </summary>
    [StringLength(50, ErrorMessage = "TextoExtra3 no puede exceder 50 caracteres.")]
    public string? TextoExtra3 { get; set; }

    /// <summary>
    /// Fecha extra del documento. Opcional.
    /// </summary>
    public DateTime? FechaExtra { get; set; }
}

public class UpdateDocumentoCommandHandler : IRequestHandler<UpdateDocumentoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateDocumentoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateDocumentoCommand request, CancellationToken cancellationToken)
    {
        // Construimos el diccionario solo con los campos que se proporcionaron
        var datos = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.CodigoClienteProveedor))
            datos["CCODIGOCTEPROV"] = request.CodigoClienteProveedor;

        if (request.Fecha.HasValue)
            datos["CFECHA"] = request.Fecha.Value.ToString("MM/dd/yyyy");

        if (!string.IsNullOrWhiteSpace(request.Referencia))
            datos["CREFERENCIA"] = request.Referencia;

        if (!string.IsNullOrWhiteSpace(request.Observaciones))
            datos["COBSERVACIONES"] = request.Observaciones;

        if (!string.IsNullOrWhiteSpace(request.CodigoAgente))
            datos["CCODIGOAGENTE"] = request.CodigoAgente;

        if (request.TipoCambio.HasValue)
            datos["CTIPOCAMBIO"] = request.TipoCambio.Value.ToString("F6");

        if (!string.IsNullOrWhiteSpace(request.TextoExtra1))
            datos["CTEXTOEXTRA1"] = request.TextoExtra1;

        if (!string.IsNullOrWhiteSpace(request.TextoExtra2))
            datos["CTEXTOEXTRA2"] = request.TextoExtra2;

        if (!string.IsNullOrWhiteSpace(request.TextoExtra3))
            datos["CTEXTOEXTRA3"] = request.TextoExtra3;

        if (request.FechaExtra.HasValue)
            datos["CFECHAEXTRA"] = request.FechaExtra.Value.ToString("MM/dd/yyyy");

        if (datos.Count == 0)
            throw new ArgumentException(
                "Se debe proporcionar al menos un campo para actualizar: " +
                "CodigoClienteProveedor, Fecha, Referencia, Observaciones, " +
                "CodigoAgente, TipoCambio, TextoExtra1, TextoExtra2, TextoExtra3 o FechaExtra.");

        return await _sdk.ActualizarDocumentoAsync(request.CodigoConcepto, request.Serie, request.Folio, datos);
    }
}
