using AtsResumeBuilder.Api.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace AtsResumeBuilder.Api.Services;

public class ResumePdfService : IResumePdfService
{
    private const string TextColor = "#111827";
    private const string SecondaryTextColor = "#374151";
    private const string HeadingTextColor = "#4B5563";
    private const string HeaderRuleColor = "#D1D5DB";
    private const string SectionRuleColor = "#D1D5DB";
    private const float PxToPoint = 0.75f;
    private const float BaseFontSize = 9.4f;
    private const float ContactFontSize = 8.4f;
    private const float HeaderNameFontSize = 21;
    private const float HeaderTitleFontSize = 10.9f;
    private const float SectionHeadingFontSize = 10.1f;
    private const float SectionContentTopSpacing = 5.25f;
    private const float EntrySpacing = 7.5f;
    private const float DateColumnWidth = 120;
    private const int ProfilePhotoPixelSize = 82;
    private const int ProfilePhotoCornerRadius = 9;
    private static readonly string[] ChromeExecutableCandidates =
    [
        "/usr/bin/google-chrome",
        "/usr/bin/google-chrome-stable",
        "/usr/bin/chromium",
        "/usr/bin/chromium-browser",
        "/opt/google/chrome/chrome",
        @"C:\Program Files\Google\Chrome\Application\chrome.exe",
        @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
        @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
        @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
    ];

    private sealed record ContactItem(string Text, string? Href = null);

    public ResumePdfResult GeneratePdf(ResumeDto resume)
    {
        Console.WriteLine(
            "[PDF] Generation started. Requested template: {0}; language: {1}; backend HTML engine will be attempted first.",
            resume.Template ?? "unknown",
            resume.Language ?? "unknown");

        var htmlPdf = TryGenerateHtmlPdf(resume);
        if (htmlPdf is not null)
        {
            Console.WriteLine("[PDF] Engine selected: html-chrome.");
            return new ResumePdfResult(htmlPdf, "html-chrome");
        }

        Console.WriteLine("[PDF] Engine selected: questpdf-fallback.");
        return new ResumePdfResult(GenerateQuestPdf(resume), "questpdf-fallback");
    }

    private static byte[]? TryGenerateHtmlPdf(ResumeDto resume)
    {
        var browserPath = FindBrowserExecutable();
        if (!HasText(browserPath))
        {
            Console.WriteLine("[PDF] HTML/Chrome rendering unavailable: no Chrome/Chromium executable found.");
            return null;
        }

        Console.WriteLine("[PDF] HTML/Chrome rendering available. Executable: {0}", browserPath);

        var tempDirectory = Path.Combine(Path.GetTempPath(), $"ats-resume-pdf-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        var htmlPath = Path.Combine(tempDirectory, "resume.html");
        var pdfPath = Path.Combine(tempDirectory, "resume.pdf");

        try
        {
            File.WriteAllText(htmlPath, BuildResumeHtml(resume), new UTF8Encoding(false));

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = browserPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            process.StartInfo.ArgumentList.Add("--headless");
            process.StartInfo.ArgumentList.Add("--disable-gpu");
            process.StartInfo.ArgumentList.Add("--disable-dev-shm-usage");
            process.StartInfo.ArgumentList.Add("--no-sandbox");
            process.StartInfo.ArgumentList.Add("--no-pdf-header-footer");
            process.StartInfo.ArgumentList.Add("--print-to-pdf-no-header");
            process.StartInfo.ArgumentList.Add("--run-all-compositor-stages-before-draw");
            process.StartInfo.ArgumentList.Add("--virtual-time-budget=1000");
            process.StartInfo.ArgumentList.Add($"--print-to-pdf={pdfPath}");
            process.StartInfo.ArgumentList.Add(new Uri(htmlPath).AbsoluteUri);

            if (!process.Start())
            {
                Console.WriteLine("[PDF] HTML/Chrome rendering failed: browser process did not start.");
                return null;
            }

            if (!process.WaitForExit(20000))
            {
                Console.WriteLine("[PDF] HTML/Chrome rendering timed out after 20 seconds.");
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Falling back to QuestPDF is safer than surfacing browser cleanup errors.
                }

                return null;
            }

            if (process.ExitCode != 0 || !File.Exists(pdfPath))
            {
                Console.WriteLine(
                    "[PDF] HTML/Chrome rendering failed. ExitCode: {0}; PdfExists: {1}.",
                    process.ExitCode,
                    File.Exists(pdfPath));
                return null;
            }

            var pdf = File.ReadAllBytes(pdfPath);
            if (!IsPdf(pdf))
            {
                Console.WriteLine("[PDF] HTML/Chrome rendering produced an invalid PDF response.");
                return null;
            }

            return pdf;
        }
        catch (Exception exception)
        {
            Console.WriteLine("[PDF] HTML/Chrome rendering failed with exception: {0}", exception.Message);
            return null;
        }
        finally
        {
            try
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
            catch
            {
                // Temporary files are best-effort cleanup.
            }
        }
    }

