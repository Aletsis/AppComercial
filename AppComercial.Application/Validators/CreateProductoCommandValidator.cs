using FluentValidation;
using AppComercial.Application.Features.Productos;

namespace AppComercial.Application.Validators;

public class CreateProductoCommandValidator : AbstractValidator<CreateProductoCommand>
{
    public CreateProductoCommandValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código del producto es requerido.")
            .Length(1, 30).WithMessage("El código debe tener entre 1 y 30 caracteres.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del producto es requerido.")
            .Length(1, 60).WithMessage("El nombre debe tener entre 1 y 60 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(255).WithMessage("La descripción no puede exceder 255 caracteres.");

        RuleFor(x => x.TipoProducto)
            .InclusiveBetween(1, 3).WithMessage("TipoProducto debe ser 1 (Producto), 2 (Paquete) o 3 (Servicio).");

        RuleFor(x => x.ControlExistencia)
            .InclusiveBetween(0, 4).WithMessage("ControlExistencia debe ser un valor entre 0 y 4.");

        RuleFor(x => x.IdUnidadBase)
            .GreaterThan(0).WithMessage("El Id de la Unidad Base debe ser mayor a 0.");

        RuleFor(x => x.Precio1)
            .GreaterThanOrEqualTo(0).WithMessage("El precio debe ser mayor o igual a 0.");

        RuleFor(x => x.Impuesto1)
            .InclusiveBetween(0, 100).WithMessage("El impuesto 1 debe ser un porcentaje entre 0 y 100.");

        RuleFor(x => x.Clasificacion1)
            .MaximumLength(30).WithMessage("La clasificación 1 no puede exceder 30 caracteres.");

        RuleFor(x => x.Clasificacion2)
            .MaximumLength(30).WithMessage("La clasificación 2 no puede exceder 30 caracteres.");

        RuleFor(x => x.Clasificacion5)
            .MaximumLength(30).WithMessage("La clasificación 5 no puede exceder 30 caracteres.");

        RuleFor(x => x.CodigoSat)
            .MaximumLength(20).WithMessage("El código SAT no puede exceder 20 caracteres.");
    }
}