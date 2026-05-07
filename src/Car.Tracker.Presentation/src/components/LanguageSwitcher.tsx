import { useTranslation } from 'react-i18next'
import { DEFAULT_LANGUAGE, SUPPORTED_LANGUAGES, setLanguage, type SupportedLanguage } from '../i18n'
import { MaterialIcon } from './MaterialIcon'

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
    <div className="languageSwitcher" title={t('language')}>
      <MaterialIcon name="language" size={24} />
      <select
        value={SUPPORTED_LANGUAGES.includes(current) ? current : DEFAULT_LANGUAGE}
        onChange={(e) => void setLanguage(e.target.value as SupportedLanguage)}
        className="languageSelect"
        aria-label={t('language')}
      >
        {SUPPORTED_LANGUAGES.map((lng) => (
          <option key={lng} value={lng}>
            {FLAGS[lng]} {t(`languages.${lng}`)}
          </option>
        ))}
      </select>
    </div>
  )
}
