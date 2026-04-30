import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { CarCreateModal } from './CarCreateModal'
import { MaterialIcon } from './MaterialIcon'

function NavItem({ to, label, icon }: { to: string; label: string; icon: React.ReactNode }) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) => `navItem ${isActive ? 'navItem--active' : ''}`}
      aria-label={label}
      title={label}
    >
      <span className="navIcon" aria-hidden>
        {icon}
      </span>
      <span className="navLabel">{label}</span>
    </NavLink>
  )
}

export function AppShell() {
  const { t } = useTranslation(['common'])
  const navigate = useNavigate()
  const [addOpen, setAddOpen] = useState(false)
  const [createCarOpen, setCreateCarOpen] = useState(false)

  function closeAdd() {
    setAddOpen(false)
  }

  return (
    <div className="appShell">
      <aside className="sidebar" aria-label="Primary">
        <div className="sidebarBrand">{t('common:appName')}</div>
        <nav className="sidebarNav">
          <NavItem to="/" label="Cars" icon={<MaterialIcon name="directions_car" size={20} />} />
          <NavItem to="/settings" label="Settings" icon={<MaterialIcon name="settings" size={20} />} />
        </nav>
      </aside>

      <main className="shellMain">
        <Outlet />
      </main>

      {/* Mobile bottom nav */}
      <footer className="bottomNav" aria-label="Primary">
        <NavItem to="/" label="Cars" icon={<MaterialIcon name="directions_car" size={22} />} />
        <button
          type="button"
          className="bottomAdd"
          onClick={() => setAddOpen((o) => !o)}
          aria-label={t('common:actions.add')}
          title={t('common:actions.add')}
        >
          <MaterialIcon name="add" size={28} />
        </button>
        <NavItem to="/settings" label="Settings" icon={<MaterialIcon name="settings" size={22} />} />
      </footer>

      {/* Desktop floating action button */}
      <button
        type="button"
        className="fab"
        onClick={() => setAddOpen(true)}
        aria-label={t('common:actions.add')}
        title={t('common:actions.add')}
      >
        <MaterialIcon name="add" size={30} />
      </button>

      {addOpen ? (
        <div className="addOverlay" role="presentation" onMouseDown={closeAdd}>
          <div className="addMenu" role="menu" onMouseDown={(e) => e.stopPropagation()}>
            <button
              type="button"
              className="addMenuItem"
              role="menuitem"
              onClick={() => {
                closeAdd()
                setCreateCarOpen(true)
              }}
            >
              <span style={{ display: 'inline-flex', alignItems: 'center', gap: 10 }}>
                <MaterialIcon name="directions_car" size={22} />
                Add car
              </span>
            </button>
            <button
              type="button"
              className="addMenuItem"
              role="menuitem"
              onClick={() => {
                closeAdd()
                window.alert('To add an expense, open a car first.')
                navigate('/')
              }}
            >
              <span style={{ display: 'inline-flex', alignItems: 'center', gap: 10 }}>
                <MaterialIcon name="payments" size={22} />
                Add expense
              </span>
            </button>
            <button
              type="button"
              className="addMenuItem"
              role="menuitem"
              onClick={() => {
                closeAdd()
                window.alert('To manage maintenance, open a car first.')
                navigate('/')
              }}
            >
              <span style={{ display: 'inline-flex', alignItems: 'center', gap: 10 }}>
                <MaterialIcon name="build" size={22} />
                Add maintenance
              </span>
            </button>
          </div>
        </div>
      ) : null}

      <CarCreateModal
        open={createCarOpen}
        onClose={() => setCreateCarOpen(false)}
        onCreated={() => {
          // CarsPage refreshes on mount; navigate back to list.
          navigate('/')
        }}
      />
    </div>
  )
}

