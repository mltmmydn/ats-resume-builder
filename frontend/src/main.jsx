import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'

function App() {
  return (
    <main
      style={{
        minHeight: '100vh',
        padding: '32px',
        fontFamily: 'Arial, sans-serif',
        background: '#ffffff',
        color: '#111827',
      }}
    >
      <h1>ATS Resume Builder</h1>
      <p>Safari test page opened successfully.</p>
      <p>If you can see this on iPhone, the problem is inside App.jsx or App.css.</p>
    </main>
  )
}

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)