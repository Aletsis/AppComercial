using System;
using System.Collections.Generic;

namespace AppComercial.Application.DTOs;

public class ProveedorDetalleDto
{
    // Generales
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? DenominacionComercial { get; set; }
    public string? RFC { get; set; }
    public string? CURP { get; set; }
    public string? RepresentanteLegal { get; set; }
    public int TipoCliente { get; set; } // 3 = Proveedor, 2 = Mixto
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

    // Condiciones de Compra y Crédito
    public int MonedaId { get; set; }
    public double LimiteCreditoProveedor { get; set; }
    public int DiasCreditoProveedor { get; set; }
    public int TiempoEntrega { get; set; }
    public int DiasEmbarqueProveedor { get; set; }
    public double ImpuestoProveedor1 { get; set; }
    public double ImpuestoProveedor2 { get; set; }
    public double ImpuestoProveedor3 { get; set; }
    public double RetencionProveedor1 { get; set; }
    public double RetencionProveedor2 { get; set; }

    // Contacto y Asignaciones
    public string? Email1 { get; set; }
    public string? Email2 { get; set; }
    public string? Email3 { get; set; }
    public string? ContactoNombre { get; set; }
    public string? ContactoTelefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? Mensajeria { get; set; }
    public string? CuentaMensajeria { get; set; }

    // Clasificaciones de Proveedor
    public int ClasificacionProveedor1Id { get; set; }
    public int ClasificacionProveedor2Id { get; set; }
    public int ClasificacionProveedor3Id { get; set; }
    public int ClasificacionProveedor4Id { get; set; }
    public int ClasificacionProveedor5Id { get; set; }
    public int ClasificacionProveedor6Id { get; set; }
}
