using AppComercial.Api.Infrastructure;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.UnidadesMedida;

public class GetUnidadesMedidaQuery : IRequest<IEnumerable<AdmUnidadesMedidaPeso>>
{
    public string? Nombre { get; set; }
}

public class GetUnidadesMedidaQueryHandler : IRequestHandler<GetUnidadesMedidaQuery, IEnumerable<AdmUnidadesMedidaPeso>>
{
    private readonly ContpaqiDbContext _context;

    public GetUnidadesMedidaQueryHandler(ContpaqiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdmUnidadesMedidaPeso>> Handle(GetUnidadesMedidaQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UnidadesMedidaPeso.AsQueryable();

        if (!string.IsNullOrEmpty(request.Nombre))
        {
            query = query.Where(u => u.NombreUnidad.Contains(request.Nombre));
        }

        return await query.ToListAsync(cancellationToken);
    }
}
