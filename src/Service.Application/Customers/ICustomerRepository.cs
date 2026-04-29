using Service.Application.Common;
using Service.Domain.Customers;

namespace Service.Application.Customers;

public interface ICustomerRepository
{
    Task<PagedResult<CustomerDto>> List(ListCustomersQuery query, CancellationToken ct);
    Task<CustomerDto?> GetById(Guid id, CancellationToken ct);
    Task<Customer?> GetEntityById(Guid id, CancellationToken ct);
    Task Add(Customer customer, CancellationToken ct);
    Task Update(Customer customer, CancellationToken ct);
    Task Delete(Customer customer, CancellationToken ct);
}
