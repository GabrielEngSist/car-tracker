import { useTranslation } from 'react-i18next'
import { DEFAULT_LANGUAGE, SUPPORTED_LANGUAGES, setLanguage, type SupportedLanguage } from '../i18n'

const FLAGS: Record<SupportedLanguage, string> = {
  'pt-BR': '🇧🇷',
  en: '🇺🇸',
  es: '🇪🇸',
  fr: '🇫🇷',
  it: '🇮🇹',
}

export function LanguageSwitcher() {
  const { t, i18n } = useTranslation('common')
  const current = (i18n.resolvedLanguage ?? i18n.language ?? DEFAULT_LANGUAGE) as SupportedLanguage

  return (
    <label style={{ display: 'inline-flex', alignItems: 'center', gap: 8, fontSize: 13, opacity: 0.9 }}>
      <span style={{ opacity: 0.85 }}>{t('language')}</span>
      <select
        value={SUPPORTED_LANGUAGES.includes(current) ? current : DEFAULT_LANGUAGE}
        onChange={(e) => void setLanguage(e.target.value as SupportedLanguage)}
        style={{
          padding: '6px 8px',
          borderRadius: 10,
          border: '1px solid rgba(255,255,255,0.16)',
          background: 'rgba(255,255,255,0.08)',
          color: 'inherit',
        }}
        aria-label={t('language')}
      >
        {SUPPORTED_LANGUAGES.map((lng) => (
          <option key={lng} value={lng}>
            {FLAGS[lng]} {t(`languages.${lng}`)}
          </option>
        ))}
      </select>
    </label>
  )
}

