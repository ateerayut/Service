namespace Service.Application.Orders;

public record CreateOrderCommand(
    Guid CustomerId);
