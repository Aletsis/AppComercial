using AppComercial.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Infrastructure.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly ContpaqiDbContext _context;

    public ProductoRepository(ContpaqiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdmProductos>> GetAllAsync()
    {
        return await _context.Productos.AsNoTracking().ToListAsync();
    }

    public async Task<AdmProductos?> GetByIdAsync(int id)
    {
        return await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.CIDPRODUCTO == id);
    }

    public async Task<AdmProductos?> GetByCodigoAsync(string codigo)
    {
        return await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.CCODIGOPRODUCTO == codigo);
    }

    public async Task<IEnumerable<AdmProductos>> GetByFiltersAsync(string? codigo, int? tipoProducto)
    {
        var query = _context.Productos.AsNoTracking();

        if (!string.IsNullOrEmpty(codigo))
        {
            query = query.Where(p => p.CCODIGOPRODUCTO == codigo);
        }

        if (tipoProducto.HasValue)
        {
            query = query.Where(p => p.CTIPOPRODUCTO == tipoProducto.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<AdmProductos>> GetByNombreAsync(string nombre)
    {
        return await _context.Productos.AsNoTracking()
            .Where(p => p.CNOMBREPRODUCTO.Contains(nombre))
            .ToListAsync();
    }
}