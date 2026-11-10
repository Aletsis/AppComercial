using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AppComercial.Application.Features.Auth;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using AppComercial.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AppComercial.Api.Tests;

public class LoginCommandHandlerTests
{
    private static RepositorioAdminDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<RepositorioAdminDbContext>()
            .UseInMemoryDatabase(databaseName: $"RepositorioAdminPAQ_{Guid.NewGuid()}")
            .Options;

        return new RepositorioAdminDbContext(options);
    }

    [Fact]
    public async Task Handle_ThrowsArgumentException_WhenUsernameIsEmpty()
    {
        using var dbContext = CreateInMemoryDbContext();
        var mockSdk = new Mock<IContpaqiSdk>();
        var mockConfig = new Mock<IConfiguration>();

        var handler = new LoginCommandHandler(dbContext, mockSdk.Object, mockConfig.Object);
        var command = new LoginCommand { Username = "   ", Password = "" };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("El nombre de usuario es requerido", ex.Message);
    }

    [Fact]
    public async Task Handle_ThrowsKeyNotFoundException_WhenUserDoesNotExist()
    {
        using var dbContext = CreateInMemoryDbContext();
        var mockSdk = new Mock<IContpaqiSdk>();
        var mockConfig = new Mock<IConfiguration>();

        var handler = new LoginCommandHandler(dbContext, mockSdk.Object, mockConfig.Object);
        var command = new LoginCommand { Username = "INEXISTENTE", Password = "" };

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("El usuario 'INEXISTENTE' no existe", ex.Message);
    }

    [Fact]
    public async Task Handle_ThrowsKeyNotFoundException_WhenUserBelongsToDifferentSystem()
    {
        using var dbContext = CreateInMemoryDbContext();
        dbContext.Usuarios.Add(new CacUsuario
        {
            IdAutoInc = 1,
            IdUsuario = 10,
            IdSistema = "1", // Nómina o Contabilidad, no Comercial ("5")
            Clave = "SUPERVISOR",
            Nombre = "Supervisor Nóminas"
        });
        await dbContext.SaveChangesAsync();

        var mockSdk = new Mock<IContpaqiSdk>();
        var mockConfig = new Mock<IConfiguration>();

        var handler = new LoginCommandHandler(dbContext, mockSdk.Object, mockConfig.Object);
        var command = new LoginCommand { Username = "SUPERVISOR", Password = "" };

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("no existe en la base de datos del sistema", ex.Message);
    }

    [Theory]
    [InlineData("SUPERVISOR")]
    [InlineData("supervisor")]
    [InlineData("  SUPERVISOR  ")]
    public async Task Handle_ReturnsJwtToken_WhenUserExists(string inputUsername)
    {
        using var dbContext = CreateInMemoryDbContext();
        dbContext.Usuarios.Add(new CacUsuario
        {
            IdAutoInc = 1,
            IdUsuario = 100,
            IdSistema = "5",
            Clave = "SUPERVISOR",
            Nombre = "Supervisor General",
            IdPerfil = 1
        });
        await dbContext.SaveChangesAsync();

        var mockSdk = new Mock<IContpaqiSdk>();
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["JwtSettings:SecretKey"])
            .Returns("una_clave_muy_secreta_para_desarrollo_de_al_menos_256_bits_12345678");

        var handler = new LoginCommandHandler(dbContext, mockSdk.Object, mockConfig.Object);
        var command = new LoginCommand { Username = inputUsername, Password = "" };

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal("SUPERVISOR", response.Username);
        Assert.Equal("Supervisor General", response.Nombre);
        Assert.Equal(1, response.Nivel);

        // Validar claims dentro del JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(response.Token);
        var nameClaim = jwt.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == ClaimTypes.Name);
        var nameIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == ClaimTypes.NameIdentifier);
        var nivelClaim = jwt.Claims.FirstOrDefault(c => c.Type == "nivel");

        Assert.NotNull(nameClaim);
        Assert.Equal("SUPERVISOR", nameClaim.Value);
        Assert.NotNull(nameIdClaim);
        Assert.Equal("100", nameIdClaim.Value);
        Assert.NotNull(nivelClaim);
        Assert.Equal("1", nivelClaim.Value);
    }
}
