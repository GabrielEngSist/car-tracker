import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { CarApi, type CarDto } from '../api'

function carLabel(c: CarDto) {
  return `${c.name ? `${c.name} · ` : ''}${c.model} (${c.year})`
}

export function MaintenanceServicesPage() {
  const [cars, setCars] = useState<CarDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    setError(null)
    void CarApi.listCars()
      .then(setCars)
      .catch((e) => setError(e instanceof Error ? e.message : String(e)))
  }, [])

  return (
    <div className="page">
      <header className="pageHeader">
        <div>
          <h1 style={{ margin: 0 }}>Manutenções e serviços</h1>
          <p style={{ margin: '6px 0 0', opacity: 0.75 }}>Atalhos para manutenção e lançamentos por carro.</p>
        </div>
      </header>

      {error ? <p style={{ color: 'var(--danger)' }}>{error}</p> : null}

      <section className="card">
        {cars === null ? (
          <p style={{ margin: 0, opacity: 0.85 }}>Carregando…</p>
        ) : cars.length === 0 ? (
          <p style={{ margin: 0, opacity: 0.85 }}>Nenhum carro cadastrado.</p>
        ) : (
          <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 10 }}>
            {cars.map((c) => (
              <li key={c.id} style={{ border: '1px solid var(--border)', borderRadius: 12, padding: 12, background: 'var(--surface)' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, alignItems: 'center', flexWrap: 'wrap' }}>
                  <div style={{ fontWeight: 800 }}>{carLabel(c)}</div>
                  <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
                    <Link to={`/cars/${c.id}`}>Serviços / despesas</Link>
                    <Link to={`/cars/${c.id}/maintenance`}>Manutenção</Link>
                    <Link to={`/cars/${c.id}/fuelings`}>Abastecimentos</Link>
                  </div>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}

