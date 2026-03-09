using AppComercial.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Infrastructure;

public class ContpaqiDbContext : DbContext
{
    public ContpaqiDbContext(DbContextOptions<ContpaqiDbContext> options)
        : base(options)
    {
    }
    public DbSet<AdmClientes> Clientes => Set<AdmClientes>();
    public DbSet<AdmProductos> Productos => Set<AdmProductos>();
    public DbSet<AdmConceptos> Conceptos => Set<AdmConceptos>();
    public DbSet<AdmAlmacenes> Almacenes => Set<AdmAlmacenes>();
    public DbSet<AdmDocumentos> Documentos => Set<AdmDocumentos>();
    public DbSet<AdmMovimientos> Movimientos => Set<AdmMovimientos>();
    public DbSet<AdmMonedas> Monedas => Set<AdmMonedas>();
    public DbSet<AdmUnidadesMedidaPeso> UnidadesMedidaPeso => Set<AdmUnidadesMedidaPeso>();
    public DbSet<AdmAgentes> Agentes => Set<AdmAgentes>();
    public DbSet<AdmDomicilios> Domicilios => Set<AdmDomicilios>();
    public DbSet<AdmUsuarios> Usuarios => Set<AdmUsuarios>();
    public DbSet<AdmPerfiles> Perfiles => Set<AdmPerfiles>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Desactiva el sufijo 's' pluralizado si cambias convenciones
    }
}
