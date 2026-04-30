import { useEffect, useMemo, useRef, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import {
  CarApi,
  type CarDto,
  type CarRegistryDto,
  type ConsultaPlacaDto,
  type ConsultaPrecoFipeDto,
  type ConsultaPrecoFipeItemDto,
  type ExpenseEntryDto,
  type ExpenseEntryType,
  type FuelingEntryDto,
} from '../api'
import { CostPerKmReportPanel } from '../components/CostPerKmReportPanel'
import { CarEditModal } from '../components/CarEditModal'
import { IconDelete, IconEdit, IconRow } from '../components/IconButtons'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import { ThemeToggle } from '../components/ThemeToggle'
import { useTranslation } from 'react-i18next'

function todayIsoDate(): string {
  const d = new Date()
  const yyyy = d.getFullYear()
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd}`
}

function formatLocal(iso: string) {
  try {
    return new Date(iso).toLocaleString(undefined, { dateStyle: 'short', timeStyle: 'short' })
  } catch {
    return iso
  }
}

function disp(v: string | number | null | undefined) {
  if (v === null || v === undefined || v === '') return '—'
  return String(v)
}

function infoRow(label: string, value: string | number | null | undefined, key: string) {
  return (
    <div
      key={key}
      style={{
        display: 'grid',
        gridTemplateColumns: 'minmax(140px, 200px) 1fr',
        gap: 10,
        padding: '6px 0',
        borderBottom: '1px solid color-mix(in srgb, var(--border) 55%, transparent)',
      }}
    >
      <div style={{ fontSize: 12, opacity: 0.75 }}>{label}</div>
      <div style={{ fontSize: 13, wordBreak: 'break-word' }}>{disp(value)}</div>
    </div>
  )
}

function prettyJson(raw: string | null) {
  if (!raw) return '—'
  try {
    return JSON.stringify(JSON.parse(raw), null, 2)
  } catch {
    return raw
  }
}

function ConsultaPlacaBlock({ c }: { c: ConsultaPlacaDto }) {
  return (
    <div>
      {infoRow('Status', c.status, 'cp-s')}
      {infoRow('Mensagem', c.mensagem, 'cp-m')}
      {infoRow('Data da solicitação', c.dataSolicitacao, 'cp-ds')}
      {infoRow('Placa (requisição)', c.requestPlaca, 'cp-rp')}
      {infoRow('Placa', c.placa, 'cp-p')}
      {infoRow('Chassi', c.chassi, 'cp-ch')}
      {infoRow('Ano fabricação', c.anoFabricacao, 'cp-af')}
      {infoRow('Ano modelo', c.anoModelo, 'cp-am')}
      {infoRow('Marca', c.marca, 'cp-ma')}
      {infoRow('Modelo', c.modelo, 'cp-mo')}
      {infoRow('Cor', c.cor, 'cp-co')}
      {infoRow('Segmento', c.segmento, 'cp-se')}
      {infoRow('Combustível', c.combustivel, 'cp-cb')}
      {infoRow('Procedência', c.procedencia, 'cp-pr')}
      {infoRow('Município', c.municipio, 'cp-mu')}
      {infoRow('UF', c.ufMunicipio, 'cp-uf')}
      {infoRow('Tipo de veículo', c.tipoVeiculo, 'cp-tv')}
      {infoRow('Subsegmento', c.subSegmento, 'cp-ss')}
      {infoRow('Nº motor', c.numeroMotor, 'cp-nm')}
      {infoRow('Nº caixa câmbio', c.numeroCaixaCambio, 'cp-nc')}
      {infoRow('Potência', c.potencia, 'cp-pt')}
      {infoRow('Cilindradas', c.cilindradas, 'cp-ci')}
      {infoRow('Nº eixos', c.numeroEixos, 'cp-ne')}
      {infoRow('Capacidade máx. tração', c.capacidadeMaximaTracao, 'cp-cmt')}
      {infoRow('Capacidade passageiros', c.capacidadePassageiro, 'cp-cp')}
      {infoRow('Consulta gravada em', formatLocal(c.createdAt), 'cp-ca')}
      {infoRow('Consulta atualizada em', formatLocal(c.updatedAt), 'cp-ua')}
    </div>
  )
}

function ConsultaFipeBlock({ f }: { f: ConsultaPrecoFipeDto }) {
  return (
    <div>
      {infoRow('Status', f.status, 'cf-s')}
      {infoRow('Mensagem', f.mensagem, 'cf-m')}
      {infoRow('Data da solicitação', f.dataSolicitacao, 'cf-ds')}
      {infoRow('Placa (requisição)', f.requestPlaca, 'cf-rp')}
      {infoRow('Veículo — placa', f.veiculoPlaca, 'cf-vp')}
      {infoRow('Veículo — chassi', f.veiculoChassi, 'cf-vc')}
      {infoRow('Veículo — ano fab.', f.veiculoAnoFabricacao, 'cf-vaf')}
      {infoRow('Veículo — ano modelo', f.veiculoAnoModelo, 'cf-vam')}
      {infoRow('Veículo — marca', f.veiculoMarca, 'cf-vm')}
      {infoRow('Veículo — modelo', f.veiculoModelo, 'cf-vmd')}
      {infoRow('Veículo — cor', f.veiculoCor, 'cf-vco')}
      {infoRow('Veículo — segmento', f.veiculoSegmento, 'cf-vs')}
      {infoRow('Veículo — combustível', f.veiculoCombustivel, 'cf-vcb')}
      {infoRow('Veículo — procedência', f.veiculoProcedencia, 'cf-vpr')}
      {infoRow('Veículo — município', f.veiculoMunicipio, 'cf-vmu')}
      {infoRow('Veículo — UF', f.veiculoUfMunicipio, 'cf-vuf')}
      {infoRow('Tipo de veículo', f.tipoVeiculo, 'cf-tv')}
      {infoRow('Subsegmento', f.subSegmento, 'cf-ss')}
      {infoRow('Nº motor', f.numeroMotor, 'cf-nm')}
      {infoRow('Nº caixa câmbio', f.numeroCaixaCambio, 'cf-nc')}
      {infoRow('Potência', f.potencia, 'cf-pt')}
      {infoRow('Cilindradas', f.cilindradas, 'cf-ci')}
      {infoRow('Nº eixos', f.numeroEixos, 'cf-ne')}
      {infoRow('Capacidade máx. tração', f.capacidadeMaximaTracao, 'cf-cmt')}
      {infoRow('Capacidade passageiros', f.capacidadePassageiro, 'cf-cp')}
      {infoRow('Peso bruto total', f.pesoBrutoTotal, 'cf-pbt')}
      {infoRow('Consulta gravada em', formatLocal(f.createdAt), 'cf-ca')}
      {infoRow('Consulta atualizada em', formatLocal(f.updatedAt), 'cf-ua')}
      {f.itens.length > 0 ? (
        <div style={{ marginTop: 14 }}>
          <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 8 }}>Itens FIPE</div>
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
              <thead>
                <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(255,255,255,0.15)' }}>
                  <th style={{ padding: '6px 8px' }}>Código</th>
                  <th style={{ padding: '6px 8px' }}>Modelo / versão</th>
                  <th style={{ padding: '6px 8px' }}>Preço</th>
                  <th style={{ padding: '6px 8px' }}>Mês ref.</th>
                </tr>
              </thead>
              <tbody>
                {f.itens.map((it: ConsultaPrecoFipeItemDto) => (
                  <tr key={it.id} style={{ borderBottom: '1px solid rgba(255,255,255,0.06)', verticalAlign: 'top' }}>
                    <td style={{ padding: '8px' }}>{disp(it.codigoFipe)}</td>
                    <td style={{ padding: '8px' }}>{disp(it.modeloVersao)}</td>
                    <td style={{ padding: '8px' }}>{disp(it.preco)}</td>
                    <td style={{ padding: '8px' }}>{disp(it.mesReferencia)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {f.itens.map((it: ConsultaPrecoFipeItemDto) =>
            it.historicoJson ? (
              <details key={`h-${it.id}`} style={{ marginTop: 10 }}>
                <summary style={{ cursor: 'pointer', fontSize: 13, opacity: 0.9 }}>
                  Histórico JSON — {disp(it.codigoFipe)} / {disp(it.modeloVersao)}
                </summary>
                <pre
                  style={{
                    marginTop: 8,
                    padding: 10,
                    fontSize: 11,
                    lineHeight: 1.4,
                    overflow: 'auto',
                    maxHeight: 240,
                    borderRadius: 8,
                    background: 'color-mix(in srgb, var(--bg) 70%, transparent)',
                  }}
                >
                  {prettyJson(it.historicoJson)}
                </pre>
              </details>
            ) : null,
          )}
        </div>
      ) : null}
    </div>
  )
}

export function CarDetailsPage() {
  const { t } = useTranslation(['common', 'carDetails'])
  const { carId } = useParams()
  const navigate = useNavigate()
  const [car, setCar] = useState<CarDto | null>(null)
  const [entries, setEntries] = useState<ExpenseEntryDto[] | null>(null)
  const [fuelings, setFuelings] = useState<FuelingEntryDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)

  const recentFuelingsSorted = useMemo(() => {
    if (!fuelings) return []
    return [...fuelings].sort((a, b) => {
      const d = String(b.performedAt).localeCompare(String(a.performedAt))
      return d !== 0 ? d : b.kmAtFueling - a.kmAtFueling
    })
  }, [fuelings])

  const [linkedOpen, setLinkedOpen] = useState(false)
  const [registry, setRegistry] = useState<CarRegistryDto | null>(null)
  const [registryLoading, setRegistryLoading] = useState(false)
  const [registryError, setRegistryError] = useState<string | null>(null)

  const [carEditOpen, setCarEditOpen] = useState(false)

  const [currentKm, setCurrentKm] = useState<number>(0)

  const [editingEntryId, setEditingEntryId] = useState<string | null>(null)
  const editingEntryIdRef = useRef<string | null>(null)
  useEffect(() => {
    editingEntryIdRef.current = editingEntryId
  }, [editingEntryId])
  const [type, setType] = useState<ExpenseEntryType>('Service')
  const [title, setTitle] = useState('')
  const [price, setPrice] = useState<number>(0)
  const [supplierBrand, setSupplierBrand] = useState('')
  const [productModel, setProductModel] = useState('')
  const [performedAt, setPerformedAt] = useState<string>(todayIsoDate())
  const [kmAtService, setKmAtService] = useState<number>(0)
  const [notes, setNotes] = useState('')

  const canSaveEntry = useMemo(() => title.trim().length > 0 && price >= 0 && kmAtService >= 0, [title, price, kmAtService])

  async function refresh(id: string): Promise<CarDto> {
    setError(null)
    setRegistry(null)
    setRegistryError(null)
    const [c, e, rawFuelings] = await Promise.all([CarApi.getCar(id), CarApi.listEntries(id), CarApi.listFuelingsByCar(id)])
    setCar(c)
    setEntries(e)
    setFuelings(rawFuelings)
    setCurrentKm(c.currentKm)
    if (!editingEntryIdRef.current) setKmAtService(c.currentKm)
    return c
  }

  useEffect(() => {
    if (!carId) return
    refresh(carId).catch((e) => setError(e instanceof Error ? e.message : String(e)))
  }, [carId])

  useEffect(() => {
    if (!carId || !linkedOpen || registry !== null) return
    let cancelled = false
    setRegistryLoading(true)
    setRegistryError(null)
    void CarApi.getCarRegistry(carId)
      .then((r) => {
        if (!cancelled) setRegistry(r)
      })
      .catch((e) => {
        if (!cancelled) setRegistryError(e instanceof Error ? e.message : String(e))
      })
      .finally(() => {
        setRegistryLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [carId, linkedOpen, registry])

  function beginEditEntry(e: ExpenseEntryDto) {
    setEditingEntryId(e.id)
    setType(e.type)
    setTitle(e.title)
    setPrice(e.price)
    setSupplierBrand(e.supplierBrand ?? '')
    setProductModel(e.productModel ?? '')
    setPerformedAt(e.performedAt)
    setKmAtService(e.kmAtService)
    setNotes(e.notes ?? '')
  }

  function cancelEditEntry(kmFallback?: number) {
    const km = kmFallback ?? car?.currentKm ?? 0
    setEditingEntryId(null)
    setTitle('')
    setPrice(0)
    setSupplierBrand('')
    setProductModel('')
    setPerformedAt(todayIsoDate())
    setKmAtService(km)
    setNotes('')
    setType('Service')
  }

  async function onUpdateKm(e: React.FormEvent) {
    e.preventDefault()
    if (!carId) return
    setError(null)
    try {
      const c = await CarApi.patchCar(carId, { currentKm })
      setCar(c)
      setRegistry(null)
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    }
  }

  async function onSaveEntry(e: React.FormEvent) {
    e.preventDefault()
    if (!carId || !canSaveEntry) return
    setError(null)
    try {
      const wasEditing = !!editingEntryId
      if (editingEntryId) {
        await CarApi.patchEntry(carId, editingEntryId, {
          type,
          title: title.trim(),
          price,
          supplierBrand: supplierBrand.trim() ? supplierBrand.trim() : null,
          productModel: productModel.trim() ? productModel.trim() : null,
          performedAt,
          kmAtService,
          notes: notes.trim() ? notes.trim() : null,
        })
      } else {
        await CarApi.createEntry(carId, {
          type,
          title: title.trim(),
          price,
          supplierBrand: supplierBrand.trim() ? supplierBrand.trim() : null,
          productModel: productModel.trim() ? productModel.trim() : null,
          performedAt,
          kmAtService,
          notes: notes.trim() ? notes.trim() : null,
        })
      }
      const c = await refresh(carId)
      if (wasEditing) {
        cancelEditEntry(c.currentKm)
      } else {
        setTitle('')
        setPrice(0)
        setSupplierBrand('')
        setProductModel('')
        setPerformedAt(todayIsoDate())
        setKmAtService(c.currentKm)
        setNotes('')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err))
    }
  }

  async function onDeleteEntry(entry: ExpenseEntryDto) {
    if (!carId) return
    if (!window.confirm(t('carDetails:confirm.deleteEntry', { title: entry.title }))) return
    setError(null)
    try {
      await CarApi.deleteEntry(carId, entry.id)
      const c = await refresh(carId)
      if (editingEntryId === entry.id) cancelEditEntry(c.currentKm)
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    }
  }

  async function onDeleteCar() {
    if (!carId || !car) return
    if (!window.confirm(t('carDetails:confirm.deleteCar'))) return
    setError(null)
    try {
      await CarApi.deleteCar(carId)
      navigate('/')
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e))
    }
  }

  if (!carId) return <p>Missing car id.</p>

  return (
    <div className="page">
      <header className="pageHeader" style={{ alignItems: 'flex-start' }}>
        <div>
          <div style={{ opacity: 0.8, fontSize: 13 }}>
            <Link to="/">{t('carDetails:backToCars')}</Link>
          </div>
          <h1 style={{ margin: '8px 0 0' }}>
            {car ? (
              <>
                {car.name ? `${car.name} · ` : ''}
                {car.model} ({car.year})
              </>
            ) : (
              'Loading…'
            )}
          </h1>
          {car?.placa ? (
            <div style={{ marginTop: 6, opacity: 0.85, fontSize: 14 }}>
              {t('carDetails:labels.plate')} {car.placa}
            </div>
          ) : null}
        </div>
        <div className="pageHeaderActions" style={{ alignItems: 'center' }}>
          <ThemeToggle />
          <LanguageSwitcher />
          {car ? <div style={{ opacity: 0.85 }}>{car.currentKm.toLocaleString()} km</div> : null}
          {car ? (
            <IconRow>
              <IconEdit label={t('common:actions.edit')} onClick={() => setCarEditOpen(true)} />
              <IconDelete label={t('common:actions.delete')} onClick={onDeleteCar} />
            </IconRow>
          ) : null}
        </div>
      </header>

      {error ? <p style={{ color: 'var(--danger)', marginTop: 12 }}>{error}</p> : null}

      <CostPerKmReportPanel carId={carId} title={t('carDetails:costReportTitle')} />

      <section className="card">
        <button
          type="button"
          onClick={() => setLinkedOpen((o) => !o)}
          aria-expanded={linkedOpen}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 10,
            width: '100%',
            textAlign: 'left',
            padding: '4px 0',
            background: 'none',
            border: 'none',
            color: 'inherit',
            font: 'inherit',
            cursor: 'pointer',
          }}
        >
          <span style={{ fontSize: 12, opacity: 0.75, width: 18 }}>{linkedOpen ? '▼' : '▶'}</span>
          <span style={{ fontWeight: 700, fontSize: 16 }}>{t('carDetails:additionalInfo.title')}</span>
        </button>
        <p style={{ margin: '8px 0 0 28px', opacity: 0.75, fontSize: 13 }}>
          {t('carDetails:additionalInfo.subtitle')}
        </p>
        {linkedOpen ? (
          <div style={{ marginTop: 16, marginLeft: 0 }}>
            {registryLoading ? <p style={{ opacity: 0.85 }}>Carregando…</p> : null}
            {registryError ? <p style={{ color: 'var(--danger)' }}>{registryError}</p> : null}
            {registry && !registryLoading ? (
              <div style={{ display: 'grid', gap: 20 }}>
                <div>
                  <h3 style={{ margin: '0 0 10px', fontSize: 15 }}>{t('carDetails:additionalInfo.appTitle')}</h3>
                  <div style={{ border: '1px solid var(--border)', borderRadius: 8, padding: 12, background: 'var(--surface)' }}>
                    {infoRow('Modelo', registry.car.model, 'cad-m')}
                    {infoRow('Ano', registry.car.year, 'cad-y')}
                    {infoRow('Quilometragem', registry.car.currentKm, 'cad-km')}
                    {infoRow('Apelido', registry.car.name, 'cad-n')}
                    {infoRow('Placa', registry.car.placa, 'cad-p')}
                    {infoRow('Criado em', formatLocal(registry.car.createdAt), 'cad-c')}
                    {infoRow('Atualizado em', formatLocal(registry.car.updatedAt), 'cad-u')}
                  </div>
                </div>

                <div>
                  <h3 style={{ margin: '0 0 10px', fontSize: 15 }}>{t('carDetails:additionalInfo.plateTitle')}</h3>
                  <div style={{ border: '1px solid var(--border)', borderRadius: 8, padding: 12, background: 'var(--surface)' }}>
                    {registry.consultaPlaca ? (
                      <ConsultaPlacaBlock c={registry.consultaPlaca} />
                    ) : (
                      <p style={{ margin: 0, opacity: 0.8 }}>{t('carDetails:additionalInfo.plateEmpty')}</p>
                    )}
                  </div>
                </div>

                <div>
                  <h3 style={{ margin: '0 0 10px', fontSize: 15 }}>{t('carDetails:additionalInfo.fipeTitle')}</h3>
                  <div style={{ border: '1px solid var(--border)', borderRadius: 8, padding: 12, background: 'var(--surface)' }}>
                    {registry.consultaPrecoFipe ? (
                      <ConsultaFipeBlock f={registry.consultaPrecoFipe} />
                    ) : (
                      <p style={{ margin: 0, opacity: 0.8 }}>{t('carDetails:additionalInfo.fipeEmpty')}</p>
                    )}
                  </div>
                </div>

                <div>
                  <h3 style={{ margin: '0 0 10px', fontSize: 15 }}>{t('carDetails:additionalInfo.entriesTitle')}</h3>
                  <div style={{ border: '1px solid var(--border)', borderRadius: 8, padding: 12, maxHeight: 320, overflow: 'auto', background: 'var(--surface)' }}>
                    {registry.expenseEntries.length === 0 ? (
                      <p style={{ margin: 0, opacity: 0.8 }}>{t('carDetails:additionalInfo.entriesEmpty')}</p>
                    ) : (
                      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                        <thead>
                          <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(255,255,255,0.15)' }}>
                            <th style={{ padding: '6px 8px' }}>Data</th>
                            <th style={{ padding: '6px 8px' }}>Título</th>
                            <th style={{ padding: '6px 8px' }}>Tipo</th>
                            <th style={{ padding: '6px 8px' }}>Km</th>
                            <th style={{ padding: '6px 8px' }}>Valor</th>
                          </tr>
                        </thead>
                        <tbody>
                          {registry.expenseEntries.map((ex) => (
                            <tr key={ex.id} style={{ borderBottom: '1px solid rgba(255,255,255,0.06)' }}>
                              <td style={{ padding: '8px', whiteSpace: 'nowrap' }}>{ex.performedAt}</td>
                              <td style={{ padding: '8px' }}>{ex.title}</td>
                              <td style={{ padding: '8px' }}>{ex.type}</td>
                              <td style={{ padding: '8px' }}>{ex.kmAtService.toLocaleString()}</td>
                              <td style={{ padding: '8px' }}>
                                {ex.price.toLocaleString(undefined, { style: 'currency', currency: 'USD' })}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    )}
                  </div>
                </div>

                <div>
                  <h3 style={{ margin: '0 0 10px', fontSize: 15 }}>{t('carDetails:additionalInfo.planTitle')}</h3>
                  <div style={{ border: '1px solid var(--border)', borderRadius: 8, padding: 12, background: 'var(--surface)' }}>
                    {registry.maintenancePlanItems.length === 0 ? (
                      <p style={{ margin: 0, opacity: 0.8 }}>
                        {t('carDetails:additionalInfo.planEmptyPrefix')}{' '}
                        <Link to={`/cars/${carId}/maintenance`}>{t('carDetails:additionalInfo.openMaintenance')}</Link>
                      </p>
                    ) : (
                      <>
                        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                          <thead>
                            <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(255,255,255,0.15)' }}>
                              <th style={{ padding: '6px 8px' }}>Título</th>
                              <th style={{ padding: '6px 8px' }}>Intervalo km</th>
                              <th style={{ padding: '6px 8px' }}>Intervalo dias</th>
                              <th style={{ padding: '6px 8px' }}>Ativo</th>
                            </tr>
                          </thead>
                          <tbody>
                            {registry.maintenancePlanItems.map((p) => (
                              <tr key={p.id} style={{ borderBottom: '1px solid rgba(255,255,255,0.06)' }}>
                                <td style={{ padding: '8px' }}>{p.title}</td>
                                <td style={{ padding: '8px' }}>{disp(p.dueKmInterval)}</td>
                                <td style={{ padding: '8px' }}>{disp(p.dueTimeIntervalDays)}</td>
                                <td style={{ padding: '8px' }}>{p.active ? 'Sim' : 'Não'}</td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                        <div style={{ marginTop: 10 }}>
                          <Link to={`/cars/${carId}/maintenance`}>{t('carDetails:additionalInfo.manageMaintenance')}</Link>
                        </div>
                      </>
                    )}
                  </div>
                </div>
              </div>
            ) : null}
          </div>
        ) : null}
      </section>

      <section className="grid2" style={{ marginTop: 18 }}>
        <div style={{ padding: 16, border: '1px solid var(--border)', borderRadius: 12, background: 'var(--surface)' }}>
          <h2 style={{ marginTop: 0 }}>{t('carDetails:odometer.title')}</h2>
          <form onSubmit={onUpdateKm} style={{ display: 'flex', gap: 10, alignItems: 'end' }}>
            <label style={{ flex: 1 }}>
              <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('carDetails:odometer.currentKmLabel')}</div>
              <input type="number" min={0} value={currentKm} onChange={(e) => setCurrentKm(Number(e.target.value))} />
            </label>
            <button type="submit" disabled={!car}>
              {t('carDetails:odometer.save')}
            </button>
          </form>
        </div>

        <div style={{ padding: 16, border: '1px solid var(--border)', borderRadius: 12, background: 'var(--surface)' }}>
          <h2 style={{ marginTop: 0 }}>{t('carDetails:maintenanceCard.title')}</h2>
          <p style={{ margin: 0, opacity: 0.8 }}>{t('carDetails:maintenanceCard.subtitle')}</p>
          <div style={{ marginTop: 10, display: 'flex', flexWrap: 'wrap', gap: 12 }}>
            <Link to={`/cars/${carId}/maintenance`}>{t('carDetails:maintenanceCard.open')}</Link>
            <Link to={`/cars/${carId}/fuelings`}>{t('carDetails:maintenanceCard.openFuelings')}</Link>
          </div>
        </div>
      </section>

      <section className="card" style={{ marginTop: 14 }}>
        <h2 style={{ marginTop: 0 }}>{t('carDetails:fuelingsCard.title')}</h2>
        {fuelings === null ? (
          <p style={{ opacity: 0.85 }}>{t('common:status.loading')}</p>
        ) : recentFuelingsSorted.length === 0 ? (
          <p style={{ opacity: 0.85 }}>{t('carDetails:fuelingsCard.empty')}</p>
        ) : (
          <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 10 }}>
            {recentFuelingsSorted.slice(0, 5).map((f) => {
              const liters = Number(f.liters)
              const total = Number(f.totalPrice)
              const pricePerLiter = liters > 0 ? total / liters : null
              return (
                <li key={f.id} style={{ border: '1px solid var(--border)', borderRadius: 10, padding: 10 }}>
                  <div style={{ fontWeight: 700, fontSize: 13 }}>
                    {f.performedAt} · {f.kmAtFueling.toLocaleString()} km · {f.fuelType}
                  </div>
                  <div style={{ opacity: 0.85, fontSize: 13, marginTop: 4 }}>
                    {liters.toLocaleString(undefined, { maximumFractionDigits: 2 })} L ·{' '}
                    {total.toLocaleString(undefined, { style: 'currency', currency: 'BRL' })}
                    {pricePerLiter != null ? ` · ${pricePerLiter.toLocaleString(undefined, { style: 'currency', currency: 'BRL' })}/L` : ''}
                  </div>
                </li>
              )
            })}
          </ul>
        )}
        <div style={{ marginTop: 12 }}>
          <Link to={`/cars/${carId}/fuelings`}>{t('carDetails:fuelingsCard.viewAll')}</Link>
        </div>
      </section>

      <section className="card">
        <h2 style={{ marginTop: 0 }}>{editingEntryId ? t('carDetails:entryForm.editTitle') : t('carDetails:entryForm.addTitle')}</h2>
        <form onSubmit={onSaveEntry} className="gridForm carEntryForm">
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('carDetails:entryForm.typeLabel')}</div>
            <select value={type} onChange={(e) => setType(e.target.value as ExpenseEntryType)}>
              <option value="Service">{t('carDetails:entryForm.typeService')}</option>
              <option value="Part">{t('carDetails:entryForm.typePart')}</option>
            </select>
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('carDetails:entryForm.titleLabel')}</div>
            <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Oil change" required />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('carDetails:entryForm.priceLabel')}</div>
            <input type="number" min={0} step="0.01" value={price} onChange={(e) => setPrice(Number(e.target.value))} />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('carDetails:entryForm.dateLabel')}</div>
            <input type="date" value={performedAt} onChange={(e) => setPerformedAt(e.target.value)} />
          </label>

          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('carDetails:entryForm.supplierBrandLabel')}</div>
            <input value={supplierBrand} onChange={(e) => setSupplierBrand(e.target.value)} placeholder="Bosch" />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('carDetails:entryForm.productModelLabel')}</div>
            <input value={productModel} onChange={(e) => setProductModel(e.target.value)} placeholder="5W-30" />
          </label>
          <label>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('carDetails:entryForm.kmAtServiceLabel')}</div>
            <input type="number" min={0} value={kmAtService} onChange={(e) => setKmAtService(Number(e.target.value))} />
          </label>
          <label style={{ gridColumn: '1 / -1' }}>
            <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 6 }}>{t('carDetails:entryForm.notesLabel')}</div>
            <input value={notes} onChange={(e) => setNotes(e.target.value)} placeholder={t('carDetails:entryForm.notesPlaceholder')} />
          </label>

          <div style={{ gridColumn: '1 / -1', display: 'flex', justifyContent: 'flex-end', gap: 10 }}>
            {editingEntryId ? (
              <button type="button" onClick={() => cancelEditEntry()}>
                {t('carDetails:entryForm.cancelEdit')}
              </button>
            ) : null}
            <button type="submit" disabled={!canSaveEntry}>
              {editingEntryId ? t('carDetails:entryForm.saveChanges') : t('carDetails:entryForm.addEntry')}
            </button>
          </div>
        </form>
      </section>

      <section style={{ marginTop: 18 }}>
        <h2 style={{ marginTop: 0 }}>{t('carDetails:history.title')}</h2>
        {entries === null ? (
          <p>{t('common:status.loading')}</p>
        ) : entries.length === 0 ? (
          <p>{t('carDetails:history.empty')}</p>
        ) : (
          <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'grid', gap: 10 }}>
            {entries.map((e) => (
              <li key={e.id} style={{ border: '1px solid var(--border)', borderRadius: 12, padding: 12, background: 'var(--surface)' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, alignItems: 'flex-start' }}>
                  <div style={{ minWidth: 0 }}>
                    <div style={{ fontWeight: 700 }}>
                      {e.title} <span style={{ opacity: 0.7, fontWeight: 500 }}>({e.type})</span>
                    </div>
                    <div style={{ opacity: 0.85, fontSize: 13 }}>
                      {e.performedAt} · {e.kmAtService.toLocaleString()} km
                      {e.supplierBrand ? ` · ${e.supplierBrand}` : ''}
                      {e.productModel ? ` · ${e.productModel}` : ''}
                    </div>
                    {e.notes ? <div style={{ opacity: 0.8, fontSize: 13, marginTop: 6 }}>{e.notes}</div> : null}
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10, flexShrink: 0 }}>
                    <div style={{ fontWeight: 700 }}>{e.price.toLocaleString(undefined, { style: 'currency', currency: 'USD' })}</div>
                    <IconRow>
                      <IconEdit label={`Edit ${e.title}`} onClick={() => beginEditEntry(e)} />
                      <IconDelete label={`Delete ${e.title}`} onClick={() => onDeleteEntry(e)} />
                    </IconRow>
                  </div>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>

      <CarEditModal
        open={carEditOpen && car !== null}
        car={car}
        onClose={() => setCarEditOpen(false)}
        onSave={async (body) => {
          if (!carId) return
          const c = await CarApi.patchCar(carId, {
            model: body.model,
            year: body.year,
            currentKm: body.currentKm,
            name: body.name,
            placa: body.placa,
          })
          setCar(c)
          setCurrentKm(c.currentKm)
          setRegistry(null)
        }}
      />
    </div>
  )
}
