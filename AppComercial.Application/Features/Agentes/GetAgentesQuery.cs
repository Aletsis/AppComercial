using AppComercial.Application.Common.Interfaces;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Application.Features.Agentes;

public class GetAgentesQuery : IRequest<IEnumerable<AdmAgentes>>
{
    public string? Codigo { get; set; }
}

public class GetAgentesQueryHandler : IRequestHandler<GetAgentesQuery, IEnumerable<AdmAgentes>>
{
    private readonly IContpaqiDbContext _context;

    public GetAgentesQueryHandler(IContpaqiDbContext context)
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
