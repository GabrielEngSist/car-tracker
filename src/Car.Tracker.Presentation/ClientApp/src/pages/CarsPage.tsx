import { useEffect, useMemo, useState } from 'react'
import { CarApi, type CarDto } from '../api'
import { CarEditModal } from '../components/CarEditModal'
import { IconDelete, IconEdit, IconOpen, IconRow } from '../components/IconButtons'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { ehPlacaValida, normalizarPlaca } from '../placaBrasil'
import { useTranslation } from 'react-i18next'

export function CarsPage() {
  const { t } = useTranslation(['common', 'cars'])
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
    return ehPlacaValida(placaNorm) ? null : t('cars:addCar.plateInvalid')
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
    const nameAndModel = `${c.name ? `${c.name} · ` : ''}${c.model}`
    if (!window.confirm(t('cars:confirmDelete', { nameAndModel }))) return
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
          <h1 style={{ margin: 0 }}>{t('common:appName')}</h1>
          <p style={{ margin: '6px 0 0', opacity: 0.8 }}>{t('cars:subtitle')}</p>
        </div>
        <LanguageSwitcher />
      </header>

      <section style={{ marginTop: 22, padding: 16, border: '1px solid rgba(255,255,255,0.12)', borderRadius: 12 }}>
        <h2 style={{ marginTop: 0 }}>{t('cars:addCar.title')}</h2>
        <form onSubmit={onCreate} style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 140px 160px', gap: 12 }}>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('cars:addCar.nicknameLabel')}</div>
            <input value={name} onChange={(e) => setName(e.target.value)} placeholder={t('cars:addCar.nicknamePlaceholder')} />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('cars:addCar.modelLabel')}</div>
            <input value={model} onChange={(e) => setModel(e.target.value)} placeholder={t('cars:addCar.modelPlaceholder')} required />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('cars:addCar.yearLabel')}</div>
            <input type="number" value={year} onChange={(e) => setYear(Number(e.target.value))} min={1900} max={3000} />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('cars:addCar.currentKmLabel')}</div>
            <input type="number" value={currentKm} onChange={(e) => setCurrentKm(Number(e.target.value))} min={0} />
          </label>

          <label style={{ gridColumn: '1 / -1' }}>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('cars:addCar.plateLabel')}</div>
            <input
              value={placa}
              onChange={(e) => setPlaca(e.target.value)}
              placeholder={t('cars:addCar.platePlaceholder')}
              autoCapitalize="characters"
              spellCheck={false}
            />
            {placaErro ? <div style={{ color: 'salmon', fontSize: 12, marginTop: 6 }}>{placaErro}</div> : null}
            <div style={{ fontSize: 12, opacity: 0.65, marginTop: 6 }}>
              {t('cars:addCar.autoHelp')}
            </div>
          </label>

          <div style={{ gridColumn: '1 / -1', display: 'flex', flexWrap: 'wrap', justifyContent: 'flex-end', gap: 10 }}>
            <button type="button" disabled={!canAutoRegister || autoSaving} onClick={() => void onCadastrarAutomaticamente()}>
              {autoSaving ? t('cars:addCar.autoButtonLoading') : t('cars:addCar.autoButtonIdle')}
            </button>
            <button type="submit" disabled={!canCreate}>
              {t('cars:addCar.createButton')}
            </button>
          </div>
        </form>

        {error ? <p style={{ color: 'salmon', marginTop: 12 }}>{error}</p> : null}
      </section>

      <section style={{ marginTop: 22 }}>
        <h2 style={{ marginTop: 0 }}>{t('cars:list.title')}</h2>
        {cars === null ? (
          <p>{t('common:status.loading')}</p>
        ) : cars.length === 0 ? (
          <p>{t('cars:list.empty')}</p>
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
                      {c.placa ? (
                        <span style={{ marginRight: 8 }}>
                          {t('cars:platePrefix')} {c.placa}
                        </span>
                      ) : null}
                      {c.currentKm.toLocaleString()} km
                    </div>
                  </div>
                  <IconRow>
                    <IconOpen to={`/cars/${c.id}`} label={t('cars:openCar')} />
                    <IconEdit label={t('cars:editCar')} onClick={() => setEditingCar(c)} />
                    <IconDelete label={t('cars:deleteCar')} onClick={() => onDeleteCar(c)} />
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
