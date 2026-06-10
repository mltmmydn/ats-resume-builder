# ATS Resume Builder Frontend

The frontend is a React + Vite application with a live resume preview and browser-based PDF saving. It can run by itself or optionally use the repository's .NET 8 backend for direct PDF downloads.

## Local Development

```bash
npm install
npm run dev
```

Without environment configuration, the application shows a single **Save as PDF** action that calls `window.print()`.

To enable the optional backend **Download PDF** action, copy `.env.example` to `.env.local` and run the .NET API:

```env
VITE_API_BASE_URL=http://localhost:5000
```

Restart Vite after changing environment variables.

## Cloudflare Pages

Use these build settings for a frontend-only deployment:

```text
Root directory: frontend
Build command: npm run build
Output directory: dist
```

Leave `VITE_API_BASE_URL` unset. The deployed application will use **Save as PDF** through the browser print dialog. Browser print also includes an uploaded profile photo when the Modern With Photo template is selected.

## Validation

```bash
npm run lint
npm run build
```

See the repository root [README](../README.md) for complete project and backend documentation.
