using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using AppComercial.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Infrastructure;

/// <summary>
/// DbContext para la base de datos compartida CompacWAdmin de CONTPAQi.
/// Contiene información del sistema como usuarios activos, empresas registradas, etc.
/// Requiere una cadena de conexión "CompacWAdmin" apuntando a esa BD en el mismo servidor.
/// </summary>
public class CompacWAdminDbContext : DbContext, ICompacWAdminDbContext
{
    public CompacWAdminDbContext(DbContextOptions<CompacWAdminDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Usuarios con sesión abierta actualmente en alguna empresa de CONTPAQi.
    /// Esta tabla se actualiza en tiempo real.
    /// </summary>
    public DbSet<AdmUsuariosActivos> UsuariosActivos => Set<AdmUsuariosActivos>();

    /// <summary>Empresas registradas en el sistema CONTPAQi.</summary>
    public DbSet<AdmEmpresas> Empresas => Set<AdmEmpresas>();
}
