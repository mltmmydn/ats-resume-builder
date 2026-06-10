using AtsResumeBuilder.Api.Dtos;

namespace AtsResumeBuilder.Api.Services;

public interface IResumePdfService
{
    byte[] GeneratePdf(ResumeDto resume);
}
