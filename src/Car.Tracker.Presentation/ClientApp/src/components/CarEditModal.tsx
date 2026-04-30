import { useEffect, useMemo, useState } from 'react'
import type { CarDto } from '../api'
import { ehPlacaValida, normalizarPlaca } from '../placaBrasil'

type Props = {
  open: boolean
  car: CarDto | null
  onClose: () => void
  onSave: (body: {
    model: string
    year: number
    currentKm: number
    name: string | null
    placa: string | null
  }) => Promise<void>
}

export function CarEditModal({ open, car, onClose, onSave }: Props) {
  const [name, setName] = useState('')
  const [model, setModel] = useState('')
  const [year, setYear] = useState(2000)
  const [currentKm, setCurrentKm] = useState(0)
  const [placa, setPlaca] = useState('')
  const [saving, setSaving] = useState(false)

  const placaNorm = useMemo(() => normalizarPlaca(placa), [placa])
  const placaErro = useMemo(() => {
    if (!placaNorm) return null
    return ehPlacaValida(placaNorm) ? null : 'Placa inválida.'
  }, [placaNorm])

  useEffect(() => {
    if (!open || !car) return
    setName(car.name ?? '')
    setModel(car.model)
    setYear(car.year)
    setCurrentKm(car.currentKm)
    setPlaca(car.placa ?? '')
  }, [open, car])

  if (!open || !car) return null

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!model.trim() || placaErro) return
    setSaving(true)
    try {
      await onSave({
        model: model.trim(),
        year,
        currentKm,
        name: name.trim() ? name.trim() : null,
        placa: placaNorm ? placaNorm : '',
      })
      onClose()
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <div className="modal-panel" role="dialog" aria-modal="true" aria-labelledby="car-edit-title" onMouseDown={(e) => e.stopPropagation()}>
        <h3 id="car-edit-title">Edit car</h3>
        <form onSubmit={onSubmit} style={{ display: 'grid', gap: 12 }}>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Nickname (optional)</div>
            <input value={name} onChange={(e) => setName(e.target.value)} />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Model *</div>
            <input value={model} onChange={(e) => setModel(e.target.value)} required />
          </label>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <label>
              <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Year</div>
              <input type="number" min={1900} max={3000} value={year} onChange={(e) => setYear(Number(e.target.value))} />
            </label>
            <label>
              <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Current km</div>
              <input type="number" min={0} value={currentKm} onChange={(e) => setCurrentKm(Number(e.target.value))} />
            </label>
          </div>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Placa (opcional)</div>
            <input value={placa} onChange={(e) => setPlaca(e.target.value)} placeholder="ABC1D23" spellCheck={false} />
            {placaErro ? <div style={{ color: 'salmon', fontSize: 12, marginTop: 6 }}>{placaErro}</div> : null}
          </label>
          <div className="modal-actions">
            <button type="button" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" disabled={saving || !!placaErro}>
              Save
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
