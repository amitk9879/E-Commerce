using Catalog.API.Application.DTOs;
using FluentValidation;

namespace Catalog.API.Application.Validators
{
    public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150).WithMessage("Product name cannot exceed 150 characters.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price metric must be positive and greater than zero.");
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).WithMessage("Initial stock inventories cannot be a negative value.");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Products must explicitly target a valid Category Id boundary.");
        }
    }
}
