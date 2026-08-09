import { useState, type ReactNode } from 'react'
import { CalendarRange } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { toInputDateLocal } from '@/lib/format'

export interface Periodo {
  dataInicio: string
  dataFim: string
}

type PresetId = 'tudo' | 'esteMes' | 'mesPassado' | 'ultimos30' | 'esteAno' | 'personalizado'

interface Preset {
  id: PresetId
  label: string
  calcular?: () => Periodo
}

const PRESETS: Preset[] = [
  { id: 'tudo', label: 'Tudo', calcular: () => ({ dataInicio: '', dataFim: '' }) },
  {
    id: 'esteMes',
    label: 'Este mês',
    calcular: () => {
      const hoje = new Date()
      return {
        dataInicio: toInputDateLocal(new Date(hoje.getFullYear(), hoje.getMonth(), 1)),
        dataFim: toInputDateLocal(new Date(hoje.getFullYear(), hoje.getMonth() + 1, 0))
      }
    }
  },
  {
    id: 'mesPassado',
    label: 'Mês passado',
    calcular: () => {
      const hoje = new Date()
      return {
        dataInicio: toInputDateLocal(new Date(hoje.getFullYear(), hoje.getMonth() - 1, 1)),
        dataFim: toInputDateLocal(new Date(hoje.getFullYear(), hoje.getMonth(), 0))
      }
    }
  },
  {
    id: 'ultimos30',
    label: 'Últimos 30 dias',
    calcular: () => {
      const hoje = new Date()
      const inicio = new Date(hoje)
      inicio.setDate(inicio.getDate() - 29)
      return { dataInicio: toInputDateLocal(inicio), dataFim: toInputDateLocal(hoje) }
    }
  },
  {
    id: 'esteAno',
    label: 'Este ano',
    calcular: () => {
      const hoje = new Date()
      return {
        dataInicio: toInputDateLocal(new Date(hoje.getFullYear(), 0, 1)),
        dataFim: toInputDateLocal(new Date(hoje.getFullYear(), 11, 31))
      }
    }
  },
  { id: 'personalizado', label: 'Personalizado' }
]

interface PeriodoFilterProps {
  onChange: (periodo: Periodo) => void
  action?: ReactNode
}

export function PeriodoFilter({ onChange, action }: PeriodoFilterProps) {
  const [presetAtivo, setPresetAtivo] = useState<PresetId>('tudo')
  const [dataInicio, setDataInicio] = useState('')
  const [dataFim, setDataFim] = useState('')
  const [erro, setErro] = useState<string | null>(null)
  const selecionarPreset = (preset: Preset) => {
    setPresetAtivo(preset.id)
    setErro(null)
    if (!preset.calcular) {
      return
    }
    const periodo = preset.calcular()
    setDataInicio(periodo.dataInicio)
    setDataFim(periodo.dataFim)
    onChange(periodo)
  }
  const aplicarPersonalizado = () => {
    if (dataInicio && dataFim && dataInicio > dataFim) {
      setErro('A data de início não pode ser depois da data de fim.')
      return
    }
    setErro(null)
    onChange({ dataInicio, dataFim })
  }
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex flex-wrap items-center gap-1.5">
          <CalendarRange className="mr-1 h-4 w-4 shrink-0 text-slate-400" />
          {PRESETS.map((preset) => (
            <button
              key={preset.id}
              type="button"
              onClick={() => selecionarPreset(preset)}
              className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
                presetAtivo === preset.id
                  ? 'bg-indigo-600 text-white shadow-sm shadow-indigo-600/20'
                  : 'text-slate-600 hover:bg-slate-100'
              }`}
            >
              {preset.label}
            </button>
          ))}
        </div>
        {action}
      </div>
      {presetAtivo === 'personalizado' && (
        <div className="mt-4 flex flex-col gap-3 border-t border-slate-100 pt-4 sm:flex-row sm:items-end">
          <div className="flex flex-col gap-1.5">
            <label htmlFor="periodo-inicio" className="text-sm font-medium text-slate-700">
              Data início
            </label>
            <input
              id="periodo-inicio"
              type="date"
              value={dataInicio}
              max={dataFim || undefined}
              onChange={(e) => {
                setDataInicio(e.target.value)
                setErro(null)
              }}
              className="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm text-slate-900 shadow-sm transition-colors focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/40 sm:w-44"
            />
          </div>
          <div className="flex flex-col gap-1.5">
            <label htmlFor="periodo-fim" className="text-sm font-medium text-slate-700">
              Data fim
            </label>
            <input
              id="periodo-fim"
              type="date"
              value={dataFim}
              min={dataInicio || undefined}
              onChange={(e) => {
                setDataFim(e.target.value)
                setErro(null)
              }}
              className="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm text-slate-900 shadow-sm transition-colors focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/40 sm:w-44"
            />
          </div>
          <Button type="button" size="md" onClick={aplicarPersonalizado}>
            Aplicar
          </Button>
        </div>
      )}
      {erro && <p className="mt-2 text-xs font-medium text-red-600">{erro}</p>}
    </div>
  )
}
