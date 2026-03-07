using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Almacenes;

public class GetAlmacenesQuery : IRequest<IEnumerable<AdmAlmacenes>>
{
    public string? Codigo { get; set; }
}

public class GetAlmacenesQueryHandler : IRequestHandler<GetAlmacenesQuery, IEnumerable<AdmAlmacenes>>
{
    private readonly ContpaqiDbContext _dbContext;

    public GetAlmacenesQueryHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmAlmacenes>> Handle(GetAlmacenesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Almacenes.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Codigo))
        {
            query = query.Where(a => a.CCODIGOALMACEN == request.Codigo);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
