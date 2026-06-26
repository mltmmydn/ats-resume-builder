using AtsResumeBuilder.Api.Dtos;

namespace AtsResumeBuilder.Api.Services;

public sealed record ResumePdfResult(byte[] Pdf, string Engine);

public interface IResumePdfService
{
    ResumePdfResult GeneratePdf(ResumeDto resume);
}
