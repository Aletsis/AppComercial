using Xunit;
using Moq;
using FluentValidation.TestHelper;
using AppComercial.Application.Validators;
using AppComercial.Application.Features.Clientes;
using AppComercial.Application.Features.Proveedores;
using AppComercial.Application.DTOs;
using AppComercial.Domain.Interfaces;
using AppComercial.Application.Common.Interfaces;
using AppComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.UnitTests;

public class ClientesUnitTests
{
    private readonly CreateClienteCommandValidator _createClienteValidator = new();
    private readonly CreateProveedorCommandValidator _createProveedorValidator = new();

    [Fact]
    public void CreateClienteValidator_ShouldHaveError_WhenCodigoIsEmpty()
    {
        var command = new CreateClienteCommand { Codigo = "" };
        var result = _createClienteValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Codigo);
    }

    [Fact]
    public void CreateClienteValidator_ShouldHaveError_WhenRazonSocialIsEmpty()
    {
        var command = new CreateClienteCommand { Codigo = "CTE01", RazonSocial = "" };
        var result = _createClienteValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RazonSocial);
    }

    [Fact]
    public void CreateClienteValidator_ShouldHaveError_WhenRfcIsInvalid()
    {
        var command = new CreateClienteCommand 
        { 
            Codigo = "CTE01", 
            RazonSocial = "EMPRESA DE PRUEBA",
            RFC = "INVALIDO"
        };
        var result = _createClienteValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RFC);
    }

    [Fact]
    public void CreateClienteValidator_ShouldPass_WhenRfcIsValidPersonaMoral()
    {
        var command = new CreateClienteCommand 
        { 
            Codigo = "CTE01", 
            RazonSocial = "EMPRESA DE PRUEBA",
            RFC = "ABC680524P76",
            ListaPrecio = 1,
            LimiteCredito = 50000,
            DiasCredito = 30
        };
        var result = _createClienteValidator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.RFC);
        result.ShouldNotHaveValidationErrorFor(x => x.Codigo);
        result.ShouldNotHaveValidationErrorFor(x => x.RazonSocial);
    }

    [Fact]
    public void CreateClienteValidator_ShouldHaveError_WhenDomicilioCpIsInvalid()
    {
        var command = new CreateClienteCommand 
        { 
            Codigo = "CTE01", 
            RazonSocial = "EMPRESA DE PRUEBA",
            RFC = "ABC680524P76",
            DomicilioFiscal = new CrearDomicilioDto
            {
                Calle = "Av. Reforma",
                NumeroExterior = "123",
                CodigoPostal = "123" // Inválido, debe tener 5 dígitos
            }
        };
        var result = _createClienteValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("DomicilioFiscal.CodigoPostal");
    }

    [Fact]
    public void CreateClienteValidator_ShouldPass_WhenDomicilioCpIsValid()
    {
        var command = new CreateClienteCommand 
        { 
            Codigo = "CTE01", 
            RazonSocial = "EMPRESA DE PRUEBA",
            RFC = "ABC680524P76",
            DomicilioFiscal = new CrearDomicilioDto
            {
                Calle = "Av. Reforma",
                NumeroExterior = "123",
                CodigoPostal = "06500"
            }
        };
        var result = _createClienteValidator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor("DomicilioFiscal.CodigoPostal");
    }

    [Fact]
    public void CreateProveedorValidator_ShouldHaveError_WhenCodigoIsEmpty()
    {
        var command = new CreateProveedorCommand { Codigo = "" };
        var result = _createProveedorValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Codigo);
    }

    [Fact]
    public void CreateProveedorValidator_ShouldPass_WhenDataIsValid()
    {
        var command = new CreateProveedorCommand 
        { 
            Codigo = "PROV01", 
            RazonSocial = "DISTRIBUIDORA DEL NORTE",
            RFC = "DNO950101XYZ",
            LimiteCreditoProveedor = 100000,
            DiasCreditoProveedor = 45,
            TiempoEntrega = 5
        };
        var result = _createProveedorValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
