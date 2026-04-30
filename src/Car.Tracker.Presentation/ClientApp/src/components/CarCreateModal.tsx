import { useTranslation } from 'react-i18next'
import { CarCreateForm } from './CarCreateCard'

type Props = {
  open: boolean
  onClose: () => void
  onCreated: () => Promise<void> | void
}

export function CarCreateModal({ open, onClose, onCreated }: Props) {
  const { t } = useTranslation(['cars', 'common'])
  if (!open) return null

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <div
        className="modal-panel modal-panel--fullscreen"
        role="dialog"
        aria-modal="true"
        aria-labelledby="car-create-title"
        onMouseDown={(e) => e.stopPropagation()}
      >
        <button type="button" className="modal-close" onClick={onClose} aria-label={t('common:actions.close')} title={t('common:actions.close')}>
          ✕
        </button>
        <h3 id="car-create-title">{t('cars:addCar.title')}</h3>
        <div style={{ marginTop: 12 }}>
          <CarCreateForm
            variant="bare"
            onCreated={async () => {
              await onCreated()
              onClose()
            }}
          />
        </div>

        <div className="modal-actions">
          <button type="button" onClick={onClose}>
            {t('common:actions.cancel')}
          </button>
        </div>
      </div>
    </div>
  )
}

