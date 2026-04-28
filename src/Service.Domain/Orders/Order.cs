using Service.Domain.Customers;

namespace Service.Domain.Orders;

public class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public DateTimeOffset CreateDate { get; private set; } = DateTimeOffset.UtcNow;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        return new Order
        {
            Id = Guid.CreateVersion7(),
            CustomerId = customerId,
            CreateDate = DateTimeOffset.UtcNow
        };
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        if (unitPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be greater than zero.");

        _items.Add(OrderItem.Create(Id, productId, quantity, unitPrice));
    }
}
