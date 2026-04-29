using Service.Application.Customers;

namespace Service.UnitTests;

public class CustomerValidatorTests
{
    [Fact]
    public void CreateCustomerCommandValidator_Valid_ReturnsNoErrors()
    {
        var validator = new CreateCustomerCommandValidator();
        var result = validator.Validate(new CreateCustomerCommand("John Doe"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateCustomerCommandValidator_EmptyName_ReturnsErrors()
    {
        var validator = new CreateCustomerCommandValidator();
        var result = validator.Validate(new CreateCustomerCommand(""));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ListCustomersQueryValidator_Valid_ReturnsNoErrors()
    {
        var validator = new ListCustomersQueryValidator();
        var result = validator.Validate(new ListCustomersQuery(1, 20, null));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ListCustomersQueryValidator_InvalidPaging_ReturnsErrors()
    {
        var validator = new ListCustomersQueryValidator();
        var result = validator.Validate(new ListCustomersQuery(0, 101, null));
        Assert.False(result.IsValid);
    }
}
