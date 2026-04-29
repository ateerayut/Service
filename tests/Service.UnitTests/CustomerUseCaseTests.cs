using Service.Application.Common;
using Service.Application.Customers;
using Service.Domain.Customers;

namespace Service.UnitTests;

public class CustomerUseCaseTests
{
    [Fact]
    public async Task CreateCustomerUseCase_ValidCommand_AddsCustomer()
    {
        var repo = new FakeCustomerRepository();
        var useCase = new CreateCustomerUseCase(repo, new CreateCustomerCommandValidator());

        var result = await useCase.Execute(
            new CreateCustomerCommand("John Doe"),
            CancellationToken.None);

        Assert.Null(result.Validation);
        Assert.Single(repo.Customers);
        Assert.Equal(result.Value, repo.Customers[0].Id);
    }

    [Fact]
    public async Task CreateCustomerUseCase_InvalidCommand_ReturnsValidationError()
    {
        var repo = new FakeCustomerRepository();
        var useCase = new CreateCustomerUseCase(repo, new CreateCustomerCommandValidator());

        var result = await useCase.Execute(
            new CreateCustomerCommand(""),
            CancellationToken.None);

        Assert.NotNull(result.Validation);
        Assert.Empty(repo.Customers);
    }

    [Fact]
    public async Task ListCustomersUseCase_ReturnsCustomers()
    {
        var repo = new FakeCustomerRepository();
        repo.Customers.Add(Customer.Create("John"));
        repo.Customers.Add(Customer.Create("Jane"));
        var useCase = new ListCustomersUseCase(repo, new ListCustomersQueryValidator());

        var result = await useCase.Execute(
            new ListCustomersQuery(1, 20, null),
            CancellationToken.None);

        Assert.Null(result.Validation);
        Assert.Equal(2, result.Value!.Items.Count);
        Assert.Equal(2, result.Value.TotalItems);
    }

    [Fact]
    public async Task ListCustomersUseCase_WithSearch_ReturnsMatching()
    {
        var repo = new FakeCustomerRepository();
        repo.Customers.Add(Customer.Create("John Doe"));
        repo.Customers.Add(Customer.Create("Jane Smith"));
        var useCase = new ListCustomersUseCase(repo, new ListCustomersQueryValidator());

        var result = await useCase.Execute(
            new ListCustomersQuery(1, 20, Search: "Jane"),
            CancellationToken.None);

        Assert.Null(result.Validation);
        Assert.Single(result.Value!.Items);
        Assert.Equal("Jane Smith", result.Value.Items[0].Name);
    }

    [Fact]
    public async Task GetCustomerByIdUseCase_Existing_ReturnsCustomer()
    {
        var repo = new FakeCustomerRepository();
        var customer = Customer.Create("John");
        repo.Customers.Add(customer);
        var useCase = new GetCustomerByIdUseCase(repo);

        var result = await useCase.Execute(customer.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Id);
    }

    [Fact]
    public async Task UpdateCustomerUseCase_Existing_UpdatesName()
    {
        var repo = new FakeCustomerRepository();
        var customer = Customer.Create("John");
        repo.Customers.Add(customer);
        var useCase = new UpdateCustomerUseCase(repo, new UpdateCustomerCommandValidator());

        var result = await useCase.Execute(
            new UpdateCustomerCommand(customer.Id, "John Smith"),
            CancellationToken.None);

        Assert.True(result.Value);
        Assert.Equal("John Smith", customer.Name);
    }

    [Fact]
    public async Task DeleteCustomerUseCase_Existing_RemovesCustomer()
    {
        var repo = new FakeCustomerRepository();
        var customer = Customer.Create("John");
        repo.Customers.Add(customer);
        var useCase = new DeleteCustomerUseCase(repo);

        var deleted = await useCase.Execute(customer.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Empty(repo.Customers);
    }

    [Fact]
    public async Task UpdateCustomerUseCase_Missing_ReturnsFalse()
    {
        var repo = new FakeCustomerRepository();
        var useCase = new UpdateCustomerUseCase(repo, new UpdateCustomerCommandValidator());

        var result = await useCase.Execute(
            new UpdateCustomerCommand(Guid.NewGuid(), "New Name"),
            CancellationToken.None);

        Assert.False(result.Value);
    }

    [Fact]
    public async Task UpdateCustomerUseCase_Invalid_ReturnsValidationError()
    {
        var repo = new FakeCustomerRepository();
        var customer = Customer.Create("John");
        repo.Customers.Add(customer);
        var useCase = new UpdateCustomerUseCase(repo, new UpdateCustomerCommandValidator());

        var result = await useCase.Execute(
            new UpdateCustomerCommand(customer.Id, ""),
            CancellationToken.None);

        Assert.NotNull(result.Validation);
        Assert.Equal("John", customer.Name);
    }

    [Fact]
    public async Task DeleteCustomerUseCase_Missing_ReturnsFalse()
    {
        var repo = new FakeCustomerRepository();
        var useCase = new DeleteCustomerUseCase(repo);

        var deleted = await useCase.Execute(Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
    }

    [Fact]
    public async Task ListCustomersUseCase_EmptyRepo_ReturnsEmptyPagedResult()
    {
        var repo = new FakeCustomerRepository();
        var useCase = new ListCustomersUseCase(repo, new ListCustomersQueryValidator());

        var result = await useCase.Execute(new ListCustomersQuery(1, 20, null), CancellationToken.None);

        Assert.Null(result.Validation);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalItems);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public List<Customer> Customers { get; } = [];

        public Task<PagedResult<CustomerDto>> List(ListCustomersQuery query, CancellationToken ct)
        {
            var customersQuery = Customers.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(query.Search))
                customersQuery = customersQuery.Where(c => c.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase));

            var total = customersQuery.Count();
            var items = customersQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new CustomerDto(c.Id, c.Name, c.CreateDate))
                .ToList();

            return Task.FromResult(new PagedResult<CustomerDto>(items, query.Page, query.PageSize, total));
        }

        public Task<CustomerDto?> GetById(Guid id, CancellationToken ct)
        {
            var c = Customers.SingleOrDefault(x => x.Id == id);
            return Task.FromResult(c == null ? null : new CustomerDto(c.Id, c.Name, c.CreateDate));
        }

        public Task<Customer?> GetEntityById(Guid id, CancellationToken ct) =>
            Task.FromResult(Customers.SingleOrDefault(x => x.Id == id));

        public Task Add(Customer customer, CancellationToken ct) { Customers.Add(customer); return Task.CompletedTask; }
        public Task Update(Customer customer, CancellationToken ct) => Task.CompletedTask;
        public Task Delete(Customer customer, CancellationToken ct) { Customers.Remove(customer); return Task.CompletedTask; }
    }
}
