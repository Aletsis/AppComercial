using AppComercial.Application.Common.Interfaces;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Application.Features.Monedas;

public class GetMonedasQuery : IRequest<IEnumerable<AdmMonedas>>
{
}

public class GetMonedasQueryHandler : IRequestHandler<GetMonedasQuery, IEnumerable<AdmMonedas>>
{
    private readonly IContpaqiDbContext _context;

    public GetMonedasQueryHandler(IContpaqiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdmMonedas>> Handle(GetMonedasQuery request, CancellationToken cancellationToken)
    {
        return await _context.Monedas.ToListAsync(cancellationToken);
    }
}
