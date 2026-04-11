using AppComercial.Application.Features.Clasificaciones;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClasificacionesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClasificacionesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetClasificacionesQuery());
        return Ok(result);
    }

    [HttpGet("valores")]
    public async Task<IActionResult> GetValores([FromQuery] int? clasificacionId)
    {
        var result = await _mediator.Send(new GetClasificacionesValoresQuery { ClasificacionId = clasificacionId });
        return Ok(result);
    }
}
