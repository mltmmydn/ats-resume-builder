# ATS Resume Builder

ATS Resume Builder is a resume creation application designed to make CV preparation simpler, cleaner, and more accessible.

I built this project after noticing that creating a resume can sometimes become unnecessarily complicated. Many tools offer too many visual options, make the process harder than expected, or place basic export features behind paid steps.

The idea behind this project is simple:

```text
Fill in your resume information
Preview it instantly
Choose a suitable template
Save it as a PDF
```

The main goal is to help users create a clean and readable resume without losing focus on the content.

## What This Project Does

With this application, users can:

- Fill in personal information
- Add work experience, education, projects, skills, languages, certificates, and references
- Add optional custom fields such as Portfolio, Website, Medium, or LeetCode
- Choose between an ATS-friendly template and a modern template with photo
- Switch between English and Turkish interface labels
- Preview the resume instantly while editing
- Save the resume as a PDF through the browser print dialog

The ATS-friendly template is intentionally simple. It avoids unnecessary visual complexity and focuses on a clean, single-column layout.

## Why I Built It

This project started from a practical need.

While helping with resume preparation, I realized that many people do not need a complex design tool. They mostly need a clear structure, editable sections, and a format that is easy to read.

So I wanted to build a tool that keeps the process simple:

- no unnecessary design clutter
- no forced photo usage
- no required GitHub or portfolio field
- no complicated editor flow
- no paid step just to create a basic PDF

## Current Status

The main application is the React frontend.

The frontend handles the resume form, live preview, template selection, photo preview, references, language selection, and browser-based PDF saving.

The .NET 8 QuestPDF API remains available as an optional direct PDF download service. The frontend can also be deployed by itself without hosting the backend.

## Features

- Live resume preview
- ATS-friendly resume template
- Optional modern photo template
- Dynamic resume sections
- Optional references section
- Optional custom contact fields
- English and Turkish interface support
- Light and dark mode for the application UI
- Browser-based PDF export
- Optional direct PDF download through the .NET backend
- Responsive layout

## Project Structure

```text
ats-resume-builder/
|-- frontend/                     React + Vite application
|-- backend/                      .NET 8 QuestPDF API
|-- .gitignore
`-- README.md
```

## Run the Frontend

Requirements:

- Node.js 20.19+ or 22.12+
- npm

```bash
cd frontend
npm install
npm run dev
```

Open the local URL printed by Vite.

By default, the frontend runs in frontend-only mode. Its **Save as PDF** button opens the browser print dialog.

To enable direct PDF downloads through the existing .NET backend, create `frontend/.env.local`:

```env
VITE_API_BASE_URL=http://localhost:5000
```

Restart the Vite development server after changing environment variables.

## Run the Backend

Requirements:

- .NET 8 SDK

```bash
cd backend/AtsResumeBuilder.Api
dotnet restore
dotnet run
```

Swagger is available at:

```text
http://localhost:5000/swagger
```

Running the backend is optional unless `VITE_API_BASE_URL` is configured in the frontend.

## Cloudflare Pages

The React frontend can be deployed to Cloudflare Pages without hosting the .NET backend:

```text
Root directory: frontend
Build command: npm run build
Output directory: dist
```

Do not configure `VITE_API_BASE_URL` for a frontend-only deployment. The application will show one **Save as PDF** action that uses the browser print dialog.

If a separately hosted compatible backend is available later, set `VITE_API_BASE_URL` to its public base URL to enable **Download PDF**.

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
dotnet restore
dotnet build
```

## PDF Export

Frontend-only mode:

1. Complete the resume form and select a template.
2. Click **Save as PDF**.
3. Choose **Save as PDF** or **Microsoft Print to PDF** in the browser print dialog.
4. Save the file.

The Modern With Photo template includes the uploaded profile photo in browser print output.

Backend-enabled mode:

- **Download PDF** generates a PDF through the configured .NET API.
- **Print** remains available in the bottom export section as a browser print fallback.

For best ATS compatibility, use the ATS-friendly template without a photo.

## Tech Stack

Frontend:

- React
- Vite
- JavaScript
- CSS

Backend:

- ASP.NET Core Web API
- .NET 8
- QuestPDF
- Swagger

## Known Limitations

- Resume data is not saved after page refresh.
- Browser print export depends on the browser print engine.
- Direct PDF download is available only when `VITE_API_BASE_URL` points to a running backend API.
- Automated tests have not been added yet.

## Future Improvements

- Save resume data locally or through a backend
- Split large React files into smaller components
- Add more templates
- Add screenshots and a live demo link
- Add automated tests

## License

No license has been selected yet.
