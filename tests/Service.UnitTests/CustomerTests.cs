using Service.Domain.Customers;

namespace Service.UnitTests;

public class CustomerTests
{
    [Fact]
    public void Create_ValidInput_ReturnsCustomerWithCreateDate()
    {
        var beforeCreate = DateTimeOffset.UtcNow;

        var customer = Customer.Create("Alice");

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal('7', customer.Id.ToString()[14]);
        Assert.Equal("Alice", customer.Name);
        Assert.True(customer.CreateDate >= beforeCreate);
        Assert.True(customer.CreateDate <= DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_ThrowsArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            Customer.Create(name));
    }
}
