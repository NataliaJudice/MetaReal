import { Trophy, Rocket, Flame, Target, Sparkles } from 'lucide-react'
import { formatCurrency, NOMES_MES } from '@/lib/format'
import { Button } from '@/components/ui/Button'
import type { MetaProgresso } from '@/types'

interface Tier {
  cor: string
  trilho: string
  texto: string
  badge: string
  icon: typeof Target
  mensagem: string
}

function obterTier(percentual: number): Tier {
  if (percentual >= 1) {
    return {
      cor: 'var(--color-emerald-500)',
      trilho: 'var(--color-emerald-100)',
      texto: 'text-emerald-700',
      badge: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
      icon: Trophy,
      mensagem: percentual >= 1.5 ? 'Show de bola! Meta estourada 🎉' : 'Meta batida! Parabéns 🏆'
    }
  }
  if (percentual >= 0.75) {
    return {
      cor: 'var(--color-indigo-600)',
      trilho: 'var(--color-indigo-100)',
      texto: 'text-indigo-700',
      badge: 'bg-indigo-50 text-indigo-700 ring-indigo-200',
      icon: Flame,
      mensagem: 'Reta final, quase lá!'
    }
  }
  if (percentual >= 0.4) {
    return {
      cor: 'var(--color-amber-500)',
      trilho: 'var(--color-amber-100)',
      texto: 'text-amber-700',
      badge: 'bg-amber-50 text-amber-700 ring-amber-200',
      icon: Rocket,
      mensagem: 'No embalo — continue assim!'
    }
  }
  return {
    cor: 'var(--color-slate-400)',
    trilho: 'var(--color-slate-100)',
    texto: 'text-slate-600',
    badge: 'bg-slate-100 text-slate-600 ring-slate-200',
    icon: Target,
    mensagem: 'Bora começar a bater essa meta!'
  }
}

function Anel({
  percentual,
  tamanho,
  espessura,
  cor,
  trilho
}: {
  percentual: number
  tamanho: number
  espessura: number
  cor: string
  trilho: string
}) {
  const raio = (tamanho - espessura) / 2
  const circunferencia = 2 * Math.PI * raio
  const preenchido = Math.min(percentual, 1)
  const offset = circunferencia * (1 - preenchido)

  return (
    <svg width={tamanho} height={tamanho} viewBox={`0 0 ${tamanho} ${tamanho}`} className="-rotate-90">
      <circle cx={tamanho / 2} cy={tamanho / 2} r={raio} fill="none" stroke={trilho} strokeWidth={espessura} />
      <circle
        cx={tamanho / 2}
        cy={tamanho / 2}
        r={raio}
        fill="none"
        stroke={cor}
        strokeWidth={espessura}
        strokeLinecap="round"
        strokeDasharray={circunferencia}
        strokeDashoffset={offset}
        style={{ transition: 'stroke-dashoffset 0.6s ease, stroke 0.3s ease' }}
      />
    </svg>
  )
}

interface MetaProgressCardProps {
  progresso: MetaProgresso | null
  variant?: 'full' | 'compact'
  onDefinirMeta?: () => void
}

export function MetaProgressCard({ progresso, variant = 'full', onDefinirMeta }: MetaProgressCardProps) {
  const semMeta = !progresso || progresso.valorMeta <= 0
  const percentual = progresso?.percentualConcluido ?? 0
  const tier = obterTier(semMeta ? 0 : percentual)
  const Icon = tier.icon
  const nomeMes = progresso ? NOMES_MES[progresso.mes - 1] : ''
  const restante = progresso ? Math.max(progresso.valorMeta - progresso.valorAtual, 0) : 0

  if (variant === 'compact') {
    return (
      <div className="flex items-center gap-4 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm transition-shadow hover:shadow-md">
        <div className="relative shrink-0">
          <Anel percentual={semMeta ? 0 : percentual} tamanho={64} espessura={7} cor={tier.cor} trilho={tier.trilho} />
          <div className="absolute inset-0 flex items-center justify-center">
            <Icon className="h-5 w-5" style={{ color: tier.cor }} />
          </div>
        </div>
        <div className="min-w-0 flex-1">
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Meta de {nomeMes || 'do mês'}</p>
          {semMeta ? (
            <p className="text-sm font-medium text-slate-500">Nenhuma meta definida ainda.</p>
          ) : (
            <>
              <p className="truncate text-sm font-bold text-slate-900">
                {formatCurrency(progresso!.valorAtual)}{' '}
                <span className="font-normal text-slate-400">de {formatCurrency(progresso!.valorMeta)}</span>
              </p>
              <p className={`text-xs font-semibold ${tier.texto}`}>{(percentual * 100).toFixed(0)}% concluído</p>
            </>
          )}
        </div>
        {onDefinirMeta && (
          <Button size="sm" variant="secondary" onClick={onDefinirMeta}>
            {semMeta ? 'Definir meta' : 'Editar'}
          </Button>
        )}
      </div>
    )
  }

  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm transition-shadow hover:shadow-md">
      <div className="flex flex-col items-center gap-5 text-center">
        <div className="relative">
          <Anel percentual={semMeta ? 0 : percentual} tamanho={148} espessura={12} cor={tier.cor} trilho={tier.trilho} />
          <div className="absolute inset-0 flex flex-col items-center justify-center">
            <Icon className="mb-1 h-6 w-6" style={{ color: tier.cor }} />
            <span className="text-3xl font-extrabold tracking-tight text-slate-900">
              {semMeta ? '—' : `${(percentual * 100).toFixed(0)}%`}
            </span>
          </div>
        </div>

        <div className="w-full">
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Meta de {nomeMes || 'do mês'}</p>

          {semMeta ? (
            <>
              <p className="mt-1 text-base font-bold text-slate-700">Nenhuma meta definida.</p>
              <p className="mt-1 text-sm text-slate-400">
                Assim que o Gerente definir, o progresso aparece aqui.
              </p>
            </>
          ) : (
            <>
              <p
                className={`mt-2 inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-sm font-bold ring-1 ring-inset ${tier.badge}`}
              >
                <Sparkles className="h-3.5 w-3.5" />
                {tier.mensagem}
              </p>
              <p className="mt-3 text-2xl font-extrabold text-slate-900">{formatCurrency(progresso!.valorAtual)}</p>
              <p className="text-sm font-medium text-slate-400">de {formatCurrency(progresso!.valorMeta)}</p>
              {restante > 0 && (
                <p className="mt-3 border-t border-slate-100 pt-3 text-sm text-slate-500">
                  Faltam <span className="font-bold text-slate-700">{formatCurrency(restante)}</span> para bater a meta
                </p>
              )}
            </>
          )}

          {onDefinirMeta && (
            <Button size="sm" variant="secondary" onClick={onDefinirMeta} className="mt-4">
              {semMeta ? 'Definir meta' : 'Editar meta'}
            </Button>
          )}
        </div>
      </div>
    </div>
  )
}
