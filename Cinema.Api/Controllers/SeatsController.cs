using Cinema.Application.Seats.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeatsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SeatsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // PUT: api/seats/{id}/type
    [HttpPut("{id:guid}/type")]
    public async Task<IActionResult> ChangeType(Guid id, [FromBody] ChangeSeatTypeCommand command)
    {
        if (id != command.SeatId)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { result.Error.Code, result.Error.Description });
        }
        return NoContent();
    }
}