using AtsResumeBuilder.Api.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AtsResumeBuilder.Api.Services;

public class ResumePdfService : IResumePdfService
{
    public byte[] GeneratePdf(ResumeDto resume)
    {
        var profilePhoto = GetProfilePhoto(resume);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style
                    .FontSize(10.5f)
                    .LineHeight(1.35f)
                    .FontColor(Colors.Black));

                page.Content().Column(column =>
                {
                    column.Spacing(3);
                    AddHeader(column, resume.PersonalInfo, profilePhoto);
                    AddSummary(column, resume.PersonalInfo?.Summary);
                    AddExperiences(column, resume.Experiences ?? [], resume.Language);
                    AddVolunteerExperiences(column, resume.VolunteerExperiences ?? [], resume.Language);
                    AddEducation(column, resume.Education ?? [], resume.Language);
                    AddProjects(column, resume.Projects ?? []);
                    AddSkills(column, resume.Skills ?? []);
                    AddLanguages(column, resume.Languages ?? []);
                    AddCertificates(column, resume.Certificates ?? []);
                    AddReferences(column, resume.ReferenceMode, resume.References ?? []);
                });
            });
        }).GeneratePdf();
    }

    private static void AddHeader(
        ColumnDescriptor column,
        PersonalInfoDto? personalInfo,
        byte[]? profilePhoto)
    {
        if (profilePhoto is not null)
        {
            column.Item()
                .AlignRight()
                .Width(72)
                .Height(72)
                .Image(profilePhoto)
                .FitArea();
        }

        column.Item()
            .AlignCenter()
            .Text(personalInfo?.FullName ?? string.Empty)
            .FontSize(20)
            .Bold();

        if (!string.IsNullOrWhiteSpace(personalInfo?.JobTitle))
            column.Item().AlignCenter().Text(personalInfo.JobTitle).FontSize(12).SemiBold();

        var contact = JoinNonEmpty(
            personalInfo?.Email,
            personalInfo?.Phone,
            personalInfo?.Location);

        if (contact.Length > 0)
            column.Item().AlignCenter().Text(contact).FontSize(9);

        var optionalContacts = new List<string?>
        {
            LabeledValue("LinkedIn", personalInfo?.LinkedInUrl),
            LabeledValue("GitHub", personalInfo?.GitHubUrl),
            LabeledValue("Portfolio", personalInfo?.PortfolioUrl)
        };

        optionalContacts.AddRange(personalInfo?.CustomFields?
            .Where(field => HasText(field.Label) && HasText(field.Value))
            .Select(field => LabeledValue(field.Label, field.Value)) ?? []);

        var populatedOptionalContacts = optionalContacts
            .Where(HasText)
            .Select(value => value!)
            .ToArray();

        if (populatedOptionalContacts.Length > 0)
            column.Item().AlignCenter().Text(string.Join(" | ", populatedOptionalContacts)).FontSize(9);

        column.Item().PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
    }

    private static void AddSummary(ColumnDescriptor column, string? summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
            return;

        AddSectionHeading(column, "PROFESSIONAL SUMMARY");
        column.Item().Text(summary);
    }

    private static void AddExperiences(
        ColumnDescriptor column,
        IReadOnlyCollection<ExperienceDto> experiences,
        string? language)
    {
        if (experiences.Count == 0)
            return;

        AddSectionHeading(column, "WORK EXPERIENCE");

        foreach (var experience in experiences)
        {
            var dateRange = JoinDateRange(
                experience.StartDate,
                NormalizeCurrentWorkStatus(experience.EndDate, language));

            column.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    if (HasText(experience.JobTitle))
                        left.Item().Text(experience.JobTitle!).SemiBold();

                    var companyAndLocation = JoinNonEmpty(
                        experience.CompanyName,
                        experience.Location);

                    if (companyAndLocation.Length > 0)
                        left.Item().Text(companyAndLocation).FontSize(9.5f).Italic();
                });

                if (dateRange.Length > 0)
                    row.ConstantItem(120).AlignRight().Text(dateRange).FontSize(9.5f).SemiBold();
            });

            foreach (var responsibility in (experience.Responsibilities ?? []).Where(HasText))
                column.Item().PaddingLeft(10).Text($"\u2022 {responsibility.Trim()}");
        }
    }

    private static void AddEducation(
        ColumnDescriptor column,
        IReadOnlyCollection<EducationDto> education,
        string? language)
    {
        if (education.Count == 0)
            return;

        AddSectionHeading(column, "EDUCATION");

        foreach (var item in education)
        {
            var program = string.Join(" / ", new[] { item.Degree, item.Department }.Where(HasText));
            var gpaLabel = string.Equals(language, "tr", StringComparison.OrdinalIgnoreCase)
                ? "GNO"
                : "GPA";
            var programAndGpa = JoinNonEmpty(
                program,
                HasText(item.Gpa) ? $"{gpaLabel}: {item.Gpa}" : null);

            var dateRange = JoinDateRange(item.StartDate, item.EndDate);

            column.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    if (HasText(item.SchoolName))
                        left.Item().Text(item.SchoolName!).SemiBold();

                    if (programAndGpa.Length > 0)
                        left.Item().Text(programAndGpa).FontSize(9.5f).Italic();
                });

                if (dateRange.Length > 0)
                    row.ConstantItem(120).AlignRight().Text(dateRange).FontSize(9.5f).SemiBold();
            });
        }
    }

    private static void AddVolunteerExperiences(
        ColumnDescriptor column,
        IReadOnlyCollection<VolunteerExperienceDto> volunteerExperiences,
        string? language)
    {
        var populatedExperiences = volunteerExperiences.Where(HasVolunteerContent).ToList();
        if (populatedExperiences.Count == 0)
            return;

        AddSectionHeading(
            column,
            string.Equals(language, "tr", StringComparison.OrdinalIgnoreCase)
                ? "GÖNÜLLÜ DENEYİM"
                : "VOLUNTEER EXPERIENCE");

        foreach (var volunteer in populatedExperiences)
        {
            var endDate = volunteer.IsCurrent
                ? CurrentRoleLabel(language)
                : NormalizeCurrentWorkStatus(volunteer.EndDate, language);
            var dateRange = JoinDateRange(volunteer.StartDate, endDate);

            column.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    if (HasText(volunteer.Role))
                        left.Item().Text(volunteer.Role!).SemiBold();

                    var organizationAndLocation = JoinNonEmpty(
                        volunteer.OrganizationName,
                        volunteer.Location);

                    if (organizationAndLocation.Length > 0)
                        left.Item().Text(organizationAndLocation).FontSize(9.5f).Italic();
                });

                if (dateRange.Length > 0)
                    row.ConstantItem(120).AlignRight().Text(dateRange).FontSize(9.5f).SemiBold();
            });

            foreach (var responsibility in (volunteer.Responsibilities ?? []).Where(HasText))
                column.Item().PaddingLeft(10).Text($"\u2022 {responsibility.Trim()}");
        }
    }

    private static void AddProjects(
        ColumnDescriptor column,
        IReadOnlyCollection<ProjectDto> projects)
    {
        if (projects.Count == 0)
            return;

        AddSectionHeading(column, "PROJECTS");

        foreach (var project in projects)
        {
            column.Item().PaddingTop(3).Text(project.ProjectName ?? string.Empty).SemiBold();

            if (HasText(project.Description))
                column.Item().Text(project.Description!);

            if (HasText(project.Technologies))
                column.Item().Text($"Technologies: {project.Technologies}").FontSize(9);
        }
    }

    private static void AddSkills(ColumnDescriptor column, IReadOnlyCollection<SkillDto> skills)
    {
        var populatedSkills = skills
            .Where(skill => skill.Skills?.Any(HasText) ?? false)
            .ToList();

        if (populatedSkills.Count == 0)
            return;

        AddSectionHeading(column, "SKILLS");

        foreach (var skill in populatedSkills)
        {
            var values = string.Join(
                ", ",
                (skill.Skills ?? []).Where(HasText).Select(value => value.Trim()));
            var category = HasText(skill.Category)
                ? skill.Category!.Trim().TrimEnd(':').Trim()
                : "Skills";
            category = category.Length > 0 ? category : "Skills";
            var line = values.Length > 0 ? $"{category}: {values}" : string.Empty;

            if (line.Length > 0)
                column.Item().Text(line);
        }
    }

    private static void AddLanguages(
        ColumnDescriptor column,
        IReadOnlyCollection<LanguageDto> languages)
    {
        if (languages.Count == 0)
            return;

        AddSectionHeading(column, "LANGUAGES");

        foreach (var language in languages)
        {
            var line = JoinTitle(language.LanguageName, language.Level);
            if (line.Length > 0)
                column.Item().Text(line);
        }
    }

    private static void AddCertificates(
        ColumnDescriptor column,
        IReadOnlyCollection<CertificateDto> certificates)
    {
        if (certificates.Count == 0)
            return;

        AddSectionHeading(column, "CERTIFICATES");

        foreach (var certificate in certificates)
        {
            column.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    var hasName = HasText(certificate.CertificateName);

                    if (hasName)
                        text.Span($"\u2022 {certificate.CertificateName!.Trim()}").SemiBold();

                    if (HasText(certificate.Issuer))
                        text.Span(hasName
                            ? $" | {certificate.Issuer!.Trim()}"
                            : certificate.Issuer!.Trim());
                });

                if (HasText(certificate.Date))
                    row.ConstantItem(120)
                        .AlignRight()
                        .Text(certificate.Date!.Trim())
                        .FontSize(9.5f)
                        .SemiBold();
            });

            foreach (var detail in (certificate.Details ?? []).Where(HasText))
                column.Item().PaddingLeft(14).Text($"\u2022 {detail.Trim()}").FontSize(9.5f);
        }
    }

    private static void AddReferences(
        ColumnDescriptor column,
        string? referenceMode,
        IReadOnlyCollection<ReferenceDto> references)
    {
        if (string.Equals(referenceMode, "none", StringComparison.OrdinalIgnoreCase))
            return;

        AddSectionHeading(column, "REFERENCES");

        if (!string.Equals(referenceMode, "contacts", StringComparison.OrdinalIgnoreCase))
        {
            column.Item().Text("References available upon request.");
            return;
        }

        foreach (var reference in references)
        {
            var role = JoinNonEmpty(reference.JobTitle, reference.Company);
            var heading = JoinTitle(reference.FullName, role);
            var contact = JoinNonEmpty(reference.Email, reference.Phone, reference.Relationship);

            if (heading.Length > 0)
                column.Item().PaddingTop(3).Text(heading).SemiBold();

            if (contact.Length > 0)
                column.Item().Text(contact).FontSize(9);
        }
    }

    private static void AddSectionHeading(ColumnDescriptor column, string heading)
    {
        column.Item()
            .PaddingTop(8)
            .PaddingBottom(2)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Darken2)
            .Text(heading)
            .FontSize(12)
            .Bold();
    }

    private static byte[]? GetProfilePhoto(ResumeDto resume)
    {
        if (!string.Equals(resume.Template, "modern", StringComparison.OrdinalIgnoreCase) ||
            !HasText(resume.ProfilePhotoBase64))
        {
            return null;
        }

        try
        {
            var base64 = resume.ProfilePhotoBase64!;
            var separatorIndex = base64.IndexOf(',');

            if (base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) &&
                separatorIndex >= 0)
            {
                base64 = base64[(separatorIndex + 1)..];
            }

            var image = Convert.FromBase64String(base64);
            return IsSupportedImage(image) ? image : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static string JoinDateRange(string? startDate, string? endDate)
    {
        return string.Join(" - ", new[] { startDate, endDate }.Where(HasText));
    }

    private static string? NormalizeCurrentWorkStatus(string? value, string? language)
    {
        if (!HasText(value))
            return value;

        var normalized = value!.Trim();
        if (!string.Equals(normalized, "Ongoing", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(normalized, "On going", StringComparison.OrdinalIgnoreCase))
        {
            return normalized;
        }

        return CurrentRoleLabel(language);
    }

    private static string CurrentRoleLabel(string? language)
    {
        return string.Equals(language, "tr", StringComparison.OrdinalIgnoreCase)
            ? "Devam ediyor"
            : "Present";
    }

    private static bool HasVolunteerContent(VolunteerExperienceDto volunteer)
    {
        return HasText(volunteer.OrganizationName) ||
               HasText(volunteer.Role) ||
               HasText(volunteer.Location) ||
               HasText(volunteer.StartDate) ||
               HasText(volunteer.EndDate) ||
               (volunteer.Responsibilities?.Any(HasText) ?? false);
    }

    private static string? LabeledValue(string? label, string? value)
    {
        return HasText(label) && HasText(value)
            ? $"{label!.Trim().TrimEnd(':')}: {value!.Trim()}"
            : null;
    }

    private static string JoinTitle(string? primary, string? secondary)
    {
        return string.Join(" - ", new[] { primary, secondary }.Where(HasText));
    }

    private static string JoinNonEmpty(params string?[] values)
    {
        return string.Join(" | ", values.Where(HasText).Select(value => value!.Trim()));
    }

    private static bool HasText(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool IsSupportedImage(byte[] image)
    {
        var isPng = image.Length >= 8 &&
                    image[0] == 0x89 &&
                    image[1] == 0x50 &&
                    image[2] == 0x4E &&
                    image[3] == 0x47 &&
                    image[4] == 0x0D &&
                    image[5] == 0x0A &&
                    image[6] == 0x1A &&
                    image[7] == 0x0A;

        var isJpeg = image.Length >= 3 &&
                     image[0] == 0xFF &&
                     image[1] == 0xD8 &&
                     image[2] == 0xFF;

        return isPng || isJpeg;
    }
}
