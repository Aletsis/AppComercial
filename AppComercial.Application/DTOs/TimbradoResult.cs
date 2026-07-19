namespace AppComercial.Application.DTOs;

public class TimbradoResult
{
    public string UUID { get; set; } = string.Empty;
    public string CadenaOriginal { get; set; } = string.Empty;
    public string SelloDigitalEmisor { get; set; } = string.Empty;
    public string SelloDigitalSAT { get; set; } = string.Empty;
    public string NoCertificadoEmisor { get; set; } = string.Empty;
    public string NoCertificadoSAT { get; set; } = string.Empty;
    public string FechaTimbrado { get; set; } = string.Empty;
    public string XmlAntigravity { get; set; } = string.Empty;
}
