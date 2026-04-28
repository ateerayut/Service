using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Application;
using Service.Application.Common;
using Service.Application.Products;
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
    public void AddInfrastructure_RegistersProductRepository()
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
}
