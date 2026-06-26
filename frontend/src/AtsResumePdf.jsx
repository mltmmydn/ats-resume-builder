import { Document, Page, StyleSheet, Text, View } from '@react-pdf/renderer'

const styles = StyleSheet.create({
  page: {
    paddingTop: 42,
    paddingRight: 48,
    paddingBottom: 42,
    paddingLeft: 48,
    color: '#111827',
    fontFamily: 'Helvetica',
    fontSize: 10.5,
    lineHeight: 1.35,
  },
  header: {
    marginBottom: 12,
    paddingBottom: 8,
    borderBottomWidth: 1,
    borderBottomColor: '#9ca3af',
    borderBottomStyle: 'solid',
    textAlign: 'center',
  },
  name: {
    fontSize: 22,
    fontFamily: 'Helvetica-Bold',
    textTransform: 'uppercase',
  },
  title: {
    marginTop: 4,
    fontSize: 12,
    fontFamily: 'Helvetica-Bold',
  },
  contactLine: {
    marginTop: 5,
    color: '#374151',
    fontSize: 9.5,
  },
  section: {
    marginTop: 10,
  },
  sectionTitle: {
    marginBottom: 5,
    paddingBottom: 2,
    borderBottomWidth: 1,
    borderBottomColor: '#4b5563',
    borderBottomStyle: 'solid',
    fontSize: 11.5,
    fontFamily: 'Helvetica-Bold',
    textTransform: 'uppercase',
  },
  entry: {
    marginBottom: 7,
  },
  entryHeading: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  entryHeadingMain: {
    flex: 1,
    marginRight: 12,
  },
  entryTitle: {
    fontFamily: 'Helvetica-Bold',
  },
  subtitle: {
    color: '#374151',
    fontFamily: 'Helvetica-Oblique',
  },
  date: {
    flexShrink: 0,
    fontFamily: 'Helvetica-Bold',
  },
  paragraph: {
    marginBottom: 2,
  },
  bulletRow: {
    flexDirection: 'row',
    marginTop: 2,
    paddingLeft: 8,
  },
  bulletMarker: {
    width: 6,
    marginRight: 4,
  },
  bulletText: {
    flex: 1,
  },
  strong: {
    fontFamily: 'Helvetica-Bold',
  },
})

function Section({ title, children }) {
  return (
    <View style={styles.section}>
      <Text style={styles.sectionTitle}>{title}</Text>
      {children}
    </View>
  )
}

function EntryHeading({ title, subtitle, date }) {
  return (
    <View style={styles.entryHeading}>
      <View style={styles.entryHeadingMain}>
        {title && <Text style={styles.entryTitle}>{title}</Text>}
        {subtitle && <Text style={styles.subtitle}>{subtitle}</Text>}
      </View>
      {date && <Text style={styles.date}>{date}</Text>}
    </View>
  )
}

function BulletList({ items }) {
  if (!items.length) return null

  return items.map((item, index) => (
    <View style={styles.bulletRow} key={index}>
      <Text style={styles.bulletMarker}>-</Text>
      <Text style={styles.bulletText}>{item}</Text>
    </View>
  ))
}

function AtsResumePdf({ resume, labels }) {
  const {
    personal,
    experiences,
    volunteerExperiences,
    education,
    projects,
    skills,
    languages,
    certificates,
    references,
  } = resume

  return (
    <Document
      author={personal.fullName}
      creator="ATS Resume Builder"
      producer="ATS Resume Builder"
      title={`${personal.fullName || labels.yourName} ATS Resume`}
    >
      <Page size="A4" style={styles.page} wrap>
        <View style={styles.header}>
          <Text style={styles.name}>{personal.fullName || labels.yourName}</Text>
          {personal.jobTitle && <Text style={styles.title}>{personal.jobTitle}</Text>}
          {personal.contactItems.length > 0 && (
            <Text style={styles.contactLine}>{personal.contactItems.join(' | ')}</Text>
          )}
        </View>

        {personal.summary && (
          <Section title={labels.summary}>
            <Text>{personal.summary}</Text>
          </Section>
        )}

        {experiences.length > 0 && (
          <Section title={labels.workExperience}>
            {experiences.map((experience, index) => (
              <View style={styles.entry} key={index} wrap={false}>
                <EntryHeading
                  title={experience.title}
                  subtitle={experience.subtitle}
                  date={experience.date}
                />
                <BulletList items={experience.responsibilities} />
              </View>
            ))}
          </Section>
        )}

        {volunteerExperiences.length > 0 && (
          <Section title={labels.volunteerExperience}>
            {volunteerExperiences.map((volunteer, index) => (
              <View style={styles.entry} key={index} wrap={false}>
                <EntryHeading
                  title={volunteer.title}
                  subtitle={volunteer.subtitle}
                  date={volunteer.date}
                />
                <BulletList items={volunteer.responsibilities} />
              </View>
            ))}
          </Section>
        )}

        {education.length > 0 && (
          <Section title={labels.education}>
            {education.map((educationItem, index) => (
              <View style={styles.entry} key={index} wrap={false}>
                <EntryHeading
                  title={educationItem.school}
                  subtitle={educationItem.program}
                  date={educationItem.date}
                />
              </View>
            ))}
          </Section>
        )}

        {projects.length > 0 && (
          <Section title={labels.projects}>
            {projects.map((project, index) => (
              <View style={styles.entry} key={index} wrap={false}>
                <Text style={styles.entryTitle}>{project.name}</Text>
                {project.description && (
                  <Text style={styles.paragraph}>{project.description}</Text>
                )}
                {project.technologies && (
                  <Text>
                    <Text style={styles.strong}>{labels.technologies}: </Text>
                    {project.technologies}
                  </Text>
                )}
              </View>
            ))}
          </Section>
        )}

        {skills.length > 0 && (
          <Section title={labels.skills}>
            {skills.map((skill, index) => (
              <Text style={styles.paragraph} key={index}>
                <Text style={styles.strong}>{skill.category}: </Text>
                {skill.items}
              </Text>
            ))}
          </Section>
        )}

        {languages.length > 0 && (
          <Section title={labels.languages}>
            <Text>{languages.join(' | ')}</Text>
          </Section>
        )}

        {certificates.length > 0 && (
          <Section title={labels.certificates}>
            {certificates.map((certificate, index) => (
              <View style={styles.entry} key={index} wrap={false}>
                <View style={styles.entryHeading}>
                  <Text style={styles.entryHeadingMain}>
                    <Text style={styles.strong}>{certificate.name}</Text>
                    {certificate.issuer && ` | ${certificate.issuer}`}
                  </Text>
                  {certificate.date && <Text style={styles.date}>{certificate.date}</Text>}
                </View>
                <BulletList items={certificate.details} />
              </View>
            ))}
          </Section>
        )}

        {references.mode === 'uponRequest' && (
          <Section title={labels.references}>
            <Text>{labels.referencesUponRequest}</Text>
          </Section>
        )}

        {references.mode === 'contacts' && references.contacts.length > 0 && (
          <Section title={labels.references}>
            {references.contacts.map((reference, index) => (
              <View style={styles.entry} key={index} wrap={false}>
                <Text>
                  <Text style={styles.strong}>{reference.name}</Text>
                  {reference.role && ` - ${reference.role}`}
                </Text>
                {reference.contact && <Text>{reference.contact}</Text>}
              </View>
            ))}
          </Section>
        )}
      </Page>
    </Document>
  )
}

export default AtsResumePdf
