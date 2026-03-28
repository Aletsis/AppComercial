using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using AppComercial.Api.Infrastructure;

namespace AppComercial.Api.Features.Productos;

/// <summary>
/// Comando para dar de alta un nuevo Producto o Servicio en CONTPAQi Comercial.
/// </summary>
public class CreateProductoCommand : IRequest<int>
{
    /// <summary>
    /// Código único del producto. Máximo 30 caracteres. Requerido.
    /// Ejemplo: "PROD-001", "SRV-MANT"
    /// </summary>
    [Required(ErrorMessage = "El código del producto es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código debe tener entre 1 y 30 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre corto del producto. Máximo 60 caracteres. Requerido.
    /// Ejemplo: "Laptop Dell XPS 15"
    /// </summary>
    [Required(ErrorMessage = "El nombre del producto es requerido.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del producto. Máximo 255 caracteres. Opcional.
    /// </summary>
    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de producto. Por defecto: 1.
    /// Valores válidos:
    ///   1 = Producto físico
    ///   2 = Paquete o kit
    ///   3 = Servicio
    /// </summary>
    [Range(1, 3, ErrorMessage = "TipoProducto debe ser 1 (Producto), 2 (Paquete) o 3 (Servicio).")]
    public int TipoProducto { get; set; } = 1;

    /// <summary>
    /// Método de control de existencia en almacén. Por defecto: 0.
    /// Valores válidos:
    ///   0 = Sin control de existencia
    ///   1 = Control por unidades
    ///   2 = Control por lotes
    ///   3 = Control por series (número de serie)
    ///   4 = Control por pedimentos
    /// </summary>
    [Range(0, 4, ErrorMessage = "ControlExistencia debe ser un valor entre 0 y 4.")]
    public int ControlExistencia { get; set; } = 0;

    /// <summary>
    /// Id de la unidad de medida base del producto. Por defecto: 1.
    /// Debe existir en el catálogo de Unidades de Medida de CONTPAQi.
    /// Ejemplo: 1, 2, 3
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El Id de la Unidad Base debe ser mayor a 0.")]
    public int IdUnidadBase { get; set; } = 1;

    /// <summary>
    /// Precio de lista 1 del producto. Por defecto: 0.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0.")]
    public double Precio1 { get; set; } = 0;

    /// <summary>
    /// Porcentaje de impuesto 1 (IVA) aplicado al producto. Por defecto: 0.
    /// Ejemplo: 16 (para 16% de IVA).
    /// </summary>
    [Range(0, 100, ErrorMessage = "El impuesto 1 debe ser un porcentaje entre 0 y 100.")]
    public double Impuesto1 { get; set; } = 0;

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
}

public class CreateProductoCommandHandler : IRequestHandler<CreateProductoCommand, int>
{
    private readonly IContpaqiSdk _sdk;
    private readonly ContpaqiDbContext _context;

    public CreateProductoCommandHandler(IContpaqiSdk sdk, ContpaqiDbContext context)
    {
        _sdk = sdk;
        _context = context;
    }

    public async Task<int> Handle(CreateProductoCommand request, CancellationToken cancellationToken)
    {
        var unidad = await _context.UnidadesMedidaPeso
            .FirstOrDefaultAsync(u => u.Id == request.IdUnidadBase, cancellationToken);

        if (unidad == null)
            throw new Exception($"La unidad de medida con ID {request.IdUnidadBase} no existe.");

        var nuevoProducto = new tProducto
        {
            cCodigoProducto          = request.Codigo,
            cNombreProducto          = request.Nombre,
            cDescripcionProducto     = request.Descripcion,
            cTipoProducto            = request.TipoProducto,
            cControlExistencia       = request.ControlExistencia,
            cCodigoUnidadBase        = unidad.NombreUnidad,
            cPrecio1                 = request.Precio1,
            cImpuesto1               = request.Impuesto1,
            // Valores requeridos por el SDK que se inicializan con defaults seguros
            cStatusProducto          = 1,   // 1 = Alta
            cMetodoCosteo            = 0,
            cCodigoUnidadNoConvertible = null,
            cNombreCaracteristica1   = null,
            cNombreCaracteristica2   = null,
            cNombreCaracteristica3   = null,
            cCodigoValorClasificacion1 = string.IsNullOrWhiteSpace(request.Clasificacion1) ? null : request.Clasificacion1,
            cCodigoValorClasificacion2 = string.IsNullOrWhiteSpace(request.Clasificacion2) ? null : request.Clasificacion2,
            cCodigoValorClasificacion3 = null,
            cCodigoValorClasificacion4 = null,
            cCodigoValorClasificacion5 = string.IsNullOrWhiteSpace(request.Clasificacion5) ? null : request.Clasificacion5,
            cCodigoValorClasificacion6 = null,
            cTextoExtra1             = null,
            cTextoExtra2             = null,
            cTextoExtra3             = null,
        };

        var nuevoId = await _sdk.CrearProductoAsync(nuevoProducto);

        var datosExtra = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(request.CodigoSat))
            datosExtra["CCLAVESAT"] = request.CodigoSat;

        if (request.IdUnidadXml.HasValue)
            datosExtra["CIDUNIXML"] = request.IdUnidadXml.Value.ToString();

        if (datosExtra.Count > 0)
        {
            await _sdk.ActualizarProductoAsync(request.Codigo, datosExtra);
        }

        return nuevoId;
    }
}
