import { useEffect, useState } from 'react'
import './App.css'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim().replace(/\/+$/, '') || ''
const backendPdfEnabled = Boolean(apiBaseUrl)

const emptyExperience = {
  company: '',
  title: '',
  location: '',
  startDate: '',
  endDate: '',
  currentlyWorking: false,
  responsibilities: '',
}

const emptyVolunteerExperience = {
  organizationName: '',
  role: '',
  location: '',
  startDate: '',
  endDate: '',
  isCurrent: false,
  responsibilities: '',
}

const emptyEducation = {
  school: '',
  degree: '',
  department: '',
  startDate: '',
  endDate: '',
  gpa: '',
}

const emptyProject = {
  name: '',
  description: '',
  technologies: '',
}

const emptyLanguage = { name: '', level: '' }
const emptyCertificate = { name: '', issuer: '', date: '', details: '' }
const emptyCustomField = { label: '', value: '' }
const emptySkillCategory = { category: '', items: '' }
const emptyReference = {
  fullName: '',
  jobTitle: '',
  company: '',
  email: '',
  phone: '',
  relationship: '',
}

const turkishTranslations = {
  'Resume workspace': 'Özgeçmiş çalışma alanı',
  'ATS Resume Builder': 'ATS Uyumlu Özgeçmiş Oluşturucu',
  'Complete the fields and review your resume as you type.':
    'Alanları doldurun ve yazarken özgeçmişinizi inceleyin.',
  Print: 'Yazdır',
  'Download PDF': 'PDF İndir',
  'Save as PDF': 'PDF olarak Kaydet',
  'Generating PDF...': 'PDF Oluşturuluyor...',
  'Generates a PDF using the backend API.': 'Backend API kullanarak PDF oluşturur.',
  'Your resume will open in the browser print dialog. Choose “Save as PDF” to download it.':
    'Özgeçmişiniz tarayıcı yazdırma ekranında açılır. İndirmek için “PDF olarak kaydet” seçeneğini seçin.',
  'Ready to export?': 'Dışa aktarmaya hazır mısınız?',
  'Review your resume preview, then export it as PDF.':
    'Özgeçmiş önizlemenizi kontrol edin, ardından PDF olarak dışa aktarın.',
  'Could not download the PDF. Make sure the configured backend is running.':
    'PDF indirilemedi. Yapılandırılmış backend servisinin çalıştığından emin olun.',
  'Live preview': 'Canlı önizleme',
  'Personal Information': 'Kişisel Bilgiler',
  'Full Name': 'Ad Soyad',
  'Job Title': 'Pozisyon',
  Email: 'E-posta',
  Phone: 'Telefon',
  Location: 'Konum',
  'LinkedIn URL (optional)': 'LinkedIn Adresi (isteğe bağlı)',
  'GitHub URL (optional)': 'GitHub Adresi (isteğe bağlı)',
  'Portfolio URL (optional)': 'Portföy Adresi (isteğe bağlı)',
  'Additional Fields (optional)': 'Ek Alanlar (isteğe bağlı)',
  'Field Label': 'Alan Etiketi',
  'Field Value': 'Alan Değeri',
  'Add Field': 'Alan Ekle',
  Summary: 'Özet',
  'Work Experience': 'İş Deneyimi',
  Experience: 'Deneyim',
  'Company Name': 'Şirket Adı',
  'Start Date': 'Başlangıç Tarihi',
  'End Date': 'Bitiş Tarihi',
  'Ongoing role': 'Devam ediyor',
  Present: 'Devam ediyor',
  'Responsibilities (one bullet per line)': 'Sorumluluklar (her satıra bir madde)',
  'Add Experience': 'Deneyim Ekle',
  'Volunteer Experience': 'Gönüllü Deneyim',
  Volunteer: 'Gönüllü Deneyim',
  'Add Volunteer Experience': 'Gönüllü Deneyim Ekle',
  'Organization Name': 'Kurum / Organizasyon Adı',
  'Role / Position': 'Rol / Pozisyon',
  'Responsibilities / Contributions': 'Sorumluluklar / Katkılar',
  Education: 'Eğitim',
  'School Name': 'Okul Adı',
  Degree: 'Derece',
  Department: 'Bölüm',
  GPA: 'GNO',
  'Add Education': 'Eğitim Ekle',
  Projects: 'Projeler',
  Project: 'Proje',
  'Project Name': 'Proje Adı',
  'Project Description': 'Proje Açıklaması',
  'Technologies Used': 'Kullanılan Teknolojiler',
  'Add Project': 'Proje Ekle',
  Skills: 'Yetenekler',
  'Skill Category': 'Yetenek Kategorisi',
  'Category Name': 'Kategori Adı',
  'Add Skill Category': 'Yetenek Kategorisi Ekle',
  Languages: 'Diller',
  Language: 'Dil',
  'Language Name': 'Dil Adı',
  Level: 'Seviye',
  'Add Language': 'Dil Ekle',
  Certificates: 'Sertifikalar',
  Certificate: 'Sertifika',
  'Certificate Name': 'Sertifika Adı',
  Issuer: 'Veren Kurum',
  Date: 'Tarih',
  'Details / Topics Covered': 'Detaylar / İşlenen Konular',
  'Add Certificate': 'Sertifika Ekle',
  Remove: 'Kaldır',
  'Professional Summary': 'Profesyonel Özet',
  Technologies: 'Teknolojiler',
  'Your Name': 'Adınız Soyadınız',
}

const actionTranslations = {
  'Resume Actions': 'Özgeçmiş İşlemleri',
  'Clear Form': 'Formu Temizle',
  'Load Example': 'Örneği Yükle',
  Template: 'Şablon',
  'ATS Friendly': 'ATS Uyumlu',
  'Modern With Photo': 'Fotoğraflı Modern',
  'Profile Photo': 'Profil Fotoğrafı',
  'Remove Photo': 'Fotoğrafı Kaldır',
  'For best ATS compatibility, use the ATS Friendly template without a photo.':
    'En iyi ATS uyumluluğu için fotoğrafsız ATS Uyumlu şablonu kullanın.',
}

