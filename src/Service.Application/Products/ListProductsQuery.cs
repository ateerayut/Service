namespace Service.Application.Products;

public record ListProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null);
