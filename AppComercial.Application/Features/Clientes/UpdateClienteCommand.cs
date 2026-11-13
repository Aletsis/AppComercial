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

namespace AppComercial.Application.Features.Clientes;

public class UpdateClienteCommand : IRequest<int>
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

    // Comerciales y Crédito
    public int? ListaPrecio { get; set; }
    public double? LimiteCredito { get; set; }
    public int? DiasCredito { get; set; }
    public bool? PermiteVentaCredito { get; set; }
    public bool? PermiteExcederCredito { get; set; }
    public double? DescuentoProntoPago { get; set; }
    public int? DiasProntoPago { get; set; }
    public double? InteresMoratorio { get; set; }
    public int? DiaPago { get; set; }
    public int? DiasRevision { get; set; }

    // Contacto y Asignaciones
    public string? Email { get; set; }
    public string? Email2 { get; set; }
    public string? Email3 { get; set; }
    public string? Telefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? ContactoNombre { get; set; }
    public string? CodigoAlmacen { get; set; }
    public string? CodigoAgenteVenta { get; set; }
    public string? CodigoAgenteCobro { get; set; }

    // Estatus
    public bool? Activo { get; set; }

    /// <summary>
    /// Tipo de catálogo en CONTPAQi: 1 = Solo Cliente, 2 = Ambos (Cliente y Proveedor).
    /// </summary>
    public int? TipoCliente { get; set; }
}

public class UpdateClienteCommandHandler : IRequestHandler<UpdateClienteCommand, int>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IContpaqiDbContext _context;

    public UpdateClienteCommandHandler(IContpaqiSdk sdk, IContpaqiDbContext context)
    {
        _sdk = sdk;
        _context = context;
    }

    public async Task<int> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del cliente es requerido.");

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.CCODIGOCLIENTE == codigo, cancellationToken);
        if (cliente == null)
            throw new Exception($"El cliente con código '{codigo}' no existe en CONTPAQi.");

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

        if (request.ListaPrecio.HasValue)
            actualizarParams["CLISTAPRECIOCLIENTE"] = request.ListaPrecio.Value.ToString();

        if (request.LimiteCredito.HasValue)
            actualizarParams["CLIMITECREDITOCLIENTE"] = request.LimiteCredito.Value.ToString(CultureInfo.InvariantCulture);

        if (request.DiasCredito.HasValue)
            actualizarParams["CDIASCREDITOCLIENTE"] = request.DiasCredito.Value.ToString();

        if (request.PermiteVentaCredito.HasValue)
            actualizarParams["CBANVENTACREDITO"] = request.PermiteVentaCredito.Value ? "1" : "0";

        if (request.PermiteExcederCredito.HasValue)
            actualizarParams["CBANEXCEDERCREDITO"] = request.PermiteExcederCredito.Value ? "1" : "0";

        if (request.DescuentoProntoPago.HasValue)
            actualizarParams["CDESCUENTOPRONTOPAGO"] = request.DescuentoProntoPago.Value.ToString(CultureInfo.InvariantCulture);

        if (request.DiasProntoPago.HasValue)
            actualizarParams["CDIASPRONTOPAGO"] = request.DiasProntoPago.Value.ToString();

        if (request.InteresMoratorio.HasValue)
            actualizarParams["CINTERESMORATORIO"] = request.InteresMoratorio.Value.ToString(CultureInfo.InvariantCulture);

        if (request.DiaPago.HasValue)
            actualizarParams["CDIAPAGO"] = request.DiaPago.Value.ToString();

        if (request.DiasRevision.HasValue)
            actualizarParams["CDIASREVISION"] = request.DiasRevision.Value.ToString();

        if (request.Activo.HasValue)
        {
            actualizarParams["CESTATUS"] = request.Activo.Value ? "1" : "0";
            if (!request.Activo.Value)
                actualizarParams["CFECHABAJA"] = DateTime.Now.ToString("MM/dd/yyyy");
        }

        if (request.TipoCliente.HasValue && (request.TipoCliente.Value == 1 || request.TipoCliente.Value == 2))
        {
            actualizarParams["CTIPOCLIENTE"] = request.TipoCliente.Value.ToString();
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
                .FirstOrDefaultAsync(d => d.CatalogoId == cliente.CIDCLIENTEPROVEEDOR && d.TipoCatalogo == 1 && d.TipoDireccion == 0, cancellationToken);

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
                    // No abortar actualización si la dirección falla
                }
            }
            else
            {
                var dir = new tDireccion
                {
                    cCodCatalogo = codigo,
                    cTipoCatalogo = 1,
                    cTipoDireccion = 0,
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
                    // Ignorar si falla el alta de dirección
                }
            }
        }

        return result;
    }
}
