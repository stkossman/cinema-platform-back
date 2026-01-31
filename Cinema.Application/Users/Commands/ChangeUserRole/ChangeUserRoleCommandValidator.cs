using FluentValidation;

namespace Cinema.Application.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleName)
            .NotEmpty()
            .Must(r => r == "Admin" || r == "User")
            .WithMessage("Role must be either 'Admin' or 'User'.");
    }
}