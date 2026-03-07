using AppComercial.Api.Infrastructure;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Monedas;

public class GetMonedasQuery : IRequest<IEnumerable<AdmMonedas>>
{
}

public class GetMonedasQueryHandler : IRequestHandler<GetMonedasQuery, IEnumerable<AdmMonedas>>
{
    private readonly ContpaqiDbContext _context;

    public GetMonedasQueryHandler(ContpaqiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdmMonedas>> Handle(GetMonedasQuery request, CancellationToken cancellationToken)
    {
        return await _context.Monedas.ToListAsync(cancellationToken);
    }
}
