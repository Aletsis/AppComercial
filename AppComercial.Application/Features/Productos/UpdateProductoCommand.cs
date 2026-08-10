using AppComercial.Application.Features.Productos;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using AppComercial.Application.Common.Interfaces;

namespace AppComercial.Application.Features.Productos;

/// <summary>
/// Comando para actualizar los datos de un Producto existente en CONTPAQi Comercial.
/// El código del producto se proporciona en la URL (ruta) del endpoint PUT.
/// Al menos uno de los campos opcionales debe ser enviado.
/// </summary>
public class UpdateProductoCommand : IRequest<int>
{
    /// <summary>
    /// Código del producto a actualizar. Se asigna automáticamente desde la URL.
    /// No es necesario incluirlo en el cuerpo del request.
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nuevo nombre corto del producto. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>
    /// Nueva descripción del producto. Opcional. Máximo 255 caracteres.
    /// </summary>
    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Nuevo tipo de producto. Opcional.
    /// Valores válidos: 1 = Producto, 2 = Paquete, 3 = Servicio.
    /// </summary>
    [Range(1, 3, ErrorMessage = "TipoProducto debe ser 1 (Producto), 2 (Paquete) o 3 (Servicio).")]
    public int? TipoProducto { get; set; }

    /// <summary>
    /// Nuevo control de existencia. Opcional.
    /// Valores válidos: 0=Sin control, 1=Unidades, 2=Lotes, 3=Series, 4=Pedimentos.
    /// </summary>
    [Range(0, 4, ErrorMessage = "ControlExistencia debe ser un valor entre 0 y 4.")]
    public int? ControlExistencia { get; set; }

    /// <summary>
    /// Nuevo precio de lista 1. Opcional. Debe ser mayor o igual a 0.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0.")]
    public double? Precio1 { get; set; }
    
    /// <summary>
    /// Nuevo precio de lista 2 (Mayoreo). Opcional. Debe ser mayor o igual a 0.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0.")]
    public double? Precio2 { get; set; }

    /// <summary>
    /// Nuevo porcentaje de impuesto 1 (IVA). Opcional. Entre 0 y 100.
    /// </summary>
    [Range(0, 100, ErrorMessage = "El impuesto 1 debe ser un porcentaje entre 0 y 100.")]
    public double? Impuesto1 { get; set; }

    /// <summary>
    /// Texto extra 1 del producto. Opcional. Máximo 50 caracteres.
    /// </summary>
    [StringLength(50, ErrorMessage = "TextoExtra1 no puede exceder 50 caracteres.")]
    public string? TextoExtra1 { get; set; }

    /// <summary>
    /// Texto extra 2 del producto. Opcional. Máximo 50 caracteres.
    /// </summary>
    [StringLength(50, ErrorMessage = "TextoExtra2 no puede exceder 50 caracteres.")]
    public string? TextoExtra2 { get; set; }

    /// <summary>
    /// Texto extra 3 del producto. Opcional. Máximo 50 caracteres.
    /// </summary>
    [StringLength(50, ErrorMessage = "TextoExtra3 no puede exceder 50 caracteres.")]
    public string? TextoExtra3 { get; set; }

    /// <summary>
    /// Clasificación 1 (Departamento). Código del valor de clasificación.
    /// </summary>
    [StringLength(30, ErrorMessage = "La clasificación 1 no puede exceder 30 caracteres.")]
    public string? Clasificacion1 { get; set; }

    /// <summary>
    /// Clasificación 2 (Código de barras). Código del valor de clasificación.
    /// </summary>
    [StringLength(30, ErrorMessage = "La clasificación 2 no puede exceder 30 caracteres.")]
    public string? Clasificacion2 { get; set; }

    /// <summary>
    /// Clasificación 5 (Tipo de producto). Código del valor de clasificación.
    /// </summary>
    [StringLength(30, ErrorMessage = "La clasificación 5 no puede exceder 30 caracteres.")]
    public string? Clasificacion5 { get; set; }

    /// <summary>
    /// Código SAT del producto (Clave SAT).
    /// </summary>
    [StringLength(20, ErrorMessage = "El código SAT no puede exceder 20 caracteres.")]
    public string? CodigoSat { get; set; }

    /// <summary>
    /// Id de la unidad dentro del XML. Se asume que es el ID interno.
    /// </summary>
    public int? IdUnidadXml { get; set; }

    /// <summary>
    /// Id de la unidad de medida base del producto. Opcional.
    /// </summary>
    public int? IdUnidadBase { get; set; }

    /// <summary>
    /// Código alterno del producto (Código de barras).
    /// </summary>
    [StringLength(30, ErrorMessage = "El código alterno no puede exceder 30 caracteres.")]
    public string? CodigoAlterno { get; set; }
}

