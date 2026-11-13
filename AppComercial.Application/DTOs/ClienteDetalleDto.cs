using System;
using System.Collections.Generic;

namespace AppComercial.Application.DTOs;

public class ClienteDetalleDto
{
    // Generales
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? DenominacionComercial { get; set; }
    public string? RFC { get; set; }
    public string? CURP { get; set; }
    public string? RepresentanteLegal { get; set; }
    public int TipoCliente { get; set; } // 1 = Cliente, 2 = Mixto
    public bool Activo { get; set; }
    public DateTime? FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }

    // Fiscales
    public string? RegimenFiscal { get; set; }
    public string? UsoCFDI { get; set; }
    public string? MetodoPago { get; set; }
    public string? NumCtaPago { get; set; }

    // Domicilios
    public DomicilioClienteDto? DomicilioFiscal { get; set; }
    public List<DomicilioClienteDto> DomiciliosEnvio { get; set; } = new();

    // Comerciales y Crédito
    public int MonedaId { get; set; }
    public int ListaPrecio { get; set; }
    public double LimiteCredito { get; set; }
    public int DiasCredito { get; set; }
    public bool PermiteVentaCredito { get; set; }
    public bool PermiteExcederCredito { get; set; }
    public double DescuentoProntoPago { get; set; }
    public int DiasProntoPago { get; set; }
    public double InteresMoratorio { get; set; }
    public int DiaPago { get; set; }
    public int DiasRevision { get; set; }

    // Contacto y Asignaciones
    public string? Email1 { get; set; }
    public string? Email2 { get; set; }
    public string? Email3 { get; set; }
    public string? ContactoNombre { get; set; }
    public string? ContactoTelefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? Mensajeria { get; set; }
    public string? CuentaMensajeria { get; set; }
    public int AlmacenId { get; set; }
    public int AgenteVentaId { get; set; }
    public int AgenteCobroId { get; set; }

    // Clasificaciones
    public int Clasificacion1Id { get; set; }
    public int Clasificacion2Id { get; set; }
    public int Clasificacion3Id { get; set; }
    public int Clasificacion4Id { get; set; }
    public int Clasificacion5Id { get; set; }
    public int Clasificacion6Id { get; set; }
}
