import { useEffect, useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { CarApi, type MaintenancePlanItemDto, type MaintenanceStatusDto } from '../api'
import { CostPerKmReportPanel } from '../components/CostPerKmReportPanel'
import { IconDelete, IconEdit, IconRow, IconToggleActive } from '../components/IconButtons'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { ThemeToggle } from '../components/ThemeToggle'
import { PlanEditModal } from '../components/PlanEditModal'
import { useTranslation } from 'react-i18next'

export function MaintenancePage() {
  const { t } = useTranslation(['common', 'maintenance'])
  const { carId } = useParams()
  const [plans, setPlans] = useState<MaintenancePlanItemDto[] | null>(null)
  const [status, setStatus] = useState<MaintenanceStatusDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)

  const [title, setTitle] = useState('')
  const [dueKmInterval, setDueKmInterval] = useState<number | ''>('')
  const [dueTimeIntervalDays, setDueTimeIntervalDays] = useState<number | ''>('')

  const [editPlan, setEditPlan] = useState<MaintenancePlanItemDto | null>(null)

  const canCreate = useMemo(() => title.trim().length > 0 && (dueKmInterval !== '' || dueTimeIntervalDays !== ''), [title, dueKmInterval, dueTimeIntervalDays])

  async function refresh(id: string) {
    setError(null)
    const [p, s] = await Promise.all([CarApi.listPlans(id), CarApi.getStatus(id)])
    setPlans(p)
    setStatus(s)
  }

  useEffect(() => {
    if (!carId) return
    refresh(carId).catch((e) => setError(e instanceof Error ? e.message : String(e)))
  }, [carId])

  async function onCreate(e: React.FormEvent) {
    e.preventDefault()
    if (!carId || !canCreate) return
    setError(null)
    try {
      await CarApi.createPlan(carId, {
        title: title.trim(),
        dueKmInterval: dueKmInterval === '' ? null : dueKmInterval,
        dueTimeIntervalDays: dueTimeIntervalDays === '' ? null : dueTimeIntervalDays,
        active: true,
      })
      setTitle('')
      setDueKmInterval('')
      setDueTimeIntervalDays('')
      await refresh(carId)
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    }
  }

  async function onToggleActive(p: MaintenancePlanItemDto) {
    if (!carId) return
    setError(null)
    try {
      await CarApi.patchPlan(carId, p.id, { active: !p.active })
      await refresh(carId)
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    }
  }

  async function onDeletePlan(p: MaintenancePlanItemDto) {
    if (!carId) return
    if (!window.confirm(t('maintenance:confirmDeletePlan', { title: p.title }))) return
    setError(null)
    try {
      await CarApi.deletePlan(carId, p.id)
      if (editPlan?.id === p.id) setEditPlan(null)
      await refresh(carId)
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    }
  }

  if (!carId) return <p>Missing car id.</p>

  return (
    <div className="page">
      <header className="pageHeader">
        <div>
          <div style={{ opacity: 0.8, fontSize: 13 }}>
            <Link to={`/cars/${carId}`}>{t('maintenance:backToCar')}</Link>
          </div>
          <h1 style={{ margin: '8px 0 0' }}>{t('maintenance:title')}</h1>
          <p style={{ margin: '6px 0 0', opacity: 0.8 }}>{t('maintenance:subtitle')}</p>
        </div>
        <div className="pageHeaderActions">
          <ThemeToggle />
          <LanguageSwitcher />
        </div>
      </header>

      {error ? <p style={{ color: 'var(--danger)', marginTop: 12 }}>{error}</p> : null}

      <CostPerKmReportPanel carId={carId} title={t('maintenance:costReport.title')} />

      <section className="card">
        <h2 style={{ marginTop: 0 }}>{t('maintenance:addItem.title')}</h2>
        <form onSubmit={onCreate} className="gridForm maintenanceCreate">
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('maintenance:addItem.titleLabel')}</div>
            <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Oil change" required />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('maintenance:addItem.everyKmLabel')}</div>
            <input
              type="number"
              min={1}
              value={dueKmInterval}
              onChange={(e) => setDueKmInterval(e.target.value === '' ? '' : Number(e.target.value))}
              placeholder="10000"
            />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('maintenance:addItem.everyDaysLabel')}</div>
            <input
              type="number"
              min={1}
              value={dueTimeIntervalDays}
              onChange={(e) => setDueTimeIntervalDays(e.target.value === '' ? '' : Number(e.target.value))}
              placeholder={t('maintenance:addItem.everyDaysPlaceholder')}
            />
          </label>
          <div style={{ gridColumn: '1 / -1', display: 'flex', justifyContent: 'flex-end' }}>
            <button type="submit" disabled={!canCreate}>
              {t('maintenance:addItem.submit')}
            </button>
          </div>
        </form>
      </section>

      <section style={{ marginTop: 18 }}>
        <h2 style={{ marginTop: 0 }}>{t('maintenance:nextDue.title')}</h2>
        {status === null ? (
          <p>{t('common:status.loading')}</p>
        ) : status.length === 0 ? (
          <p>{t('maintenance:nextDue.empty')}</p>
        ) : (
          <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 10 }}>
            {status.map((s) => (
              <li
                key={s.planItemId}
                style={{
                  border: '1px solid var(--border)',
                  borderRadius: 12,
                  padding: 12,
                  background: s.overdue ? 'color-mix(in srgb, var(--danger) 16%, transparent)' : 'var(--surface)',
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, alignItems: 'flex-start' }}>
                  <div>
                    <div style={{ fontWeight: 800 }}>{s.title}</div>
                    <div style={{ opacity: 0.85, fontSize: 13, marginTop: 4 }}>
                      {s.lastPerformedAt
                        ? t('maintenance:status.last', { value: s.lastPerformedAt })
                        : t('maintenance:status.last', { value: t('common:placeholders.dash') })}
                      {s.lastKmAtService != null ? ` · ${s.lastKmAtService.toLocaleString()} km` : ''}
                    </div>
                    <div style={{ opacity: 0.9, fontSize: 13, marginTop: 6 }}>
                      {s.nextDueDate
                        ? t('maintenance:status.nextDate', { value: s.nextDueDate })
                        : t('maintenance:status.nextDate', { value: t('common:placeholders.dash') })}
                      {' · '}
                      {s.nextDueKm != null
                        ? t('maintenance:status.nextKm', { value: s.nextDueKm.toLocaleString() })
                        : t('maintenance:status.nextKm', { value: t('common:placeholders.dash') })}
                    </div>
                  </div>
                  <div style={{ fontWeight: 800, flexShrink: 0 }}>
                    {s.overdue ? t('maintenance:status.overdue') : t('maintenance:status.ok')}
                  </div>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section style={{ marginTop: 18 }}>
        <h2 style={{ marginTop: 0 }}>{t('maintenance:planItems.title')}</h2>
        {plans === null ? (
          <p>{t('common:status.loading')}</p>
        ) : plans.length === 0 ? (
          <p>{t('maintenance:nextDue.empty')}</p>
        ) : (
          <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 10 }}>
            {plans.map((p) => (
              <li key={p.id} style={{ border: '1px solid var(--border)', borderRadius: 12, padding: 12, background: 'var(--surface)' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, alignItems: 'flex-start' }}>
                  <div>
                    <div style={{ fontWeight: 700 }}>{p.title}</div>
                    <div style={{ opacity: 0.85, fontSize: 13 }}>
                      {p.dueKmInterval != null
                        ? t('maintenance:planItems.everyKm', { km: p.dueKmInterval.toLocaleString() })
                        : t('maintenance:planItems.noKmRule')}
                      {' · '}
                      {p.dueTimeIntervalDays != null
                        ? t('maintenance:planItems.everyDays', { days: p.dueTimeIntervalDays })
                        : t('maintenance:planItems.noTimeRule')}
                    </div>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10, flexShrink: 0 }}>
                    <span style={{ opacity: 0.8, fontSize: 13 }}>
                      {p.active ? t('maintenance:planItems.active') : t('maintenance:planItems.inactive')}
                    </span>
                    <IconRow>
                      <IconToggleActive active={p.active} onClick={() => onToggleActive(p)} />
                      <IconEdit label={`Edit ${p.title}`} onClick={() => setEditPlan(p)} />
                      <IconDelete label={`Delete ${p.title}`} onClick={() => onDeletePlan(p)} />
                    </IconRow>
                  </div>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>

      <PlanEditModal
        open={editPlan !== null}
        plan={editPlan}
        onClose={() => setEditPlan(null)}
        onSave={async (body) => {
          if (!carId || !editPlan) return
          await CarApi.patchPlan(carId, editPlan.id, body)
          await refresh(carId)
        }}
      />
    </div>
  )
}
