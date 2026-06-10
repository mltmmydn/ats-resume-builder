namespace AtsResumeBuilder.Api.Dtos;

public class ExperienceDto
{
    public string? CompanyName { get; set; }
    public string? JobTitle { get; set; }
    public string? Location { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public List<string> Responsibilities { get; set; } = new();
}
