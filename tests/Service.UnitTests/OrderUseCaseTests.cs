using Service.Application.Common;
using Service.Application.Orders;
using Service.Domain.Orders;

namespace Service.UnitTests;

public class OrderUseCaseTests
{
    [Fact]
    public async Task CreateOrderUseCase_ValidCommand_AddsOrder()
    {
        var repo = new FakeOrderRepository();
        var useCase = new CreateOrderUseCase(repo, new CreateOrderCommandValidator());
        var customerId = Guid.NewGuid();

        var result = await useCase.Execute(
            new CreateOrderCommand(customerId),
            CancellationToken.None);

        Assert.Null(result.Validation);
        Assert.Single(repo.Orders);
        Assert.Equal(customerId, repo.Orders[0].CustomerId);
    }

    [Fact]
    public async Task ListOrdersUseCase_ReturnsOrders()
    {
        var repo = new FakeOrderRepository();
        var customerId = Guid.NewGuid();
        repo.Orders.Add(Order.Create(customerId));
        var useCase = new ListOrdersUseCase(repo, new ListOrdersQueryValidator());

        var result = await useCase.Execute(
            new ListOrdersQuery(1, 20, null),
            CancellationToken.None);

        Assert.Null(result.Validation);
        Assert.Single(result.Value!.Items);
        Assert.Equal(customerId, result.Value.Items[0].CustomerId);
    }

    [Fact]
    public async Task AddOrderItemUseCase_ValidCommand_AddsItem()
    {
        var repo = new FakeOrderRepository();
        var order = Order.Create(Guid.NewGuid());
        repo.Orders.Add(order);
        var useCase = new AddOrderItemUseCase(repo, new AddOrderItemCommandValidator());
        var productId = Guid.NewGuid();

        var result = await useCase.Execute(
            new AddOrderItemCommand(order.Id, productId, 5, 100),
            CancellationToken.None);

        Assert.True(result.Value);
        Assert.Single(order.Items);
        Assert.Equal(productId, order.Items.First().ProductId);
        Assert.Equal(5, order.Items.First().Quantity);
        Assert.Equal(500, order.Items.First().TotalPrice);
    }

    [Fact]
    public async Task AddOrderItemUseCase_InvalidQuantity_ReturnsValidationError()
    {
        var repo = new FakeOrderRepository();
        var order = Order.Create(Guid.NewGuid());
        repo.Orders.Add(order);
        var useCase = new AddOrderItemUseCase(repo, new AddOrderItemCommandValidator());

        var result = await useCase.Execute(
            new AddOrderItemCommand(order.Id, Guid.NewGuid(), 0, 100),
            CancellationToken.None);

        Assert.NotNull(result.Validation);
        Assert.Empty(order.Items);
    }

    [Fact]
    public async Task DeleteOrderUseCase_Existing_RemovesOrder()
    {
        var repo = new FakeOrderRepository();
        var order = Order.Create(Guid.NewGuid());
        repo.Orders.Add(order);
        var useCase = new DeleteOrderUseCase(repo);

        var deleted = await useCase.Execute(order.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Empty(repo.Orders);
    }

    [Fact]
    public async Task ListOrdersUseCase_ByCustomerId_ReturnsOnlyCustomerOrders()
    {
        var repo = new FakeOrderRepository();
        var customer1Id = Guid.NewGuid();
        var customer2Id = Guid.NewGuid();
        repo.Orders.Add(Order.Create(customer1Id));
        repo.Orders.Add(Order.Create(customer1Id));
        repo.Orders.Add(Order.Create(customer2Id));
        var useCase = new ListOrdersUseCase(repo, new ListOrdersQueryValidator());

        var result = await useCase.Execute(
            new ListOrdersQuery(1, 20, CustomerId: customer1Id),
            CancellationToken.None);

        Assert.Null(result.Validation);
        Assert.Equal(2, result.Value!.Items.Count);
        Assert.All(result.Value.Items, o => Assert.Equal(customer1Id, o.CustomerId));
    }

    [Fact]
    public async Task GetOrderByIdUseCase_Existing_ReturnsOrderWithItems()
    {
        var repo = new FakeOrderRepository();
        var order = Order.Create(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), 2, 50);
        repo.Orders.Add(order);
        var useCase = new GetOrderByIdUseCase(repo);

        var result = await useCase.Execute(order.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(100, result.Items[0].TotalPrice);
    }

    [Fact]
    public async Task AddOrderItemUseCase_MissingOrder_ReturnsFalse()
    {
        var repo = new FakeOrderRepository();
        var useCase = new AddOrderItemUseCase(repo, new AddOrderItemCommandValidator());

        var result = await useCase.Execute(
            new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), 1, 100),
            CancellationToken.None);

        Assert.False(result.Value);
    }

    [Fact]
    public async Task DeleteOrderUseCase_Missing_ReturnsFalse()
    {
        var repo = new FakeOrderRepository();
        var useCase = new DeleteOrderUseCase(repo);

        var deleted = await useCase.Execute(Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        public List<Order> Orders { get; } = [];

        public Task<PagedResult<OrderDto>> List(ListOrdersQuery query, CancellationToken ct)
        {
            var ordersQuery = Orders.AsEnumerable();
            if (query.CustomerId.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CustomerId == query.CustomerId.Value);

            var total = ordersQuery.Count();
            var items = ordersQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(ToDto)
                .ToList();

            return Task.FromResult(new PagedResult<OrderDto>(items, query.Page, query.PageSize, total));
        }

        public Task<OrderDto?> GetById(Guid id, CancellationToken ct)
        {
            var o = Orders.SingleOrDefault(x => x.Id == id);
            return Task.FromResult(o == null ? null : ToDto(o));
        }

        public Task<Order?> GetEntityById(Guid id, CancellationToken ct) =>
            Task.FromResult(Orders.SingleOrDefault(x => x.Id == id));

        public Task Add(Order order, CancellationToken ct) { Orders.Add(order); return Task.CompletedTask; }
        public Task Update(Order order, CancellationToken ct) => Task.CompletedTask;
        public Task Delete(Order order, CancellationToken ct) { Orders.Remove(order); return Task.CompletedTask; }

        private static OrderDto ToDto(Order o) =>
            new(o.Id, o.CustomerId, o.CreateDate, o.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList());
    }
}
