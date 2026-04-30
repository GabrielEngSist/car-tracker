/** Alinha com `PlacaBrasil` no backend (antiga, Mercosul 7 ou 8). */

export function normalizarPlaca(placa: string): string {
  return placa.trim().replace(/-/g, '').toUpperCase()
}

const RE_ANTIGA = /^[A-Z]{3}\d{4}$/
const RE_MERCOSUL7 = /^[A-Z]{3}\d[A-Z0-9]\d{2}$/
const RE_MERCOSUL8 = /^[A-Z]{3}\d{2}[A-Z]\d{2}$/

export function ehPlacaValida(normalizada: string): boolean {
  if (!normalizada || normalizada.length < 7 || normalizada.length > 8) return false
  return RE_ANTIGA.test(normalizada) || RE_MERCOSUL7.test(normalizada) || RE_MERCOSUL8.test(normalizada)
}
