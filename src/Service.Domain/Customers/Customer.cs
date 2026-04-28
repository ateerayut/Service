namespace Service.Domain.Customers;

public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTimeOffset CreateDate { get; private set; } = DateTimeOffset.UtcNow;

    private Customer() { }

    public static Customer Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new Customer
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            CreateDate = DateTimeOffset.UtcNow
        };
    }
}
