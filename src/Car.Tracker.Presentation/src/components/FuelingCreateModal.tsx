import { useEffect, useMemo, useState } from 'react'
import { CarApi, type CarDto, type FuelTypeDto } from '../api'
import { useTranslation } from 'react-i18next'

function todayIsoDate(): string {
  const d = new Date()
  const yyyy = d.getFullYear()
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd}`
}

type Props = {
  open: boolean
  onClose: () => void
  onCreated: () => Promise<void> | void
  /** Quando informado, o abastecimento é sempre deste carro (ex.: página do veículo). */
  prefilledCarId?: string
}

const FUEL_OPTIONS: FuelTypeDto[] = ['Gasolina', 'Alcool', 'Diesel', 'KV']

export function FuelingCreateModal({ open, onClose, onCreated, prefilledCarId }: Props) {
  const { t } = useTranslation(['common', 'modals'])
  const [cars, setCars] = useState<CarDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  const [carId, setCarId] = useState('')
  const [performedAt, setPerformedAt] = useState(todayIsoDate())
  const [kmAtFueling, setKmAtFueling] = useState<number>(0)
  const [liters, setLiters] = useState<number>(0)
  const [totalPrice, setTotalPrice] = useState<number>(0)
  const [fuelType, setFuelType] = useState<FuelTypeDto>('Gasolina')
  const [isFullTank, setIsFullTank] = useState(false)
  const [stationName, setStationName] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (!open) return
    setError(null)
    setCars(null)
    setIsFullTank(false)

    if (prefilledCarId) {
      setCarId(prefilledCarId)
    } else {
      setCarId('')
    }

    void CarApi.listCars()
      .then((list) => {
        setCars(list)
        if (prefilledCarId) return
        if (list.length > 0) setCarId((current) => (current ? current : list[0].id))
      })
      .catch((e) => setError(e instanceof Error ? e.message : String(e)))
  }, [open, prefilledCarId])

  const canSave = useMemo(() => carId && kmAtFueling >= 0 && liters > 0 && totalPrice >= 0 && !!performedAt, [carId, kmAtFueling, liters, totalPrice, performedAt])

  if (!open) return null

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!canSave) return
    setSaving(true)
    setError(null)
    try {
      await CarApi.createFueling(carId, {
        performedAt,
        kmAtFueling,
        liters,
        totalPrice,
        fuelType,
        isFullTank,
        stationName: stationName.trim() ? stationName.trim() : null,
        notes: notes.trim() ? notes.trim() : null,
      })
      await onCreated()
      onClose()
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err))
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <div className="modal-panel modal-panel--fullscreen" role="dialog" aria-modal="true" aria-labelledby="fueling-create-title" onMouseDown={(e) => e.stopPropagation()}>
        <button type="button" className="modal-close" onClick={onClose} aria-label={t('common:actions.close')} title={t('common:actions.close')}>
          ✕
        </button>

        <h3 id="fueling-create-title">{t('modals:fuelingCreate.title')}</h3>

        <form onSubmit={onSubmit} className="gridForm" style={{ marginTop: 12 }}>
          {!prefilledCarId ? (
            <label>
              <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Carro</div>
              <select value={carId} onChange={(e) => setCarId(e.target.value)} disabled={cars === null || cars.length === 0}>
                {cars?.map((c) => (
                  <option key={c.id} value={c.id}>
                    {(c.name ? `${c.name} · ` : '') + c.model}
                  </option>
                ))}
              </select>
            </label>
          ) : null}

          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Data</div>
            <input type="date" value={performedAt} onChange={(e) => setPerformedAt(e.target.value)} />
          </label>

          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Km</div>
            <input type="number" min={0} value={kmAtFueling} onChange={(e) => setKmAtFueling(Number(e.target.value))} />
          </label>

          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Litros</div>
            <input type="number" min={0.01} step="0.01" value={liters} onChange={(e) => setLiters(Number(e.target.value))} />
          </label>

          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Total</div>
            <input type="number" min={0} step="0.01" value={totalPrice} onChange={(e) => setTotalPrice(Number(e.target.value))} />
          </label>

          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Combustível</div>
            <select value={fuelType} onChange={(e) => setFuelType(e.target.value as FuelTypeDto)}>
              {FUEL_OPTIONS.map((f) => (
                <option key={f} value={f}>
                  {f}
                </option>
              ))}
            </select>
          </label>

          <label style={{ display: 'flex', flexDirection: 'row', alignItems: 'flex-start', gap: 10, gridColumn: '1 / -1' }}>
            <input type="checkbox" checked={isFullTank} onChange={(e) => setIsFullTank(e.target.checked)} style={{ marginTop: 3 }} />
            <span style={{ fontSize: 13, lineHeight: 1.35 }}>{t('modals:fuelingCreate.fullTank')}</span>
          </label>

          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Posto (opcional)</div>
            <input value={stationName} onChange={(e) => setStationName(e.target.value)} placeholder="Shell" />
          </label>

          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Observações (opcional)</div>
            <input value={notes} onChange={(e) => setNotes(e.target.value)} />
          </label>

          {error ? <p style={{ color: 'var(--danger)', margin: 0 }}>{error}</p> : null}

          <div className="modal-actions" style={{ gridColumn: '1 / -1' }}>
            <button type="button" onClick={onClose}>
              {t('common:actions.cancel')}
            </button>
            <button type="submit" disabled={!canSave || saving}>
              {saving ? t('modals:fuelingCreate.saving') : t('common:actions.save')}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

