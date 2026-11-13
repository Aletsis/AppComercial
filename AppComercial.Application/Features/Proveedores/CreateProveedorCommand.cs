using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using AppComercial.Application.Common.Interfaces;
using AppComercial.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Proveedores;

public class CreateProveedorCommand : IRequest<int>
{
    // Generales
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? DenominacionComercial { get; set; }
    public string RFC { get; set; } = string.Empty;
    public string? CURP { get; set; }
    public string? RepresentanteLegal { get; set; }

    // Fiscales
    public string? RegimenFiscal { get; set; }
    public string? UsoCFDI { get; set; }
    public string? MetodoPago { get; set; }
    public string? NumCtaPago { get; set; }

    // Domicilio Fiscal
    public CrearDomicilioDto? DomicilioFiscal { get; set; }

    // Condiciones de Compra y Crédito
    public string? NombreMoneda { get; set; } = "Peso Mexicano";
    public double LimiteCreditoProveedor { get; set; } = 0.0;
    public int DiasCreditoProveedor { get; set; } = 0;
    public int TiempoEntrega { get; set; } = 0;
    public int DiasEmbarqueProveedor { get; set; } = 0;
    public double ImpuestoProveedor1 { get; set; } = 0.0;
    public double ImpuestoProveedor2 { get; set; } = 0.0;
    public double ImpuestoProveedor3 { get; set; } = 0.0;
    public double RetencionProveedor1 { get; set; } = 0.0;
    public double RetencionProveedor2 { get; set; } = 0.0;

    // Contacto y Asignaciones
    public string? Email { get; set; }
    public string? Email2 { get; set; }
    public string? Email3 { get; set; }
    public string? Telefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? ContactoNombre { get; set; }
    public string? Mensajeria { get; set; }
    public string? CuentaMensajeria { get; set; }

    /// <summary>
    /// Tipo de catálogo en CONTPAQi: 3 = Solo Proveedor, 2 = Ambos (Cliente y Proveedor).
    /// </summary>
    public int TipoCliente { get; set; } = 3;
}

