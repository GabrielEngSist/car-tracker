import { useEffect, useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { CarApi, type MaintenancePlanItemDto, type MaintenanceStatusDto } from '../api'
import { IconDelete, IconEdit, IconRow, IconToggleActive } from '../components/IconButtons'
import { PlanEditModal } from '../components/PlanEditModal'

export function MaintenancePage() {
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
    if (!window.confirm(`Delete plan “${p.title}”?`)) return
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
    <div style={{ maxWidth: 980, margin: '0 auto', padding: 24 }}>
      <header style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between' }}>
        <div>
          <div style={{ opacity: 0.8, fontSize: 13 }}>
            <Link to={`/cars/${carId}`}>← Car</Link>
          </div>
          <h1 style={{ margin: '8px 0 0' }}>Maintenance</h1>
          <p style={{ margin: '6px 0 0', opacity: 0.8 }}>“Whichever comes first” (km or time).</p>
        </div>
      </header>

      {error ? <p style={{ color: 'salmon', marginTop: 12 }}>{error}</p> : null}

      <section style={{ marginTop: 18, padding: 16, border: '1px solid rgba(255,255,255,0.12)', borderRadius: 12 }}>
        <h2 style={{ marginTop: 0 }}>Add maintenance plan item</h2>
        <form onSubmit={onCreate} style={{ display: 'grid', gridTemplateColumns: '1fr 180px 220px', gap: 12 }}>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Title *</div>
            <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Oil change" required />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Every (km)</div>
            <input
              type="number"
              min={1}
              value={dueKmInterval}
              onChange={(e) => setDueKmInterval(e.target.value === '' ? '' : Number(e.target.value))}
              placeholder="10000"
            />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Every (days)</div>
            <input
              type="number"
              min={1}
              value={dueTimeIntervalDays}
              onChange={(e) => setDueTimeIntervalDays(e.target.value === '' ? '' : Number(e.target.value))}
              placeholder="182 (≈ 6 months)"
            />
          </label>
          <div style={{ gridColumn: '1 / -1', display: 'flex', justifyContent: 'flex-end' }}>
            <button type="submit" disabled={!canCreate}>
              Add plan item
            </button>
          </div>
        </form>
      </section>

      <section style={{ marginTop: 18 }}>
        <h2 style={{ marginTop: 0 }}>Next due</h2>
        {status === null ? (
          <p>Loading…</p>
        ) : status.length === 0 ? (
          <p>No plan items yet.</p>
        ) : (
          <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 10 }}>
            {status.map((s) => (
              <li
                key={s.planItemId}
                style={{
                  border: '1px solid rgba(255,255,255,0.12)',
                  borderRadius: 12,
                  padding: 12,
                  background: s.overdue ? 'rgba(250, 128, 114, 0.12)' : undefined,
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, alignItems: 'flex-start' }}>
                  <div>
                    <div style={{ fontWeight: 800 }}>{s.title}</div>
                    <div style={{ opacity: 0.85, fontSize: 13, marginTop: 4 }}>
                      {s.lastPerformedAt ? `Last: ${s.lastPerformedAt}` : 'Last: —'}
                      {s.lastKmAtService != null ? ` · ${s.lastKmAtService.toLocaleString()} km` : ''}
                    </div>
                    <div style={{ opacity: 0.9, fontSize: 13, marginTop: 6 }}>
                      {s.nextDueDate ? `Next date: ${s.nextDueDate}` : 'Next date: —'}
                      {' · '}
                      {s.nextDueKm != null ? `Next km: ${s.nextDueKm.toLocaleString()}` : 'Next km: —'}
                    </div>
                  </div>
                  <div style={{ fontWeight: 800, flexShrink: 0 }}>{s.overdue ? 'OVERDUE' : 'OK'}</div>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section style={{ marginTop: 18 }}>
        <h2 style={{ marginTop: 0 }}>Plan items</h2>
        {plans === null ? (
          <p>Loading…</p>
        ) : plans.length === 0 ? (
          <p>No plan items yet.</p>
        ) : (
          <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 10 }}>
            {plans.map((p) => (
              <li key={p.id} style={{ border: '1px solid rgba(255,255,255,0.12)', borderRadius: 12, padding: 12 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, alignItems: 'flex-start' }}>
                  <div>
                    <div style={{ fontWeight: 700 }}>{p.title}</div>
                    <div style={{ opacity: 0.85, fontSize: 13 }}>
                      {p.dueKmInterval != null ? `Every ${p.dueKmInterval.toLocaleString()} km` : 'No km rule'}
                      {' · '}
                      {p.dueTimeIntervalDays != null ? `Every ${p.dueTimeIntervalDays} days` : 'No time rule'}
                    </div>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10, flexShrink: 0 }}>
                    <span style={{ opacity: 0.8, fontSize: 13 }}>{p.active ? 'Active' : 'Inactive'}</span>
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
