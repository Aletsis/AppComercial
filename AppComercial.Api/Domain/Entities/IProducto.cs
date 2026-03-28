namespace AppComercial.Api.Domain.Entities;

public interface IProducto
{
    int Id { get; }
    string Codigo { get; }
    string Nombre { get; }
    string? Descripcion { get; }
    decimal Precio { get; }
    int UnidadMedidaId { get; }
    bool Activo { get; }
}