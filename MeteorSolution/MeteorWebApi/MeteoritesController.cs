using Microsoft.AspNetCore.Mvc;
using MediatR;

[ApiController]
[Route("api/[controller]")]
public class MeteoritesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeteoritesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("filter")]
    [ProducesResponseType(typeof(IEnumerable<MeteoriteGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFiltered([FromQuery] MeteoriteFilterDto filter)
    {
        var result = await _mediator.Send(new GetFilteredMeteoritesQuery(filter));
        return Ok(result);
    }
}