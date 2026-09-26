using Xunit;
using Moq;
using AutoMapper;
using AppComercial.Application.Features.Productos;
using AppComercial.Application.DTOs;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace AppComercial.Api.Tests;

public class GetProductosQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsProductosFromRepository()
    {
        // Arrange
        var mockRepo = new Mock<IProductoRepository>();
        var mockMapper = new Mock<IMapper>();
        
        var expectedProductos = new List<AdmProductos>
        {
            new AdmProductos { CIDPRODUCTO = 1, CCODIGOPRODUCTO = "P001", CNOMBREPRODUCTO = "Producto 1" }
        };
        
        var dtoResult = new List<ProductoDto>
        {
            new ProductoDto { Id = 1, Codigo = "P001", Nombre = "Producto 1" }
        };

        mockRepo.Setup(r => r.GetByFiltersAsync(null, null)).ReturnsAsync(expectedProductos);
        
        mockMapper.Setup(m => m.Map<IEnumerable<ProductoDto>>(It.IsAny<IEnumerable<AdmProductos>>()))
                  .Returns(dtoResult);

        var handler = new GetProductosQueryHandler(mockRepo.Object, mockMapper.Object);
        var query = new GetProductosQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Producto 1", result.Items.First().Nombre);
        mockRepo.Verify(r => r.GetByFiltersAsync(null, null), Times.Once);
    }
}