using AppComercial.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ContpaqiDbContext _context;

    public ClienteRepository(ContpaqiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdmClientes>> GetAllAsync()
    {
        return await _context.Clientes.AsNoTracking().ToListAsync();
    }

    public async Task<AdmClientes?> GetByIdAsync(int id)
    {
        return await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.CIDCLIENTEPROVEEDOR == id);
    }

    public async Task<AdmClientes?> GetByCodigoAsync(string codigo)
    {
        return await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.CCODIGOCLIENTE == codigo);
    }

    public async Task<IEnumerable<AdmClientes>> GetByNombreAsync(string nombre)
    {
        return await _context.Clientes.AsNoTracking()
            .Where(c => c.CRAZONSOCIAL.Contains(nombre))
            .ToListAsync();
    }
}