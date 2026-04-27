namespace Service.Domain.Products;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    private Product() { }

    public static Product Create(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("Name required");

        if (price <= 0)
            throw new Exception("Invalid price");

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price
        };
    }
}