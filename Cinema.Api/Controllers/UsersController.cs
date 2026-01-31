using Cinema.Application.Users.Commands.ChangeUserRole;
using Cinema.Application.Users.Queries;
using Cinema.Application.Users.Queries.GetAllUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllUsersQuery());
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleRequest request)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == id.ToString())
        {
            return BadRequest("You cannot change your role to yourself.");
        }

        var command = new ChangeUserRoleCommand(id, request.RoleName);
        return HandleResult(await Mediator.Send(command));
    }
}

public record ChangeRoleRequest(string RoleName);