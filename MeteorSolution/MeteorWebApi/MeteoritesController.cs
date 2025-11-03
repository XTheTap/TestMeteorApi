using MediatR;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task GetFiltered([FromQuery] MeteoriteFilterDto filter, CancellationToken token)
    {
        Response.StatusCode = StatusCodes.Status200OK;
        Response.ContentType = "application/x-ndjson; charset=utf-8";

        await foreach (var item in _mediator.CreateStream(new GetFilteredMeteoritesQuery(filter), token))
        {
            await JsonSerializer.SerializeAsync(Response.Body, item, cancellationToken: token);
            await Response.Body.WriteAsync(new byte[] { (byte)'\n' }, token);
            await Response.Body.FlushAsync(token);
        }
    }
}