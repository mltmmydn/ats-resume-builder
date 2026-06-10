namespace AtsResumeBuilder.Api.Dtos;

public class VolunteerExperienceDto
{
    public string? OrganizationName { get; set; }
    public string? Role { get; set; }
    public string? Location { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public List<string> Responsibilities { get; set; } = new();
}
