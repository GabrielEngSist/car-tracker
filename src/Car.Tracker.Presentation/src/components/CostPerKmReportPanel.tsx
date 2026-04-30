import { useEffect, useMemo, useState } from 'react'
import { CarApi, type CostPerKmReportDto, type CostReportQuery } from '../api'

function brl(n: number) {
  return n.toLocaleString(undefined, { style: 'currency', currency: 'BRL' })
}

type Basis = CostReportQuery['basis']

export function CostPerKmReportPanel({ carId, title }: { carId: string; title?: string }) {
  const [basis, setBasis] = useState<Basis>('period')
  const [period, setPeriod] = useState<string>('total')
  const [distanceRef, setDistanceRef] = useState<string>('km100')
  const [report, setReport] = useState<CostPerKmReportDto | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const query = useMemo<CostReportQuery>(
    () => ({
      basis,
      period,
      distanceRef,
    }),
    [basis, period, distanceRef],
  )

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)
    void CarApi.getCostPerKmReport(carId, query)
      .then((r) => {
        if (!cancelled) setReport(r)
      })
      .catch((e) => {
        if (!cancelled) setError(e instanceof Error ? e.message : String(e))
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [carId, query])

  return (
    <section className="card">
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12, alignItems: 'flex-end', justifyContent: 'space-between' }}>
        <div style={{ flex: '1 1 200px', minWidth: 0 }}>
          <h2 style={{ marginTop: 0 }}>{title ?? 'Relatório: custo por km'}</h2>
          <p style={{ margin: '4px 0 0', opacity: 0.75, fontSize: 13 }}>
            Inclui <strong>despesas / manutenções</strong> registradas e <strong>abastecimentos</strong> na mesma janela.
          </p>
        </div>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, alignItems: 'flex-end' }}>
          <label style={{ fontSize: 12, opacity: 0.85 }}>
            <div style={{ marginBottom: 4 }}>Base</div>
            <select value={basis} onChange={(e) => setBasis(e.target.value as Basis)}>
              <option value="period">Por período</option>
              <option value="lifetime">Vida útil (todo histórico)</option>
            </select>
          </label>
          {basis === 'period' ? (
            <label style={{ fontSize: 12, opacity: 0.85 }}>
              <div style={{ marginBottom: 4 }}>Agregação de tempo</div>
              <select value={period} onChange={(e) => setPeriod(e.target.value)}>
                <option value="total">Total</option>
                <option value="1d">1 dia</option>
                <option value="1m">1 mês (~30 dias)</option>
                <option value="6m">6 meses (~182 dias)</option>
                <option value="1y">1 ano (~365 dias)</option>
              </select>
            </label>
          ) : null}
          <label style={{ fontSize: 12, opacity: 0.85 }}>
            <div style={{ marginBottom: 4 }}>Referência de km</div>
            <select value={distanceRef} onChange={(e) => setDistanceRef(e.target.value)}>
              <option value="total">Total na janela</option>
              <option value="km1">1 km</option>
              <option value="km10">10 km</option>
              <option value="km100">100 km</option>
              <option value="km1000">1000 km</option>
            </select>
          </label>
        </div>
      </div>

      {error ? <p style={{ color: 'var(--danger)', marginTop: 12 }}>{error}</p> : null}
      {loading ? <p style={{ marginTop: 12, opacity: 0.85 }}>Calculando…</p> : null}

      {!loading && report ? (
        <div style={{ marginTop: 14 }}>
          <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 10 }}>{report.aggregatorLabel}</div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))', gap: 10 }}>
            <div style={{ padding: 10, border: '1px solid var(--border)', borderRadius: 10 }}>
              <div style={{ fontSize: 11, opacity: 0.75 }}>Custos · manutenção</div>
              <div style={{ fontWeight: 800 }}>{brl(report.maintenanceExpenseTotal)}</div>
            </div>
            <div style={{ padding: 10, border: '1px solid var(--border)', borderRadius: 10 }}>
              <div style={{ fontSize: 11, opacity: 0.75 }}>Custos · combustível</div>
              <div style={{ fontWeight: 800 }}>{brl(report.fuelExpenseTotal)}</div>
            </div>
            <div style={{ padding: 10, border: '1px solid var(--border)', borderRadius: 10 }}>
              <div style={{ fontSize: 11, opacity: 0.75 }}>Total</div>
              <div style={{ fontWeight: 800 }}>{brl(report.grandTotalExpense)}</div>
            </div>
            <div style={{ padding: 10, border: '1px solid var(--border)', borderRadius: 10 }}>
              <div style={{ fontSize: 11, opacity: 0.75 }}>Δ km observado na janela</div>
              <div style={{ fontWeight: 800 }}>{report.kmDelta != null ? `${report.kmDelta.toLocaleString()} km` : '—'}</div>
            </div>
            <div style={{ padding: 10, border: '1px solid var(--border)', borderRadius: 10 }}>
              <div style={{ fontSize: 11, opacity: 0.75 }}>Custo / km</div>
              <div style={{ fontWeight: 800 }}>{report.costPerKm != null ? `${brl(report.costPerKm)} / km` : '—'}</div>
            </div>
            <div style={{ padding: 10, border: '1px solid var(--border)', borderRadius: 10 }}>
              <div style={{ fontSize: 11, opacity: 0.75 }}>
                Referência{' '}
                {report.distanceReferenceKm != null ? `(${report.distanceReferenceKm.toLocaleString()} km)` : '(total na janela)'}
              </div>
              <div style={{ fontWeight: 800 }}>
                {report.estimatedCostAtDistanceReference != null ? brl(report.estimatedCostAtDistanceReference) : brl(report.grandTotalExpense)}
              </div>
            </div>
          </div>
          <div style={{ marginTop: 10, fontSize: 12, opacity: 0.7 }}>
            Janela: {report.windowStartInclusive} → {report.windowEndInclusive} · Registros: {report.expenseEntryCountIncluded} despesas,{' '}
            {report.fuelingEntryCountIncluded} abastecimentos.
          </div>
        </div>
      ) : null}
    </section>
  )
}
