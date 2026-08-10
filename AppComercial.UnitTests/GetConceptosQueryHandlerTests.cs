using Xunit;
using Microsoft.EntityFrameworkCore;
using AppComercial.Application.Features.Conceptos;
using AppComercial.Domain.Entities;
using AppComercial.Infrastructure;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.UnitTests;

public class GetConceptosQueryHandlerTests
{
    private ContpaqiDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ContpaqiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ContpaqiDbContext(options);
        return context;
    }

    [Fact]
    public async Task Handle_ReturnsOnlyActiveConcepts_ByDefault()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        dbContext.Conceptos.AddRange(
            new AdmConceptos
            {
                CIDCONCEPTODOCUMENTO = 1,
                CCODIGOCONCEPTO = "FACT01",
                CNOMBRECONCEPTO = "Factura Activa",
                CIDDOCUMENTODE = 4,
                CESTATUS = 1
            },
            new AdmConceptos
            {
                CIDCONCEPTODOCUMENTO = 2,
                CCODIGOCONCEPTO = "FACT02",
                CNOMBRECONCEPTO = "Factura Inactiva",
                CIDDOCUMENTODE = 4,
                CESTATUS = 0
            },
            new AdmConceptos
            {
                CIDCONCEPTODOCUMENTO = 3,
                CCODIGOCONCEPTO = "NC01",
                CNOMBRECONCEPTO = "Nota Credito Activa",
                CIDDOCUMENTODE = 5,
                CESTATUS = 1
            }
        );
        await dbContext.SaveChangesAsync();

        var handler = new GetConceptosQueryHandler(dbContext);
        var query = new GetConceptosQuery { TipoDocumento = 4 };

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("FACT01", result[0].CCODIGOCONCEPTO);
        Assert.Equal(1, result[0].CESTATUS);
    }

    [Fact]
    public async Task Handle_ReturnsAllConcepts_WhenSoloActivosIsFalse()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        dbContext.Conceptos.AddRange(
            new AdmConceptos
            {
                CIDCONCEPTODOCUMENTO = 1,
                CCODIGOCONCEPTO = "FACT01",
                CNOMBRECONCEPTO = "Factura Activa",
                CIDDOCUMENTODE = 4,
                CESTATUS = 1
            },
            new AdmConceptos
            {
                CIDCONCEPTODOCUMENTO = 2,
                CCODIGOCONCEPTO = "FACT02",
                CNOMBRECONCEPTO = "Factura Inactiva",
                CIDDOCUMENTODE = 4,
                CESTATUS = 0
            }
        );
        await dbContext.SaveChangesAsync();

        var handler = new GetConceptosQueryHandler(dbContext);
        var query = new GetConceptosQuery { TipoDocumento = 4, SoloActivos = false };

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }
}
