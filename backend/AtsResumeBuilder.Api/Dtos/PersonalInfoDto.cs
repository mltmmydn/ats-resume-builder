namespace AtsResumeBuilder.Api.Dtos;

public class PersonalInfoDto
{
    public string? FullName { get; set; }
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? Summary { get; set; }
    public List<CustomFieldDto> CustomFields { get; set; } = new();
}
