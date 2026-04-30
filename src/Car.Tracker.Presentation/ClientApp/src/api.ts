export type CarDto = {
  id: string
  model: string
  year: number
  currentKm: number
  name: string | null
  placa: string | null
  createdAt: string
  updatedAt: string
}

export type CreateCarBody = {
  model?: string | null
  year?: number | null
  currentKm: number
  name?: string | null
  placa?: string | null
  autoBuscarDados?: boolean
}

export type ExpenseEntryType = 'Service' | 'Part'

export type ExpenseEntryDto = {
  id: string
  carId: string
  type: ExpenseEntryType
  title: string
  price: number
  supplierBrand: string | null
  productModel: string | null
  performedAt: string // yyyy-mm-dd
  kmAtService: number
  notes: string | null
}

export type MaintenancePlanItemDto = {
  id: string
  carId: string
  title: string
  dueKmInterval: number | null
  dueTimeIntervalDays: number | null
  active: boolean
}

export type ConsultaPlacaDto = {
  id: string
  carId: string
  status: string | null
  mensagem: string | null
  dataSolicitacao: string | null
  requestPlaca: string | null
  placa: string | null
  chassi: string | null
  anoFabricacao: string | null
  anoModelo: string | null
  marca: string | null
  modelo: string | null
  cor: string | null
  segmento: string | null
  combustivel: string | null
  procedencia: string | null
  municipio: string | null
  ufMunicipio: string | null
  tipoVeiculo: string | null
  subSegmento: string | null
  numeroMotor: string | null
  numeroCaixaCambio: string | null
  potencia: string | null
  cilindradas: string | null
  numeroEixos: string | null
  capacidadeMaximaTracao: string | null
  capacidadePassageiro: string | null
  createdAt: string
  updatedAt: string
}

export type ConsultaPrecoFipeItemDto = {
  id: string
  consultaPrecoFipeId: string
  codigoFipe: string | null
  modeloVersao: string | null
  preco: string | null
  mesReferencia: string | null
  historicoJson: string | null
  createdAt: string
  updatedAt: string
}

export type ConsultaPrecoFipeDto = {
  id: string
  carId: string
  status: string | null
  mensagem: string | null
  dataSolicitacao: string | null
  requestPlaca: string | null
  veiculoPlaca: string | null
  veiculoChassi: string | null
  veiculoAnoFabricacao: string | null
  veiculoAnoModelo: string | null
  veiculoMarca: string | null
  veiculoModelo: string | null
  veiculoCor: string | null
  veiculoSegmento: string | null
  veiculoCombustivel: string | null
  veiculoProcedencia: string | null
  veiculoMunicipio: string | null
  veiculoUfMunicipio: string | null
  tipoVeiculo: string | null
  subSegmento: string | null
  numeroMotor: string | null
  numeroCaixaCambio: string | null
  potencia: string | null
  cilindradas: string | null
  numeroEixos: string | null
  capacidadeMaximaTracao: string | null
  capacidadePassageiro: string | null
  pesoBrutoTotal: string | null
  createdAt: string
  updatedAt: string
  itens: ConsultaPrecoFipeItemDto[]
}

export type CarRegistryDto = {
  car: CarDto
  consultaPlaca: ConsultaPlacaDto | null
  consultaPrecoFipe: ConsultaPrecoFipeDto | null
  expenseEntries: ExpenseEntryDto[]
  maintenancePlanItems: MaintenancePlanItemDto[]
}

export type MaintenanceStatusDto = {
  planItemId: string
  title: string
  dueKmInterval: number | null
  dueTimeIntervalDays: number | null
  lastPerformedAt: string | null
  lastKmAtService: number | null
  nextDueDate: string | null
  nextDueKm: number | null
  overdueByTime: boolean
  overdueByKm: boolean
  overdue: boolean
}

export type FuelTypeDto = 'Gasolina' | 'Alcool' | 'Diesel' | 'KV'

