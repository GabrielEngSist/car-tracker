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

  const groupedFuelings = useMemo(() => {
    const rows = items ?? []
    const byCar = new Map<string, FuelingEntryDto[]>()
    for (const f of rows) {
      const list = byCar.get(f.carId) ?? []
      list.push(f)
      byCar.set(f.carId, list)
    }
    for (const [, list] of byCar) {
      list.sort((a, b) => {
        const d = String(b.performedAt).localeCompare(String(a.performedAt))
        return d !== 0 ? d : b.kmAtFueling - a.kmAtFueling
      })
    }
    const carIds = Array.from(byCar.keys()).sort((a, b) => {
      const la = carById.get(a)
      const lb = carById.get(b)
      return (la ? carLabel(la) : a).localeCompare(lb ? carLabel(lb) : b)
    })
    return { byCar, carIds }
  }, [items, carById])

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
          <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 18 }}>
            {groupedFuelings.carIds.map((cid) => {
              const group = groupedFuelings.byCar.get(cid) ?? []
              const c = carById.get(cid)
              return (
                <li key={cid} style={{ border: '1px solid var(--border)', borderRadius: 12, padding: 12, background: 'var(--surface)' }}>
                  <div style={{ fontWeight: 800, marginBottom: 10 }}>
                    {c ? carLabel(c) : cid}
                  </div>
                  <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 10 }}>
                    {group.map((f) => {
                      const liters = Number(f.liters)
                      const total = Number(f.totalPrice)
                      const pricePerLiter = liters > 0 ? total / liters : null
                      return (
                        <li
                          key={f.id}
                          style={{
                            border: '1px solid color-mix(in srgb, var(--border) 70%, transparent)',
                            borderRadius: 10,
                            padding: 10,
                          }}
                        >
                          <div style={{ fontWeight: 700, fontSize: 13 }}>
                            {f.performedAt} · {f.kmAtFueling.toLocaleString()} km · {f.fuelType}
                            {f.stationName ? ` · ${f.stationName}` : ''}
                          </div>
                          <div style={{ opacity: 0.9, fontSize: 13, marginTop: 4 }}>
                            {liters.toLocaleString(undefined, { maximumFractionDigits: 2 })} L ·{' '}
                            {total.toLocaleString(undefined, { style: 'currency', currency: 'BRL' })}
                            {pricePerLiter != null ? ` · ${pricePerLiter.toLocaleString(undefined, { style: 'currency', currency: 'BRL' })}/L` : ''}
                          </div>
                          {f.notes ? <div style={{ opacity: 0.8, fontSize: 13, marginTop: 6 }}>{f.notes}</div> : null}
                        </li>
                      )
                    })}
                  </ul>
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

