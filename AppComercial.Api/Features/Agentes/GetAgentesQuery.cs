using AppComercial.Api.Infrastructure;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Agentes;

public class GetAgentesQuery : IRequest<IEnumerable<AdmAgentes>>
{
    public string? Codigo { get; set; }
}

public class GetAgentesQueryHandler : IRequestHandler<GetAgentesQuery, IEnumerable<AdmAgentes>>
{
    private readonly ContpaqiDbContext _context;

    public GetAgentesQueryHandler(ContpaqiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdmAgentes>> Handle(GetAgentesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Agentes.AsQueryable();

        if (!string.IsNullOrEmpty(request.Codigo))
        {
            query = query.Where(a => a.CodigoAgente == request.Codigo);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
