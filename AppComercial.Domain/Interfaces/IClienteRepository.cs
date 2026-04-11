using AppComercial.Domain.Entities;

namespace AppComercial.Domain.Interfaces;

public interface IClienteRepository
{
    Task<IEnumerable<AdmClientes>> GetAllAsync();
    Task<AdmClientes?> GetByIdAsync(int id);
    Task<AdmClientes?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<AdmClientes>> GetByNombreAsync(string nombre);
}