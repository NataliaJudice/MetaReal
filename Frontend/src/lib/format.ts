const currencyFormatter = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const percentFormatter = new Intl.NumberFormat('pt-BR', {
  style: 'percent',
  minimumFractionDigits: 1,
  maximumFractionDigits: 1
})
const dateFormatter = new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' })

export const NOMES_MES = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
]

export const formatCurrency = (value: number) => currencyFormatter.format(value ?? 0)
export const formatPercent = (value: number) => percentFormatter.format(value ?? 0)
export const formatDate = (isoDate: string) => dateFormatter.format(new Date(isoDate))

export const formatMesAno = (mes: number, ano: number) => `${NOMES_MES[mes - 1]}/${ano}`

export const toInputDate = (isoDate: string) => isoDate.slice(0, 10)

export function toInputDateLocal(date: Date): string {
  const mes = String(date.getMonth() + 1).padStart(2, '0')
  const dia = String(date.getDate()).padStart(2, '0')
  return `${date.getFullYear()}-${mes}-${dia}`
}
export const todayInputDate = () => toInputDateLocal(new Date())
