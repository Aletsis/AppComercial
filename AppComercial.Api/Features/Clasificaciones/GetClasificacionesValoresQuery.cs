using AppComercial.Api.Infrastructure;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Clasificaciones;

public class GetClasificacionesValoresQuery : IRequest<IEnumerable<AdmClasificacionesValores>>
{
    public int? ClasificacionId { get; set; }
}

public class GetClasificacionesValoresQueryHandler : IRequestHandler<GetClasificacionesValoresQuery, IEnumerable<AdmClasificacionesValores>>
{
    private readonly ContpaqiDbContext _context;

    public GetClasificacionesValoresQueryHandler(ContpaqiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdmClasificacionesValores>> Handle(GetClasificacionesValoresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ClasificacionesValores.AsQueryable();

        if (request.ClasificacionId.HasValue)
        {
            query = query.Where(v => v.ClasificacionId == request.ClasificacionId.Value);
        }

        return await query.OrderBy(v => v.Id).ToListAsync(cancellationToken);
    }
}
