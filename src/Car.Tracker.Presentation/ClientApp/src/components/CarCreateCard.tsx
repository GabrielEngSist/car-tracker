import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { CarApi } from '../api'
import { ehPlacaValida, normalizarPlaca } from '../placaBrasil'

type Props = {
  onCreated: () => Promise<void> | void
  onError?: (message: string) => void
  variant?: 'card' | 'bare'
}

export function CarCreateForm({ onCreated, onError, variant = 'card' }: Props) {
  const { t } = useTranslation(['cars'])

  const [name, setName] = useState('')
  const [model, setModel] = useState('')
  const [year, setYear] = useState<number>(new Date().getFullYear())
  const [currentKm, setCurrentKm] = useState<number>(0)
  const [placa, setPlaca] = useState('')
  const [autoSaving, setAutoSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const placaNorm = useMemo(() => normalizarPlaca(placa), [placa])
  const placaErro = useMemo(() => {
    if (!placaNorm) return null
    return ehPlacaValida(placaNorm) ? null : t('cars:addCar.plateInvalid')
  }, [placaNorm, t])

  const canCreate = useMemo(
    () => model.trim().length > 0 && year > 1900 && currentKm >= 0 && !placaErro,
    [model, year, currentKm, placaErro],
  )

  const canAutoRegister = useMemo(
    () => placaNorm.length > 0 && ehPlacaValida(placaNorm) && currentKm >= 0,
    [placaNorm, currentKm],
  )

  function reportError(e: unknown) {
    const message = e instanceof Error ? e.message : String(e)
    setError(message)
    onError?.(message)
  }

  function resetForm() {
    setName('')
    setModel('')
    setYear(new Date().getFullYear())
    setCurrentKm(0)
    setPlaca('')
  }

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
      resetForm()
      await onCreated()
    } catch (err) {
      reportError(err)
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
      resetForm()
      await onCreated()
    } catch (err) {
      reportError(err)
    } finally {
      setAutoSaving(false)
    }
  }

  const form = (
    <form onSubmit={onCreate} className="gridForm carsCreate">
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
          {placaErro ? <div style={{ color: 'var(--danger)', fontSize: 12, marginTop: 6 }}>{placaErro}</div> : null}
          <div style={{ fontSize: 12, opacity: 0.65, marginTop: 6 }}>{t('cars:addCar.autoHelp')}</div>
        </label>

        <div style={{ gridColumn: '1 / -1', display: 'flex', flexWrap: 'wrap', justifyContent: 'flex-end', gap: 10 }}>
          <button type="button" disabled={!canAutoRegister || autoSaving} onClick={() => void onCadastrarAutomaticamente()}>
            {autoSaving ? t('cars:addCar.autoButtonLoading') : t('cars:addCar.autoButtonIdle')}
          </button>
          <button type="submit" disabled={!canCreate}>
            {t('cars:addCar.createButton')}
          </button>
        </div>
      {error ? <p style={{ color: 'var(--danger)', marginTop: 12 }}>{error}</p> : null}
    </form>
  )

  if (variant === 'bare') return form

  return (
    <section className="card" style={{ marginTop: 22 }}>
      <h2 style={{ marginTop: 0 }}>{t('cars:addCar.title')}</h2>
      {form}
    </section>
  )
}

