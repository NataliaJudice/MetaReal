export type ProgressTone = 'neutral' | 'warning' | 'primary' | 'success'

const tones: Record<ProgressTone, string> = {
  neutral: 'bg-slate-400',
  warning: 'bg-amber-500',
  primary: 'bg-indigo-600',
  success: 'bg-emerald-500'
}

export function toneParaPercentual(percentual: number): ProgressTone {
  if (percentual >= 1) return 'success'
  if (percentual >= 0.75) return 'primary'
  if (percentual >= 0.4) return 'warning'
  return 'neutral'
}

export function ProgressBar({
  percentual,
  tone,
  className = ''
}: {
  percentual: number
  tone?: ProgressTone
  className?: string
}) {
  const preenchido = Math.min(Math.max(percentual, 0), 1)
  const cor = tones[tone ?? toneParaPercentual(percentual)]

  return (
    <div
      className={`h-2 w-full overflow-hidden rounded-full bg-slate-100 ${className}`}
      role="progressbar"
      aria-valuenow={Math.round(percentual * 100)}
      aria-valuemin={0}
      aria-valuemax={100}
    >
      <div
        className={`h-full rounded-full transition-[width,background-color] duration-500 ease-out ${cor}`}
        style={{ width: `${preenchido * 100}%` }}
      />
    </div>
  )
}
