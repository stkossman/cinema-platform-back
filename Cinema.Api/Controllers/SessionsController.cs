using Cinema.Application.Sessions.Commands.CreateSession;
using Cinema.Application.Sessions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // POST: api/sessions
    [HttpPost]
    public async Task<IActionResult> Create(CreateSessionCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { code = result.Error.Code, message = result.Error.Description });
        }
        return CreatedAtAction(nameof(GetByDate), new { date = DateTime.UtcNow }, result.Value);
    }

    // GET: api/sessions?date=2026-01-22
    [HttpGet]
    public async Task<IActionResult> GetByDate([FromQuery] DateTime date)
    {
        var query = new GetSessionsByDateQuery(date); 
        var result = await _mediator.Send(query);

        return Ok(result.Value);
    }
}