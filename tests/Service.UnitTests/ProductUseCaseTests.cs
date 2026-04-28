using Service.Application.Products;
using Service.Domain.Products;

namespace Service.UnitTests;

public class ProductUseCaseTests
{
    [Fact]
    public async Task CreateProductUseCase_ValidCommand_AddsProduct()
    {
        var repo = new FakeProductRepository();
        var useCase = new CreateProductUseCase(repo, new CreateProductValidator());

        var result = await useCase.Execute(
            new CreateProductCommand("Keyboard", 1200),
            CancellationToken.None);

        Assert.Null(result.Validation);
        Assert.Single(repo.Products);
        Assert.Equal(result.Value, repo.Products[0].Id);
    }

    [Fact]
    public async Task CreateProductUseCase_InvalidCommand_ReturnsValidationError()
    {
        var repo = new FakeProductRepository();
        var useCase = new CreateProductUseCase(repo, new CreateProductValidator());

        var result = await useCase.Execute(
            new CreateProductCommand("", 0),
            CancellationToken.None);

        Assert.NotNull(result.Validation);
        Assert.Empty(repo.Products);
    }

    [Fact]
    public async Task ListProductsUseCase_ReturnsProducts()
    {
        var repo = new FakeProductRepository();
        repo.Products.Add(Product.Create("Keyboard", 1200));
        repo.Products.Add(Product.Create("Mouse", 500));
        var useCase = new ListProductsUseCase(repo);

        var products = await useCase.Execute(CancellationToken.None);

        Assert.Equal(2, products.Count);
    }

    [Fact]
    public async Task GetProductByIdUseCase_MissingProduct_ReturnsNull()
    {
        var repo = new FakeProductRepository();
        var useCase = new GetProductByIdUseCase(repo);

        var product = await useCase.Execute(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(product);
    }

    [Fact]
    public async Task UpdateProductUseCase_ExistingProduct_UpdatesProduct()
    {
        var repo = new FakeProductRepository();
        var product = Product.Create("Keyboard", 1200);
        repo.Products.Add(product);
        var useCase = new UpdateProductUseCase(repo, new UpdateProductValidator());

        var result = await useCase.Execute(
            new UpdateProductCommand(product.Id, "Mouse", 500),
            CancellationToken.None);

        Assert.True(result.Value);
        Assert.Equal("Mouse", product.Name);
        Assert.Equal(500, product.Price);
    }

    [Fact]
    public async Task UpdateProductUseCase_MissingProduct_ReturnsFalse()
    {
        var repo = new FakeProductRepository();
        var useCase = new UpdateProductUseCase(repo, new UpdateProductValidator());

        var result = await useCase.Execute(
            new UpdateProductCommand(Guid.NewGuid(), "Mouse", 500),
            CancellationToken.None);

        Assert.False(result.Value);
    }

    [Fact]
    public async Task UpdateProductUseCase_InvalidCommand_DoesNotUpdate()
    {
        var repo = new FakeProductRepository();
        var product = Product.Create("Keyboard", 1200);
        repo.Products.Add(product);
        var useCase = new UpdateProductUseCase(repo, new UpdateProductValidator());

        var result = await useCase.Execute(
            new UpdateProductCommand(product.Id, "", 0),
            CancellationToken.None);

        Assert.NotNull(result.Validation);
        Assert.Equal("Keyboard", product.Name);
        Assert.Equal(1200, product.Price);
    }

    [Fact]
    public async Task DeleteProductUseCase_ExistingProduct_RemovesProduct()
    {
        var repo = new FakeProductRepository();
        var product = Product.Create("Keyboard", 1200);
        repo.Products.Add(product);
        var useCase = new DeleteProductUseCase(repo);

        var deleted = await useCase.Execute(product.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Empty(repo.Products);
    }

    [Fact]
    public async Task DeleteProductUseCase_MissingProduct_ReturnsFalse()
    {
        var repo = new FakeProductRepository();
        var useCase = new DeleteProductUseCase(repo);

        var deleted = await useCase.Execute(Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public List<Product> Products { get; } = [];

        public Task<IReadOnlyList<ProductDto>> List(CancellationToken ct)
        {
            IReadOnlyList<ProductDto> products = Products
                .Select(ToDto)
                .ToList();

            return Task.FromResult(products);
        }

        public Task<ProductDto?> GetById(Guid id, CancellationToken ct)
        {
            return Task.FromResult(
                Products
                    .Where(product => product.Id == id)
                    .Select(ToDto)
                    .SingleOrDefault());
        }

        public Task<Product?> GetEntityById(Guid id, CancellationToken ct)
        {
            return Task.FromResult(
                Products.SingleOrDefault(product => product.Id == id));
        }

        public Task Add(Product product, CancellationToken ct)
        {
            Products.Add(product);
            return Task.CompletedTask;
        }

        public Task Update(Product product, CancellationToken ct) =>
            Task.CompletedTask;

        public Task Delete(Product product, CancellationToken ct)
        {
            Products.Remove(product);
            return Task.CompletedTask;
        }

        private static ProductDto ToDto(Product product) =>
            new(product.Id, product.Name, product.Price);
    }
}