const referenceTranslations = {
  References: 'Referanslar',
  Reference: 'Referans',
  'Reference display': 'Referans gösterimi',
  'Do not include references': 'Referansları dahil etme',
  'References available upon request': 'Referanslar talep üzerine sunulabilir',
  'References available upon request.': 'Referanslar talep üzerine sunulabilir.',
  'Add reference contacts': 'Referans kişileri ekle',
  'Add Reference': 'Referans Ekle',
  Company: 'Şirket',
  Relationship: 'İlişki',
  'Light Mode': 'Açık Mod',
  'Dark Mode': 'Koyu Mod',
}

const initialResume = {
  personal: {
    fullName: 'Alex Morgan',
    jobTitle: 'Software Engineer',
    email: 'alex.morgan@example.com',
    phone: '+1 555 123 4567',
    location: 'Seattle, WA',
    linkedin: 'linkedin.com/in/alexmorgan',
    github: 'github.com/alexmorgan',
    portfolio: '',
    customFields: [{ label: 'Portfolio', value: 'alexmorgan.dev' }],
    summary:
      'Software engineer with 5+ years of experience building reliable web applications and scalable APIs. Skilled in React, TypeScript, Node.js, and cloud-native development with a strong focus on clean code and measurable product impact.',
  },
  experiences: [
    {
      company: 'Northstar Technologies',
      title: 'Senior Software Engineer',
      location: 'Seattle, WA',
      startDate: 'Jan 2023',
      endDate: '',
      currentlyWorking: true,
      responsibilities:
        'Led development of a React analytics platform used by 10,000+ customers\nReduced API response time by 40% through caching and query optimization\nMentored four engineers and introduced automated code quality checks',
    },
    {
      company: 'Bright Labs',
      title: 'Software Engineer',
      location: 'Portland, OR',
      startDate: 'Jun 2020',
      endDate: 'Dec 2022',
      currentlyWorking: false,
      responsibilities:
        'Built and maintained TypeScript services processing 2M+ daily events\nIncreased test coverage from 55% to 88% across core services',
    },
  ],
  volunteerExperiences: [],
  education: [
    {
      school: 'University of Washington',
      degree: 'Bachelor of Science',
      department: 'Computer Science',
      startDate: '2016',
      endDate: '2020',
      gpa: '3.8 / 4.0',
    },
  ],
  projects: [
    {
      name: 'Developer Metrics Dashboard',
      description:
        'Built an open-source dashboard that helps engineering teams track delivery performance and identify workflow bottlenecks.',
      technologies: 'React, TypeScript, Node.js, PostgreSQL, Docker',
    },
  ],
  skills: [
    {
      category: 'Technical Skills',
      items: 'JavaScript, TypeScript, React, Node.js, Python, REST APIs',
    },
    { category: 'Tools', items: 'Git, Docker, AWS, GitHub Actions, Terraform' },
    { category: 'Databases', items: 'PostgreSQL, MySQL, MongoDB, Redis' },
    {
      category: 'Architecture / Patterns',
      items: 'Microservices, Clean Architecture, MVC, Test-Driven Development',
    },
  ],
  languages: [
    { name: 'English', level: 'Native' },
    { name: 'Spanish', level: 'Professional working proficiency' },
  ],
  certificates: [
    {
      name: 'AWS Certified Solutions Architect - Associate',
      issuer: 'Amazon Web Services',
      date: '2024',
      details:
        'Designing resilient architectures\nDesigning high-performing architectures\nDesigning secure applications and architectures',
    },
  ],
  referenceMode: 'uponRequest',
  references: [],
}

const emptyResume = {
  personal: {
    fullName: '',
    jobTitle: '',
    email: '',
    phone: '',
    location: '',
    linkedin: '',
    github: '',
    portfolio: '',
    customFields: [],
    summary: '',
  },
  experiences: [],
  volunteerExperiences: [],
  education: [],
  projects: [],
  skills: [],
  languages: [],
  certificates: [],
  referenceMode: 'uponRequest',
  references: [],
}

const cloneResume = (resume) => JSON.parse(JSON.stringify(resume))

const getInitialTheme = () => {
  try {
    return localStorage.getItem('resume-builder-theme') === 'dark' ? 'dark' : 'light'
  } catch {
    return 'light'
  }
}

function Field({
  label,
  value,
  onChange,
  type = 'text',
  placeholder,
  wide = false,
  disabled = false,
}) {
  return (
    <label className={wide ? 'field field-wide' : 'field'}>
      <span>{label}</span>
      <input
        type={type}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        disabled={disabled}
      />
    </label>
  )
}

function TextAreaField({ label, value, onChange, placeholder, rows = 4 }) {
  return (
    <label className="field field-wide">
      <span>{label}</span>
      <textarea
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        rows={rows}
      />
    </label>
  )
}

function FormSection({ title, children }) {
  return (
    <section className="form-section">
      <h2>{title}</h2>
      {children}
    </section>
  )
}

function RepeatedItem({ title, onRemove, removeLabel, children, className = '' }) {
  return (
    <div className={`repeated-item ${className}`.trim()}>
      <div className="repeated-item-heading">
        <h3>{title}</h3>
        <button type="button" className="remove-button" onClick={onRemove}>
          {removeLabel}
        </button>
      </div>
      <div className="field-grid">{children}</div>
    </div>
  )
}

function OngoingRoleToggle({ checked, onChange, label }) {
  return (
    <label className="ongoing-role-toggle">
      <input
        type="checkbox"
        checked={checked}
        onChange={(event) => onChange(event.target.checked)}
      />
      <span className="toggle-track" aria-hidden="true">
        <span className="toggle-knob" />
      </span>
      <span className="toggle-label">{label}</span>
    </label>
  )
}

function PreviewSection({ title, children }) {
  return (
    <section className="resume-section">
      <h2>{title}</h2>
      {children}
    </section>
  )
}

function ContactItem({ label, value, href }) {
  if (!value?.trim()) return null

  return href ? (
    <a href={href} target="_blank" rel="noreferrer">
      {label && <strong>{label}: </strong>}
      {value.trim()}
    </a>
  ) : (
    <span>
      {label && <strong>{label}: </strong>}
      {value.trim()}
    </span>
  )
}

