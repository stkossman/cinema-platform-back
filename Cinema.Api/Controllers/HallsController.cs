using Cinema.Application.Halls.Commands.CreateHall;
using Cinema.Application.Halls.Commands.DeleteHall;
using Cinema.Application.Halls.Commands.UpdateHall;
using Cinema.Application.Halls.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HallsController : ControllerBase
{
    private readonly IMediator _mediator;

    public HallsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    // GET: api/halls
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllHallsQuery());
        return Ok(result.Value);
    }

    // POST: api/halls
    [HttpPost]
    public async Task<IActionResult> Create(CreateHallCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { result.Error.Code, result.Error.Description });
        }
        return Ok(result.Value);
    }
    
    // PUT: api/halls/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHallCommand command)
    {
        if (id != command.HallId)
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

    // GET: api/halls/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetHallByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { result.Error.Code, result.Error.Description });
        }

        return Ok(result.Value);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteHallCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return NotFound(new { result.Error.Code, result.Error.Description });
        }

        return NoContent();
    }
}