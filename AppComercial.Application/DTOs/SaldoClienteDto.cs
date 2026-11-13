using System;
using System.Collections.Generic;

namespace AppComercial.Application.DTOs;

public class DocumentoSaldoDto
{
    public int DocumentoId { get; set; }
    public string Serie { get; set; } = string.Empty;
    public double Folio { get; set; }
    public DateTime? Fecha { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public double Total { get; set; }
    public double SaldoPendiente { get; set; }
    public int DiasVencido { get; set; }
    public bool Vencido => DiasVencido > 0;
}

public class SaldoClienteDto
{
    public int ClienteId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? RFC { get; set; }
    public double LimiteCredito { get; set; }
    public int DiasCredito { get; set; }
    public double SaldoActual { get; set; }
    public double SaldoVencido { get; set; }
    public double CreditoDisponible { get; set; }
    public int TotalDocumentosPendientes { get; set; }
    public int TotalDocumentosVencidos { get; set; }
    public List<DocumentoSaldoDto> DocumentosPendientes { get; set; } = new();
}

public class SaldoProveedorDto
{
    public int ProveedorId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? RFC { get; set; }
    public double LimiteCredito { get; set; }
    public int DiasCredito { get; set; }
    public double SaldoActual { get; set; }
    public double SaldoVencido { get; set; }
    public double CreditoDisponible { get; set; }
    public int TotalDocumentosPendientes { get; set; }
    public int TotalDocumentosVencidos { get; set; }
    public List<DocumentoSaldoDto> DocumentosPendientes { get; set; } = new();
}
