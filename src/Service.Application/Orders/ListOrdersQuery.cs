namespace Service.Application.Orders;

public record ListOrdersQuery(
    int Page,
    int PageSize,
    Guid? CustomerId);
