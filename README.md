# ATS Resume Builder

ATS Resume Builder is a simple resume creation tool that helps people focus on their experience and skills instead of spending time learning a complicated editor.

For Turkish documentation, see [README.tr.md](README.tr.md).

The flow is intentionally straightforward:

1. Fill in your resume information.
2. Preview changes instantly.
3. Choose a suitable template.
4. Export or save the resume as a PDF.

## Why I Built This

Finding the right resume format, keeping it ATS-friendly, and avoiding unnecessary design complexity can often be harder than expected.

I first noticed this need while helping a friend prepare a CV. As the process continued, I realized that this was not only a one-person problem, but something that could be simplified and made useful for many people.

Many resume builder tools include too many visual options, confusing flows, or paid steps just to export a completed document as a PDF. With this project, my goal was to create a simpler, more accessible flow that keeps the content at the center.

Based on this idea, I built **ATS Resume Builder**, a web application that allows users to enter their resume information, edit it through a live preview, optionally add a profile photo, and save the result as a PDF in an ATS-friendly format.

## Features

- Live resume preview while editing
- A4 page boundary visibility in the preview
- Clean, single-column ATS-friendly template
- Modern template with optional profile photo support
- English and Turkish interface labels
- Light/dark application theme
- Dynamic work experience and volunteer experience sections
- Education, projects, and profession-neutral skill categories
- Languages and certificates with optional details or topics
- Optional references and custom personal fields
- Additional Fields as the single place for optional personal links and personal details
- Custom fields that detect links such as LinkedIn, GitHub, Portfolio, Website, Medium, or LeetCode
- Link display options: Label, Short URL, or Full URL
- Browser-based **Save as PDF** support
- Optional direct PDF download through the local .NET backend
- Responsive layout

For the best ATS compatibility, the ATS-friendly template stays simple, text-focused, and does not include a photo.

### Additional Fields and Links

Optional personal links are managed through **Additional Fields**. This keeps the main Personal Information form clean. Each additional field is made of a simple **Label** and **Value** pair, so it can be used for either a link or a normal personal detail.

Values that contain a domain, such as `meltemmeydan.dev` or `github.com/example`, are detected as links even when they do not start with `https://`. When a value is detected as a URL, the user can choose how the link appears on the resume:

- **Label** shows the field label.
- **Short URL** removes the protocol, `www.`, and trailing slash.
- **Full URL** shows the entered value as it is.

For links without a protocol, the app automatically adds `https://` internally so the links remain clickable. Newly added fields also show the display options automatically when their value is detected as a URL. Normal non-URL text values do not show URL display options. Empty fields are not shown in the preview or PDF output.

### A4 Preview

The resume preview is designed to make the A4 page structure easier to understand. As the content grows, new page starts are visually separated in the preview area. This helps users follow how many pages the resume will take and where page transitions will happen in the PDF output.

These visual page helpers are only used to improve the preview experience. They do not appear as unnecessary lines, labels, or helper elements in the exported PDF.

## PDF Export

The project supports two export modes.

### Frontend-Only Mode

When `VITE_API_BASE_URL` is not configured, the application shows a **Save as PDF** button. This opens the browser print dialog, where the resume can be saved as a PDF.

The Modern With Photo template keeps the selected profile photo in the browser-generated PDF. The ATS-friendly template does not show or export a photo.

### Optional Backend Mode

The repository also includes a .NET 8 backend built with QuestPDF. It provides direct PDF generation and enables the **Download PDF** action when the frontend is configured with:

```env
VITE_API_BASE_URL=http://localhost:5000
```

The backend is optional. It remains in the repository as a local development and reference implementation, but the live frontend does not depend on it.

## Live Deployment

The frontend is deployed on Cloudflare Pages. Cloudflare hosts the React/Vite application without separately hosting the .NET backend.

This is intentional: it keeps the live demo simple, free to host, and available without requiring another backend service. In production, `VITE_API_BASE_URL` is left unset, so PDF saving uses the browser's **Save as PDF** flow.

Cloudflare Pages settings:

```text
Root directory: frontend
Build command: npm run build
Output directory: dist
VITE_API_BASE_URL: leave unset
```

## Screenshots

Screenshots will be added here.

Suggested folder: `docs/screenshots/`

## Project Structure

```text
ats-resume-builder/
|-- frontend/                     React + Vite application
|-- backend/
|   `-- AtsResumeBuilder.Api/     Optional .NET 8 QuestPDF API
|-- .gitignore
|-- README.md
`-- README.tr.md
```

## Run Locally

### Frontend

Requirements:

- Node.js 20.19+ or 22.12+
- npm

```bash
cd frontend
npm install
npm run dev
```

Open the local URL printed by Vite.

Without an environment variable, the frontend runs by itself and uses browser **Save as PDF**.

### Optional Backend

Requirements:

- .NET 8 SDK

```bash
cd backend/AtsResumeBuilder.Api
dotnet restore
dotnet run
```

To enable direct PDF downloads locally, create `frontend/.env.local`:

```env
VITE_API_BASE_URL=http://localhost:5000
```

Restart the Vite development server after changing environment variables.

The backend API runs at:

```text
http://localhost:5000
```

Swagger is available at:

```text
http://localhost:5000/swagger
```

## Validation

Frontend:

```bash
cd frontend
npm run lint
npm run build
```

Backend:

```bash
cd backend/AtsResumeBuilder.Api
dotnet build
```

## Technology

Frontend:

- React
- Vite
- JavaScript
- CSS

Optional backend:

- ASP.NET Core Web API
- .NET 8
- QuestPDF
- Swagger

## Known Limitations

- Resume data is not saved after a page refresh.
- Browser PDF output can vary slightly between browsers.
- Direct backend PDF download requires the optional API to be running.
- Automated tests have not been added yet.

