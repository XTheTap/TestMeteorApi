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
    [ProducesResponseType(typeof(IEnumerable<MeteoriteGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task GetFiltered([FromQuery] MeteoriteFilterDto filter, CancellationToken token)
{
    Response.StatusCode = StatusCodes.Status200OK;
    Response.ContentType = "application/json; charset=utf-8";

    await using var writer = new Utf8JsonWriter(Response.Body, new JsonWriterOptions { Indented = false });
    writer.WriteStartArray();

    await foreach (var item in _mediator.CreateStream(new GetFilteredMeteoritesQuery(filter), token))
    {
        JsonSerializer.Serialize(writer, item);
        await writer.FlushAsync(token);
    }

    writer.WriteEndArray();
    await writer.FlushAsync(token);
}
}