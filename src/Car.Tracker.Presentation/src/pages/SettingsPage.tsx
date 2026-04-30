import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { ThemeToggle } from '../components/ThemeToggle'
import { useTranslation } from 'react-i18next'

export function SettingsPage() {
  useTranslation(['common'])

  return (
    <div className="page">
      <h1 style={{ margin: 0 }}>Settings</h1>
      <p style={{ margin: '6px 0 0', opacity: 0.75 }}>Personalize your app appearance and language.</p>

      <section className="card">
        <h2 style={{ marginTop: 0 }}>Appearance</h2>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12, alignItems: 'center' }}>
          <ThemeToggle />
        </div>
      </section>

      <section className="card">
        <h2 style={{ marginTop: 0 }}>Language</h2>
        <LanguageSwitcher />
      </section>
    </div>
  )
}

