namespace Service.Application.Customers;

public record UpdateCustomerCommand(
    Guid Id,
    string Name);