function LanguageSwitch({ language, onChange }) {
  return (
    <div className="language-switch" aria-label="Language">
      <button
        type="button"
        className={language === 'tr' ? 'active' : ''}
        onClick={() => onChange('tr')}
      >
        TR
      </button>
      <button
        type="button"
        className={language === 'en' ? 'active' : ''}
        onClick={() => onChange('en')}
      >
        EN
      </button>
    </div>
  )
}

function ThemeSwitch({ theme, onChange, t }) {
  return (
    <div className="theme-switch" aria-label="Theme">
      <button
        type="button"
        className={theme === 'light' ? 'active' : ''}
        onClick={() => onChange('light')}
        aria-label={t('Light Mode')}
        title={t('Light Mode')}
      >
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <circle cx="12" cy="12" r="4" />
          <path d="M12 2v2M12 20v2M4.93 4.93l1.42 1.42M17.66 17.66l1.41 1.41M2 12h2M20 12h2M4.93 19.07l1.42-1.42M17.66 6.34l1.41-1.41" />
        </svg>
      </button>
      <button
        type="button"
        className={theme === 'dark' ? 'active' : ''}
        onClick={() => onChange('dark')}
        aria-label={t('Dark Mode')}
        title={t('Dark Mode')}
      >
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="M20.5 14.2A8.5 8.5 0 0 1 9.8 3.5 8.5 8.5 0 1 0 20.5 14.2Z" />
        </svg>
      </button>
    </div>
  )
}

function TemplateSelector({ template, onChange, t }) {
  const options = [
    { value: 'ats', label: t('ATS Friendly') },
    { value: 'modern', label: t('Modern With Photo') },
  ]

  return (
    <div className="template-selector" role="group" aria-label={t('Template')}>
      {options.map((option) => (
        <button
          type="button"
          aria-pressed={option.value === template}
          className={option.value === template ? 'active' : ''}
          key={option.value}
          onClick={() => onChange(option.value)}
        >
          {option.label}
        </button>
      ))}
    </div>
  )
}

