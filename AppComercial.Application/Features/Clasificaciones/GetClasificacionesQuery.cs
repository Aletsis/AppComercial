using AppComercial.Application.Common.Interfaces;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Application.Features.Clasificaciones;

public class GetClasificacionesQuery : IRequest<IEnumerable<AdmClasificaciones>>
{
}

public class GetClasificacionesQueryHandler : IRequestHandler<GetClasificacionesQuery, IEnumerable<AdmClasificaciones>>
{
    private readonly IContpaqiDbContext _context;

    public GetClasificacionesQueryHandler(IContpaqiDbContext context)
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
