namespace Service.Application.Products;

public record CreateProductCommand(
    string Name,
    decimal Price);
