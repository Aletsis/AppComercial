namespace AppComercial.Application.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? RFC { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; }
}