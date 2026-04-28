using Service.Application.Products;

namespace Service.UnitTests;

public class ProductValidatorTests
{
    [Fact]
    public void CreateProductValidator_ValidCommand_IsValid()
    {
        var validator = new CreateProductValidator();

        var result = validator.Validate(new CreateProductCommand("Keyboard", 1200));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", 1200)]
    [InlineData("Keyboard", 0)]
    [InlineData("Keyboard", -1)]
    public void CreateProductValidator_InvalidCommand_ReturnsErrors(
        string name,
        decimal price)
    {
        var validator = new CreateProductValidator();

        var result = validator.Validate(new CreateProductCommand(name, price));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateProductValidator_EmptyId_ReturnsError()
    {
        var validator = new UpdateProductValidator();

        var result = validator.Validate(
            new UpdateProductCommand(Guid.Empty, "Keyboard", 1200));

        Assert.False(result.IsValid);
    }
}
