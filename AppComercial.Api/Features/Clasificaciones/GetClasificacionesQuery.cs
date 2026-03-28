using AppComercial.Api.Infrastructure;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Clasificaciones;

public class GetClasificacionesQuery : IRequest<IEnumerable<AdmClasificaciones>>
{
}

public class GetClasificacionesQueryHandler : IRequestHandler<GetClasificacionesQuery, IEnumerable<AdmClasificaciones>>
{
    private readonly ContpaqiDbContext _context;

    public GetClasificacionesQueryHandler(ContpaqiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdmClasificaciones>> Handle(GetClasificacionesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Clasificaciones
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }
}
