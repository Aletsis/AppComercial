using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using AppComercial.Application.Common.Interfaces;
using AppComercial.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Proveedores;

public class UpdateProveedorCommand : IRequest<int>
{
    public string Codigo { get; set; } = string.Empty;

    // Generales
    public string? RazonSocial { get; set; }
    public string? DenominacionComercial { get; set; }
    public string? RFC { get; set; }
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
    public double? LimiteCreditoProveedor { get; set; }
    public int? DiasCreditoProveedor { get; set; }
    public int? TiempoEntrega { get; set; }
    public int? DiasEmbarqueProveedor { get; set; }
    public double? ImpuestoProveedor1 { get; set; }
    public double? ImpuestoProveedor2 { get; set; }
    public double? ImpuestoProveedor3 { get; set; }
    public double? RetencionProveedor1 { get; set; }
    public double? RetencionProveedor2 { get; set; }

    // Contacto y Asignaciones
    public string? Email { get; set; }
    public string? Email2 { get; set; }
    public string? Email3 { get; set; }
    public string? Telefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? ContactoNombre { get; set; }
    public string? Mensajeria { get; set; }
    public string? CuentaMensajeria { get; set; }

    // Estatus
    public bool? Activo { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public int TipoCliente { get; set; } = 3; // 3 = Proveedor
}

public class UpdateProveedorCommandHandler : IRequestHandler<UpdateProveedorCommand, int>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IContpaqiDbContext _context;

    public UpdateProveedorCommandHandler(IContpaqiSdk sdk, IContpaqiDbContext context)
    {
        _sdk = sdk;
        _context = context;
    }

    public async Task<int> Handle(UpdateProveedorCommand request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del proveedor es requerido.");

        var proveedor = await _context.Clientes.FirstOrDefaultAsync(c => c.CCODIGOCLIENTE == codigo, cancellationToken);
        if (proveedor == null)
            throw new Exception($"El proveedor con código '{codigo}' no existe en CONTPAQi.");

        var actualizarParams = new Dictionary<string, string>();

        if (request.RazonSocial != null)
        {
            var razonSocial = request.RazonSocial.Trim();
            if (razonSocial.Length > SdkConstantes.kLongNombre - 1)
                razonSocial = razonSocial.Substring(0, SdkConstantes.kLongNombre - 1);
            actualizarParams["CRAZONSOCIAL"] = razonSocial;
        }

        if (request.DenominacionComercial != null)
            actualizarParams["CDENCOMERCIAL"] = request.DenominacionComercial.Trim();

        if (request.RFC != null)
        {
            var rfc = request.RFC.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(rfc))
                rfc = "XAXX010101000";
            if (rfc.Length > SdkConstantes.kLongRFC - 1)
                rfc = rfc.Substring(0, SdkConstantes.kLongRFC - 1);
            actualizarParams["CRFC"] = rfc;
        }

        if (request.CURP != null)
            actualizarParams["CCURP"] = request.CURP.Trim();

        if (request.RepresentanteLegal != null)
            actualizarParams["CREPLEGAL"] = request.RepresentanteLegal.Trim();

        if (request.RegimenFiscal != null)
            actualizarParams["CREGIMFISC"] = request.RegimenFiscal.Trim();

        if (request.UsoCFDI != null)
            actualizarParams["CUSOCFDI"] = request.UsoCFDI.Trim();

        if (request.MetodoPago != null)
            actualizarParams["CMETODOPAG"] = request.MetodoPago.Trim();

        if (request.NumCtaPago != null)
            actualizarParams["CNUMCTAPAG"] = request.NumCtaPago.Trim();

        if (request.Email != null)
            actualizarParams["CEMAIL1"] = request.Email.Trim();

        if (request.Email2 != null)
            actualizarParams["CEMAIL2"] = request.Email2.Trim();

        if (request.Email3 != null)
            actualizarParams["CEMAIL3"] = request.Email3.Trim();

        if (request.Telefono != null)
            actualizarParams["CCON1TEL"] = request.Telefono.Trim();

        if (request.ContactoNombre != null)
            actualizarParams["CCON1NOM"] = request.ContactoNombre.Trim();

        if (request.WhatsApp != null)
            actualizarParams["CWHATSAPP"] = request.WhatsApp.Trim();

        if (request.Mensajeria != null)
            actualizarParams["CMENSAJERIA"] = request.Mensajeria.Trim();

        if (request.CuentaMensajeria != null)
            actualizarParams["CCUENTAMENSAJERIA"] = request.CuentaMensajeria.Trim();

        if (request.LimiteCreditoProveedor.HasValue)
            actualizarParams["CLIMITECREDITOPROVEEDOR"] = request.LimiteCreditoProveedor.Value.ToString(CultureInfo.InvariantCulture);

        if (request.DiasCreditoProveedor.HasValue)
            actualizarParams["CDIASCREDITOPROVEEDOR"] = request.DiasCreditoProveedor.Value.ToString();

        if (request.TiempoEntrega.HasValue)
            actualizarParams["CTIEMPOENTREGA"] = request.TiempoEntrega.Value.ToString();

        if (request.DiasEmbarqueProveedor.HasValue)
            actualizarParams["CDIASEMBARQUEPROVEEDOR"] = request.DiasEmbarqueProveedor.Value.ToString();

