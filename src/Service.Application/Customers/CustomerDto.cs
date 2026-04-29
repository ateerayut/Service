namespace Service.Application.Customers;

public record CustomerDto(
    Guid Id,
    string Name,
    DateTimeOffset CreateDate);
