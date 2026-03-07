using AppComercial.Api.Infrastructure;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Domicilios;

public class GetDomiciliosQuery : IRequest<IEnumerable<AdmDomicilios>>
{
    public int? CatalogoId { get; set; }
    public int? TipoCatalogo { get; set; }
}

public class GetDomiciliosQueryHandler : IRequestHandler<GetDomiciliosQuery, IEnumerable<AdmDomicilios>>
{
    private readonly ContpaqiDbContext _context;

    public GetDomiciliosQueryHandler(ContpaqiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdmDomicilios>> Handle(GetDomiciliosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Domicilios.AsQueryable();

        if (request.CatalogoId.HasValue)
        {
            query = query.Where(d => d.CatalogoId == request.CatalogoId.Value);
        }

        if (request.TipoCatalogo.HasValue)
        {
            query = query.Where(d => d.TipoCatalogo == request.TipoCatalogo.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
