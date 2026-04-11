namespace AppComercial.Application.DTOs;

public class ProductoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    /// <summary>Lista de Precios 1 → PrecioMenudeo en POS</summary>
    public double Precio { get; set; }
    /// <summary>Lista de Precios 2 → PrecioMayoreo en POS</summary>
    public double Precio2 { get; set; }
    /// <summary>Porcentaje de IVA (e.g. 16.0 = 16%)</summary>
    public double Impuesto1 { get; set; }
    public string? CodigoSat { get; set; }
    public int UnidadMedidaId { get; set; }
    public string UnidadMedidaNombre { get; set; } = string.Empty;
    public int Clasificacion1Id { get; set; }
    public int Clasificacion5Id { get; set; }
    public bool Activo { get; set; }
}