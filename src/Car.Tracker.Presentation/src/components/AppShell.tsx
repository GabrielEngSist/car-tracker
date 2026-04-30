import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { CarCreateModal } from './CarCreateModal'
import { MaterialIcon } from './MaterialIcon'
import { FuelingCreateModal } from './FuelingCreateModal'

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
  const [createFuelingOpen, setCreateFuelingOpen] = useState(false)

  function closeAdd() {
    setAddOpen(false)
  }

  return (
    <div className="appShell">
      <aside className="sidebar" aria-label="Primary">
        <div className="sidebarBrand">{t('common:appName')}</div>
        <nav className="sidebarNav">
          <NavItem to="/" label={t('common:nav.cars')} icon={<MaterialIcon name="directions_car" size={20} />} />
          <NavItem to="/fuelings" label={t('common:nav.fuelings')} icon={<MaterialIcon name="local_gas_station" size={20} />} />
          <NavItem to="/maintenance-services" label={t('common:nav.maintenance')} icon={<MaterialIcon name="build" size={20} />} />
          <NavItem to="/settings" label={t('common:nav.settings')} icon={<MaterialIcon name="settings" size={20} />} />
        </nav>
      </aside>

      <main className="shellMain">
        <Outlet />
      </main>

      {/* Mobile bottom nav */}
      <footer className="bottomNav" aria-label="Primary">
        <NavItem to="/" label={t('common:nav.cars')} icon={<MaterialIcon name="directions_car" size={22} />} />
        <NavItem to="/fuelings" label={t('common:nav.fuelings')} icon={<MaterialIcon name="local_gas_station" size={22} />} />
        <button
          type="button"
          className="bottomAdd"
          onClick={() => setAddOpen((o) => !o)}
          aria-label={t('common:actions.add')}
          title={t('common:actions.add')}
        >
          <MaterialIcon name="add" size={28} />
        </button>
        <NavItem to="/maintenance-services" label={t('common:nav.maintenance')} icon={<MaterialIcon name="build" size={22} />} />
        <NavItem to="/settings" label={t('common:nav.settings')} icon={<MaterialIcon name="settings" size={22} />} />
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
                {t('common:addMenu.addCar')}
              </span>
            </button>
            <button
              type="button"
              className="addMenuItem"
              role="menuitem"
              onClick={() => {
                closeAdd()
                setCreateFuelingOpen(true)
              }}
            >
              <span style={{ display: 'inline-flex', alignItems: 'center', gap: 10 }}>
                <MaterialIcon name="local_gas_station" size={22} />
                {t('common:addMenu.addFueling')}
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
                {t('common:addMenu.addExpense')}
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
                {t('common:addMenu.addMaintenance')}
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

      <FuelingCreateModal
        open={createFuelingOpen}
        onClose={() => setCreateFuelingOpen(false)}
        onCreated={() => {
          navigate('/fuelings')
        }}
      />
    </div>
  )
}

