import { useEffect, useState } from 'react'
import { CarApi, type CarDto } from '../api'
import { CarEditModal } from '../components/CarEditModal'
import { IconDelete, IconEdit, IconOpen, IconRow } from '../components/IconButtons'
import { useTranslation } from 'react-i18next'

export function CarsPage() {
  const { t } = useTranslation(['common', 'cars'])
  const [cars, setCars] = useState<CarDto[] | null>(null)
  const [, setError] = useState<string | null>(null)

  const [editingCar, setEditingCar] = useState<CarDto | null>(null)

  async function refresh() {
    setError(null)
    const list = await CarApi.listCars()
    setCars(list)
  }

  useEffect(() => {
    refresh().catch((e) => setError(e instanceof Error ? e.message : String(e)))
  }, [])

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
    <div className="page">
      <header className="pageHeader">
        <div>
          <h1 style={{ margin: 0 }}>{t('common:appName')}</h1>
          <p style={{ margin: '6px 0 0', opacity: 0.8 }}>{t('cars:subtitle')}</p>
        </div>
      </header>

      <section style={{ marginTop: 22 }}>
        <h2 style={{ marginTop: 0 }}>{t('cars:list.title')}</h2>
        {cars === null ? (
          <p>{t('common:status.loading')}</p>
        ) : cars.length === 0 ? (
          <p>{t('cars:list.empty')}</p>
        ) : (
          <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 12 }}>
            {cars.map((c) => (
              <li key={c.id} style={{ border: '1px solid var(--border)', borderRadius: 12, padding: 14, background: 'var(--surface)' }}>
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
