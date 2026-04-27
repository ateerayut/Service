namespace Service.Api.Features.Products;

public record CreateProductRequest(
    string Name,
    decimal Price
);