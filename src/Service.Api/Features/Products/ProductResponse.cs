using Service.Application.Products;

namespace Service.Api.Features.Products;

public record ProductResponse(
    Guid Id,
    string Name,
    decimal Price)
{
    public static ProductResponse FromDto(ProductDto product) =>
        new(product.Id, product.Name, product.Price);
}
