import { ChevronLeft, ChevronRight, CalendarDays } from 'lucide-react'
import { NOMES_MES } from '@/lib/format'

interface MonthNavigatorProps {
  mes: number
  ano: number
  onChange: (mes: number, ano: number) => void
  limitarFuturo?: boolean
}

export function MonthNavigator({ mes, ano, onChange, limitarFuturo = true }: MonthNavigatorProps) {
  const hoje = new Date()
  const noMesAtual = mes === hoje.getMonth() + 1 && ano === hoje.getFullYear()
  const proximoBloqueado = limitarFuturo && noMesAtual

  const mover = (delta: number) => {
    const referencia = new Date(ano, mes - 1 + delta, 1)
    onChange(referencia.getMonth() + 1, referencia.getFullYear())
  }

  return (
    <div className="inline-flex items-center gap-1 rounded-xl border border-slate-200 bg-white p-1 shadow-sm">
      <button
        onClick={() => mover(-1)}
        className="rounded-lg p-2 text-slate-500 transition-colors hover:bg-slate-100 hover:text-slate-700"
        aria-label="Mês anterior"
      >
        <ChevronLeft className="h-4 w-4" />
      </button>

      <span className="flex min-w-[10rem] items-center justify-center gap-2 px-2 text-sm font-semibold text-slate-900">
        <CalendarDays className="h-4 w-4 text-slate-400" />
        {NOMES_MES[mes - 1]} {ano}
      </span>

      <button
        onClick={() => mover(1)}
        disabled={proximoBloqueado}
        className="rounded-lg p-2 text-slate-500 transition-colors hover:bg-slate-100 hover:text-slate-700 disabled:cursor-not-allowed disabled:opacity-30 disabled:hover:bg-transparent"
        aria-label="Próximo mês"
      >
        <ChevronRight className="h-4 w-4" />
      </button>
    </div>
  )
}
