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
    [InlineData("   ", 1200)]
    [InlineData("K", 1200)]
    [InlineData("Keyboard", 0)]
    [InlineData("Keyboard", -1)]
    [InlineData("This name is intentionally made very long to exceed the maximum allowed length of two hundred characters. It needs to be quite substantial to ensure that the validation logic correctly identifies it as an invalid input. We are adding more and more text here to make absolutely sure it crosses the 200 mark without any doubt.", 1200)]
    public void CreateProductValidator_InvalidCommand_ReturnsErrors(
        string name,
        decimal price)
    {
        var validator = new CreateProductValidator();

        var result = validator.Validate(new CreateProductCommand(name, price));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ListProductsValidator_InvalidPaging_ReturnsErrors()
    {
        var validator = new ListProductsValidator();

        var result = validator.Validate(new ListProductsQuery(Page: 0, PageSize: 101));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateProductValidator_ValidCommand_IsValid()
    {
        var validator = new UpdateProductValidator();

        var result = validator.Validate(
            new UpdateProductCommand(Guid.NewGuid(), "Valid Name", 100));

        Assert.True(result.IsValid);
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