public class CreateProveedorCommandHandler : IRequestHandler<CreateProveedorCommand, int>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IContpaqiDbContext _context;

    public CreateProveedorCommandHandler(IContpaqiSdk sdk, IContpaqiDbContext context)
    {
        _sdk = sdk;
        _context = context;
    }

    public async Task<int> Handle(CreateProveedorCommand request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del proveedor es requerido.");

        var razonSocial = (request.RazonSocial ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(razonSocial))
            throw new ArgumentException("La razón social del proveedor es requerida.");

        var rfc = (request.RFC ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(rfc))
            rfc = "XAXX010101000";

        // Validar si el código ya existe en CONTPAQi Comercial
        var proveedorExistente = await _context.Clientes.FirstOrDefaultAsync(c => c.CCODIGOCLIENTE == codigo, cancellationToken);
        if (proveedorExistente != null)
        {
            // Si ya existe como cliente puro (CTIPOCLIENTE == 1), lo convertimos a Ambos (2) y agregamos datos de proveedor
            if (proveedorExistente.CTIPOCLIENTE == 1)
            {
                var actualizarParams = new Dictionary<string, string>
                {
                    ["CTIPOCLIENTE"] = "2"
                };

                if (request.LimiteCreditoProveedor > 0)
                    actualizarParams["CLIMITECREDITOPROVEEDOR"] = request.LimiteCreditoProveedor.ToString(CultureInfo.InvariantCulture);
                if (request.DiasCreditoProveedor > 0)
                    actualizarParams["CDIASCREDITOPROVEEDOR"] = request.DiasCreditoProveedor.ToString();
                if (request.TiempoEntrega > 0)
                    actualizarParams["CTIEMPOENTREGA"] = request.TiempoEntrega.ToString();

                await _sdk.ActualizarClienteAsync(codigo, actualizarParams);
                return proveedorExistente.CIDCLIENTEPROVEEDOR;
            }

            var rolActual = proveedorExistente.CTIPOCLIENTE == 2 ? "Cliente y Proveedor" : "Proveedor";
            throw new Exception($"El código '{codigo}' ya existe en CONTPAQi Comercial registrado como {rolActual}.");
        }

        var tipoFinal = (request.TipoCliente == 2) ? 2 : 3;
        var nuevoProveedorParams = new tCteProv
        {
            cCodigoCliente = codigo,
            cRazonSocial = razonSocial,
            cRFC = rfc,
            cCURP = request.CURP?.Trim() ?? string.Empty,
            cDenComercial = request.DenominacionComercial?.Trim() ?? string.Empty,
            cRepLegal = request.RepresentanteLegal?.Trim() ?? string.Empty,
            cNombreMoneda = string.IsNullOrWhiteSpace(request.NombreMoneda) ? "Peso Mexicano" : request.NombreMoneda.Trim(),
            cTipoCliente = tipoFinal,
            cLimiteCreditoProveedor = request.LimiteCreditoProveedor,
            cDiasCreditoProveedor = request.DiasCreditoProveedor,
            cTiempoEntrega = request.TiempoEntrega,
            cDiasEmbarqueProveedor = request.DiasEmbarqueProveedor,
            cImpuestoProveedor1 = request.ImpuestoProveedor1,
            cImpuestoProveedor2 = request.ImpuestoProveedor2,
            cImpuestoProveedor3 = request.ImpuestoProveedor3,
            cRetencionProveedor1 = request.RetencionProveedor1,
            cRetencionProveedor2 = request.RetencionProveedor2,
            cMensajeria = request.Mensajeria?.Trim() ?? string.Empty,
            cCuentaMensajeria = request.CuentaMensajeria?.Trim() ?? string.Empty,
            cEstatus = 1
        };

        var nuevoId = await _sdk.CrearClienteAsync(nuevoProveedorParams);

        var updateParams = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(request.RegimenFiscal))
            updateParams["CREGIMFISC"] = request.RegimenFiscal.Trim();
        if (!string.IsNullOrWhiteSpace(request.UsoCFDI))
            updateParams["CUSOCFDI"] = request.UsoCFDI.Trim();
        if (!string.IsNullOrWhiteSpace(request.MetodoPago))
            updateParams["CMETODOPAG"] = request.MetodoPago.Trim();
        if (!string.IsNullOrWhiteSpace(request.NumCtaPago))
            updateParams["CNUMCTAPAG"] = request.NumCtaPago.Trim();
        if (!string.IsNullOrWhiteSpace(request.Email))
            updateParams["CEMAIL1"] = request.Email.Trim();
        if (!string.IsNullOrWhiteSpace(request.Email2))
            updateParams["CEMAIL2"] = request.Email2.Trim();
        if (!string.IsNullOrWhiteSpace(request.Email3))
            updateParams["CEMAIL3"] = request.Email3.Trim();
        if (!string.IsNullOrWhiteSpace(request.Telefono))
            updateParams["CCON1TEL"] = request.Telefono.Trim();
        if (!string.IsNullOrWhiteSpace(request.ContactoNombre))
            updateParams["CCON1NOM"] = request.ContactoNombre.Trim();
        if (!string.IsNullOrWhiteSpace(request.WhatsApp))
            updateParams["CWHATSAPP"] = request.WhatsApp.Trim();

        if (updateParams.Count > 0)
        {
            try
            {
                await _sdk.ActualizarClienteAsync(codigo, updateParams);
            }
            catch
            {
                // No bloquear creación si el esquema particular no soporta alguno de los campos
            }
        }

        // Crear domicilio fiscal si fue proporcionado
        if (request.DomicilioFiscal != null && !string.IsNullOrWhiteSpace(request.DomicilioFiscal.CodigoPostal))
        {
            try
            {
                var dir = new tDireccion
                {
                    cCodCatalogo = codigo,
                    cTipoCatalogo = 2, // 2 = Proveedor
                    cTipoDireccion = 0, // 0 = Fiscal
                    cNombreCalle = string.IsNullOrWhiteSpace(request.DomicilioFiscal.Calle) ? "Conocido" : request.DomicilioFiscal.Calle.Trim(),
                    cNumeroExterior = request.DomicilioFiscal.NumeroExterior?.Trim() ?? string.Empty,
                    cNumeroInterior = request.DomicilioFiscal.NumeroInterior?.Trim() ?? string.Empty,
                    cColonia = request.DomicilioFiscal.Colonia?.Trim() ?? string.Empty,
                    cCodigoPostal = request.DomicilioFiscal.CodigoPostal.Trim(),
                    cCiudad = request.DomicilioFiscal.Ciudad?.Trim() ?? string.Empty,
                    cEstado = request.DomicilioFiscal.Estado?.Trim() ?? string.Empty,
                    cPais = string.IsNullOrWhiteSpace(request.DomicilioFiscal.Pais) ? "México" : request.DomicilioFiscal.Pais.Trim(),
                    cTelefono1 = request.DomicilioFiscal.Telefono?.Trim() ?? (request.Telefono?.Trim() ?? string.Empty),
                    cEmail = request.DomicilioFiscal.Email?.Trim() ?? (request.Email?.Trim() ?? string.Empty)
                };

                await _sdk.CrearDireccionAsync(dir);
            }
            catch
            {
                // Log o continuar, el proveedor ya quedó registrado
            }
        }

        return nuevoId;
    }
}