        if (request.ImpuestoProveedor1.HasValue)
            actualizarParams["CIMPUESTOPROVEEDOR1"] = request.ImpuestoProveedor1.Value.ToString(CultureInfo.InvariantCulture);

        if (request.ImpuestoProveedor2.HasValue)
            actualizarParams["CIMPUESTOPROVEEDOR2"] = request.ImpuestoProveedor2.Value.ToString(CultureInfo.InvariantCulture);

        if (request.ImpuestoProveedor3.HasValue)
            actualizarParams["CIMPUESTOPROVEEDOR3"] = request.ImpuestoProveedor3.Value.ToString(CultureInfo.InvariantCulture);

        if (request.RetencionProveedor1.HasValue)
            actualizarParams["CRETENCIONPROVEEDOR1"] = request.RetencionProveedor1.Value.ToString(CultureInfo.InvariantCulture);

        if (request.RetencionProveedor2.HasValue)
            actualizarParams["CRETENCIONPROVEEDOR2"] = request.RetencionProveedor2.Value.ToString(CultureInfo.InvariantCulture);

        if (request.Activo.HasValue)
        {
            actualizarParams["CESTATUS"] = request.Activo.Value ? "1" : "0";
            if (!request.Activo.Value)
                actualizarParams["CFECHABAJA"] = DateTime.Now.ToString("MM/dd/yyyy");
        }

        int result = 0;
        if (actualizarParams.Count > 0)
        {
            result = await _sdk.ActualizarClienteAsync(codigo, actualizarParams);
        }

        // Manejar Domicilio Fiscal si se proporcionó
        if (request.DomicilioFiscal != null && !string.IsNullOrWhiteSpace(request.DomicilioFiscal.CodigoPostal))
        {
            var domFiscalExistente = await _context.Domicilios
                .FirstOrDefaultAsync(d => d.CatalogoId == proveedor.CIDCLIENTEPROVEEDOR && d.TipoCatalogo == 2 && d.TipoDireccion == 0, cancellationToken);

            if (domFiscalExistente != null)
            {
                var dirParams = new Dictionary<string, string>
                {
                    ["CNOMBRECALLE"] = string.IsNullOrWhiteSpace(request.DomicilioFiscal.Calle) ? "Conocido" : request.DomicilioFiscal.Calle.Trim(),
                    ["CNUMEROEXTERIOR"] = request.DomicilioFiscal.NumeroExterior?.Trim() ?? string.Empty,
                    ["CNUMEROINTERIOR"] = request.DomicilioFiscal.NumeroInterior?.Trim() ?? string.Empty,
                    ["CCOLONIA"] = request.DomicilioFiscal.Colonia?.Trim() ?? string.Empty,
                    ["CCODIGOPOSTAL"] = request.DomicilioFiscal.CodigoPostal.Trim(),
                    ["CCIUDAD"] = request.DomicilioFiscal.Ciudad?.Trim() ?? string.Empty,
                    ["CESTADO"] = request.DomicilioFiscal.Estado?.Trim() ?? string.Empty,
                    ["CPAIS"] = string.IsNullOrWhiteSpace(request.DomicilioFiscal.Pais) ? "México" : request.DomicilioFiscal.Pais.Trim()
                };

                if (!string.IsNullOrWhiteSpace(request.DomicilioFiscal.Telefono))
                    dirParams["CTELEFONO1"] = request.DomicilioFiscal.Telefono.Trim();
                if (!string.IsNullOrWhiteSpace(request.DomicilioFiscal.Email))
                    dirParams["CEMAIL"] = request.DomicilioFiscal.Email.Trim();

                try
                {
                    await _sdk.ActualizarDireccionAsync(domFiscalExistente.Id, dirParams);
                }
                catch
                {
                    // No abortar
                }
            }
            else
            {
                var dir = new tDireccion
                {
                    cCodCatalogo = codigo,
                    cTipoCatalogo = 2, // Proveedor
                    cTipoDireccion = 0, // Fiscal
                    cNombreCalle = string.IsNullOrWhiteSpace(request.DomicilioFiscal.Calle) ? "Conocido" : request.DomicilioFiscal.Calle.Trim(),
                    cNumeroExterior = request.DomicilioFiscal.NumeroExterior?.Trim() ?? string.Empty,
                    cNumeroInterior = request.DomicilioFiscal.NumeroInterior?.Trim() ?? string.Empty,
                    cColonia = request.DomicilioFiscal.Colonia?.Trim() ?? string.Empty,
                    cCodigoPostal = request.DomicilioFiscal.CodigoPostal.Trim(),
                    cCiudad = request.DomicilioFiscal.Ciudad?.Trim() ?? string.Empty,
                    cEstado = request.DomicilioFiscal.Estado?.Trim() ?? string.Empty,
                    cPais = string.IsNullOrWhiteSpace(request.DomicilioFiscal.Pais) ? "México" : request.DomicilioFiscal.Pais.Trim(),
                    cTelefono1 = request.DomicilioFiscal.Telefono?.Trim() ?? string.Empty,
                    cEmail = request.DomicilioFiscal.Email?.Trim() ?? string.Empty
                };

                try
                {
                    await _sdk.CrearDireccionAsync(dir);
                }
                catch
                {
                    // Ignorar si falla
                }
            }
        }

        return result;
    }
}
