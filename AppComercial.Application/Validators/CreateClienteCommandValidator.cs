using FluentValidation;
using AppComercial.Application.Features.Clientes;
using System.Text.RegularExpressions;

namespace AppComercial.Application.Validators;

public class CreateClienteCommandValidator : AbstractValidator<CreateClienteCommand>
{
    private static readonly Regex RfcRegex = new(@"^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CpRegex = new(@"^\d{5}$", RegexOptions.Compiled);

    public CreateClienteCommandValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código del cliente es requerido.")
            .Length(1, 30).WithMessage("El código del cliente debe tener entre 1 y 30 caracteres.");

        RuleFor(x => x.RazonSocial)
            .NotEmpty().WithMessage("La razón social es requerida.")
            .Length(1, 60).WithMessage("La razón social no puede exceder 60 caracteres.");

        RuleFor(x => x.RFC)
            .NotEmpty().WithMessage("El RFC es requerido.")
            .Must(rfc => string.IsNullOrWhiteSpace(rfc) || RfcRegex.IsMatch(rfc.Trim()))
            .WithMessage("El formato del RFC no es válido (debe tener 12 caracteres para persona moral o 13 para persona física).");

        RuleFor(x => x.ListaPrecio)
            .InclusiveBetween(1, 10).WithMessage("La lista de precios debe ser un valor entre 1 y 10.");

        RuleFor(x => x.LimiteCredito)
            .GreaterThanOrEqualTo(0).WithMessage("El límite de crédito no puede ser negativo.");

        RuleFor(x => x.DiasCredito)
            .GreaterThanOrEqualTo(0).WithMessage("Los días de crédito no pueden ser negativos.");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
                .MaximumLength(60).WithMessage("El email no puede exceder 60 caracteres.");
        });

        When(x => x.DomicilioFiscal != null, () =>
        {
            RuleFor(x => x.DomicilioFiscal!.CodigoPostal)
                .NotEmpty().WithMessage("El código postal del domicilio fiscal es requerido para CFDI 4.0.")
                .Must(cp => !string.IsNullOrWhiteSpace(cp) && CpRegex.IsMatch(cp.Trim()))
                .WithMessage("El código postal debe constar de 5 dígitos numéricos.");

            RuleFor(x => x.DomicilioFiscal!.Calle)
                .MaximumLength(60).WithMessage("La calle no puede exceder 60 caracteres.");

            RuleFor(x => x.DomicilioFiscal!.NumeroExterior)
                .MaximumLength(30).WithMessage("El número exterior no puede exceder 30 caracteres.");

            RuleFor(x => x.DomicilioFiscal!.Colonia)
                .MaximumLength(60).WithMessage("La colonia no puede exceder 60 caracteres.");
        });
    }
}
