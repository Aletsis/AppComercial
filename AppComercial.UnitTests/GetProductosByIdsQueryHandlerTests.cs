using Xunit;
using Moq;
using AutoMapper;
using AppComercial.Application.Features.Productos;
using AppComercial.Application.DTOs;
using AppComercial.Application.Common.Interfaces;
using AppComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Api.Tests;

public class GetProductosByIdsQueryHandlerTests
{
    private static IContpaqiDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppComercial.Infrastructure.ContpaqiDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppComercial.Infrastructure.ContpaqiDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenIdsIsEmpty()
    {
        var mockMapper = new Mock<IMapper>();
        var db = CreateInMemoryDbContext("TestDb_Empty");
        var handler = new GetProductosByIdsQueryHandler(db, mockMapper.Object);

        var result = await handler.Handle(new GetProductosByIdsQuery(new List<int>()), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ReturnsMatchingProductos_WhenIdsExist()
    {
        var db = CreateInMemoryDbContext("TestDb_WithProducts");
        db.Productos.AddRange(
            new AdmProductos { CIDPRODUCTO = 10, CCODIGOPRODUCTO = "P010", CNOMBREPRODUCTO = "Prod Diez" },
            new AdmProductos { CIDPRODUCTO = 20, CCODIGOPRODUCTO = "P020", CNOMBREPRODUCTO = "Prod Veinte" },
            new AdmProductos { CIDPRODUCTO = 30, CCODIGOPRODUCTO = "P030", CNOMBREPRODUCTO = "Prod Treinta" }
        );
        await db.SaveChangesAsync();

        var mockMapper = new Mock<IMapper>();
        mockMapper.Setup(m => m.Map<IEnumerable<ProductoDto>>(It.IsAny<IEnumerable<AdmProductos>>()))
            .Returns<IEnumerable<AdmProductos>>(entities => entities.Select(e => new ProductoDto
            {
                Id = e.CIDPRODUCTO,
                Codigo = e.CCODIGOPRODUCTO ?? "",
                Nombre = e.CNOMBREPRODUCTO ?? ""
            }));

        var handler = new GetProductosByIdsQueryHandler(db, mockMapper.Object);
        var result = (await handler.Handle(new GetProductosByIdsQuery(new[] { 10, 30 }), CancellationToken.None)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Id == 10 && p.Codigo == "P010");
        Assert.Contains(result, p => p.Id == 30 && p.Codigo == "P030");
        Assert.DoesNotContain(result, p => p.Id == 20);
    }
}
