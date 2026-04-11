using System.Runtime.InteropServices;

namespace AppComercial.Domain.Interfaces.SdkModels;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
public struct tUnidad
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdkConstantes.kLongNombre)]
    public string cNombreUnidad;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] // Abreviatura unit length
    public string cAbreviatura;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdkConstantes.kLongDesCorta)]
    public string cDespliegue;
}
