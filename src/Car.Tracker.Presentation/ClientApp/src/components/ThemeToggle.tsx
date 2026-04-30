import { useTheme } from '../theme'
import { MaterialIcon } from './MaterialIcon'

export function ThemeToggle() {
  const { theme, toggle } = useTheme()

  return (
    <button
      type="button"
      onClick={toggle}
      aria-label="Toggle theme"
      title={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
      style={{
        padding: '6px 10px',
        borderRadius: 10,
        border: '1px solid var(--control-border)',
        background: 'var(--control-bg)',
        color: 'inherit',
        cursor: 'pointer',
        display: 'inline-flex',
        alignItems: 'center',
        gap: 8,
      }}
    >
      <MaterialIcon name={theme === 'dark' ? 'dark_mode' : 'light_mode'} size={20} />
      {theme === 'dark' ? 'Dark' : 'Light'}
    </button>
  )
}

