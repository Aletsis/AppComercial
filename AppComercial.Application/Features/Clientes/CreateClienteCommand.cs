using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using AppComercial.Application.Common.Interfaces;
using AppComercial.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Clientes;

public class CreateClienteCommand : IRequest<int>
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

    // Domicilio Fiscal (Integrado para CFDI 4.0)
    public CrearDomicilioDto? DomicilioFiscal { get; set; }

    // Comerciales y Crédito
    public string? NombreMoneda { get; set; } = "Peso Mexicano";
    public int ListaPrecio { get; set; } = 1;
    public double LimiteCredito { get; set; } = 0.0;
    public int DiasCredito { get; set; } = 0;
    public bool PermiteVentaCredito { get; set; } = true;
    public bool PermiteExcederCredito { get; set; } = false;
    public double DescuentoProntoPago { get; set; } = 0.0;
    public int DiasProntoPago { get; set; } = 0;
    public double InteresMoratorio { get; set; } = 0.0;
    public int DiaPago { get; set; } = 0;
    public int DiasRevision { get; set; } = 0;

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

    /// <summary>
    /// Tipo de catálogo en CONTPAQi: 1 = Solo Cliente, 2 = Ambos (Cliente y Proveedor).
    /// </summary>
    public int TipoCliente { get; set; } = 1;
}

public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, int>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IContpaqiDbContext _context;

    public CreateClienteCommandHandler(IContpaqiSdk sdk, IContpaqiDbContext context)
    {
        _sdk = sdk;
        _context = context;
    }

    public async Task<int> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del cliente es requerido.");

        var razonSocial = (request.RazonSocial ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(razonSocial))
            throw new ArgumentException("La razón social del cliente es requerida.");

        var rfc = (request.RFC ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(rfc))
            rfc = "XAXX010101000";

        // Validar si el código ya existe en CONTPAQi Comercial
        var clienteExistente = await _context.Clientes.FirstOrDefaultAsync(c => c.CCODIGOCLIENTE == codigo, cancellationToken);
        if (clienteExistente != null)
        {
            // Si ya existe como proveedor puro (CTIPOCLIENTE == 3), lo convertimos a Ambos (2) y agregamos datos de cliente
            if (clienteExistente.CTIPOCLIENTE == 3)
            {
                var actualizarParams = new Dictionary<string, string>
                {
                    ["CTIPOCLIENTE"] = "2"
                };

                if (request.ListaPrecio > 0)
                    actualizarParams["CLISTAPRECIOCLIENTE"] = request.ListaPrecio.ToString();
                if (request.LimiteCredito > 0)
                    actualizarParams["CLIMITECREDITOCLIENTE"] = request.LimiteCredito.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (request.DiasCredito > 0)
                    actualizarParams["CDIASCREDITOCLIENTE"] = request.DiasCredito.ToString();

                actualizarParams["CBANVENTACREDITO"] = request.PermiteVentaCredito ? "1" : "0";
                actualizarParams["CBANEXCEDERCREDITO"] = request.PermiteExcederCredito ? "1" : "0";

                await _sdk.ActualizarClienteAsync(codigo, actualizarParams);
                return clienteExistente.CIDCLIENTEPROVEEDOR;
            }

            var rolActual = clienteExistente.CTIPOCLIENTE == 2 ? "Cliente y Proveedor" : "Cliente";
            throw new Exception($"El código '{codigo}' ya existe en CONTPAQi Comercial registrado como {rolActual}.");
        }

        var tipoFinal = (request.TipoCliente == 2) ? 2 : 1;
        var nuevoClienteParams = new tCteProv
        {
            cCodigoCliente = codigo,
            cRazonSocial = razonSocial,
            cRFC = rfc,
            cCURP = request.CURP?.Trim() ?? string.Empty,
            cDenComercial = request.DenominacionComercial?.Trim() ?? string.Empty,
            cRepLegal = request.RepresentanteLegal?.Trim() ?? string.Empty,
            cNombreMoneda = string.IsNullOrWhiteSpace(request.NombreMoneda) ? "Peso Mexicano" : request.NombreMoneda.Trim(),
            cTipoCliente = tipoFinal,
            cListaPreciosCliente = request.ListaPrecio <= 0 ? 1 : request.ListaPrecio,
            cLimiteCreditoCliente = request.LimiteCredito,
            cDiasCreditoCliente = request.DiasCredito,
            cBanVentaCredito = request.PermiteVentaCredito ? 1 : 0,
            cBanExcederCredito = request.PermiteExcederCredito ? 1 : 0,
            cDescuentoProntoPago = request.DescuentoProntoPago,
            cDiasProntoPago = request.DiasProntoPago,
            cInteresMoratorio = request.InteresMoratorio,
            cDiaPago = request.DiaPago,
            cDiasRevision = request.DiasRevision,
            cCodigoAlmacen = request.CodigoAlmacen?.Trim() ?? string.Empty,
            cCodigoAgenteVenta = request.CodigoAgenteVenta?.Trim() ?? string.Empty,
            cCodigoAgenteCobro = request.CodigoAgenteCobro?.Trim() ?? string.Empty,
            cEstatus = 1
        };

        var nuevoId = await _sdk.CrearClienteAsync(nuevoClienteParams);

        // Actualizar campos fiscales, contactos y bancarios mediante SetDato
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
                    cTipoCatalogo = 1, // 1 = Cliente
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
                // Log o continuar, el cliente ya quedó registrado
            }
        }

        return nuevoId;
    }
}