export type FuelingEntryDto = {
  id: string
  carId: string
  performedAt: string // yyyy-mm-dd
  kmAtFueling: number
  liters: number
  totalPrice: number
  fuelType: FuelTypeDto
  /** Tanque completado neste lançamento (marca pontos para autonomia). */
  isFullTank: boolean
  stationName: string | null
  notes: string | null
}

export type CostPerKmReportDto = {
  carId: string
  mode: string
  periodVariant: string
  distanceReferenceVariant: string
  aggregatorLabel: string
  windowStartInclusive: string
  windowEndInclusive: string
  maintenanceExpenseTotal: number
  fuelExpenseTotal: number
  grandTotalExpense: number
  expenseEntryCountIncluded: number
  fuelingEntryCountIncluded: number
  minKmObservedInWindow: number | null
  maxKmObservedInWindow: number | null
  kmDelta: number | null
  costPerKm: number | null
  distanceReferenceKm: number | null
  estimatedCostAtDistanceReference: number | null
}

export type CostReportQuery = {
  basis: 'period' | 'lifetime'
  /** Quando basis=period — total | 1d | 1m | 6m | 1y */
  period: string
  /** total | km1 | km10 | km100 | km1000 */
  distanceRef: string
}

export type FuelTypeEfficiencyAggregateDto = {
  fuelType: FuelTypeDto
  totalLiters: number
  attributedKm: number
  averageKmPerLiter: number | null
  totalFuelSpend: number
  fuelCostPerKm: number | null
}

export type FuelFullTankIntervalDetailDto = {
  segmentIndex: number
  startPerformedAt: string
  startKmAtFueling: number
  endPerformedAt: string
  endKmAtFueling: number
  deltaKm: number
  totalLitersInInterval: number
  totalFuelPriceInInterval: number
  averageKmPerLiter: number | null
  fuelCostPerKm: number | null
}

export type FuelFullTankEfficiencyReportDto = {
  carId: string
  mode: string
  periodVariant: string
  aggregatorLabel: string
  windowStartInclusive: string
  windowEndInclusive: string
  fullTankMarkersInWindow: number
  intervalsUsedCount: number
  overallAverageKmPerLiter: number | null
  overallLitersPer100Km: number | null
  overallFuelCostPerKm: number | null
  byFuelType: FuelTypeEfficiencyAggregateDto[]
  intervals: FuelFullTankIntervalDetailDto[]
}

export type FuelFullTankReportQuery = {
  basis: 'period' | 'lifetime'
  /** Quando basis=period — total | 1d | 1m | 6m | 1y */
  period: string
}

async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(path, {
    headers: { 'content-type': 'application/json', ...(init?.headers ?? {}) },
    ...init,
  })

  if (!res.ok) {
    const text = await res.text().catch(() => '')
    throw new Error(text || `${res.status} ${res.statusText}`)
  }

  if (res.status === 204 || res.status === 205) {
    return (undefined as unknown) as T
  }

  const ct = res.headers.get('content-type') ?? ''
  if (!ct.includes('application/json')) {
    return (undefined as unknown) as T
  }
  return (await res.json()) as T
}

