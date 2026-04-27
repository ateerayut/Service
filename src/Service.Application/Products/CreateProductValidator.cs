using FluentValidation;

namespace Service.Application.Products;

public class CreateProductValidator
    : AbstractValidator<(string Name, decimal Price)>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0);
    }
}