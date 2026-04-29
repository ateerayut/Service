using Microsoft.EntityFrameworkCore;
using Service.Application.Common;
using Service.Application.Customers;
using Service.Domain.Customers;
using Service.Infrastructure.Persistence;

namespace Service.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<CustomerDto>> List(
        ListCustomersQuery query,
        CancellationToken ct)
    {
        var customersQuery = _db.Customers
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            customersQuery = customersQuery
                .Where(customer => customer.Name.Contains(search));
        }

        var totalItems = await customersQuery.CountAsync(ct);

        var items = await customersQuery
            .OrderBy(customer => customer.Name)
            .ThenBy(customer => customer.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(customer => new CustomerDto(
                customer.Id,
                customer.Name,
                customer.CreateDate))
            .ToListAsync(ct);

        return new PagedResult<CustomerDto>(
            items,
            query.Page,
            query.PageSize,
            totalItems);
    }

    public async Task<CustomerDto?> GetById(Guid id, CancellationToken ct)
    {
        return await _db.Customers
            .AsNoTracking()
            .Where(customer => customer.Id == id)
            .Select(customer => new CustomerDto(
                customer.Id,
                customer.Name,
                customer.CreateDate))
            .SingleOrDefaultAsync(ct);
    }

    public Task<Customer?> GetEntityById(Guid id, CancellationToken ct)
    {
        return _db.Customers
            .SingleOrDefaultAsync(customer => customer.Id == id, ct);
    }

    public async Task Add(Customer customer, CancellationToken ct)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
    }

    public Task Update(Customer customer, CancellationToken ct)
    {
        _db.Customers.Update(customer);
        return _db.SaveChangesAsync(ct);
    }

    public Task Delete(Customer customer, CancellationToken ct)
    {
        _db.Customers.Remove(customer);
        return _db.SaveChangesAsync(ct);
    }
}
