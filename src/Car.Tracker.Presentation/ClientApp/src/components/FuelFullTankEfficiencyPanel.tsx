import { useEffect, useMemo, useState } from 'react'
import { CarApi, type FuelFullTankEfficiencyReportDto, type FuelFullTankReportQuery } from '../api'
import { useTranslation } from 'react-i18next'

function brl(n: number) {
  return n.toLocaleString(undefined, { style: 'currency', currency: 'BRL' })
}

type Basis = FuelFullTankReportQuery['basis']

export function FuelFullTankEfficiencyPanel({ carId }: { carId: string }) {
  const { t } = useTranslation(['fuelings'])
  const [basis, setBasis] = useState<Basis>('period')
  const [period, setPeriod] = useState<string>('total')
  const [report, setReport] = useState<FuelFullTankEfficiencyReportDto | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const query = useMemo<FuelFullTankReportQuery>(
    () => ({
      basis,
      period,
    }),
    [basis, period],
  )

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)
    void CarApi.getFuelFullTankReport(carId, query)
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
          <h2 style={{ marginTop: 0 }}>{t('fuelings:efficiency.title')}</h2>
          <p style={{ margin: '4px 0 0', opacity: 0.75, fontSize: 13 }}>{t('fuelings:efficiency.hint')}</p>
        </div>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, alignItems: 'flex-end' }}>
          <label style={{ fontSize: 12, opacity: 0.85 }}>
            <div style={{ marginBottom: 4 }}>{t('fuelings:efficiency.basisLabel')}</div>
            <select value={basis} onChange={(e) => setBasis(e.target.value as Basis)}>
              <option value="period">{t('fuelings:efficiency.basisPeriod')}</option>
              <option value="lifetime">{t('fuelings:efficiency.basisLifetime')}</option>
            </select>
          </label>
          {basis === 'period' ? (
            <label style={{ fontSize: 12, opacity: 0.85 }}>
              <div style={{ marginBottom: 4 }}>{t('fuelings:efficiency.periodLabel')}</div>
              <select value={period} onChange={(e) => setPeriod(e.target.value)}>
                <option value="total">{t('fuelings:efficiency.period_total')}</option>
                <option value="1d">{t('fuelings:efficiency.period_1d')}</option>
                <option value="1m">{t('fuelings:efficiency.period_1m')}</option>
                <option value="6m">{t('fuelings:efficiency.period_6m')}</option>
                <option value="1y">{t('fuelings:efficiency.period_1y')}</option>
              </select>
            </label>
          ) : null}
        </div>
      </div>

      {error ? <p style={{ color: 'var(--danger)', marginTop: 12 }}>{error}</p> : null}
      {loading ? <p style={{ marginTop: 12, opacity: 0.85 }}>{t('fuelings:efficiency.calculating')}</p> : null}

      {!loading && report ? (
        <div style={{ marginTop: 14 }}>
          <div style={{ fontSize: 12, opacity: 0.8, marginBottom: 10 }}>
            {t('fuelings:efficiency.window', { start: report.windowStartInclusive, end: report.windowEndInclusive })}
          </div>
          <div style={{ fontSize: 12, opacity: 0.75, marginBottom: 12 }}>
            {report.aggregatorLabel} ·{' '}
            {t('fuelings:efficiency.markersInWindow')}: <strong>{report.fullTankMarkersInWindow}</strong> ·{' '}
            {t('fuelings:efficiency.intervalsUsed')}: <strong>{report.intervalsUsedCount}</strong>
          </div>

          {report.intervalsUsedCount === 0 ? (
            <p style={{ opacity: 0.85 }}>{t('fuelings:efficiency.noIntervals')}</p>
          ) : (
            <>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))', gap: 10 }}>
                <div style={{ padding: 10, border: '1px solid var(--border)', borderRadius: 10 }}>
                  <div style={{ fontSize: 11, opacity: 0.75 }}>{t('fuelings:efficiency.overallKmPerLiter')}</div>
                  <div style={{ fontWeight: 800 }}>
                    {report.overallAverageKmPerLiter != null ? `${report.overallAverageKmPerLiter.toLocaleString()} km/L` : '—'}
                  </div>
                </div>
                <div style={{ padding: 10, border: '1px solid var(--border)', borderRadius: 10 }}>
                  <div style={{ fontSize: 11, opacity: 0.75 }}>{t('fuelings:efficiency.overallLPer100')}</div>
                  <div style={{ fontWeight: 800 }}>
                    {report.overallLitersPer100Km != null ? `${report.overallLitersPer100Km.toLocaleString()} L/100km` : '—'}
                  </div>
                </div>
                <div style={{ padding: 10, border: '1px solid var(--border)', borderRadius: 10 }}>
                  <div style={{ fontSize: 11, opacity: 0.75 }}>{t('fuelings:efficiency.overallFuelCostPerKm')}</div>
                  <div style={{ fontWeight: 800 }}>
                    {report.overallFuelCostPerKm != null ? `${brl(report.overallFuelCostPerKm)} / km` : '—'}
                  </div>
                </div>
              </div>

              {report.byFuelType.length > 0 ? (
                <div style={{ marginTop: 16 }}>
                  <div style={{ fontWeight: 700, marginBottom: 8 }}>{t('fuelings:efficiency.byFuelType')}</div>
                  <div style={{ overflowX: 'auto' }}>
                    <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                      <thead>
                        <tr style={{ textAlign: 'left', borderBottom: '1px solid var(--border)' }}>
                          <th style={{ padding: '8px 6px' }}>{t('fuelings:efficiency.fuelType')}</th>
                          <th style={{ padding: '8px 6px' }}>L</th>
                          <th style={{ padding: '8px 6px' }}>{t('fuelings:efficiency.attribKm')}</th>
                          <th style={{ padding: '8px 6px' }}>km/L</th>
                          <th style={{ padding: '8px 6px' }}>{t('fuelings:efficiency.overallFuelCostPerKm')}</th>
                          <th style={{ padding: '8px 6px' }}>Total</th>
                        </tr>
                      </thead>
                      <tbody>
                        {report.byFuelType.map((row) => (
                          <tr key={row.fuelType} style={{ borderBottom: '1px solid color-mix(in srgb, var(--border) 60%, transparent)' }}>
                            <td style={{ padding: '8px 6px' }}>{row.fuelType}</td>
                            <td style={{ padding: '8px 6px', whiteSpace: 'nowrap' }}>{row.totalLiters.toLocaleString()}</td>
                            <td style={{ padding: '8px 6px', whiteSpace: 'nowrap' }}>{row.attributedKm.toLocaleString()}</td>
                            <td style={{ padding: '8px 6px' }}>{row.averageKmPerLiter != null ? row.averageKmPerLiter.toLocaleString() : '—'}</td>
                            <td style={{ padding: '8px 6px' }}>{row.fuelCostPerKm != null ? brl(row.fuelCostPerKm) : '—'}</td>
                            <td style={{ padding: '8px 6px', whiteSpace: 'nowrap' }}>{brl(row.totalFuelSpend)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              ) : null}

              <details style={{ marginTop: 16 }}>
                <summary style={{ cursor: 'pointer', fontWeight: 600 }}>{t('fuelings:efficiency.intervalsHeading')}</summary>
                <ul style={{ listStyle: 'none', padding: 0, margin: '12px 0 0', display: 'grid', gap: 10 }}>
                  {report.intervals.map((seg) => (
                    <li
                      key={seg.segmentIndex}
                      style={{
                        padding: 10,
                        border: '1px solid color-mix(in srgb, var(--border) 70%, transparent)',
                        borderRadius: 10,
                        fontSize: 13,
                      }}
                    >
                      <div style={{ fontWeight: 700 }}>{t('fuelings:efficiency.seg', { n: seg.segmentIndex })}</div>
                      <div style={{ opacity: 0.88, marginTop: 6 }}>
                        {seg.startPerformedAt} @ {seg.startKmAtFueling.toLocaleString()} km → {seg.endPerformedAt} @{' '}
                        {seg.endKmAtFueling.toLocaleString()} km · Δ {seg.deltaKm.toLocaleString()} km
                      </div>
                      <div style={{ marginTop: 6 }}>
                        {seg.totalLitersInInterval.toLocaleString()} L · {brl(seg.totalFuelPriceInInterval)}
                        {' · '}
                        {seg.averageKmPerLiter != null ? `${seg.averageKmPerLiter.toLocaleString()} km/L` : '—'} · combustível{' '}
                        {seg.fuelCostPerKm != null ? `${brl(seg.fuelCostPerKm)}/km` : '—'}
                      </div>
                    </li>
                  ))}
                </ul>
              </details>
            </>
          )}
        </div>
      ) : null}
    </section>
  )
}
