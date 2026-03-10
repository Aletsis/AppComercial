using System.Runtime.InteropServices;

namespace AppComercial.Api.Sdk;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
public struct tMovimiento
{
    public int aConsecutivo;

    public double aUnidades;

    public double aPrecio;

    public double aCosto;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdkConstantes.kLongCodigo)]
    public string aCodProdSer;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdkConstantes.kLongCodigo)]
    public string aCodAlmacen;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdkConstantes.kLongReferencia)]
    public string aReferencia;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdkConstantes.kLongCodigo)]
    public string aCodClasificacion;
}