function App() {
  const [resume, setResume] = useState(() => cloneResume(initialResume))
  const [language, setLanguage] = useState('en')
  const [template, setTemplate] = useState('ats')
  const [profilePhoto, setProfilePhoto] = useState('')
  const [photoInputKey, setPhotoInputKey] = useState(0)
  const [theme, setTheme] = useState(getInitialTheme)
  const [isDownloadingPdf, setIsDownloadingPdf] = useState(false)
  const [pdfError, setPdfError] = useState('')
  const t = (text) =>
    language === 'tr'
      ? referenceTranslations[text] || actionTranslations[text] || turkishTranslations[text] || text
      : text

  useEffect(() => {
    try {
      localStorage.setItem('resume-builder-theme', theme)
    } catch {
      // The selected theme still works when browser storage is unavailable.
    }
  }, [theme])

  const clearPhoto = () => {
    setProfilePhoto('')
    setPhotoInputKey((current) => current + 1)
  }

  const clearForm = () => {
    setResume(cloneResume(emptyResume))
    clearPhoto()
  }

  const loadExample = () => {
    setResume(cloneResume(initialResume))
    clearPhoto()
  }

  const handlePhotoUpload = (event) => {
    const file = event.target.files?.[0]
    if (!file) return

    const reader = new FileReader()
    reader.onload = () => setProfilePhoto(reader.result)
    reader.readAsDataURL(file)
  }

  const updatePersonal = (field, value) => {
    setResume((current) => ({
      ...current,
      personal: { ...current.personal, [field]: value },
    }))
  }

  const updateCustomField = (index, field, value) => {
    setResume((current) => ({
      ...current,
      personal: {
        ...current.personal,
        customFields: current.personal.customFields.map((item, itemIndex) =>
          itemIndex === index ? { ...item, [field]: value } : item,
        ),
      },
    }))
  }

  const addCustomField = () => {
    setResume((current) => ({
      ...current,
      personal: {
        ...current.personal,
        customFields: [...current.personal.customFields, { ...emptyCustomField }],
      },
    }))
  }

  const removeCustomField = (index) => {
    setResume((current) => ({
      ...current,
      personal: {
        ...current.personal,
        customFields: current.personal.customFields.filter(
          (_, itemIndex) => itemIndex !== index,
        ),
      },
    }))
  }

  const updateListItem = (listName, index, field, value) => {
    setResume((current) => ({
      ...current,
      [listName]: current[listName].map((item, itemIndex) =>
        itemIndex === index ? { ...item, [field]: value } : item,
      ),
    }))
  }

  const addListItem = (listName, emptyItem) => {
    setResume((current) => ({
      ...current,
      [listName]: [...current[listName], { ...emptyItem }],
    }))
  }

  const removeListItem = (listName, index) => {
    setResume((current) => ({
      ...current,
      [listName]: current[listName].filter((_, itemIndex) => itemIndex !== index),
    }))
  }

  const formatUrl = (url) => {
    if (!url) return ''
    return url.startsWith('http://') || url.startsWith('https://')
      ? url
      : `https://${url}`
  }

  const responsibilityLines = (responsibilities) =>
    (responsibilities || '')
      .split('\n')
      .map((line) => line.trim())
      .filter(Boolean)

  const commaSeparatedValues = (value) =>
    value
      .split(',')
      .map((item) => item.trim())
      .filter(Boolean)

  const educationProgramLine = (education) =>
    [
      [education.degree, education.department].filter(Boolean).join(' / '),
      education.gpa && `${t('GPA')}: ${education.gpa}`,
    ]
      .filter(Boolean)
      .join(' | ')

  const roleEndDate = (role) =>
    role.currentlyWorking || role.isCurrent || /^on\s?going$/i.test((role.endDate || '').trim())
      ? t('Present')
      : role.endDate

  const hasMeaningfulVolunteerFields = (volunteer) =>
    [
      volunteer.organizationName,
      volunteer.role,
      volunteer.location,
      volunteer.startDate,
      volunteer.endDate,
      volunteer.responsibilities,
    ].some((value) => value?.trim())

  const skillCategoryLabel = (category) =>
    (category || '').trim().replace(/:+$/, '').trim() || t('Skills')

  const createPdfFileName = (fullName) => {
    const safeName = fullName
      .trim()
      .replace(/\s+/g, '_')
      .replace(/[^\p{L}\p{N}_-]/gu, '')

    return safeName ? `${safeName}_ATS_Resume.pdf` : 'ATS_Resume.pdf'
  }

  const createBackendResume = () => ({
    language,
    template,
    profilePhotoBase64: template === 'modern' && profilePhoto ? profilePhoto : null,
    personalInfo: {
      fullName: resume.personal.fullName,
      jobTitle: resume.personal.jobTitle,
      email: resume.personal.email,
      phone: resume.personal.phone,
      location: resume.personal.location,
      linkedInUrl: resume.personal.linkedin,
      gitHubUrl: resume.personal.github,
      portfolioUrl: resume.personal.portfolio,
      summary: resume.personal.summary,
      customFields: populatedCustomFields,
    },
    experiences: resume.experiences.map((experience) => ({
      companyName: experience.company,
      jobTitle: experience.title,
      location: experience.location,
      startDate: experience.startDate,
      endDate: roleEndDate(experience),
      responsibilities: responsibilityLines(experience.responsibilities),
    })),
    volunteerExperiences: resume.volunteerExperiences
      .filter(hasMeaningfulVolunteerFields)
      .map((volunteer) => ({
        organizationName: volunteer.organizationName,
        role: volunteer.role,
        location: volunteer.location,
        startDate: volunteer.startDate,
        endDate: volunteer.endDate,
        isCurrent: volunteer.isCurrent,
        responsibilities: responsibilityLines(volunteer.responsibilities),
      })),
    education: resume.education.map((education) => ({
      schoolName: education.school,
      degree: education.degree,
      department: education.department,
      startDate: education.startDate,
      endDate: education.endDate,
      gpa: education.gpa,
    })),
    projects: resume.projects.map((project) => ({
      projectName: project.name,
      description: project.description,
      technologies: project.technologies,
    })),
    skills: resume.skills
      .map((skill) => ({
        category: skillCategoryLabel(skill.category),
        skills: commaSeparatedValues(skill.items),
      }))
      .filter((skill) => skill.skills.length > 0),
    languages: resume.languages.map((language) => ({
      languageName: language.name,
      level: language.level,
    })),
    certificates: resume.certificates.map((certificate) => ({
      certificateName: certificate.name,
      issuer: certificate.issuer,
      date: certificate.date,
      details: responsibilityLines(certificate.details),
    })),
    referenceMode: resume.referenceMode,
    references: resume.references.map((reference) => ({
      fullName: reference.fullName,
      jobTitle: reference.jobTitle,
      company: reference.company,
      email: reference.email,
      phone: reference.phone,
      relationship: reference.relationship,
    })),
  })

  const downloadBackendPdf = async () => {
    if (!backendPdfEnabled) return

    setIsDownloadingPdf(true)
    setPdfError('')

    try {
      const response = await fetch(`${apiBaseUrl}/api/resume/generate-pdf`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(createBackendResume()),
      })

      if (!response.ok) throw new Error(`PDF request failed with status ${response.status}`)

      const blob = await response.blob()
      const downloadUrl = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = downloadUrl
      link.download = createPdfFileName(resume.personal.fullName)
      document.body.appendChild(link)
      link.click()
      link.remove()
      window.setTimeout(() => URL.revokeObjectURL(downloadUrl), 0)
    } catch {
      setPdfError(
        t('Could not download the PDF. Make sure the configured backend is running.'),
      )
    } finally {
      setIsDownloadingPdf(false)
    }
  }

  const populatedCustomFields = resume.personal.customFields.filter(
    (field) => field.label.trim() && field.value.trim(),
  )
  const populatedSkills = resume.skills.filter((skill) => skill.items.trim())
  const populatedVolunteerExperiences = resume.volunteerExperiences.filter(
    hasMeaningfulVolunteerFields,
  )
  const hasOptionalContactFields =
    Boolean(resume.personal.linkedin.trim()) ||
    Boolean(resume.personal.github.trim()) ||
    Boolean(resume.personal.portfolio.trim()) ||
    populatedCustomFields.length > 0

  return (
    <main className={`app-shell ${theme === 'dark' ? 'dark-theme' : ''}`}>
      <aside className="editor-panel">
        <div className="editor-header">
          <div>
            <p className="eyebrow">{t('Resume workspace')}</p>
            <h1>{t('ATS Resume Builder')}</h1>
            <p>{t('Complete the fields and review your resume as you type.')}</p>
          </div>
          <div className="header-actions">
            <ThemeSwitch theme={theme} onChange={setTheme} t={t} />
            <LanguageSwitch language={language} onChange={setLanguage} />
          </div>
        </div>
        <div className="template-controls">
          <div className="template-field">
            <span>{t('Template')}</span>
            <TemplateSelector template={template} onChange={setTemplate} t={t} />
          </div>
          <p>{t('For best ATS compatibility, use the ATS Friendly template without a photo.')}</p>
        </div>
        {template === 'modern' && (
          <div className="photo-upload-panel">
            <label className="photo-upload-label">
              <span>{t('Profile Photo')}</span>
              <input
                key={photoInputKey}
                type="file"
                accept="image/*"
                onChange={handlePhotoUpload}
              />
            </label>
            {profilePhoto && (
              <button type="button" className="remove-button" onClick={clearPhoto}>
                {t('Remove Photo')}
              </button>
            )}
          </div>
        )}
        <div className="document-actions-section">
          <p className="document-actions-label">{t('Resume Actions')}</p>
          <div className="document-actions">
            <button type="button" className="secondary-button" onClick={clearForm}>
              {t('Clear Form')}
            </button>
            <button type="button" className="secondary-button" onClick={loadExample}>
              {t('Load Example')}
            </button>
            <div className="action-with-help primary-action">
              {backendPdfEnabled ? (
                <>
                  <button
                    type="button"
                    className="download-button"
                    onClick={downloadBackendPdf}
                    disabled={isDownloadingPdf}
                  >
                    {t(isDownloadingPdf ? 'Generating PDF...' : 'Download PDF')}
                  </button>
                  <span>{t('Generates a PDF using the backend API.')}</span>
                </>
              ) : (
                <>
                  <button type="button" className="download-button" onClick={() => window.print()}>
                    {t('Save as PDF')}
                  </button>
                  <span>
                    {t(
                      'Your resume will open in the browser print dialog. Choose “Save as PDF” to download it.',
                    )}
                  </span>
                </>
              )}
            </div>
          </div>
        </div>
        {pdfError && (
          <p className="pdf-error" role="alert">
            {pdfError}
          </p>
        )}
        <form>
          <FormSection title={t('Personal Information')}>
            <div className="field-grid">
              <Field
                label={t('Full Name')}
                value={resume.personal.fullName}
                onChange={(value) => updatePersonal('fullName', value)}
                placeholder="Alex Morgan"
              />
              <Field
                label={t('Job Title')}
                value={resume.personal.jobTitle}
                onChange={(value) => updatePersonal('jobTitle', value)}
                placeholder="Software Engineer"
              />
              <Field
                label={t('Email')}
                type="email"
                value={resume.personal.email}
                onChange={(value) => updatePersonal('email', value)}
                placeholder="alex.morgan@example.com"
              />
              <Field
                label={t('Phone')}
                type="tel"
                value={resume.personal.phone}
                onChange={(value) => updatePersonal('phone', value)}
                placeholder="+1 555 123 4567"
              />
              <Field
                label={t('Location')}
                value={resume.personal.location}
                onChange={(value) => updatePersonal('location', value)}
                placeholder="Seattle, WA"
              />
              <Field
                label={t('LinkedIn URL (optional)')}
                value={resume.personal.linkedin}
                onChange={(value) => updatePersonal('linkedin', value)}
                placeholder="linkedin.com/in/alexmorgan"
              />
              <Field
                label={t('GitHub URL (optional)')}
                value={resume.personal.github}
                onChange={(value) => updatePersonal('github', value)}
                placeholder="github.com/alexmorgan"
              />
              <Field
                label={t('Portfolio URL (optional)')}
                value={resume.personal.portfolio}
                onChange={(value) => updatePersonal('portfolio', value)}
                placeholder="alexmorgan.dev"
              />
              <TextAreaField
                label={t('Summary')}
                value={resume.personal.summary}
                onChange={(value) => updatePersonal('summary', value)}
                placeholder="Summarize your experience, strengths, and career focus."
              />
            </div>
            <div className="personal-subsection">
              <h3>{t('Additional Fields (optional)')}</h3>
              <div className="custom-fields-list">
                {resume.personal.customFields.map((field, index) => (
                  <div className="custom-field-row" key={index}>
                    <Field
                      label={t('Field Label')}
                      value={field.label}
                      onChange={(value) => updateCustomField(index, 'label', value)}
                      placeholder="Website"
                    />
                    <Field
                      label={t('Field Value')}
                      value={field.value}
                      onChange={(value) => updateCustomField(index, 'value', value)}
                      placeholder="meltemmeydan.dev"
                    />
                    <button
                      type="button"
                      className="remove-button custom-field-remove"
                      onClick={() => removeCustomField(index)}
                    >
                      {t('Remove')}
                    </button>
                  </div>
                ))}
              </div>
              <button
                type="button"
                className="add-button compact-add-button"
                onClick={addCustomField}
              >
                {t('Add Field')}
              </button>
            </div>
          </FormSection>

          <FormSection title={t('Work Experience')}>
            {resume.experiences.map((experience, index) => (
              <RepeatedItem
                key={index}
                title={`${t('Experience')} ${index + 1}`}
                onRemove={() => removeListItem('experiences', index)}
                removeLabel={t('Remove')}
                className="experience-item"
              >
                <Field
                  label={t('Company Name')}
                  value={experience.company}
                  onChange={(value) => updateListItem('experiences', index, 'company', value)}
                  placeholder="Northstar Technologies"
                />
                <Field
                  label={t('Job Title')}
                  value={experience.title}
                  onChange={(value) => updateListItem('experiences', index, 'title', value)}
                  placeholder="Senior Software Engineer"
                />
                <Field
                  label={t('Location')}
                  value={experience.location}
                  onChange={(value) => updateListItem('experiences', index, 'location', value)}
                  placeholder="Seattle, WA"
                />
                <OngoingRoleToggle
                  checked={experience.currentlyWorking}
                  onChange={(checked) =>
                    updateListItem('experiences', index, 'currentlyWorking', checked)
                  }
                  label={t('Ongoing role')}
                />
                <Field
                  label={t('Start Date')}
                  value={experience.startDate}
                  onChange={(value) => updateListItem('experiences', index, 'startDate', value)}
                  placeholder="Jan 2023"
                />
                <Field
                  label={t('End Date')}
                  value={roleEndDate(experience)}
                  onChange={(value) => updateListItem('experiences', index, 'endDate', value)}
                  placeholder={t('Present')}
                  disabled={experience.currentlyWorking}
                />
                <TextAreaField
                  label={t('Responsibilities (one bullet per line)')}
                  value={experience.responsibilities}
                  onChange={(value) =>
                    updateListItem('experiences', index, 'responsibilities', value)
                  }
                  placeholder={'Built customer-facing React features\nImproved API response time by 40%'}
                  rows={5}
                />
              </RepeatedItem>
            ))}
            <button
              type="button"
              className="add-button"
              onClick={() => addListItem('experiences', emptyExperience)}
            >
              {t('Add Experience')}
            </button>
          </FormSection>

          <FormSection title={t('Volunteer Experience')}>
            {resume.volunteerExperiences.map((volunteer, index) => (
              <RepeatedItem
                key={index}
                title={`${t('Volunteer')} ${index + 1}`}
                onRemove={() => removeListItem('volunteerExperiences', index)}
                removeLabel={t('Remove')}
                className="experience-item"
              >
                <Field
                  label={t('Organization Name')}
                  value={volunteer.organizationName}
                  onChange={(value) =>
                    updateListItem('volunteerExperiences', index, 'organizationName', value)
                  }
                  placeholder="Code for Community"
                />
                <Field
                  label={t('Role / Position')}
                  value={volunteer.role}
                  onChange={(value) =>
                    updateListItem('volunteerExperiences', index, 'role', value)
                  }
                  placeholder="Volunteer Mentor"
                />
                <Field
                  label={t('Location')}
                  value={volunteer.location}
                  onChange={(value) =>
                    updateListItem('volunteerExperiences', index, 'location', value)
                  }
                  placeholder="Seattle, WA"
                />
                <OngoingRoleToggle
                  checked={volunteer.isCurrent}
                  onChange={(checked) =>
                    updateListItem('volunteerExperiences', index, 'isCurrent', checked)
                  }
                  label={t('Ongoing role')}
                />
                <Field
                  label={t('Start Date')}
                  value={volunteer.startDate}
                  onChange={(value) =>
                    updateListItem('volunteerExperiences', index, 'startDate', value)
                  }
                  placeholder="Mar 2024"
                />
                <Field
                  label={t('End Date')}
                  value={roleEndDate(volunteer)}
                  onChange={(value) =>
                    updateListItem('volunteerExperiences', index, 'endDate', value)
                  }
                  placeholder={t('Present')}
                  disabled={volunteer.isCurrent}
                />
                <TextAreaField
                  label={t('Responsibilities / Contributions')}
                  value={volunteer.responsibilities}
                  onChange={(value) =>
                    updateListItem('volunteerExperiences', index, 'responsibilities', value)
                  }
                  placeholder={'Mentored junior developers\nOrganized monthly coding workshops'}
                  rows={5}
                />
              </RepeatedItem>
            ))}
            <button
              type="button"
              className="add-button"
              onClick={() => addListItem('volunteerExperiences', emptyVolunteerExperience)}
            >
              {t('Add Volunteer Experience')}
            </button>
          </FormSection>

          <FormSection title={t('Education')}>
            {resume.education.map((education, index) => (
              <RepeatedItem
                key={index}
                title={`${t('Education')} ${index + 1}`}
                onRemove={() => removeListItem('education', index)}
                removeLabel={t('Remove')}
              >
                <Field
                  label={t('School Name')}
                  value={education.school}
                  onChange={(value) => updateListItem('education', index, 'school', value)}
                  placeholder="University of Washington"
                />
                <Field
                  label={t('Degree')}
                  value={education.degree}
                  onChange={(value) => updateListItem('education', index, 'degree', value)}
                  placeholder="Bachelor of Science"
                />
                <Field
                  label={t('Department')}
                  value={education.department}
                  onChange={(value) => updateListItem('education', index, 'department', value)}
                  placeholder="Computer Science"
                />
                <Field
                  label={t('Start Date')}
                  value={education.startDate}
                  onChange={(value) => updateListItem('education', index, 'startDate', value)}
                  placeholder="2016"
                />
                <Field
                  label={t('End Date')}
                  value={education.endDate}
                  onChange={(value) => updateListItem('education', index, 'endDate', value)}
                  placeholder="2020"
                />
                <Field
                  label={t('GPA')}
                  value={education.gpa}
                  onChange={(value) => updateListItem('education', index, 'gpa', value)}
                  placeholder="3.8 / 4.0"
                />
              </RepeatedItem>
            ))}
            <button
              type="button"
              className="add-button"
              onClick={() => addListItem('education', emptyEducation)}
            >
              {t('Add Education')}
            </button>
          </FormSection>

          <FormSection title={t('Projects')}>
            {resume.projects.map((project, index) => (
              <RepeatedItem
                key={index}
                title={`${t('Project')} ${index + 1}`}
                onRemove={() => removeListItem('projects', index)}
                removeLabel={t('Remove')}
              >
                <Field
                  label={t('Project Name')}
                  value={project.name}
                  onChange={(value) => updateListItem('projects', index, 'name', value)}
                  placeholder="Developer Metrics Dashboard"
                  wide
                />
                <TextAreaField
                  label={t('Project Description')}
                  value={project.description}
                  onChange={(value) => updateListItem('projects', index, 'description', value)}
                  placeholder="Describe the problem, your solution, and the result."
                />
                <Field
                  label={t('Technologies Used')}
                  value={project.technologies}
                  onChange={(value) => updateListItem('projects', index, 'technologies', value)}
                  placeholder="React, TypeScript, Node.js, PostgreSQL"
                  wide
                />
              </RepeatedItem>
            ))}
            <button
              type="button"
              className="add-button"
              onClick={() => addListItem('projects', emptyProject)}
            >
              {t('Add Project')}
            </button>
          </FormSection>

          <FormSection title={t('Skills')}>
            {resume.skills.map((skill, index) => (
              <RepeatedItem
                key={index}
                title={`${t('Skill Category')} ${index + 1}`}
                onRemove={() => removeListItem('skills', index)}
                removeLabel={t('Remove')}
              >
                <Field
                  label={t('Category Name')}
                  value={skill.category}
                  onChange={(value) => updateListItem('skills', index, 'category', value)}
                  placeholder="Technical Skills"
                />
                <Field
                  label={t('Skills')}
                  value={skill.items}
                  onChange={(value) => updateListItem('skills', index, 'items', value)}
                  placeholder="JavaScript, React, Node.js"
                />
              </RepeatedItem>
            ))}
            <button
              type="button"
              className="add-button compact-add-button"
              onClick={() => addListItem('skills', emptySkillCategory)}
            >
              {t('Add Skill Category')}
            </button>
          </FormSection>

          <FormSection title={t('Languages')}>
            {resume.languages.map((language, index) => (
              <RepeatedItem
                key={index}
                title={`${t('Language')} ${index + 1}`}
                onRemove={() => removeListItem('languages', index)}
                removeLabel={t('Remove')}
              >
                <Field
                  label={t('Language Name')}
                  value={language.name}
                  onChange={(value) => updateListItem('languages', index, 'name', value)}
                  placeholder="English"
                />
                <Field
                  label={t('Level')}
                  value={language.level}
                  onChange={(value) => updateListItem('languages', index, 'level', value)}
                  placeholder="Professional working proficiency"
                />
              </RepeatedItem>
            ))}
            <button
              type="button"
              className="add-button"
              onClick={() => addListItem('languages', emptyLanguage)}
            >
              {t('Add Language')}
            </button>
          </FormSection>

          <FormSection title={t('Certificates')}>
            <div className="certificate-form-list">
              {resume.certificates.map((certificate, index) => (
                <RepeatedItem
                  key={index}
                  title={`${t('Certificate')} ${index + 1}`}
                  onRemove={() => removeListItem('certificates', index)}
                  removeLabel={t('Remove')}
                >
                  <Field
                    label={t('Certificate Name')}
                    value={certificate.name}
                    onChange={(value) => updateListItem('certificates', index, 'name', value)}
                    placeholder="AWS Certified Solutions Architect - Associate"
                    wide
                  />
                  <Field
                    label={t('Issuer')}
                    value={certificate.issuer}
                    onChange={(value) => updateListItem('certificates', index, 'issuer', value)}
                    placeholder="Amazon Web Services"
                  />
                  <Field
                    label={t('Date')}
                    value={certificate.date}
                    onChange={(value) => updateListItem('certificates', index, 'date', value)}
                    placeholder="2024"
                  />
                  <TextAreaField
                    label={t('Details / Topics Covered')}
                    value={certificate.details || ''}
                    onChange={(value) => updateListItem('certificates', index, 'details', value)}
                    placeholder={
                      'Designing resilient architectures\nDesigning secure applications'
                    }
                    rows={3}
                  />
                </RepeatedItem>
              ))}
            </div>
            <button
              type="button"
              className="add-button compact-add-button"
              onClick={() => addListItem('certificates', emptyCertificate)}
            >
              {t('Add Certificate')}
            </button>
          </FormSection>

          <FormSection title={t('References')}>
            <label className="field reference-mode-field">
              <span>{t('Reference display')}</span>
              <select
                value={resume.referenceMode}
                onChange={(event) =>
                  setResume((current) => ({ ...current, referenceMode: event.target.value }))
                }
              >
                <option value="none">{t('Do not include references')}</option>
                <option value="uponRequest">{t('References available upon request')}</option>
                <option value="contacts">{t('Add reference contacts')}</option>
              </select>
            </label>

            {resume.referenceMode === 'contacts' && (
              <div className="reference-form-list">
                {resume.references.map((reference, index) => (
                  <RepeatedItem
                    key={index}
                    title={`${t('Reference')} ${index + 1}`}
                    onRemove={() => removeListItem('references', index)}
                    removeLabel={t('Remove')}
                  >
                    <Field
                      label={t('Full Name')}
                      value={reference.fullName}
                      onChange={(value) => updateListItem('references', index, 'fullName', value)}
                      placeholder="Jordan Lee"
                    />
                    <Field
                      label={t('Job Title')}
                      value={reference.jobTitle}
                      onChange={(value) => updateListItem('references', index, 'jobTitle', value)}
                      placeholder="Engineering Manager"
                    />
                    <Field
                      label={t('Company')}
                      value={reference.company}
                      onChange={(value) => updateListItem('references', index, 'company', value)}
                      placeholder="Northstar Technologies"
                    />
                    <Field
                      label={t('Relationship')}
                      value={reference.relationship}
                      onChange={(value) =>
                        updateListItem('references', index, 'relationship', value)
                      }
                      placeholder="Former Manager"
                    />
                    <Field
                      label={t('Email')}
                      type="email"
                      value={reference.email}
                      onChange={(value) => updateListItem('references', index, 'email', value)}
                      placeholder="jordan.lee@example.com"
                    />
                    <Field
                      label={t('Phone')}
                      type="tel"
                      value={reference.phone}
                      onChange={(value) => updateListItem('references', index, 'phone', value)}
                      placeholder="+1 555 987 6543"
                    />
                  </RepeatedItem>
                ))}
                <button
                  type="button"
                  className="add-button compact-add-button"
                  onClick={() => addListItem('references', emptyReference)}
                >
                  {t('Add Reference')}
                </button>
              </div>
            )}
          </FormSection>
        </form>

        <section className="bottom-export" aria-labelledby="bottom-export-title">
          <div>
            <h2 id="bottom-export-title">{t('Ready to export?')}</h2>
            <p>{t('Review your resume preview, then export it as PDF.')}</p>
          </div>
          <div className={`bottom-export-actions ${backendPdfEnabled ? '' : 'single-action'}`}>
            {backendPdfEnabled ? (
              <>
                <button
                  type="button"
                  className="download-button"
                  onClick={downloadBackendPdf}
                  disabled={isDownloadingPdf}
                >
                  {t(isDownloadingPdf ? 'Generating PDF...' : 'Download PDF')}
                </button>
                <button type="button" className="print-button" onClick={() => window.print()}>
                  {t('Print')}
                </button>
              </>
            ) : (
              <button type="button" className="download-button" onClick={() => window.print()}>
                {t('Save as PDF')}
              </button>
            )}
          </div>
          {pdfError && (
            <p className="pdf-error" role="alert">
              {pdfError}
            </p>
          )}
        </section>
      </aside>

      <section className="preview-panel" aria-label={t('Live preview')}>
        <div className="preview-toolbar">
          <span>{t('Live preview')}</span>
        </div>

        <article className={`resume-preview ${template === 'modern' ? 'modern-template' : ''}`}>
          <header className="resume-header">
            <div className="resume-header-content">
              <h1>{resume.personal.fullName || t('Your Name')}</h1>
              {resume.personal.jobTitle && (
                <p className="resume-title">{resume.personal.jobTitle}</p>
              )}
              <div className="contact-line">
                <ContactItem
                  value={resume.personal.email}
                  href={`mailto:${resume.personal.email}`}
                />
                <ContactItem value={resume.personal.phone} href={`tel:${resume.personal.phone}`} />
                <ContactItem value={resume.personal.location} />
              </div>
              {hasOptionalContactFields && (
                <div className="contact-line optional-contact-line">
                  <ContactItem
                    label="LinkedIn"
                    value={resume.personal.linkedin}
                    href={formatUrl(resume.personal.linkedin)}
                  />
                  <ContactItem
                    label="GitHub"
                    value={resume.personal.github}
                    href={formatUrl(resume.personal.github)}
                  />
                  <ContactItem
                    label="Portfolio"
                    value={resume.personal.portfolio}
                    href={formatUrl(resume.personal.portfolio)}
                  />
                  {populatedCustomFields.map((field, index) => (
                    <ContactItem label={field.label} value={field.value} key={index} />
                  ))}
                </div>
              )}
            </div>
            {template === 'modern' && profilePhoto && (
              <img className="profile-photo" src={profilePhoto} alt="" />
            )}
          </header>

          {resume.personal.summary && (
            <PreviewSection title={t('Professional Summary')}>
              <p>{resume.personal.summary}</p>
            </PreviewSection>
          )}

          {resume.experiences.length > 0 && (
            <PreviewSection title={t('Work Experience')}>
              {resume.experiences.map((experience, index) => (
                <div className="resume-entry" key={index}>
                  <div className="entry-heading">
                    <div>
                      <h3>{experience.title || t('Job Title')}</h3>
                      <p className="entry-subtitle">
                        {[experience.company, experience.location].filter(Boolean).join(' | ')}
                      </p>
                    </div>
                    <p className="entry-date">
                      {[experience.startDate, roleEndDate(experience)]
                        .filter(Boolean)
                        .join(' - ')}
                    </p>
                  </div>
                  {responsibilityLines(experience.responsibilities).length > 0 && (
                    <ul>
                      {responsibilityLines(experience.responsibilities).map((line, lineIndex) => (
                        <li key={lineIndex}>{line}</li>
                      ))}
                    </ul>
                  )}
                </div>
              ))}
            </PreviewSection>
          )}

          {populatedVolunteerExperiences.length > 0 && (
            <PreviewSection title={t('Volunteer Experience')}>
              {populatedVolunteerExperiences.map((volunteer, index) => (
                <div className="resume-entry" key={index}>
                  <div className="entry-heading">
                    <div>
                      <h3>{volunteer.role || t('Role / Position')}</h3>
                      <p className="entry-subtitle">
                        {[volunteer.organizationName, volunteer.location]
                          .filter(Boolean)
                          .join(' | ')}
                      </p>
                    </div>
                    <p className="entry-date">
                      {[volunteer.startDate, roleEndDate(volunteer)]
                        .filter(Boolean)
                        .join(' - ')}
                    </p>
                  </div>
                  {responsibilityLines(volunteer.responsibilities).length > 0 && (
                    <ul>
                      {responsibilityLines(volunteer.responsibilities).map((line, lineIndex) => (
                        <li key={lineIndex}>{line}</li>
                      ))}
                    </ul>
                  )}
                </div>
              ))}
            </PreviewSection>
          )}

          {resume.education.length > 0 && (
            <PreviewSection title={t('Education')}>
              {resume.education.map((education, index) => (
                <div className="resume-entry" key={index}>
                  <div className="entry-heading">
                    <div>
                      <h3>{education.school || t('School Name')}</h3>
                      {educationProgramLine(education) && (
                        <p className="entry-subtitle">{educationProgramLine(education)}</p>
                      )}
                    </div>
                    <p className="entry-date">
                      {[education.startDate, education.endDate].filter(Boolean).join(' - ')}
                    </p>
                  </div>
                </div>
              ))}
            </PreviewSection>
          )}

          {resume.projects.length > 0 && (
            <PreviewSection title={t('Projects')}>
              {resume.projects.map((project, index) => (
                <div className="resume-entry" key={index}>
                  <h3>{project.name || t('Project Name')}</h3>
                  {project.description && <p>{project.description}</p>}
                  {project.technologies && (
                    <p>
                      <strong>{t('Technologies')}:</strong> {project.technologies}
                    </p>
                  )}
                </div>
              ))}
            </PreviewSection>
          )}

          {populatedSkills.length > 0 && (
            <PreviewSection title={t('Skills')}>
              <div className="skills-list">
                {populatedSkills.map((skill, index) => (
                  <p key={index}>
                    <strong>{skillCategoryLabel(skill.category)}:</strong> {skill.items.trim()}
                  </p>
                ))}
              </div>
            </PreviewSection>
          )}

          {resume.languages.length > 0 && (
            <PreviewSection title={t('Languages')}>
              <p>
                {resume.languages
                  .map((language) => [language.name, language.level].filter(Boolean).join(' - '))
                  .filter(Boolean)
                  .join(' | ')}
              </p>
            </PreviewSection>
          )}

          {resume.certificates.length > 0 && (
            <PreviewSection title={t('Certificates')}>
              {resume.certificates.map((certificate, index) => (
                <div className="certificate-entry" key={index}>
                  <div className="certificate-heading">
                    <p>
                      <strong>• {certificate.name || t('Certificate Name')}</strong>
                      {certificate.issuer && ` | ${certificate.issuer}`}
                    </p>
                    {certificate.date && <p className="entry-date">{certificate.date}</p>}
                  </div>
                  {responsibilityLines(certificate.details).length > 0 && (
                    <ul className="certificate-details">
                      {responsibilityLines(certificate.details).map((detail, detailIndex) => (
                        <li key={detailIndex}>{detail}</li>
                      ))}
                    </ul>
                  )}
                </div>
              ))}
            </PreviewSection>
          )}

          {resume.referenceMode === 'uponRequest' && (
            <PreviewSection title={t('References')}>
              <p>{t('References available upon request.')}</p>
            </PreviewSection>
          )}

          {resume.referenceMode === 'contacts' && resume.references.length > 0 && (
            <PreviewSection title={t('References')}>
              {resume.references.map((reference, index) => (
                <div className="reference-entry" key={index}>
                  <p>
                    <strong>{reference.fullName || t('Full Name')}</strong>
                    {(reference.jobTitle || reference.company) &&
                      ` - ${[reference.jobTitle, reference.company].filter(Boolean).join(', ')}`}
                  </p>
                  <p>
                    {[reference.email, reference.phone, reference.relationship]
                      .filter(Boolean)
                      .join(' | ')}
                  </p>
                </div>
              ))}
            </PreviewSection>
          )}
        </article>
      </section>
    </main>
  )
}

export default App
