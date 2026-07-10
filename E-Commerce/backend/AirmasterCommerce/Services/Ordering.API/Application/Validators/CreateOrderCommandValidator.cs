using FluentValidation;
using Ordering.API.Application.DTOs;

namespace Ordering.API.Application.Validators
{
    public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("Orders must target a authenticated UserId reference.");
            RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Items).NotEmpty().WithMessage("An order execution list must register at least one target component.");
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty();
                item.RuleFor(i => i.Quantity).GreaterThan(0);
                item.RuleFor(i => i.UnitPrice).GreaterThan(0);
            });
        }
    }
}
