using AppComercial.Domain.Entities;
using AppComercial.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Infrastructure;

/// <summary>
/// DbContext para la base de datos RepositorioAdminPAQ de CONTPAQi.
/// Contiene el catálogo de usuarios (CAC10000) y perfiles (CAC30000)
/// compartidos entre las distintas aplicaciones PAQ.
/// </summary>
public class RepositorioAdminDbContext : DbContext, IRepositorioAdminDbContext
{
    public RepositorioAdminDbContext(DbContextOptions<RepositorioAdminDbContext> options)
        : base(options)
    {
    }

    /// <summary>Usuarios del sistema CONTPAQi (tabla CAC10000).</summary>
    public DbSet<CacUsuario> Usuarios => Set<CacUsuario>();

    /// <summary>Perfiles de permisos del sistema CONTPAQi (tabla CAC30000).</summary>
    public DbSet<CacPerfil> Perfiles => Set<CacPerfil>();
}
