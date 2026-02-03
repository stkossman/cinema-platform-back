using FluentValidation;

namespace Cinema.Application.Orders.Commands.CreateOrder;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.SeatIds).NotEmpty().Must(ids => ids.Count <= 10)
            .WithMessage("You cannot purchase more than 10 tickets at once.");
        RuleFor(x => x.PaymentToken).NotEmpty();
    }
}