public class UpdateProductoCommandHandler : IRequestHandler<UpdateProductoCommand, int>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IContpaqiDbContext _context;

    public UpdateProductoCommandHandler(IContpaqiSdk sdk, IContpaqiDbContext context)
    {
        _sdk = sdk;
        _context = context;
    }

    public async Task<int> Handle(UpdateProductoCommand request, CancellationToken cancellationToken)
    {
        var datos = new Dictionary<string, string>();

        if (request.Nombre != null)
            datos["CNOMBREPRODUCTO"] = request.Nombre;

        if (request.Descripcion != null)
            datos["CDESCRIPCIONPRODUCTO"] = request.Descripcion;

        if (request.TipoProducto.HasValue)
            datos["CTIPOPRODUCTO"] = request.TipoProducto.Value.ToString();

        if (request.ControlExistencia.HasValue)
            datos["CCONTROLEXISTENCIA"] = request.ControlExistencia.Value.ToString();

        if (request.Precio1.HasValue)
            datos["CPRECIO1"] = request.Precio1.Value.ToString("F6");

        if (request.Precio2.HasValue)
            datos["CPRECIO2"] = request.Precio2.Value.ToString("F6");

        if (request.Impuesto1.HasValue)
            datos["CIMPUESTO1"] = request.Impuesto1.Value.ToString("F6");

        if (!string.IsNullOrWhiteSpace(request.TextoExtra1))
            datos["CTEXTOEXTRA1"] = request.TextoExtra1;

        if (!string.IsNullOrWhiteSpace(request.TextoExtra2))
            datos["CTEXTOEXTRA2"] = request.TextoExtra2;

        if (!string.IsNullOrWhiteSpace(request.TextoExtra3))
            datos["CTEXTOEXTRA3"] = request.TextoExtra3;

        if (request.Clasificacion1 != null)
        {
            if (string.IsNullOrWhiteSpace(request.Clasificacion1))
                datos["CIDVALORCLASIFICACION1"] = "0";
            else
            {
                var val = await _context.ClasificacionesValores
                    .FirstOrDefaultAsync(v => v.ClasificacionId == 25 && 
                        (v.CodigoValorClasificacion == request.Clasificacion1 || v.ValorClasificacion == request.Clasificacion1), cancellationToken);
                if (val != null) datos["CIDVALORCLASIFICACION1"] = val.Id.ToString();
                else datos["CIDVALORCLASIFICACION1"] = "0";
            }
        }

        if (request.Clasificacion2 != null)
        {
            if (string.IsNullOrWhiteSpace(request.Clasificacion2))
                datos["CIDVALORCLASIFICACION2"] = "0";
            else
            {
                var val = await _context.ClasificacionesValores
                    .FirstOrDefaultAsync(v => v.ClasificacionId == 26 && 
                        (v.CodigoValorClasificacion == request.Clasificacion2 || v.ValorClasificacion == request.Clasificacion2), cancellationToken);
                if (val != null) datos["CIDVALORCLASIFICACION2"] = val.Id.ToString();
                else datos["CIDVALORCLASIFICACION2"] = "0";
            }
        }

        if (request.Clasificacion5 != null)
        {
            if (string.IsNullOrWhiteSpace(request.Clasificacion5))
                datos["CIDVALORCLASIFICACION5"] = "0";
            else
            {
                var val = await _context.ClasificacionesValores
                    .FirstOrDefaultAsync(v => v.ClasificacionId == 29 && 
                        (v.CodigoValorClasificacion == request.Clasificacion5 || v.ValorClasificacion == request.Clasificacion5), cancellationToken);
                if (val != null) datos["CIDVALORCLASIFICACION5"] = val.Id.ToString();
                else datos["CIDVALORCLASIFICACION5"] = "0";
            }
        }

        if (!string.IsNullOrWhiteSpace(request.CodigoSat))
            datos["CCLAVESAT"] = request.CodigoSat;

        if (request.IdUnidadXml.HasValue && request.IdUnidadXml.Value > 0)
            datos["CIDUNIXML"] = request.IdUnidadXml.Value.ToString();

        if (request.IdUnidadBase.HasValue && request.IdUnidadBase.Value > 0)
        {
            var unidad = await _context.UnidadesMedidaPeso
                .FirstOrDefaultAsync(u => u.Id == request.IdUnidadBase.Value, cancellationToken);
            if (unidad != null)
                datos["CIDUNIDADBASE"] = unidad.Id.ToString();
        }

        if (request.CodigoAlterno != null)
            datos["CCODALTERN"] = request.CodigoAlterno;

        if (datos.Count == 0)
            throw new ArgumentException(
                "Se debe proporcionar al menos un campo para actualizar: " +
                "Nombre, Descripcion, TipoProducto, ControlExistencia, " +
                "Precio1, Impuesto1, TextoExtra1-3, Clasificacion1, 2, 5, CodigoSat, IdUnidadXml o IdUnidadBase.");

        return await _sdk.ActualizarProductoAsync(request.Codigo, datos);
    }
}
