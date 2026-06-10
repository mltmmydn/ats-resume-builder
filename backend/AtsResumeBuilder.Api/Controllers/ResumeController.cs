using AtsResumeBuilder.Api.Dtos;
using AtsResumeBuilder.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace AtsResumeBuilder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResumeController : ControllerBase
{
    private readonly IResumePdfService _resumePdfService;

    public ResumeController(IResumePdfService resumePdfService)
    {
        _resumePdfService = resumePdfService;
    }

    [HttpPost("generate-pdf")]
    [Produces("application/pdf")]
    public ActionResult GeneratePdf([FromBody] ResumeDto resume)
    {
        var pdf = _resumePdfService.GeneratePdf(resume);
        return File(pdf, "application/pdf", CreatePdfFileName(resume.PersonalInfo?.FullName));
    }

    private static string CreatePdfFileName(string? fullName)
    {
        var normalizedName = Regex.Replace((fullName ?? string.Empty).Trim(), @"\s+", "_");
        var safeName = Regex.Replace(normalizedName, @"[^\p{L}\p{N}_-]", string.Empty);

        return safeName.Length > 0
            ? $"{safeName}_ATS_Resume.pdf"
            : "ATS_Resume.pdf";
    }
}
