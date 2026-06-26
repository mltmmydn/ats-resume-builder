using AtsResumeBuilder.Api.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AtsResumeBuilder.Api.Services;

public class ResumePdfService : IResumePdfService
{
    private const string TextColor = "#111827";
    private const string SecondaryTextColor = "#374151";
    private const string HeaderRuleColor = "#9CA3AF";
    private const string SectionRuleColor = "#4B5563";

    private sealed record ContactItem(string Text, string? Href = null);

    public byte[] GeneratePdf(ResumeDto resume)
    {
        var profilePhoto = GetProfilePhoto(resume);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginVertical(43.5f);
                page.MarginHorizontal(48);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style
                    .FontFamily("Arial", "Helvetica", "Liberation Sans", "DejaVu Sans")
                    .FontSize(9.4f)
                    .LineHeight(1.45f)
                    .FontColor(TextColor));

                page.Content().Column(column =>
                {
                    column.Spacing(0);
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
        column.Item().Column(header =>
        {
            header.Spacing(0);

            if (profilePhoto is not null)
            {
                header.Item()
                    .AlignRight()
                    .Width(61.5f)
                    .Height(61.5f)
                    .Image(profilePhoto)
                    .FitArea();
            }

            header.Item()
                .AlignCenter()
                .Text((personalInfo?.FullName ?? string.Empty).ToUpperInvariant())
                .FontSize(21)
                .LineHeight(1.1f)
                .Bold();

            if (!string.IsNullOrWhiteSpace(personalInfo?.JobTitle))
            {
                header.Item()
                    .PaddingTop(4.5f)
                    .AlignCenter()
                    .Text(personalInfo.JobTitle)
                    .FontSize(10.9f)
                    .LineHeight(1.2f)
                    .SemiBold();
            }

            var contactItems = BuildPrimaryContactItems(personalInfo).ToList();
            if (contactItems.Count > 0)
                AddContactLine(header, contactItems, 6);

            var optionalContacts = personalInfo?.CustomFields?
                .Where(field => HasText(field.Value) && (HasText(field.Label) || IsUrl(field.Value!)))
                .Select(BuildCustomContactItem)
                .Where(item => HasText(item.Text))
                .ToList() ?? [];

            if (optionalContacts.Count > 0)
                AddContactLine(header, optionalContacts, 3);
        });

        column.Item()
            .PaddingTop(9)
            .BorderBottom(0.75f)
            .BorderColor(HeaderRuleColor);
    }

    private static void AddContactLine(ColumnDescriptor column, IReadOnlyCollection<ContactItem> items, float topPadding)
    {
        column.Item()
            .PaddingTop(topPadding)
            .Text(text =>
            {
                text.AlignCenter();

                var index = 0;
                foreach (var item in items)
                {
                    if (index > 0)
                    {
                        text.Span(" | ")
                            .FontSize(8.4f)
                            .LineHeight(1.4f)
                            .FontColor(HeaderRuleColor);
                    }

                    var span = HasText(item.Href)
                        ? text.Hyperlink(item.Text, item.Href!)
                        : text.Span(item.Text);

                    span.FontSize(8.4f)
                        .LineHeight(1.4f)
                        .FontColor(SecondaryTextColor);

                    index++;
                }
            });
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
        var populatedExperiences = experiences.Where(HasExperienceContent).ToList();
        if (populatedExperiences.Count == 0)
            return;

        AddSectionHeading(column, "WORK EXPERIENCE");

        foreach (var experience in populatedExperiences)
        {
            var dateRange = JoinDateRange(
                experience.StartDate,
                NormalizeCurrentWorkStatus(experience.EndDate, language));

            column.Item().PaddingTop(7.5f).Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    if (HasText(experience.JobTitle))
                        left.Item().Text(experience.JobTitle!).FontSize(9.4f).SemiBold();

                    var companyAndLocation = JoinNonEmpty(
                        experience.CompanyName,
                        experience.Location);

                    if (companyAndLocation.Length > 0)
                        left.Item().Text(companyAndLocation).FontSize(9.1f).FontColor(SecondaryTextColor).Italic();
                });

                if (dateRange.Length > 0)
                    row.ConstantItem(120).AlignRight().Text(dateRange).FontSize(9.1f).SemiBold();
            });

            foreach (var responsibility in (experience.Responsibilities ?? []).Where(HasText))
                column.Item().PaddingTop(1).PaddingLeft(12.75f).Text($"\u2022 {responsibility.Trim()}").FontSize(9.4f);
        }
    }

    private static void AddEducation(
        ColumnDescriptor column,
        IReadOnlyCollection<EducationDto> education,
        string? language)
    {
        var populatedEducation = education.Where(HasEducationContent).ToList();
        if (populatedEducation.Count == 0)
            return;

        AddSectionHeading(column, "EDUCATION");

        foreach (var item in populatedEducation)
        {
            var program = string.Join(" / ", new[] { item.Degree, item.Department }.Where(HasText));
            var gpaLabel = string.Equals(language, "tr", StringComparison.OrdinalIgnoreCase)
                ? "GNO"
                : "GPA";
            var programAndGpa = JoinNonEmpty(
                program,
                HasText(item.Gpa) ? $"{gpaLabel}: {item.Gpa}" : null);

            var dateRange = JoinDateRange(item.StartDate, item.EndDate);

            column.Item().PaddingTop(7.5f).Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    if (HasText(item.SchoolName))
                        left.Item().Text(item.SchoolName!).FontSize(9.4f).SemiBold();

                    if (programAndGpa.Length > 0)
                        left.Item().Text(programAndGpa).FontSize(9.1f).FontColor(SecondaryTextColor).Italic();
                });

                if (dateRange.Length > 0)
                    row.ConstantItem(120).AlignRight().Text(dateRange).FontSize(9.1f).SemiBold();
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

            column.Item().PaddingTop(7.5f).Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    if (HasText(volunteer.Role))
                        left.Item().Text(volunteer.Role!).FontSize(9.4f).SemiBold();

                    var organizationAndLocation = JoinNonEmpty(
                        volunteer.OrganizationName,
                        volunteer.Location);

                    if (organizationAndLocation.Length > 0)
                        left.Item().Text(organizationAndLocation).FontSize(9.1f).FontColor(SecondaryTextColor).Italic();
                });

                if (dateRange.Length > 0)
                    row.ConstantItem(120).AlignRight().Text(dateRange).FontSize(9.1f).SemiBold();
            });

            foreach (var responsibility in (volunteer.Responsibilities ?? []).Where(HasText))
                column.Item().PaddingTop(1).PaddingLeft(12.75f).Text($"\u2022 {responsibility.Trim()}").FontSize(9.4f);
        }
    }

    private static void AddProjects(
        ColumnDescriptor column,
        IReadOnlyCollection<ProjectDto> projects)
    {
        var populatedProjects = projects.Where(HasProjectContent).ToList();
        if (populatedProjects.Count == 0)
            return;

        AddSectionHeading(column, "PROJECTS");

        foreach (var project in populatedProjects)
        {
            column.Item().PaddingTop(7.5f).Text(project.ProjectName ?? string.Empty).FontSize(9.4f).SemiBold();

            if (HasText(project.Description))
                column.Item().Text(project.Description!);

            if (HasText(project.Technologies))
                column.Item().Text(text =>
                {
                    text.Span("Technologies: ").FontSize(9.4f).SemiBold();
                    text.Span(project.Technologies!.Trim()).FontSize(9.4f);
                });
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
            {
                column.Item().PaddingBottom(1.5f).Text(text =>
                {
                    text.Span($"{category}: ").SemiBold();
                    text.Span(values);
                });
            }
        }
    }

    private static void AddLanguages(
        ColumnDescriptor column,
        IReadOnlyCollection<LanguageDto> languages)
    {
        var populatedLanguages = languages.Where(HasLanguageContent).ToList();
        if (populatedLanguages.Count == 0)
            return;

        AddSectionHeading(column, "LANGUAGES");

        foreach (var language in populatedLanguages)
        {
            var line = JoinTitle(language.LanguageName, language.Level);
            if (line.Length > 0)
                column.Item().Text(line).FontSize(9.4f);
        }
    }

    private static void AddCertificates(
        ColumnDescriptor column,
        IReadOnlyCollection<CertificateDto> certificates)
    {
        var populatedCertificates = certificates.Where(HasCertificateContent).ToList();
        if (populatedCertificates.Count == 0)
            return;

        AddSectionHeading(column, "CERTIFICATES");

        foreach (var certificate in populatedCertificates)
        {
            column.Item().PaddingTop(5.25f).Row(row =>
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
                        .FontSize(9.1f)
                        .SemiBold();
            });

            foreach (var detail in (certificate.Details ?? []).Where(HasText))
                column.Item().PaddingLeft(16.5f).Text($"\u2022 {detail.Trim()}").FontSize(9.1f);
        }
    }

    private static void AddReferences(
        ColumnDescriptor column,
        string? referenceMode,
        IReadOnlyCollection<ReferenceDto> references)
    {
        if (string.Equals(referenceMode, "none", StringComparison.OrdinalIgnoreCase))
            return;

        if (!string.Equals(referenceMode, "contacts", StringComparison.OrdinalIgnoreCase))
        {
            AddSectionHeading(column, "REFERENCES");
            column.Item().Text("References available upon request.");
            return;
        }

        var populatedReferences = references.Where(HasReferenceContent).ToList();
        if (populatedReferences.Count == 0)
            return;

        AddSectionHeading(column, "REFERENCES");

        foreach (var reference in populatedReferences)
        {
            var role = JoinNonEmpty(reference.JobTitle, reference.Company);
            var heading = JoinTitle(reference.FullName, role);
            var contact = JoinNonEmpty(reference.Email, reference.Phone, reference.Relationship);

            if (heading.Length > 0)
                column.Item().PaddingTop(7.5f).Text(heading).FontSize(9.4f).SemiBold();

            if (contact.Length > 0)
                AddReferenceContactLine(column, reference);
        }
    }

    private static void AddSectionHeading(ColumnDescriptor column, string heading)
    {
        column.Item()
            .PaddingTop(11.25f)
            .PaddingBottom(2.25f)
            .BorderBottom(0.75f)
            .BorderColor(SectionRuleColor)
            .Text(heading)
            .FontSize(10.2f)
            .LineHeight(1.25f)
            .LetterSpacing(0.6f)
            .SemiBold();
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

    private static bool HasExperienceContent(ExperienceDto experience)
    {
        return HasText(experience.CompanyName) ||
               HasText(experience.JobTitle) ||
               HasText(experience.Location) ||
               HasText(experience.StartDate) ||
               HasText(experience.EndDate) ||
               (experience.Responsibilities?.Any(HasText) ?? false);
    }

    private static bool HasEducationContent(EducationDto education)
    {
        return HasText(education.SchoolName) ||
               HasText(education.Degree) ||
               HasText(education.Department) ||
               HasText(education.StartDate) ||
               HasText(education.EndDate) ||
               HasText(education.Gpa);
    }

    private static bool HasProjectContent(ProjectDto project)
    {
        return HasText(project.ProjectName) ||
               HasText(project.Description) ||
               HasText(project.Technologies);
    }

    private static bool HasLanguageContent(LanguageDto language)
    {
        return HasText(language.LanguageName) || HasText(language.Level);
    }

    private static bool HasCertificateContent(CertificateDto certificate)
    {
        return HasText(certificate.CertificateName) ||
               HasText(certificate.Issuer) ||
               HasText(certificate.Date) ||
               (certificate.Details?.Any(HasText) ?? false);
    }

    private static bool HasReferenceContent(ReferenceDto reference)
    {
        return HasText(reference.FullName) ||
               HasText(reference.JobTitle) ||
               HasText(reference.Company) ||
               HasText(reference.Email) ||
               HasText(reference.Phone) ||
               HasText(reference.Relationship);
    }

    private static IEnumerable<ContactItem> BuildPrimaryContactItems(PersonalInfoDto? personalInfo)
    {
        if (HasText(personalInfo?.Email))
            yield return new ContactItem(personalInfo!.Email!.Trim(), CreateMailtoLink(personalInfo.Email));

        if (HasText(personalInfo?.Phone))
            yield return new ContactItem(personalInfo!.Phone!.Trim());

        if (HasText(personalInfo?.Location))
            yield return new ContactItem(personalInfo!.Location!.Trim());
    }

    private static ContactItem BuildCustomContactItem(CustomFieldDto field)
    {
        var text = LabeledValue(
            field.Label,
            field.Value,
            HasText(field.DisplayMode)
                ? field.DisplayMode
                : HasText(field.Label) ? "label" : "short") ?? string.Empty;

        return new ContactItem(text, IsUrl(field.Value ?? string.Empty) ? NormalizeUrl(field.Value!) : null);
    }

    private static void AddReferenceContactLine(ColumnDescriptor column, ReferenceDto reference)
    {
        column.Item().Text(text =>
        {
            var index = 0;
            foreach (var item in BuildReferenceContactItems(reference))
            {
                if (index > 0)
                    text.Span(" | ").FontSize(8.4f).FontColor(HeaderRuleColor);

                var span = HasText(item.Href)
                    ? text.Hyperlink(item.Text, item.Href!)
                    : text.Span(item.Text);

                span.FontSize(8.4f)
                    .FontColor(SecondaryTextColor);

                index++;
            }
        });
    }

    private static IEnumerable<ContactItem> BuildReferenceContactItems(ReferenceDto reference)
    {
        if (HasText(reference.Email))
            yield return new ContactItem(reference.Email!.Trim(), CreateMailtoLink(reference.Email));

        if (HasText(reference.Phone))
            yield return new ContactItem(reference.Phone!.Trim());

        if (HasText(reference.Relationship))
            yield return new ContactItem(reference.Relationship!.Trim());
    }

    private static string? LabeledValue(string? label, string? value, string? displayMode = null)
    {
        if (!HasText(value))
            return null;

        var cleanLabel = HasText(label) ? label!.Trim().TrimEnd(':') : string.Empty;
        if (IsUrl(value!))
        {
            var shortenedUrl = ShortenUrl(value!);
            if (!HasText(displayMode))
                return cleanLabel.Length > 0 ? $"{cleanLabel}: {shortenedUrl}" : shortenedUrl;

            return displayMode!.Trim().ToLowerInvariant() switch
            {
                "full" => value!.Trim(),
                "label" when cleanLabel.Length > 0 => cleanLabel,
                _ => shortenedUrl
            };
        }

        return cleanLabel.Length > 0
            ? $"{cleanLabel}: {value!.Trim()}"
            : value!.Trim();
    }

    private static bool IsUrl(string value)
    {
        var normalizedValue = value.Trim();

        if (Uri.TryCreate(normalizedValue, UriKind.Absolute, out var absoluteUri))
            return absoluteUri.Scheme is "http" or "https";

        return System.Text.RegularExpressions.Regex.IsMatch(
            normalizedValue,
            @"^(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z]{2,}(?:[/:?#][^\s]*)?$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static string NormalizeUrl(string value)
    {
        var normalizedValue = value.Trim();

        return normalizedValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               normalizedValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? normalizedValue
            : $"https://{normalizedValue}";
    }

    private static string CreateMailtoLink(string email)
    {
        return $"mailto:{email.Trim()}";
    }

    private static string ShortenUrl(string value)
    {
        var shortenedValue = value.Trim();

        if (shortenedValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            shortenedValue = shortenedValue["https://".Length..];
        else if (shortenedValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            shortenedValue = shortenedValue["http://".Length..];

        if (shortenedValue.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            shortenedValue = shortenedValue["www.".Length..];

        return shortenedValue.TrimEnd('/');
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
