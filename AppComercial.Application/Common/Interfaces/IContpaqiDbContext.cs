using AppComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Common.Interfaces;

public interface IContpaqiDbContext
{
    DbSet<AdmClientes> Clientes { get; }
    DbSet<AdmProductos> Productos { get; }
    DbSet<AdmConceptos> Conceptos { get; }
    DbSet<AdmAlmacenes> Almacenes { get; }
    DbSet<AdmDocumentos> Documentos { get; }
    DbSet<AdmMovimientos> Movimientos { get; }
    DbSet<AdmMonedas> Monedas { get; }
    DbSet<AdmUnidadesMedidaPeso> UnidadesMedidaPeso { get; }
    DbSet<AdmAgentes> Agentes { get; }
    DbSet<AdmDomicilios> Domicilios { get; }
    DbSet<AdmClasificaciones> Clasificaciones { get; }
    DbSet<AdmClasificacionesValores> ClasificacionesValores { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
