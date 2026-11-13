namespace AppComercial.Application.DTOs;

public class CrearDomicilioDto
{
    public string Calle { get; set; } = string.Empty;
    public string NumeroExterior { get; set; } = string.Empty;
    public string? NumeroInterior { get; set; }
    public string Colonia { get; set; } = string.Empty;
    public string? Municipio { get; set; }
    public string? Ciudad { get; set; }
    public string? Estado { get; set; }
    public string Pais { get; set; } = "México";
    public string CodigoPostal { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
}
