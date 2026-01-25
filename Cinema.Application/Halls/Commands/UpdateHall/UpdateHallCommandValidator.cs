using FluentValidation;

namespace Cinema.Application.Halls.Commands.UpdateHall;

public class UpdateHallCommandValidator : AbstractValidator<UpdateHallCommand>
{
    public UpdateHallCommandValidator()
    {
        RuleFor(x => x.HallId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}