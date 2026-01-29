using Cinema.Application.Seats.Commands;
using Cinema.Application.Seats.Commands.UpdateSeat;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

public class SeatsController : ApiController
{
    // PUT: api/seats/{id}/type
    [HttpPut("{id:guid}/type")]
    public async Task<IActionResult> ChangeType(Guid id, [FromBody] ChangeSeatTypeCommand command)
    {
        if (id != command.SeatId)
        {
            return BadRequest("ID mismatch");
        }
        return HandleResult(await Mediator.Send(command));
    }
}