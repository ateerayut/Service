using Service.Application.Orders;

namespace Service.UnitTests;

public class OrderValidatorTests
{
    [Fact]
    public void CreateOrderCommandValidator_Valid_ReturnsNoErrors()
    {
        var validator = new CreateOrderCommandValidator();
        var result = validator.Validate(new CreateOrderCommand(Guid.NewGuid()));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void AddOrderItemCommandValidator_Valid_ReturnsNoErrors()
    {
        var validator = new AddOrderItemCommandValidator();
        var result = validator.Validate(new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), 1, 10));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void AddOrderItemCommandValidator_InvalidQuantity_ReturnsErrors()
    {
        var validator = new AddOrderItemCommandValidator();
        var result = validator.Validate(new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), 0, 10));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ListOrdersQueryValidator_Valid_ReturnsNoErrors()
    {
        var validator = new ListOrdersQueryValidator();
        var result = validator.Validate(new ListOrdersQuery(1, 20, null));
        Assert.True(result.IsValid);
    }
}
