using Service.Domain.Products;

namespace Service.Domain.Orders;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Order? Order { get; private set; }
    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    private OrderItem() { }

    internal static OrderItem Create(
        Guid orderId,
        Guid productId,
        int quantity,
        decimal unitPrice)
    {
        return new OrderItem
        {
            Id = Guid.CreateVersion7(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}
