using AppComercial.Domain.Entities;

namespace AppComercial.Domain.Interfaces;

public interface IProductoRepository
{
    Task<IEnumerable<AdmProductos>> GetAllAsync();
    Task<AdmProductos?> GetByIdAsync(int id);
    Task<AdmProductos?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<AdmProductos>> GetByNombreAsync(string nombre);
    Task<IEnumerable<AdmProductos>> GetByFiltersAsync(string? codigo, int? tipoProducto);
}