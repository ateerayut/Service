namespace Service.Application.Products;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    decimal Price);
