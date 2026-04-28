using Service.Domain.Products;

namespace Service.UnitTests;

public class ProductTests
{
    [Fact]
    public void Create_ValidInput_ReturnsProduct()
    {
        var product = Product.Create("Keyboard", 1200);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Keyboard", product.Name);
        Assert.Equal(1200, product.Price);
    }

    [Fact]
    public void Update_ValidInput_ChangesProduct()
    {
        var product = Product.Create("Keyboard", 1200);

        product.Update("Mouse", 500);

        Assert.Equal("Mouse", product.Name);
        Assert.Equal(500, product.Price);
    }
}
