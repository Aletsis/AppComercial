using Xunit;
using Moq;
using AppComercial.Api.Features.Productos;
using AppComercial.Api.Infrastructure.Repositories;
using AppComercial.Api.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Api.Tests;

public class GetProductosQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsProductosFromRepository()
    {
        // Arrange
        var mockRepo = new Mock<IProductoRepository>();
        var expectedProductos = new List<AdmProductos>
        {
            new AdmProductos { CIDPRODUCTO = 1, CCODIGOPRODUCTO = "P001", CNOMBREPRODUCTO = "Producto 1" }
        };
        mockRepo.Setup(r => r.GetByFiltersAsync(null, null)).ReturnsAsync(expectedProductos);

        var handler = new GetProductosQueryHandler(mockRepo.Object);
        var query = new GetProductosQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedProductos, result);
        mockRepo.Verify(r => r.GetByFiltersAsync(null, null), Times.Once);
    }
}