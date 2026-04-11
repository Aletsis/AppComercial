using MediatR;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using AppComercial.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Application.Features.Almacenes;

public class GetAlmacenesQuery : IRequest<IEnumerable<AdmAlmacenes>>
{
    public string? Codigo { get; set; }
}

public class GetAlmacenesQueryHandler : IRequestHandler<GetAlmacenesQuery, IEnumerable<AdmAlmacenes>>
{
    private readonly IContpaqiDbContext _dbContext;

    public GetAlmacenesQueryHandler(IContpaqiDbContext dbContext)
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
