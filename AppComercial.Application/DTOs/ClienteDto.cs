namespace AppComercial.Application.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? DenominacionComercial { get; set; }
    public string? RFC { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? RegimenFiscal { get; set; }
    public string? UsoCFDI { get; set; }
    public int TipoCliente { get; set; } // 1 = Cliente, 2 = Cliente/Proveedor
    public int ListaPrecio { get; set; }
    public double LimiteCredito { get; set; }
    public int DiasCredito { get; set; }
    public bool Activo { get; set; }
}
