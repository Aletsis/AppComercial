namespace AppComercial.Api.Domain.Entities;

public interface ICliente
{
    int Id { get; }
    string Codigo { get; }
    string RazonSocial { get; }
    string? RFC { get; }
    string? Email { get; }
    bool Activo { get; }
}