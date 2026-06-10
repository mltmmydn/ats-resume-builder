namespace AtsResumeBuilder.Api.Dtos;

public class ResumeDto
{
    public string Language { get; set; } = "en";
    public PersonalInfoDto? PersonalInfo { get; set; }
    public string Template { get; set; } = "ats";
    public string? ProfilePhotoBase64 { get; set; }
    public List<ExperienceDto> Experiences { get; set; } = new();
    public List<VolunteerExperienceDto> VolunteerExperiences { get; set; } = new();
    public List<EducationDto> Education { get; set; } = new();
    public List<ProjectDto> Projects { get; set; } = new();
    public List<SkillDto> Skills { get; set; } = new();
    public List<LanguageDto> Languages { get; set; } = new();
    public List<CertificateDto> Certificates { get; set; } = new();
    public string ReferenceMode { get; set; } = "uponRequest";
    public List<ReferenceDto> References { get; set; } = new();
}
