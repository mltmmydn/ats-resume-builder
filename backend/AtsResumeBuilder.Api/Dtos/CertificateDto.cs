namespace AtsResumeBuilder.Api.Dtos;

public class CertificateDto
{
    public string? CertificateName { get; set; }
    public string? Issuer { get; set; }
    public string? Date { get; set; }
    public List<string> Details { get; set; } = new();
}
