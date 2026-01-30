using Cinema.Domain.Shared;
using MediatR;
using Cinema.Application.Common.Interfaces;

namespace Cinema.Application.Auth.Commands;

public record LoginUserCommand(string Email, string Password) : IRequest<Result<string>>;

public class LoginUserCommandHandler(IIdentityService identityService) 
    : IRequestHandler<LoginUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginUserCommand request, CancellationToken ct)
    {
        return await identityService.LoginAsync(request.Email, request.Password);
    }
}