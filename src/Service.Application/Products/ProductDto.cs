namespace Service.Application.Products;

public record ProductDto(
    Guid Id,
    string Name,
    decimal Price);
