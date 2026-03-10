using System.Runtime.InteropServices;

namespace AppComercial.Api.Sdk;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
public struct tDocumento
{
    public double aFolio;

    public int aNumMoneda;

    public double aTipoCambio;

    public double aImporte;

    public double aDescuentoDoc1;

    public double aDescuentoDoc2;

    public int aSistemaOrigen;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdkConstantes.kLongCodigo)]
    public string aCodConcepto;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdkConstantes.kLongSerie)]
    public string aSerie;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdkConstantes.kLongFecha)]
    public string aFecha;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdkConstantes.kLongCodigo)]
    public string aCodigoCteProv;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdkConstantes.kLongCodigo)]
    public string aCodigoAgente;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdkConstantes.kLongReferencia)]
    public string aReferencia;

    public int aAfecta;

    public double aGasto1;

    public double aGasto2;

    public double aGasto3;
}
