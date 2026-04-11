using AppComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Common.Interfaces;

public interface ICompacWAdminDbContext
{
    DbSet<AdmUsuariosActivos> UsuariosActivos { get; }
    DbSet<AdmEmpresas> Empresas { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
