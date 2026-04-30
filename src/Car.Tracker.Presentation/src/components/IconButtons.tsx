import { Link } from 'react-router-dom'

const base: React.CSSProperties = {
  display: 'inline-flex',
  alignItems: 'center',
  justifyContent: 'center',
  width: 38,
  height: 38,
  padding: 0,
  borderRadius: 10,
  border: '1px solid var(--control-border)',
  background: 'var(--control-bg)',
  color: 'inherit',
  cursor: 'pointer',
  textDecoration: 'none',
  flexShrink: 0,
}

function Svg({ children }: { children: React.ReactNode }) {
  return (
    <svg width={18} height={18} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      {children}
    </svg>
  )
}

export function IconOpen({ to, label = 'Open' }: { to: string; label?: string }) {
  return (
    <Link to={to} style={base} title={label} aria-label={label}>
      <Svg>
        <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6" />
        <polyline points="15 3 21 3 21 9" />
        <line x1="10" y1="14" x2="21" y2="3" />
      </Svg>
    </Link>
  )
}

export function IconEdit({ onClick, label = 'Edit', disabled }: { onClick: () => void; label?: string; disabled?: boolean }) {
  return (
    <button type="button" style={base} title={label} aria-label={label} onClick={onClick} disabled={disabled}>
      <Svg>
        <path d="M12 20h9" />
        <path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z" />
      </Svg>
    </button>
  )
}

export function IconDelete({ onClick, label = 'Delete', disabled }: { onClick: () => void; label?: string; disabled?: boolean }) {
  return (
    <button type="button" style={{ ...base, color: 'var(--danger)' }} title={label} aria-label={label} onClick={onClick} disabled={disabled}>
      <Svg>
        <polyline points="3 6 5 6 21 6" />
        <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
        <line x1="10" y1="11" x2="10" y2="17" />
        <line x1="14" y1="11" x2="14" y2="17" />
      </Svg>
    </button>
  )
}

export function IconToggleActive({
  active,
  onClick,
  label,
  disabled,
}: {
  active: boolean
  onClick: () => void
  label?: string
  disabled?: boolean
}) {
  const text = label ?? (active ? 'Deactivate plan' : 'Activate plan')
  return (
    <button type="button" style={base} title={text} aria-label={text} aria-pressed={active} onClick={onClick} disabled={disabled}>
      <Svg>
        {active ? (
          <>
            <line x1="10" y1="5" x2="10" y2="19" />
            <line x1="14" y1="5" x2="14" y2="19" />
          </>
        ) : (
          <polygon points="8 5 8 19 19 12 8 5" />
        )}
      </Svg>
    </button>
  )
}

export function IconRow({ children }: { children: React.ReactNode }) {
  return <div style={{ display: 'inline-flex', alignItems: 'center', gap: 8 }}>{children}</div>
}
