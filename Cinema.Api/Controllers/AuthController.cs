using Cinema.Application.Auth.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

public class AuthController : ApiController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsFailure) return HandleResult(result);
        
        return Ok(new { UserId = result.Value });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsFailure) return HandleResult(result);

        return Ok(new { Token = result.Value });
    }
}