using Service.Application.Customers;

namespace Service.Api.Features.Customers;

public record CustomerResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreateDate)
{
    public static CustomerResponse FromDto(CustomerDto customer) =>
        new(customer.Id, customer.Name, customer.CreateDate);
}