    private static string? FindBrowserExecutable()
    {
        var configuredPath = Environment.GetEnvironmentVariable("PDF_CHROME_PATH");
        if (HasText(configuredPath) && File.Exists(configuredPath))
            return configuredPath;

        return ChromeExecutableCandidates.FirstOrDefault(File.Exists);
    }

    private static bool IsPdf(byte[] bytes)
    {
        if (bytes.Length < 4)
            return false;

        for (var index = 0; index <= Math.Min(bytes.Length - 4, 1024); index++)
        {
            if (bytes[index] == 0x25 &&
                bytes[index + 1] == 0x50 &&
                bytes[index + 2] == 0x44 &&
                bytes[index + 3] == 0x46)
            {
                return true;
            }
        }

        return false;
    }

    private static byte[] GenerateQuestPdf(ResumeDto resume)
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
                    .FontSize(BaseFontSize)
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

    private static string BuildResumeHtml(ResumeDto resume)
    {
        var personalInfo = resume.PersonalInfo;
        var isModern = string.Equals(resume.Template, "modern", StringComparison.OrdinalIgnoreCase) &&
                       HasText(resume.ProfilePhotoBase64);
        var css = BuildResumeCss();
        var sb = new StringBuilder();

        sb.AppendLine("<!doctype html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine("<style>");
        sb.AppendLine(css);
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.Append("<article class=\"resume-preview");
        if (isModern)
            sb.Append(" modern-template");
        sb.AppendLine("\">");

        sb.AppendLine("<header class=\"resume-header\">");
        sb.AppendLine("<div class=\"resume-header-content\">");
        sb.Append("<h1>").Append(Html(personalInfo?.FullName)).AppendLine("</h1>");

        if (HasText(personalInfo?.JobTitle))
            sb.Append("<p class=\"resume-title\">").Append(Html(personalInfo!.JobTitle)).AppendLine("</p>");

        AppendContactLine(sb, BuildPrimaryContactItems(personalInfo).ToList(), optional: false);

        var optionalContacts = personalInfo?.CustomFields?
            .Where(field => HasText(field.Value) && (HasText(field.Label) || IsUrl(field.Value!)))
            .Select(BuildCustomContactItem)
            .Where(item => HasText(item.Text))
            .ToList() ?? [];

        if (optionalContacts.Count > 0)
            AppendContactLine(sb, optionalContacts, optional: true);

        sb.AppendLine("</div>");

        if (isModern)
        {
            var imageSource = NormalizeProfilePhotoDataUri(resume.ProfilePhotoBase64);
            if (HasText(imageSource))
            {
                sb.Append("<div class=\"profile-photo-frame\" style=\"background-image: url('")
                    .Append(Attr(imageSource))
                    .AppendLine("')\">");
                sb.Append("<img class=\"profile-photo\" src=\"")
                    .Append(Attr(imageSource))
                    .AppendLine("\" alt=\"\">");
                sb.AppendLine("</div>");
            }
        }

        sb.AppendLine("</header>");

        if (HasText(personalInfo?.Summary))
        {
            AppendSection(sb, T(resume.Language, "Professional Summary"), section =>
            {
                section.Append("<p>").Append(Html(personalInfo!.Summary)).AppendLine("</p>");
            });
        }

        var experiences = (resume.Experiences ?? []).Where(HasExperienceContent).ToList();
        if (experiences.Count > 0)
        {
            AppendSection(sb, T(resume.Language, "Work Experience"), section =>
            {
                foreach (var experience in experiences)
                {
                    var dateRange = JoinDateRange(
                        experience.StartDate,
                        NormalizeCurrentWorkStatus(experience.EndDate, resume.Language));
                    var subtitle = JoinNonEmpty(experience.CompanyName, experience.Location);

                    AppendEntry(
                        section,
                        experience.JobTitle,
                        subtitle,
                        dateRange,
                        experience.Responsibilities);
                }
            });
        }

        var volunteerExperiences = (resume.VolunteerExperiences ?? []).Where(HasVolunteerContent).ToList();
        if (volunteerExperiences.Count > 0)
        {
            AppendSection(sb, T(resume.Language, "Volunteer Experience"), section =>
            {
                foreach (var volunteer in volunteerExperiences)
                {
                    var endDate = volunteer.IsCurrent
                        ? CurrentRoleLabel(resume.Language)
                        : NormalizeCurrentWorkStatus(volunteer.EndDate, resume.Language);
                    var dateRange = JoinDateRange(volunteer.StartDate, endDate);
                    var subtitle = JoinNonEmpty(volunteer.OrganizationName, volunteer.Location);

                    AppendEntry(
                        section,
                        volunteer.Role,
                        subtitle,
                        dateRange,
                        volunteer.Responsibilities);
                }
            });
        }

        var education = (resume.Education ?? []).Where(HasEducationContent).ToList();
        if (education.Count > 0)
        {
            AppendSection(sb, T(resume.Language, "Education"), section =>
            {
                foreach (var item in education)
                {
                    var program = string.Join(" / ", new[] { item.Degree, item.Department }.Where(HasText));
                    var gpaLabel = string.Equals(resume.Language, "tr", StringComparison.OrdinalIgnoreCase)
                        ? "GNO"
                        : "GPA";
                    var subtitle = JoinNonEmpty(
                        program,
                        HasText(item.Gpa) ? $"{gpaLabel}: {item.Gpa}" : null);
                    var dateRange = JoinDateRange(item.StartDate, item.EndDate);

                    AppendEntry(section, item.SchoolName, subtitle, dateRange, null);
                }
            });
        }

        var projects = (resume.Projects ?? []).Where(HasProjectContent).ToList();
        if (projects.Count > 0)
        {
            AppendSection(sb, T(resume.Language, "Projects"), section =>
            {
                foreach (var project in projects)
                {
                    section.AppendLine("<div class=\"resume-entry\">");
                    section.Append("<h3>").Append(Html(project.ProjectName)).AppendLine("</h3>");
                    if (HasText(project.Description))
                        section.Append("<p>").Append(Html(project.Description)).AppendLine("</p>");
                    if (HasText(project.Technologies))
                    {
                        section.Append("<p><strong>")
                            .Append(Html(T(resume.Language, "Technologies")))
                            .Append(":</strong> ")
                            .Append(Html(project.Technologies))
                            .AppendLine("</p>");
                    }
                    section.AppendLine("</div>");
                }
            });
        }

        var skills = (resume.Skills ?? []).Where(skill => skill.Skills?.Any(HasText) ?? false).ToList();
        if (skills.Count > 0)
        {
            AppendSection(sb, T(resume.Language, "Skills"), section =>
            {
                section.AppendLine("<div class=\"skills-list\">");
                foreach (var skill in skills)
                {
                    var category = HasText(skill.Category)
                        ? skill.Category!.Trim().TrimEnd(':').Trim()
                        : T(resume.Language, "Skills");
                    var values = string.Join(", ", (skill.Skills ?? []).Where(HasText).Select(value => value.Trim()));
                    if (values.Length > 0)
                    {
                        section.Append("<p><strong>")
                            .Append(Html(category.Length > 0 ? category : T(resume.Language, "Skills")))
                            .Append(":</strong> ")
                            .Append(Html(values))
                            .AppendLine("</p>");
                    }
                }
                section.AppendLine("</div>");
            });
        }

        var languages = (resume.Languages ?? []).Where(HasLanguageContent).ToList();
        if (languages.Count > 0)
        {
            var line = string.Join(
                " | ",
                languages
                    .Select(language => JoinTitle(language.LanguageName, language.Level))
                    .Where(value => value.Length > 0));

            if (line.Length > 0)
            {
                AppendSection(sb, T(resume.Language, "Languages"), section =>
                {
                    section.Append("<p>").Append(Html(line)).AppendLine("</p>");
                });
            }
        }

        var certificates = (resume.Certificates ?? []).Where(HasCertificateContent).ToList();
        if (certificates.Count > 0)
        {
            AppendSection(sb, T(resume.Language, "Certificates"), section =>
            {
                foreach (var certificate in certificates)
                {
                    section.AppendLine("<div class=\"certificate-entry\">");
                    section.AppendLine("<div class=\"certificate-heading\">");
                    section.Append("<p><strong>\u2022 ")
                        .Append(Html(certificate.CertificateName))
                        .Append("</strong>");
                    if (HasText(certificate.Issuer))
                        section.Append(" | ").Append(Html(certificate.Issuer));
                    section.AppendLine("</p>");
                    if (HasText(certificate.Date))
                        section.Append("<p class=\"entry-date\">").Append(Html(certificate.Date)).AppendLine("</p>");
                    section.AppendLine("</div>");

                    var details = (certificate.Details ?? []).Where(HasText).ToList();
                    if (details.Count > 0)
                    {
                        section.AppendLine("<ul class=\"certificate-details\">");
                        foreach (var detail in details)
                            section.Append("<li>").Append(Html(detail.Trim())).AppendLine("</li>");
                        section.AppendLine("</ul>");
                    }

                    section.AppendLine("</div>");
                }
            });
        }

        if (!string.Equals(resume.ReferenceMode, "none", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(resume.ReferenceMode, "contacts", StringComparison.OrdinalIgnoreCase))
            {
                AppendSection(sb, T(resume.Language, "References"), section =>
                {
                    section.Append("<p>")
                        .Append(Html(T(resume.Language, "References available upon request.")))
                        .AppendLine("</p>");
                });
            }
            else
            {
                var references = (resume.References ?? []).Where(HasReferenceContent).ToList();
                if (references.Count > 0)
                {
                    AppendSection(sb, T(resume.Language, "References"), section =>
                    {
                        foreach (var reference in references)
                        {
                            var role = string.Join(", ", new[] { reference.JobTitle, reference.Company }.Where(HasText));
                            var heading = JoinTitle(reference.FullName, role);

                            section.AppendLine("<div class=\"reference-entry\">");
                            if (heading.Length > 0)
                                section.Append("<p><strong>").Append(Html(heading)).AppendLine("</strong></p>");

                            AppendReferenceContactLine(section, reference);
                            section.AppendLine("</div>");
                        }
                    });
                }
            }
        }

        sb.AppendLine("</article>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static string BuildResumeCss() =>
        """
        @page {
          size: A4 portrait;
          margin: 58px 64px;
        }

        * {
          box-sizing: border-box;
        }

        html,
        body {
          margin: 0;
          padding: 0;
          background: #ffffff;
        }

        body {
          color: #111827;
          font: 400 12.5px/1.45 Arial, Helvetica, sans-serif;
          font-synthesis: none;
          text-rendering: geometricPrecision;
          -webkit-font-smoothing: antialiased;
        }

        a {
          color: inherit;
          text-decoration: none;
        }

        .resume-preview {
          width: 100%;
          color: #111827;
          background: #ffffff;
          print-color-adjust: exact;
          -webkit-print-color-adjust: exact;
        }

        .resume-preview h1,
        .resume-preview h2,
        .resume-preview h3,
        .resume-preview p {
          color: inherit;
          font-family: Arial, Helvetica, sans-serif;
        }

        .resume-header {
          position: relative;
          padding-bottom: 13px;
          border-bottom: 0.4px solid rgba(156, 163, 175, 0.38);
          text-align: center;
          break-inside: avoid;
          page-break-inside: avoid;
        }

        .resume-header-content {
          min-width: 0;
        }

        .modern-template .resume-header {
          display: flex;
          min-height: 92px;
          align-items: flex-start;
          justify-content: space-between;
          gap: 22px;
          text-align: left;
        }

        .modern-template .resume-header-content {
          flex: 1;
        }

        .modern-template .contact-line {
          justify-content: flex-start;
        }

        .profile-photo-frame {
          width: 82px;
          height: 82px;
          min-width: 82px;
          max-width: 82px;
          min-height: 82px;
          max-height: 82px;
          flex: 0 0 82px;
          position: relative;
          overflow: hidden;
          border-radius: 9px;
          border: 1px solid #d1d5db;
          display: flex;
          align-items: center;
          justify-content: center;
          align-self: flex-start;
          aspect-ratio: 1 / 1;
          background-position: center center;
          background-repeat: no-repeat;
          background-size: cover;
          clip-path: inset(0 round 9px);
        }

        .profile-photo {
          position: absolute;
          inset: 0;
          width: 82px !important;
          height: 82px !important;
          min-width: 82px;
          max-width: 82px;
          min-height: 82px;
          max-height: 82px;
          object-fit: cover !important;
          object-position: center center !important;
          display: block;
          border: none;
          border-radius: 8px;
          image-orientation: from-image;
        }

        .resume-header h1 {
          margin: 0;
          font-size: 28px;
          line-height: 1.1;
          font-weight: 500;
          letter-spacing: 0;
          text-transform: uppercase;
        }

        .resume-title {
          margin: 6px 0 9px;
          font-size: 14.5px;
          font-weight: 400;
        }

        .contact-line {
          display: flex;
          flex-wrap: wrap;
          justify-content: center;
          gap: 5px 0;
          color: #374151;
          font-size: 11.2px;
          line-height: 1.45;
        }

        .contact-line > *:not(:last-child)::after {
          content: " | ";
          margin: 0 8px;
          color: #9ca3af;
        }

        .contact-line .contact-link {
          color: inherit;
          cursor: pointer;
          text-decoration: none;
        }

        .optional-contact-line {
          margin-top: 5px;
        }

        .resume-section {
          margin-top: 16.5px;
          break-inside: avoid;
          break-inside: avoid-page;
          page-break-inside: avoid;
        }

        .resume-section > h2 {
          margin: 0 0 7.5px;
          padding-bottom: 3px;
          border-bottom: 0.4px solid rgba(156, 163, 175, 0.52);
          color: #4b5563;
          font-size: 13.5px;
          line-height: 1.25;
          font-weight: 400;
          letter-spacing: 0.8px;
          text-transform: uppercase;
        }

        .resume-section p {
          margin: 0;
        }

        .resume-entry,
        .reference-entry {
          margin-bottom: 11px;
          break-inside: avoid;
          break-inside: avoid-page;
          page-break-inside: avoid;
        }

        .resume-entry:last-child,
        .reference-entry:last-child {
          margin-bottom: 0;
        }

        .entry-heading {
          display: flex;
          align-items: flex-start;
          justify-content: space-between;
          gap: 16px;
          margin-bottom: 3.5px;
        }

        .resume-entry h3 {
          margin: 0 0 1px;
          color: #4b5563;
          font-size: 12.5px;
          line-height: 1.4;
          font-weight: 400;
        }

        .entry-subtitle {
          color: #374151;
          font-style: italic;
          font-weight: 400;
        }

        .entry-date {
          flex: 0 0 auto;
          color: #4b5563;
          font-weight: 400;
          white-space: nowrap;
        }

        .resume-entry ul {
          margin: 4.5px 0 0;
          padding-left: 17px;
        }

        .resume-entry li {
          margin-bottom: 1.5px;
          padding-left: 1px;
          line-height: 1.45;
        }

        .skills-list {
          display: grid;
          gap: 2.5px;
        }

        .certificate-entry {
          margin-bottom: 7.5px;
          break-inside: avoid;
          break-inside: avoid-page;
          page-break-inside: avoid;
        }

        .certificate-entry:last-child {
          margin-bottom: 0;
        }

        .certificate-heading {
          display: flex;
          align-items: flex-start;
          justify-content: space-between;
          gap: 16px;
        }

        .certificate-details {
          margin: 3.5px 0 0;
          padding-left: 22px;
        }

        .certificate-details li {
          margin-bottom: 1.5px;
          padding-left: 1px;
          line-height: 1.45;
        }

        .resume-preview strong,
        .resume-preview b {
          font-weight: 400;
        }
        """;

    private static void AppendSection(StringBuilder sb, string title, Action<StringBuilder> appendContent)
    {
        sb.AppendLine("<section class=\"resume-section\">");
        sb.Append("<h2>").Append(Html(title)).AppendLine("</h2>");
        appendContent(sb);
        sb.AppendLine("</section>");
    }

    private static void AppendEntry(
        StringBuilder sb,
        string? title,
        string? subtitle,
        string? date,
        IEnumerable<string>? bulletItems)
    {
        sb.AppendLine("<div class=\"resume-entry\">");

        if (HasText(title) || HasText(subtitle) || HasText(date))
        {
            sb.AppendLine("<div class=\"entry-heading\">");
            sb.AppendLine("<div>");
            if (HasText(title))
                sb.Append("<h3>").Append(Html(title)).AppendLine("</h3>");
            if (HasText(subtitle))
                sb.Append("<p class=\"entry-subtitle\">").Append(Html(subtitle)).AppendLine("</p>");
            sb.AppendLine("</div>");

            if (HasText(date))
                sb.Append("<p class=\"entry-date\">").Append(Html(date)).AppendLine("</p>");

            sb.AppendLine("</div>");
        }

        var bullets = (bulletItems ?? []).Where(HasText).ToList();
        if (bullets.Count > 0)
        {
            sb.AppendLine("<ul>");
            foreach (var bullet in bullets)
                sb.Append("<li>").Append(Html(bullet.Trim())).AppendLine("</li>");
            sb.AppendLine("</ul>");
        }

        sb.AppendLine("</div>");
    }

    private static void AppendContactLine(StringBuilder sb, IReadOnlyCollection<ContactItem> items, bool optional)
    {
        if (items.Count == 0)
            return;

        sb.Append("<div class=\"contact-line");
        if (optional)
            sb.Append(" optional-contact-line");
        sb.AppendLine("\">");

        foreach (var item in items)
        {
            if (HasText(item.Href))
            {
                sb.Append("<a class=\"contact-link\" href=\"")
                    .Append(Attr(item.Href))
                    .Append("\">")
                    .Append(Html(item.Text))
                    .AppendLine("</a>");
            }
            else
            {
                sb.Append("<span>").Append(Html(item.Text)).AppendLine("</span>");
            }
        }

        sb.AppendLine("</div>");
    }

    private static void AppendReferenceContactLine(StringBuilder sb, ReferenceDto reference)
    {
        var items = BuildReferenceContactItems(reference).ToList();
        if (items.Count == 0)
            return;

        sb.Append("<p>");
        for (var index = 0; index < items.Count; index++)
        {
            if (index > 0)
                sb.Append(" | ");

            var item = items[index];
            if (HasText(item.Href))
            {
                sb.Append("<a class=\"contact-link\" href=\"")
                    .Append(Attr(item.Href))
                    .Append("\">")
                    .Append(Html(item.Text))
                    .Append("</a>");
            }
            else
            {
                sb.Append(Html(item.Text));
            }
        }

        sb.AppendLine("</p>");
    }

    private static string NormalizeDataImageSource(string? value)
    {
        if (!HasText(value))
            return string.Empty;

        var trimmedValue = value!.Trim();
        if (trimmedValue.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return trimmedValue;

        return $"data:{DetectImageMimeType(trimmedValue)};base64,{trimmedValue}";
    }

    private static string NormalizeProfilePhotoDataUri(string? value)
    {
        var imageBytes = DecodeProfilePhotoBytes(value);
        if (imageBytes is not null && TryCreateRoundedSquareProfilePhoto(imageBytes, out var normalizedImage))
            return $"data:image/png;base64,{Convert.ToBase64String(normalizedImage)}";

        return NormalizeDataImageSource(value);
    }

    private static string DetectImageMimeType(string base64Image)
    {
        try
        {
            var image = Convert.FromBase64String(base64Image);
            if (image.Length >= 3 && image[0] == 0xFF && image[1] == 0xD8 && image[2] == 0xFF)
                return "image/jpeg";

            if (image.Length >= 8 &&
                image[0] == 0x89 &&
                image[1] == 0x50 &&
                image[2] == 0x4E &&
                image[3] == 0x47 &&
                image[4] == 0x0D &&
                image[5] == 0x0A &&
                image[6] == 0x1A &&
                image[7] == 0x0A)
            {
                return "image/png";
            }
        }
        catch (FormatException)
        {
            // The browser renderer will fail this image and fall back to QuestPDF if the PDF is invalid.
        }

        return "image/png";
    }

    private static string Html(string? value) => WebUtility.HtmlEncode(value?.Trim() ?? string.Empty);

    private static string Attr(string? value) => WebUtility.HtmlEncode(value?.Trim() ?? string.Empty);

    private static string T(string? language, string key)
    {
        if (!string.Equals(language, "tr", StringComparison.OrdinalIgnoreCase))
            return key;

        return key switch
        {
            "Professional Summary" => "Profesyonel Özet",
            "Work Experience" => "İş Deneyimi",
            "Volunteer Experience" => "Gönüllü Deneyim",
            "Education" => "Eğitim",
            "Projects" => "Projeler",
            "Technologies" => "Teknolojiler",
            "Skills" => "Yetenekler",
            "Languages" => "Diller",
            "Certificates" => "Sertifikalar",
            "References" => "Referanslar",
            "References available upon request." => "Referanslar talep üzerine sunulabilir.",
            _ => key,
        };
    }

    private static void AddHeader(
        ColumnDescriptor column,
        PersonalInfoDto? personalInfo,
        byte[]? profilePhoto)
    {
        var isModern = profilePhoto is not null;

        column.Item().Column(header =>
        {
            header.Spacing(0);

            if (isModern)
            {
                header.Item().MinHeight(92 * PxToPoint).Row(row =>
                {
                    row.Spacing(22 * PxToPoint);

                    row.RelativeItem().Column(content =>
                    {
                        AddHeaderText(content, personalInfo, alignCenter: false);
                    });

                    row.ConstantItem(82 * PxToPoint)
                        .Width(82 * PxToPoint)
                        .Height(82 * PxToPoint)
                        .Image(profilePhoto!)
                        .FitArea();
                });
            }
            else
            {
                AddHeaderText(header, personalInfo, alignCenter: true);
            }
        });

        column.Item()
            .PaddingTop(9)
            .BorderBottom(0.35f)
            .BorderColor(HeaderRuleColor);
    }

    private static void AddHeaderText(ColumnDescriptor header, PersonalInfoDto? personalInfo, bool alignCenter)
    {
        var name = header.Item();
        if (alignCenter)
            name = name.AlignCenter();

        name.Text((personalInfo?.FullName ?? string.Empty).ToUpperInvariant())
            .FontSize(HeaderNameFontSize)
            .LineHeight(1.1f)
            .FontColor(TextColor);

        if (!string.IsNullOrWhiteSpace(personalInfo?.JobTitle))
        {
            var title = header.Item().PaddingTop(6 * PxToPoint);
            if (alignCenter)
                title = title.AlignCenter();

            title.Text(personalInfo.JobTitle)
                .FontSize(HeaderTitleFontSize)
                .LineHeight(1.2f)
                .FontColor(TextColor);
        }

        var contactItems = BuildPrimaryContactItems(personalInfo).ToList();
        if (contactItems.Count > 0)
        {
            var contactTopSpacing = HasText(personalInfo?.JobTitle) ? 8 * PxToPoint : 0;
            AddContactLine(header, contactItems, contactTopSpacing, alignCenter);
        }

        var optionalContacts = personalInfo?.CustomFields?
            .Where(field => HasText(field.Value) && (HasText(field.Label) || IsUrl(field.Value!)))
            .Select(BuildCustomContactItem)
            .Where(item => HasText(item.Text))
            .ToList() ?? [];

        if (optionalContacts.Count > 0)
            AddContactLine(header, optionalContacts, 4 * PxToPoint, alignCenter);
    }

    private static void AddContactLine(
        ColumnDescriptor column,
        IReadOnlyCollection<ContactItem> items,
        float topPadding,
        bool alignCenter)
    {
        column.Item()
            .PaddingTop(topPadding)
            .Text(text =>
            {
                if (alignCenter)
                    text.AlignCenter();

                var index = 0;
                foreach (var item in items)
                {
                    if (index > 0)
                    {
                        text.Span("  |  ")
                            .FontSize(ContactFontSize)
                            .LineHeight(1.4f)
                            .FontColor(HeaderRuleColor);
                    }

                    var span = HasText(item.Href)
                        ? text.Hyperlink(item.Text, item.Href!)
                        : text.Span(item.Text);

                    span.FontSize(ContactFontSize)
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
        column.Item().PaddingTop(SectionContentTopSpacing).Text(summary);
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

        var isFirstEntry = true;
        foreach (var experience in populatedExperiences)
        {
            var dateRange = JoinDateRange(
                experience.StartDate,
                NormalizeCurrentWorkStatus(experience.EndDate, language));

            column.Item().PaddingTop(isFirstEntry ? SectionContentTopSpacing : EntrySpacing).Row(row =>
            {
                row.Spacing(16 * PxToPoint);

                row.RelativeItem().Column(left =>
                {
                    left.Spacing(1 * PxToPoint);

                    if (HasText(experience.JobTitle))
                        left.Item().Text(experience.JobTitle!).FontSize(BaseFontSize).FontColor(HeadingTextColor);

                    var companyAndLocation = JoinNonEmpty(
                        experience.CompanyName,
                        experience.Location);

                    if (companyAndLocation.Length > 0)
                        left.Item().Text(companyAndLocation).FontSize(BaseFontSize).FontColor(SecondaryTextColor).Italic();
                });

                if (dateRange.Length > 0)
                    row.ConstantItem(DateColumnWidth).AlignRight().Text(dateRange).FontSize(BaseFontSize).FontColor(HeadingTextColor);
            });

            var isFirstResponsibility = true;
            foreach (var responsibility in (experience.Responsibilities ?? []).Where(HasText))
            {
                column.Item()
                    .PaddingTop(isFirstResponsibility ? 3 : 0.75f)
                    .PaddingLeft(12.75f)
                    .Text($"\u2022 {responsibility.Trim()}")
                    .FontSize(BaseFontSize);

                isFirstResponsibility = false;
            }

            isFirstEntry = false;
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

        var isFirstEntry = true;
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

            column.Item().PaddingTop(isFirstEntry ? SectionContentTopSpacing : EntrySpacing).Row(row =>
            {
                row.Spacing(16 * PxToPoint);

                row.RelativeItem().Column(left =>
                {
                    left.Spacing(1 * PxToPoint);

                    if (HasText(item.SchoolName))
                        left.Item().Text(item.SchoolName!).FontSize(BaseFontSize).FontColor(HeadingTextColor);

                    if (programAndGpa.Length > 0)
                        left.Item().Text(programAndGpa).FontSize(BaseFontSize).FontColor(SecondaryTextColor).Italic();
                });

                if (dateRange.Length > 0)
                    row.ConstantItem(DateColumnWidth).AlignRight().Text(dateRange).FontSize(BaseFontSize).FontColor(HeadingTextColor);
            });

            isFirstEntry = false;
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

        var isFirstEntry = true;
        foreach (var volunteer in populatedExperiences)
        {
            var endDate = volunteer.IsCurrent
                ? CurrentRoleLabel(language)
                : NormalizeCurrentWorkStatus(volunteer.EndDate, language);
            var dateRange = JoinDateRange(volunteer.StartDate, endDate);

            column.Item().PaddingTop(isFirstEntry ? SectionContentTopSpacing : EntrySpacing).Row(row =>
            {
                row.Spacing(16 * PxToPoint);

                row.RelativeItem().Column(left =>
                {
                    left.Spacing(1 * PxToPoint);

                    if (HasText(volunteer.Role))
                        left.Item().Text(volunteer.Role!).FontSize(BaseFontSize).FontColor(HeadingTextColor);

                    var organizationAndLocation = JoinNonEmpty(
                        volunteer.OrganizationName,
                        volunteer.Location);

                    if (organizationAndLocation.Length > 0)
                        left.Item().Text(organizationAndLocation).FontSize(BaseFontSize).FontColor(SecondaryTextColor).Italic();
                });

                if (dateRange.Length > 0)
                    row.ConstantItem(DateColumnWidth).AlignRight().Text(dateRange).FontSize(BaseFontSize).FontColor(HeadingTextColor);
            });

            var isFirstResponsibility = true;
            foreach (var responsibility in (volunteer.Responsibilities ?? []).Where(HasText))
            {
                column.Item()
                    .PaddingTop(isFirstResponsibility ? 3 : 0.75f)
                    .PaddingLeft(12.75f)
                    .Text($"\u2022 {responsibility.Trim()}")
                    .FontSize(BaseFontSize);

                isFirstResponsibility = false;
            }

            isFirstEntry = false;
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

        var isFirstEntry = true;
        foreach (var project in populatedProjects)
        {
            column.Item()
                .PaddingTop(isFirstEntry ? SectionContentTopSpacing : EntrySpacing)
                .Text(project.ProjectName ?? string.Empty)
                .FontSize(BaseFontSize)
                .FontColor(HeadingTextColor);

            if (HasText(project.Description))
                column.Item().Text(project.Description!);

            if (HasText(project.Technologies))
                column.Item().Text(text =>
                {
                    text.Span("Technologies: ").FontSize(BaseFontSize).FontColor(HeadingTextColor);
                    text.Span(project.Technologies!.Trim()).FontSize(BaseFontSize);
                });

            isFirstEntry = false;
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

        var isFirstEntry = true;
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
                column.Item()
                    .PaddingTop(isFirstEntry ? SectionContentTopSpacing : 0)
                    .PaddingBottom(1.5f)
                    .Text(text =>
                    {
                        text.Span($"{category}: ").FontColor(HeadingTextColor);
                        text.Span(values);
                    });

                isFirstEntry = false;
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

        var line = string.Join(
            " | ",
            populatedLanguages
                .Select(language => JoinTitle(language.LanguageName, language.Level))
                .Where(value => value.Length > 0));

        if (line.Length > 0)
            column.Item().PaddingTop(SectionContentTopSpacing).Text(line).FontSize(BaseFontSize);
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
                row.Spacing(16 * PxToPoint);

                row.RelativeItem().Text(text =>
                {
                    var hasName = HasText(certificate.CertificateName);

                    if (hasName)
                        text.Span($"\u2022 {certificate.CertificateName!.Trim()}").FontColor(HeadingTextColor);

                    if (HasText(certificate.Issuer))
                        text.Span(hasName
                            ? $" | {certificate.Issuer!.Trim()}"
                            : certificate.Issuer!.Trim());
                });

                if (HasText(certificate.Date))
                    row.ConstantItem(DateColumnWidth)
                        .AlignRight()
                        .Text(certificate.Date!.Trim())
                        .FontSize(BaseFontSize)
                        .FontColor(HeadingTextColor);
            });

            var isFirstDetail = true;
            foreach (var detail in (certificate.Details ?? []).Where(HasText))
            {
                column.Item()
                    .PaddingTop(isFirstDetail ? 2.25f : 0.75f)
                    .PaddingLeft(16.5f)
                    .Text($"\u2022 {detail.Trim()}")
                    .FontSize(BaseFontSize);

                isFirstDetail = false;
            }
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
            column.Item().PaddingTop(SectionContentTopSpacing).Text("References available upon request.");
            return;
        }

        var populatedReferences = references.Where(HasReferenceContent).ToList();
        if (populatedReferences.Count == 0)
            return;

        AddSectionHeading(column, "REFERENCES");

        var isFirstEntry = true;
        foreach (var reference in populatedReferences)
        {
            var role = JoinNonEmpty(reference.JobTitle, reference.Company);
            var heading = JoinTitle(reference.FullName, role);
            var contact = JoinNonEmpty(reference.Email, reference.Phone, reference.Relationship);

            if (heading.Length > 0)
            {
                column.Item()
                    .PaddingTop(isFirstEntry ? SectionContentTopSpacing : EntrySpacing)
                    .Text(heading)
                    .FontSize(BaseFontSize)
                    .FontColor(HeadingTextColor);
            }

            if (contact.Length > 0)
                AddReferenceContactLine(column, reference);

            isFirstEntry = false;
        }
    }

    private static void AddSectionHeading(ColumnDescriptor column, string heading)
    {
        column.Item()
            .PaddingTop(11.25f)
            .PaddingBottom(2.25f)
            .BorderBottom(0.35f)
            .BorderColor(SectionRuleColor)
            .Text(heading)
            .FontSize(10.1f)
            .LineHeight(1.25f)
            .FontColor(HeadingTextColor);
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
            var image = DecodeProfilePhotoBytes(resume.ProfilePhotoBase64);
            if (image is null)
                return null;

            return TryCreateRoundedSquareProfilePhoto(image, out var normalizedImage)
                ? normalizedImage
                : image;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static byte[]? DecodeProfilePhotoBytes(string? value)
    {
        if (!HasText(value))
            return null;

        var base64 = value!.Trim();
        var separatorIndex = base64.IndexOf(',');

        if (base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) &&
            separatorIndex >= 0)
        {
            base64 = base64[(separatorIndex + 1)..];
        }

        var image = Convert.FromBase64String(base64);
        return IsSupportedImage(image) ? image : null;
    }

    private static bool TryCreateRoundedSquareProfilePhoto(byte[] imageBytes, out byte[] normalizedImage)
    {
        normalizedImage = [];

        try
        {
            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageBytes);
            image.Mutate(context => context.Resize(new ResizeOptions
            {
                Size = new SixLabors.ImageSharp.Size(ProfilePhotoPixelSize, ProfilePhotoPixelSize),
                Mode = ResizeMode.Crop,
                Position = AnchorPositionMode.Center,
            }));

            ApplyRoundedCornerMask(image, ProfilePhotoCornerRadius);

            using var output = new MemoryStream();
            image.SaveAsPng(output);
            normalizedImage = output.ToArray();
            return normalizedImage.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    private static void ApplyRoundedCornerMask(SixLabors.ImageSharp.Image<Rgba32> image, int radius)
    {
        var width = image.Width;
        var height = image.Height;
        var radiusSquared = radius * radius;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var cornerX = x < radius ? radius - x - 1 : x >= width - radius ? x - (width - radius) : 0;
                var cornerY = y < radius ? radius - y - 1 : y >= height - radius ? y - (height - radius) : 0;

                if (cornerX == 0 || cornerY == 0 || cornerX * cornerX + cornerY * cornerY <= radiusSquared)
                    continue;

                var pixel = image[x, y];
                pixel.A = 0;
                image[x, y] = pixel;
            }
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
            yield return new ContactItem(personalInfo!.Phone!.Trim(), CreatePhoneLink(personalInfo.Phone));

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
                    text.Span("  |  ").FontSize(BaseFontSize).FontColor(TextColor);

                var span = HasText(item.Href)
                    ? text.Hyperlink(item.Text, item.Href!)
                    : text.Span(item.Text);

                span.FontSize(BaseFontSize)
                    .LineHeight(1.45f)
                    .FontColor(TextColor);

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

    private static string CreatePhoneLink(string phone)
    {
        var trimmedPhone = phone.Trim();
        var digits = new string(trimmedPhone.Where(char.IsDigit).ToArray());
        if (digits.Length < 3)
            return string.Empty;

        return trimmedPhone.StartsWith('+') ? $"tel:+{digits}" : $"tel:{digits}";
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
