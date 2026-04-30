import { useEffect, useMemo, useState } from 'react'
import { CarApi, type CarDto, type FuelingEntryDto } from '../api'
import { FuelingCreateModal } from '../components/FuelingCreateModal'
import { useTranslation } from 'react-i18next'

function carLabel(c: CarDto) {
  return `${c.name ? `${c.name} · ` : ''}${c.model} (${c.year})`
}

export function FuelingsPage() {
  useTranslation(['common'])
  const [cars, setCars] = useState<CarDto[] | null>(null)
  const [items, setItems] = useState<FuelingEntryDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [createOpen, setCreateOpen] = useState(false)

  async function refresh() {
    setError(null)
    const [c, f] = await Promise.all([CarApi.listCars(), CarApi.listFuelings()])
    setCars(c)
    setItems(f)
  }

  useEffect(() => {
    refresh().catch((e) => setError(e instanceof Error ? e.message : String(e)))
  }, [])

  const carById = useMemo(() => new Map((cars ?? []).map((c) => [c.id, c])), [cars])

  return (
    <div className="page">
      <header className="pageHeader">
        <div>
          <h1 style={{ margin: 0 }}>Abastecimentos</h1>
          <p style={{ margin: '6px 0 0', opacity: 0.75 }}>Lista global de abastecimentos.</p>
        </div>
        <div className="pageHeaderActions">
          <button type="button" onClick={() => setCreateOpen(true)}>
            Adicionar
          </button>
        </div>
      </header>

      {error ? <p style={{ color: 'var(--danger)' }}>{error}</p> : null}

      <section className="card">
        {items === null || cars === null ? (
          <p style={{ margin: 0, opacity: 0.85 }}>Carregando…</p>
        ) : items.length === 0 ? (
          <p style={{ margin: 0, opacity: 0.85 }}>Nenhum abastecimento ainda.</p>
        ) : (
          <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 10 }}>
            {items.map((f) => {
              const c = carById.get(f.carId)
              const liters = Number(f.liters)
              const total = Number(f.totalPrice)
              const pricePerLiter = liters > 0 ? total / liters : null

              return (
                <li key={f.id} style={{ border: '1px solid var(--border)', borderRadius: 12, padding: 12, background: 'var(--surface)' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, alignItems: 'flex-start' }}>
                    <div style={{ minWidth: 0 }}>
                      <div style={{ fontWeight: 800 }}>
                        {c ? carLabel(c) : f.carId}
                      </div>
                      <div style={{ opacity: 0.85, fontSize: 13, marginTop: 4 }}>
                        {f.performedAt} · {f.kmAtFueling.toLocaleString()} km
                        {f.fuelType ? ` · ${f.fuelType}` : ''}
                        {f.stationName ? ` · ${f.stationName}` : ''}
                      </div>
                      <div style={{ opacity: 0.9, fontSize: 13, marginTop: 6 }}>
                        {liters.toLocaleString(undefined, { maximumFractionDigits: 2 })} L ·{' '}
                        {total.toLocaleString(undefined, { style: 'currency', currency: 'BRL' })}
                        {pricePerLiter != null ? ` · ${pricePerLiter.toLocaleString(undefined, { style: 'currency', currency: 'BRL' })}/L` : ''}
                      </div>
                      {f.notes ? <div style={{ opacity: 0.8, fontSize: 13, marginTop: 6 }}>{f.notes}</div> : null}
                    </div>
                  </div>
                </li>
              )
            })}
          </ul>
        )}
      </section>

      <FuelingCreateModal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onCreated={refresh}
      />
    </div>
  )
}

