using AppComercial.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Application.Common.Services;

/// <summary>
/// Se lanza cuando un código de concepto no existe o no corresponde al tipo de documento del endpoint.
/// Los controladores la traducen a 400 Bad Request.
/// </summary>
public class TipoDocumentoInvalidoException : Exception
{
    public TipoDocumentoInvalidoException(string message) : base(message) { }
}

/// <summary>
/// Valida que un concepto pertenezca al tipo de documento esperado antes de escribir en el SDK,
/// para que un endpoint (p. ej. /api/Compras) no pueda crear, editar, emitir o cancelar documentos de otro tipo.
/// </summary>
public interface IConceptoDocumentoValidator
{
    Task ValidarAsync(string codigoConcepto, int tipoDocumento, CancellationToken cancellationToken = default);
}

public class ConceptoDocumentoValidator : IConceptoDocumentoValidator
{
    private readonly IContpaqiDbContext _dbContext;

    public ConceptoDocumentoValidator(IContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ValidarAsync(string codigoConcepto, int tipoDocumento, CancellationToken cancellationToken = default)
    {
        // CCODIGOCONCEPTO en la tabla puede tener trailing spaces
        var codigo = (codigoConcepto ?? string.Empty).Trim();

        var tipoConcepto = await _dbContext.Conceptos
            .AsNoTracking()
            .Where(c => c.CCODIGOCONCEPTO != null && c.CCODIGOCONCEPTO.Trim() == codigo)
            .Select(c => (int?)c.CIDDOCUMENTODE)
            .FirstOrDefaultAsync(cancellationToken);

        if (tipoConcepto is null)
            throw new TipoDocumentoInvalidoException($"El concepto '{codigo}' no existe.");

        if (tipoConcepto != tipoDocumento)
            throw new TipoDocumentoInvalidoException(
                $"El concepto '{codigo}' es de tipo {TiposDocumento.Nombre(tipoConcepto.Value)}, " +
                $"no de tipo {TiposDocumento.Nombre(tipoDocumento)}.");
    }
}
