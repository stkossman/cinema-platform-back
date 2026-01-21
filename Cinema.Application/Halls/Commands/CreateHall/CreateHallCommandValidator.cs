using FluentValidation;

namespace Cinema.Application.Halls.Commands.CreateHall;

public class CreateHallCommandValidator : AbstractValidator<CreateHallCommand>
{
    public CreateHallCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Rows).GreaterThan(0).LessThanOrEqualTo(20);
        RuleFor(x => x.SeatsPerRow).GreaterThan(0).LessThanOrEqualTo(30);
        RuleFor(x => x.SeatTypeId).NotEmpty();
    }
}