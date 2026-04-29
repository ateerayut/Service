using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Application;
using Service.Application.Common;
using Service.Application.Products;
using Service.Application.Customers;
using Service.Application.Orders;
using Service.Domain.Products;
using Service.Infrastructure;

namespace Service.IntegrationTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersProductUseCases()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IProductRepository, FakeProductRepository>();
        services.AddApplication();

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<CreateProductUseCase>());
        Assert.NotNull(provider.GetService<ListProductsUseCase>());
        Assert.NotNull(provider.GetService<GetProductByIdUseCase>());
        Assert.NotNull(provider.GetService<UpdateProductUseCase>());
        Assert.NotNull(provider.GetService<DeleteProductUseCase>());
    }

    [Fact]
    public void AddApplication_RegistersCustomerUseCases()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ICustomerRepository, FakeCustomerRepository>();
        services.AddApplication();

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<CreateCustomerUseCase>());
        Assert.NotNull(provider.GetService<ListCustomersUseCase>());
        Assert.NotNull(provider.GetService<GetCustomerByIdUseCase>());
        Assert.NotNull(provider.GetService<UpdateCustomerUseCase>());
        Assert.NotNull(provider.GetService<DeleteCustomerUseCase>());
    }

    [Fact]
    public void AddApplication_RegistersOrderUseCases()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IOrderRepository, FakeOrderRepository>();
        services.AddApplication();

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<CreateOrderUseCase>());
        Assert.NotNull(provider.GetService<ListOrdersUseCase>());
        Assert.NotNull(provider.GetService<GetOrderByIdUseCase>());
        Assert.NotNull(provider.GetService<AddOrderItemUseCase>());
        Assert.NotNull(provider.GetService<DeleteOrderUseCase>());
    }

    [Fact]
    public void AddInfrastructure_RegistersRepositories()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=localhost;Port=5432;Database=ServiceDb;Username=postgres;Password=postgres"
            })
            .Build();

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IProductRepository>());
        Assert.NotNull(provider.GetService<ICustomerRepository>());
        Assert.NotNull(provider.GetService<IOrderRepository>());
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Task<PagedResult<ProductDto>> List(
            ListProductsQuery query,
            CancellationToken ct) =>
            Task.FromResult(
                new PagedResult<ProductDto>(
                    [],
                    query.Page,
                    query.PageSize,
                    TotalItems: 0));

        public Task<ProductDto?> GetById(Guid id, CancellationToken ct) =>
            Task.FromResult<ProductDto?>(null);

        public Task<Product?> GetEntityById(Guid id, CancellationToken ct) =>
            Task.FromResult<Product?>(null);

        public Task Add(Product product, CancellationToken ct) =>
            Task.CompletedTask;

        public Task Update(Product product, CancellationToken ct) =>
            Task.CompletedTask;

        public Task Delete(Product product, CancellationToken ct) =>
            Task.CompletedTask;
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public Task<PagedResult<CustomerDto>> List(ListCustomersQuery query, CancellationToken ct) => Task.FromResult(new PagedResult<CustomerDto>([], query.Page, query.PageSize, 0));
        public Task<CustomerDto?> GetById(Guid id, CancellationToken ct) => Task.FromResult<CustomerDto?>(null);
        public Task<Service.Domain.Customers.Customer?> GetEntityById(Guid id, CancellationToken ct) => Task.FromResult<Service.Domain.Customers.Customer?>(null);
        public Task Add(Service.Domain.Customers.Customer customer, CancellationToken ct) => Task.CompletedTask;
        public Task Update(Service.Domain.Customers.Customer customer, CancellationToken ct) => Task.CompletedTask;
        public Task Delete(Service.Domain.Customers.Customer customer, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        public Task<PagedResult<OrderDto>> List(ListOrdersQuery query, CancellationToken ct) => Task.FromResult(new PagedResult<OrderDto>([], query.Page, query.PageSize, 0));
        public Task<OrderDto?> GetById(Guid id, CancellationToken ct) => Task.FromResult<OrderDto?>(null);
        public Task<Service.Domain.Orders.Order?> GetEntityById(Guid id, CancellationToken ct) => Task.FromResult<Service.Domain.Orders.Order?>(null);
        public Task Add(Service.Domain.Orders.Order order, CancellationToken ct) => Task.CompletedTask;
        public Task Update(Service.Domain.Orders.Order order, CancellationToken ct) => Task.CompletedTask;
        public Task Delete(Service.Domain.Orders.Order order, CancellationToken ct) => Task.CompletedTask;
    }
}