export const CarApi = {
  listCars: () => api<CarDto[]>('/api/cars'),
  createCar: (body: CreateCarBody) => api<CarDto>('/api/cars', { method: 'POST', body: JSON.stringify(body) }),
  getCar: (carId: string) => api<CarDto>(`/api/cars/${carId}`),
  getCarRegistry: (carId: string) => api<CarRegistryDto>(`/api/cars/${carId}/registry`),
  patchCar: (
    carId: string,
    body: { model?: string; year?: number; currentKm?: number; name?: string | null; placa?: string | null },
  ) => api<CarDto>(`/api/cars/${carId}`, { method: 'PATCH', body: JSON.stringify(body) }),
  deleteCar: (carId: string) => api<void>(`/api/cars/${carId}`, { method: 'DELETE' }),

  listFuelings: () => api<FuelingEntryDto[]>('/api/fuelings'),
  listFuelingsByCar: (carId: string) => api<FuelingEntryDto[]>(`/api/cars/${carId}/fuelings`),
  createFueling: (
    carId: string,
    body: Omit<FuelingEntryDto, 'id' | 'carId'>,
  ) => api<FuelingEntryDto>(`/api/cars/${carId}/fuelings`, { method: 'POST', body: JSON.stringify(body) }),
  patchFueling: (
    carId: string,
    fuelingId: string,
    body: Partial<Omit<FuelingEntryDto, 'id' | 'carId'>>,
  ) => api<FuelingEntryDto>(`/api/cars/${carId}/fuelings/${fuelingId}`, { method: 'PATCH', body: JSON.stringify(body) }),
  deleteFueling: (carId: string, fuelingId: string) =>
    api<void>(`/api/cars/${carId}/fuelings/${fuelingId}`, { method: 'DELETE' }),

  getCostPerKmReport: (carId: string, query: CostReportQuery) =>
    api<CostPerKmReportDto>(
      `/api/cars/${encodeURIComponent(carId)}/reports/cost-per-km?basis=${encodeURIComponent(query.basis)}&period=${encodeURIComponent(query.period)}&distanceRef=${encodeURIComponent(query.distanceRef)}`,
    ),

  getFuelFullTankReport: (carId: string, query: FuelFullTankReportQuery) =>
    api<FuelFullTankEfficiencyReportDto>(
      `/api/cars/${encodeURIComponent(carId)}/reports/fuel-full-tank?basis=${encodeURIComponent(query.basis)}&period=${encodeURIComponent(query.period)}`,
    ),

  listEntries: (carId: string) => api<ExpenseEntryDto[]>(`/api/cars/${carId}/entries`),
  createEntry: (carId: string, body: Omit<ExpenseEntryDto, 'id' | 'carId'>) =>
    api<ExpenseEntryDto>(`/api/cars/${carId}/entries`, { method: 'POST', body: JSON.stringify(body) }),
  patchEntry: (
    carId: string,
    entryId: string,
    body: Partial<Pick<ExpenseEntryDto, 'type' | 'title' | 'price' | 'supplierBrand' | 'productModel' | 'performedAt' | 'kmAtService' | 'notes'>>,
  ) => api<ExpenseEntryDto>(`/api/cars/${carId}/entries/${entryId}`, { method: 'PATCH', body: JSON.stringify(body) }),
  deleteEntry: (carId: string, entryId: string) =>
    api<void>(`/api/cars/${carId}/entries/${entryId}`, { method: 'DELETE' }),

  listPlans: (carId: string) => api<MaintenancePlanItemDto[]>(`/api/cars/${carId}/maintenance-plans`),
  createPlan: (
    carId: string,
    body: { title: string; dueKmInterval?: number | null; dueTimeIntervalDays?: number | null; active: boolean },
  ) => api<MaintenancePlanItemDto>(`/api/cars/${carId}/maintenance-plans`, { method: 'POST', body: JSON.stringify(body) }),
  patchPlan: (
    carId: string,
    planId: string,
    body: Partial<Pick<MaintenancePlanItemDto, 'title' | 'dueKmInterval' | 'dueTimeIntervalDays' | 'active'>>,
  ) => api<MaintenancePlanItemDto>(`/api/cars/${carId}/maintenance-plans/${planId}`, { method: 'PATCH', body: JSON.stringify(body) }),
  deletePlan: (carId: string, planId: string) =>
    api<void>(`/api/cars/${carId}/maintenance-plans/${planId}`, { method: 'DELETE' }),
  getStatus: (carId: string) => api<MaintenanceStatusDto[]>(`/api/cars/${carId}/maintenance-status`),
}

