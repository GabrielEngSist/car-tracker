import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { CarApi, type CarDto, type FuelingEntryDto } from '../api'
import { CostPerKmReportPanel } from '../components/CostPerKmReportPanel'
import { FuelingCreateModal } from '../components/FuelingCreateModal'
import { useTranslation } from 'react-i18next'

export function CarFuelingsPage() {
  useTranslation(['common'])
  const { carId } = useParams()
  const [car, setCar] = useState<CarDto | null>(null)
  const [items, setItems] = useState<FuelingEntryDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [createOpen, setCreateOpen] = useState(false)

  async function refresh(id: string) {
    setError(null)
    const [c, list] = await Promise.all([CarApi.getCar(id), CarApi.listFuelingsByCar(id)])
    setCar(c)
    setItems(list)
  }

  useEffect(() => {
    if (!carId) return
    refresh(carId).catch((e) => setError(e instanceof Error ? e.message : String(e)))
  }, [carId])

  if (!carId) return <p>Falta o id do veículo.</p>

  return (
    <div className="page">
      <header className="pageHeader">
        <div>
          <div style={{ opacity: 0.8, fontSize: 13 }}>
            <Link to={`/cars/${carId}`}>← Voltar ao veículo</Link>
          </div>
          <h1 style={{ margin: '8px 0 0' }}>
            Abastecimentos
            {car ? (
              <>
                {' '}
                · {car.name ? `${car.name} · ` : ''}
                {car.model}
              </>
            ) : null}
          </h1>
        </div>
        <div className="pageHeaderActions">
          <button type="button" onClick={() => setCreateOpen(true)}>
            Adicionar
          </button>
        </div>
      </header>

      {error ? <p style={{ color: 'var(--danger)' }}>{error}</p> : null}

      <CostPerKmReportPanel carId={carId} title="Consumo × manutenção (custo médio)" />

      <section className="card" style={{ marginTop: 14 }}>
        {items === null ? (
          <p style={{ margin: 0, opacity: 0.85 }}>Carregando…</p>
        ) : items.length === 0 ? (
          <p style={{ margin: 0, opacity: 0.85 }}>Nenhum abastecimento.</p>
        ) : (
          <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 10 }}>
            {items.map((f) => {
              const liters = Number(f.liters)
              const total = Number(f.totalPrice)
              const pricePerLiter = liters > 0 ? total / liters : null

              return (
                <li key={f.id} style={{ border: '1px solid var(--border)', borderRadius: 12, padding: 12, background: 'var(--surface)' }}>
                  <div style={{ fontWeight: 800 }}>
                    {f.performedAt} · {f.kmAtFueling.toLocaleString()} km · {f.fuelType}
                  </div>
                  <div style={{ opacity: 0.85, fontSize: 13, marginTop: 4 }}>
                    {liters.toLocaleString(undefined, { maximumFractionDigits: 2 })} L ·{' '}
                    {total.toLocaleString(undefined, { style: 'currency', currency: 'BRL' })}
                    {pricePerLiter != null ? ` · ${pricePerLiter.toLocaleString(undefined, { style: 'currency', currency: 'BRL' })}/L` : ''}
                    {f.stationName ? ` · ${f.stationName}` : ''}
                  </div>
                  {f.notes ? <div style={{ opacity: 0.75, marginTop: 6, fontSize: 13 }}>{f.notes}</div> : null}
                </li>
              )
            })}
          </ul>
        )}
      </section>

      <FuelingCreateModal open={createOpen} onClose={() => setCreateOpen(false)} prefilledCarId={carId} onCreated={() => refresh(carId)} />
    </div>
  )
}
