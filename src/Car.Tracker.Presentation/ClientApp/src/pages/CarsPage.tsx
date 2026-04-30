import { useEffect, useMemo, useState } from 'react'
import { CarApi, type CarDto } from '../api'
import { CarEditModal } from '../components/CarEditModal'
import { IconDelete, IconEdit, IconOpen, IconRow } from '../components/IconButtons'
import { ehPlacaValida, normalizarPlaca } from '../placaBrasil'

export function CarsPage() {
  const [cars, setCars] = useState<CarDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)

  const [name, setName] = useState('')
  const [model, setModel] = useState('')
  const [year, setYear] = useState<number>(new Date().getFullYear())
  const [currentKm, setCurrentKm] = useState<number>(0)
  const [placa, setPlaca] = useState('')
  const [autoSaving, setAutoSaving] = useState(false)

  const [editingCar, setEditingCar] = useState<CarDto | null>(null)

  const placaNorm = useMemo(() => normalizarPlaca(placa), [placa])
  const placaErro = useMemo(() => {
    if (!placaNorm) return null
    return ehPlacaValida(placaNorm) ? null : 'Placa inválida (use formato antigo ABC1234 ou Mercosul, ex. ABC1D23).'
  }, [placaNorm])

  const canCreate = useMemo(
    () => model.trim().length > 0 && year > 1900 && currentKm >= 0 && !placaErro,
    [model, year, currentKm, placaErro],
  )

  const canAutoRegister = useMemo(
    () => placaNorm.length > 0 && ehPlacaValida(placaNorm) && currentKm >= 0,
    [placaNorm, currentKm],
  )

  async function refresh() {
    setError(null)
    const list = await CarApi.listCars()
    setCars(list)
  }

  useEffect(() => {
    refresh().catch((e) => setError(e instanceof Error ? e.message : String(e)))
  }, [])

  async function onCreate(e: React.FormEvent) {
    e.preventDefault()
    if (!canCreate) return
    setError(null)
    try {
      await CarApi.createCar({
        model: model.trim(),
        year,
        currentKm,
        name: name.trim() ? name.trim() : null,
        placa: placaNorm || null,
        autoBuscarDados: false,
      })
      setName('')
      setModel('')
      setYear(new Date().getFullYear())
      setCurrentKm(0)
      setPlaca('')
      await refresh()
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    }
  }

  async function onCadastrarAutomaticamente() {
    if (!canAutoRegister) return
    setError(null)
    setAutoSaving(true)
    try {
      await CarApi.createCar({
        currentKm,
        name: name.trim() ? name.trim() : null,
        placa: placaNorm,
        autoBuscarDados: true,
      })
      setName('')
      setModel('')
      setYear(new Date().getFullYear())
      setCurrentKm(0)
      setPlaca('')
      await refresh()
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    } finally {
      setAutoSaving(false)
    }
  }

  async function onDeleteCar(c: CarDto) {
    if (!window.confirm(`Delete ${c.name ? `${c.name} · ` : ''}${c.model}? This removes all services and plans.`)) return
    setError(null)
    try {
      await CarApi.deleteCar(c.id)
      await refresh()
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    }
  }

  return (
    <div style={{ maxWidth: 980, margin: '0 auto', padding: 24 }}>
      <header style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', gap: 12 }}>
        <div>
          <h1 style={{ margin: 0 }}>Car Tracker</h1>
          <p style={{ margin: '6px 0 0', opacity: 0.8 }}>Cars, expenses, and next-due maintenance.</p>
        </div>
      </header>

      <section style={{ marginTop: 22, padding: 16, border: '1px solid rgba(255,255,255,0.12)', borderRadius: 12 }}>
        <h2 style={{ marginTop: 0 }}>Add a car</h2>
        <form onSubmit={onCreate} style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 140px 160px', gap: 12 }}>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Nickname (optional)</div>
            <input value={name} onChange={(e) => setName(e.target.value)} placeholder="My Civic" />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Model *</div>
            <input value={model} onChange={(e) => setModel(e.target.value)} placeholder="Honda Civic" required />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Year</div>
            <input type="number" value={year} onChange={(e) => setYear(Number(e.target.value))} min={1900} max={3000} />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Current km</div>
            <input type="number" value={currentKm} onChange={(e) => setCurrentKm(Number(e.target.value))} min={0} />
          </label>

          <label style={{ gridColumn: '1 / -1' }}>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>Placa (opcional)</div>
            <input
              value={placa}
              onChange={(e) => setPlaca(e.target.value)}
              placeholder="ABC1D23 ou ABC-1234"
              autoCapitalize="characters"
              spellCheck={false}
            />
            {placaErro ? <div style={{ color: 'salmon', fontSize: 12, marginTop: 6 }}>{placaErro}</div> : null}
            <div style={{ fontSize: 12, opacity: 0.65, marginTop: 6 }}>
              Cadastro automático consulta placa e FIPE (custo de API) e preenche modelo e ano.
            </div>
          </label>

          <div style={{ gridColumn: '1 / -1', display: 'flex', flexWrap: 'wrap', justifyContent: 'flex-end', gap: 10 }}>
            <button type="button" disabled={!canAutoRegister || autoSaving} onClick={() => void onCadastrarAutomaticamente()}>
              {autoSaving ? 'Consultando…' : 'Cadastrar automaticamente'}
            </button>
            <button type="submit" disabled={!canCreate}>
              Create car
            </button>
          </div>
        </form>

        {error ? <p style={{ color: 'salmon', marginTop: 12 }}>{error}</p> : null}
      </section>

      <section style={{ marginTop: 22 }}>
        <h2 style={{ marginTop: 0 }}>Your cars</h2>
        {cars === null ? (
          <p>Loading…</p>
        ) : cars.length === 0 ? (
          <p>No cars yet.</p>
        ) : (
          <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 12 }}>
            {cars.map((c) => (
              <li key={c.id} style={{ border: '1px solid rgba(255,255,255,0.12)', borderRadius: 12, padding: 14 }}>
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 12 }}>
                  <div>
                    <div style={{ fontWeight: 700 }}>
                      {c.name ? `${c.name} · ` : ''}
                      {c.model} ({c.year})
                    </div>
                    <div style={{ opacity: 0.8, fontSize: 13 }}>
                      {c.placa ? <span style={{ marginRight: 8 }}>Placa {c.placa}</span> : null}
                      {c.currentKm.toLocaleString()} km
                    </div>
                  </div>
                  <IconRow>
                    <IconOpen to={`/cars/${c.id}`} label="Open car" />
                    <IconEdit label="Edit car" onClick={() => setEditingCar(c)} />
                    <IconDelete label="Delete car" onClick={() => onDeleteCar(c)} />
                  </IconRow>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>

      <CarEditModal
        open={editingCar !== null}
        car={editingCar}
        onClose={() => setEditingCar(null)}
        onSave={async (body) => {
          if (!editingCar) return
          await CarApi.patchCar(editingCar.id, {
            model: body.model,
            year: body.year,
            currentKm: body.currentKm,
            name: body.name,
            placa: body.placa,
          })
          await refresh()
        }}
      />
    </div>
  )
}
