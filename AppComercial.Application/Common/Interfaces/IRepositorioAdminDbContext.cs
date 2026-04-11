using AppComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Common.Interfaces;

public interface IRepositorioAdminDbContext
{
    DbSet<CacUsuario> Usuarios { get; }
    DbSet<CacPerfil> Perfiles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
