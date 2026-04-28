namespace Service.Api.Features.Products;

public record UpdateProductRequest(
    string Name,
    decimal Price
);
