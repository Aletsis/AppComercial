using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Application.Features.SalidasAlmacen;

/// <summary>
/// Comando para actualizar campos de un documento de Salida de Almacén existente en CONTPAQi Comercial.
/// El concepto, serie y folio se proporcionan en la URL (ruta) del endpoint PUT.
/// Al menos uno de los campos opcionales debe ser enviado.
/// </summary>
public class UpdateSalidaAlmacenCommand : IRequest<int>
{
    /// <summary>
    /// Código del concepto de la salida. Se asigna automáticamente desde la URL.
    /// </summary>
    public string CodigoConcepto { get; set; } = string.Empty;

    /// <summary>
    /// Serie del documento. Se asigna automáticamente desde la URL.
    /// </summary>
    public string Serie { get; set; } = string.Empty;

    /// <summary>
    /// Folio del documento. Se asigna automáticamente desde la URL.
    /// </summary>
    public string Folio { get; set; } = string.Empty;

    /// <summary>
    /// Nueva referencia o motivo de la salida. Opcional. Máximo 30 caracteres.
    /// Ejemplo: "Ajuste por merma", "Transferencia sucursal norte"
    /// </summary>
    [StringLength(30, ErrorMessage = "La referencia no puede exceder 30 caracteres.")]
    public string? Referencia { get; set; }

    /// <summary>
    /// Nueva fecha de la salida de almacén. Opcional.
    /// Formato esperado: "MM/dd/yyyy". Ejemplo: "03/15/2024"
    /// </summary>
    public DateTime? Fecha { get; set; }

    /// <summary>
    /// Observaciones sobre la salida. Opcional. Máximo 500 caracteres.
    /// </summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres.")]
    public string? Observaciones { get; set; }

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

public class UpdateSalidaAlmacenCommandHandler : IRequestHandler<UpdateSalidaAlmacenCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateSalidaAlmacenCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateSalidaAlmacenCommand request, CancellationToken cancellationToken)
    {
        var datos = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.Referencia))
            datos["CREFERENCIA"] = request.Referencia;

        if (request.Fecha.HasValue)
            datos["CFECHA"] = request.Fecha.Value.ToString("MM/dd/yyyy");

        if (!string.IsNullOrWhiteSpace(request.Observaciones))
            datos["COBSERVACIONES"] = request.Observaciones;

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
                "Referencia, Fecha, Observaciones, TextoExtra1, TextoExtra2, TextoExtra3 o FechaExtra.");

        return await _sdk.ActualizarDocumentoAsync(request.CodigoConcepto, request.Serie, request.Folio, datos);
    }
}
