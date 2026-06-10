namespace AtsResumeBuilder.Api.Dtos;

public class SkillDto
{
    public string? Category { get; set; }
    public List<string> Skills { get; set; } = new();
}
