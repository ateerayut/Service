using FluentValidation;

namespace Service.Application.Products;

public class CreateProductValidator
    : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0);
    }
}
