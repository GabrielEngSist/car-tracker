import i18n from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import { initReactI18next } from 'react-i18next'

import commonEn from './locales/en/common.json'
import commonPtBr from './locales/pt-BR/common.json'
import commonEs from './locales/es/common.json'
import commonFr from './locales/fr/common.json'
import commonIt from './locales/it/common.json'
import carsEn from './locales/en/cars.json'
import carsPtBr from './locales/pt-BR/cars.json'
import carsEs from './locales/es/cars.json'
import carsFr from './locales/fr/cars.json'
import carsIt from './locales/it/cars.json'
import carDetailsEn from './locales/en/carDetails.json'
import carDetailsPtBr from './locales/pt-BR/carDetails.json'
import carDetailsEs from './locales/es/carDetails.json'
import carDetailsFr from './locales/fr/carDetails.json'
import carDetailsIt from './locales/it/carDetails.json'
import maintenanceEn from './locales/en/maintenance.json'
import maintenancePtBr from './locales/pt-BR/maintenance.json'
import maintenanceEs from './locales/es/maintenance.json'
import maintenanceFr from './locales/fr/maintenance.json'
import maintenanceIt from './locales/it/maintenance.json'
import modalsEn from './locales/en/modals.json'
import modalsPtBr from './locales/pt-BR/modals.json'
import modalsEs from './locales/es/modals.json'
import modalsFr from './locales/fr/modals.json'
import modalsIt from './locales/it/modals.json'

export const DEFAULT_LANGUAGE = 'pt-BR'
export const SUPPORTED_LANGUAGES = ['pt-BR', 'en', 'es', 'fr', 'it'] as const
export type SupportedLanguage = (typeof SUPPORTED_LANGUAGES)[number]

const languageStorageKey = 'car-tracker.language'

void i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    fallbackLng: DEFAULT_LANGUAGE,
    supportedLngs: [...SUPPORTED_LANGUAGES],
    ns: ['common', 'cars', 'carDetails', 'maintenance', 'modals'],
    defaultNS: 'common',
    resources: {
      en: {
        common: commonEn,
        cars: carsEn,
        carDetails: carDetailsEn,
        maintenance: maintenanceEn,
        modals: modalsEn,
      },
      es: {
        common: commonEs,
        cars: carsEs,
        carDetails: carDetailsEs,
        maintenance: maintenanceEs,
        modals: modalsEs,
      },
      fr: {
        common: commonFr,
        cars: carsFr,
        carDetails: carDetailsFr,
        maintenance: maintenanceFr,
        modals: modalsFr,
      },
      it: {
        common: commonIt,
        cars: carsIt,
        carDetails: carDetailsIt,
        maintenance: maintenanceIt,
        modals: modalsIt,
      },
      'pt-BR': {
        common: commonPtBr,
        cars: carsPtBr,
        carDetails: carDetailsPtBr,
        maintenance: maintenancePtBr,
        modals: modalsPtBr,
      },
    },
    detection: {
      order: ['localStorage', 'navigator'],
      lookupLocalStorage: languageStorageKey,
      caches: ['localStorage'],
    },
    interpolation: {
      escapeValue: false,
    },
  })

export function setLanguage(lang: SupportedLanguage) {
  return i18n.changeLanguage(lang)
}

export function getLanguageStorageKey() {
  return languageStorageKey
}

export default i18n
