using Service.Domain.Orders;

namespace Service.UnitTests;

public class OrderTests
{
    [Fact]
    public void Create_ValidCustomerId_ReturnsOrder()
    {
        var customerId = Guid.CreateVersion7();

        var order = Order.Create(customerId);

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal('7', order.Id.ToString()[14]);
        Assert.Equal(customerId, order.CustomerId);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void AddItem_ValidInput_AddsOrderItem()
    {
        var order = Order.Create(Guid.CreateVersion7());
        var productId = Guid.CreateVersion7();

        order.AddItem(productId, quantity: 2, unitPrice: 150);

        var item = Assert.Single(order.Items);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(150, item.UnitPrice);
        Assert.Equal(300, item.TotalPrice);
    }

    [Fact]
    public void AddItem_InvalidQuantity_ThrowsArgumentOutOfRangeException()
    {
        var order = Order.Create(Guid.CreateVersion7());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            order.AddItem(Guid.CreateVersion7(), quantity: 0, unitPrice: 150));
    }
}
