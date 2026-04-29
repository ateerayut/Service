namespace Service.Application.Customers;

public record ListCustomersQuery(
    int Page,
    int PageSize,
    string? Search);
