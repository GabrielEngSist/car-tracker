import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { PageTitle } from '../components/PageTitle'
import { ThemeToggle } from '../components/ThemeToggle'
import { useTranslation } from 'react-i18next'

export function SettingsPage() {
  const { t } = useTranslation(['common'])

  return (
    <div className="page">
      <PageTitle>{t('common:nav.settings')}</PageTitle>
      <p style={{ margin: '6px 0 0', opacity: 0.75 }}>{t('common:settings.subtitle')}</p>

      <section className="card">
        <h2 style={{ marginTop: 0 }}>{t('common:settings.appearance')}</h2>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12, alignItems: 'center' }}>
          <ThemeToggle />
        </div>
      </section>

      <section className="card">
        <h2 style={{ marginTop: 0 }}>{t('common:language')}</h2>
        <LanguageSwitcher />
      </section>
    </div>
  )
}

