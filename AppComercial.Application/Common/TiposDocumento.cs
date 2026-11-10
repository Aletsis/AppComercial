namespace AppComercial.Application.Common;

/// <summary>
/// Tipos de documento de CONTPAQi Comercial: valores de CIDDOCUMENTODE (tabla admDocumentosModelo)
/// que tienen los conceptos en admConceptos. Es el dato que identifica de forma confiable qué tipo
/// de documento genera un concepto, a diferencia de CNATURALEZA.
/// Se pueden verificar con: SELECT CIDDOCUMENTODE, CDESCRIPCION FROM admDocumentosModelo
/// </summary>
public static class TiposDocumento
{
    public const int Cotizacion     = 1;
    public const int Factura        = 4;
    public const int NotaCredito    = 7;
    public const int Compra         = 19;
    public const int EntradaAlmacen = 32;
    public const int SalidaAlmacen  = 33;
    public const int Traspaso       = 34;

    public static string Nombre(int tipoDocumento) => tipoDocumento switch
    {
        Cotizacion     => "Cotización",
        Factura        => "Factura",
        NotaCredito    => "Nota de Crédito",
        Compra         => "Compra",
        EntradaAlmacen => "Entrada de Almacén",
        SalidaAlmacen  => "Salida de Almacén",
        Traspaso       => "Traspaso",
        _              => $"Tipo de documento {tipoDocumento}"
    };
}
