import { useEffect, useState } from 'react'
import type { MaintenancePlanItemDto } from '../api'
import { useTranslation } from 'react-i18next'

type Props = {
  open: boolean
  plan: MaintenancePlanItemDto | null
  onClose: () => void
  onSave: (body: {
    title: string
    dueKmInterval: number | null
    dueTimeIntervalDays: number | null
    active: boolean
  }) => Promise<void>
}

export function PlanEditModal({ open, plan, onClose, onSave }: Props) {
  const { t } = useTranslation(['common', 'modals'])
  const [title, setTitle] = useState('')
  const [dueKm, setDueKm] = useState<number | ''>('')
  const [dueDays, setDueDays] = useState<number | ''>('')
  const [active, setActive] = useState(true)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (!open || !plan) return
    setTitle(plan.title)
    setDueKm(plan.dueKmInterval ?? '')
    setDueDays(plan.dueTimeIntervalDays ?? '')
    setActive(plan.active)
  }, [open, plan])

  if (!open || !plan) return null

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!title.trim()) return
    if (dueKm === '' && dueDays === '') return
    setSaving(true)
    try {
      await onSave({
        title: title.trim(),
        dueKmInterval: dueKm === '' ? null : dueKm,
        dueTimeIntervalDays: dueDays === '' ? null : dueDays,
        active,
      })
      onClose()
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <div className="modal-panel" role="dialog" aria-modal="true" aria-labelledby="plan-edit-title" onMouseDown={(e) => e.stopPropagation()}>
        <h3 id="plan-edit-title">{t('modals:planEdit.title')}</h3>
        <form onSubmit={onSubmit} style={{ display: 'grid', gap: 12 }}>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('modals:planEdit.titleLabel')}</div>
            <input value={title} onChange={(e) => setTitle(e.target.value)} required />
          </label>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <label>
              <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('modals:planEdit.everyKmLabel')}</div>
              <input type="number" min={1} value={dueKm} onChange={(e) => setDueKm(e.target.value === '' ? '' : Number(e.target.value))} />
            </label>
            <label>
              <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('modals:planEdit.everyDaysLabel')}</div>
              <input type="number" min={1} value={dueDays} onChange={(e) => setDueDays(e.target.value === '' ? '' : Number(e.target.value))} />
            </label>
          </div>
          <label style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <input type="checkbox" checked={active} onChange={(e) => setActive(e.target.checked)} />
            <span>{t('modals:planEdit.active')}</span>
          </label>
          <div className="modal-actions">
            <button type="button" onClick={onClose}>
              {t('common:actions.cancel')}
            </button>
            <button type="submit" disabled={saving}>
              {t('common:actions.save')}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
