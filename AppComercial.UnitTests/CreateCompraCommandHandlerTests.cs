using Xunit;
using Moq;
using AppComercial.Application.Features.Compras;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using AppComercial.Application.Common.Interfaces;
using AppComercial.Application.Common.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.UnitTests;

public class CreateCompraCommandHandlerTests
{
    private readonly Mock<IContpaqiSdk> _sdkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IConceptoDocumentoValidator> _conceptoValidatorMock;

    public CreateCompraCommandHandlerTests()
    {
        _sdkMock = new Mock<IContpaqiSdk>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _conceptoValidatorMock = new Mock<IConceptoDocumentoValidator>();
    }

    [Fact]
    public async Task Handle_SetsTextoExtra3_WithExplicitUsuario()
    {
        // Arrange
        int docId = 999;
        _sdkMock.Setup(s => s.CrearDocumentoAsync(It.IsAny<tDocumento>()))
            .ReturnsAsync(docId);
        _sdkMock.Setup(s => s.CrearMovimientoAsync(docId, It.IsAny<tMovimiento>()))
            .ReturnsAsync(1);
        _sdkMock.Setup(s => s.LeerDatoDocumentoPorIdAsync(docId, "CFOLIO"))
            .ReturnsAsync("10");
        _sdkMock.Setup(s => s.LeerDatoDocumentoPorIdAsync(docId, "CSERIEDOCUMENTO"))
            .ReturnsAsync("A");

        Dictionary<string, string>? updatedFields = null;
        _sdkMock.Setup(s => s.ActualizarDocumentoPorIdAsync(docId, It.IsAny<Dictionary<string, string>>()))
            .Callback<int, Dictionary<string, string>>((id, dict) => updatedFields = dict)
            .ReturnsAsync(1);

        var handler = new CreateCompraCommandHandler(
            _sdkMock.Object,
            _currentUserServiceMock.Object,
            _conceptoValidatorMock.Object);

        var command = new CreateCompraCommand
        {
            CodigoConcepto = "COMP",
            CodigoProveedor = "PROV01",
            CodigoAlmacen = "ALM01",
            Usuario = "juan.perez",
            Partidas = new List<CompraPartida>
            {
                new() { CodigoProducto = "PROD01", Unidades = 5, PrecioUnitario = 100 }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(docId, result.IdDocumento);
        Assert.NotNull(updatedFields);
        Assert.True(updatedFields.ContainsKey("CTEXTOEXTRA3"));
        Assert.Equal("juan.perez", updatedFields["CTEXTOEXTRA3"]);
        Assert.Equal("juan.perez", updatedFields["CTEXTOEXTRA2"]);
    }

    [Fact]
    public async Task Handle_SetsTextoExtra3_FromCurrentUserService_WhenUsuarioNotProvided()
    {
        // Arrange
        int docId = 888;
        _sdkMock.Setup(s => s.CrearDocumentoAsync(It.IsAny<tDocumento>()))
            .ReturnsAsync(docId);
        _sdkMock.Setup(s => s.CrearMovimientoAsync(docId, It.IsAny<tMovimiento>()))
            .ReturnsAsync(1);
        _sdkMock.Setup(s => s.LeerDatoDocumentoPorIdAsync(docId, "CFOLIO"))
            .ReturnsAsync("11");
        _sdkMock.Setup(s => s.LeerDatoDocumentoPorIdAsync(docId, "CSERIEDOCUMENTO"))
            .ReturnsAsync("A");

        _currentUserServiceMock.Setup(u => u.GetCurrentUsuario())
            .Returns("admin.blanquita");

        Dictionary<string, string>? updatedFields = null;
        _sdkMock.Setup(s => s.ActualizarDocumentoPorIdAsync(docId, It.IsAny<Dictionary<string, string>>()))
            .Callback<int, Dictionary<string, string>>((id, dict) => updatedFields = dict)
            .ReturnsAsync(1);

        var handler = new CreateCompraCommandHandler(
            _sdkMock.Object,
            _currentUserServiceMock.Object,
            _conceptoValidatorMock.Object);

        var command = new CreateCompraCommand
        {
            CodigoConcepto = "COMP",
            CodigoProveedor = "PROV01",
            CodigoAlmacen = "ALM01",
            Partidas = new List<CompraPartida>
            {
                new() { CodigoProducto = "PROD01", Unidades = 1, PrecioUnitario = 50 }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(updatedFields);
        Assert.Equal("admin.blanquita", updatedFields["CTEXTOEXTRA3"]);
    }
}
