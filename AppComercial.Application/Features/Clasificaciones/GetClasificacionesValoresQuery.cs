using AppComercial.Application.Common.Interfaces;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Application.Features.Clasificaciones;

public class GetClasificacionesValoresQuery : IRequest<IEnumerable<AdmClasificacionesValores>>
{
    public int? ClasificacionId { get; set; }
}

public class GetClasificacionesValoresQueryHandler : IRequestHandler<GetClasificacionesValoresQuery, IEnumerable<AdmClasificacionesValores>>
{
    private readonly IContpaqiDbContext _context;

    public GetClasificacionesValoresQueryHandler(IContpaqiDbContext context)
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
