import React, { createContext, useContext, useEffect, useMemo, useState } from 'react'

type Theme = 'light' | 'dark'
type ThemePreference = Theme | 'system'

const STORAGE_KEY = 'car-tracker.theme'

function getSystemTheme(): Theme {
  if (typeof window === 'undefined') return 'dark'
  return window.matchMedia?.('(prefers-color-scheme: dark)')?.matches ? 'dark' : 'light'
}

function readStoredPreference(): ThemePreference {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw === 'light' || raw === 'dark' || raw === 'system' ? raw : 'system'
  } catch {
    return 'system'
  }
}

function writeStoredPreference(pref: ThemePreference) {
  try {
    localStorage.setItem(STORAGE_KEY, pref)
  } catch {
    // ignore
  }
}

function applyTheme(theme: Theme) {
  document.documentElement.dataset.theme = theme
  document.documentElement.style.colorScheme = theme
}

type ThemeContextValue = {
  theme: Theme
  preference: ThemePreference
  toggle: () => void
  setPreference: (pref: ThemePreference) => void
}

const ThemeContext = createContext<ThemeContextValue | null>(null)

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [preference, setPreferenceState] = useState<ThemePreference>(() => readStoredPreference())
  const [theme, setTheme] = useState<Theme>(() => (preference === 'system' ? getSystemTheme() : preference))

  useEffect(() => {
    const nextTheme = preference === 'system' ? getSystemTheme() : preference
    setTheme(nextTheme)
    applyTheme(nextTheme)
    writeStoredPreference(preference)
  }, [preference])

  useEffect(() => {
    if (preference !== 'system') return
    const mq = typeof window !== 'undefined' && window.matchMedia ? window.matchMedia('(prefers-color-scheme: dark)') : null
    if (!mq) return

    const onChange = () => {
      const next = getSystemTheme()
      setTheme(next)
      applyTheme(next)
    }

    mq.addEventListener?.('change', onChange)
    ;(mq as MediaQueryList & { addListener?: (cb: (e: MediaQueryListEvent) => void) => void }).addListener?.(onChange)

    return () => {
      mq.removeEventListener?.('change', onChange)
      ;(mq as MediaQueryList & { removeListener?: (cb: (e: MediaQueryListEvent) => void) => void }).removeListener?.(onChange)
    }
  }, [preference])

  const value = useMemo<ThemeContextValue>(
    () => ({
      theme,
      preference,
      toggle: () => setPreferenceState((p) => (p === 'system' ? (getSystemTheme() === 'dark' ? 'light' : 'dark') : p === 'dark' ? 'light' : 'dark')),
      setPreference: (pref) => setPreferenceState(pref),
    }),
    [preference, theme],
  )

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
}

export function useTheme() {
  const ctx = useContext(ThemeContext)
  if (!ctx) throw new Error('useTheme must be used within ThemeProvider')
  return ctx
}

