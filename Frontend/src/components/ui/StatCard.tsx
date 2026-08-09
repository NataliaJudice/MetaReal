import type { LucideIcon } from 'lucide-react'

export type StatTone = 'neutral' | 'primary' | 'success' | 'warning'

const iconTones: Record<StatTone, string> = {
  neutral: 'text-slate-400',
  primary: 'text-indigo-500',
  success: 'text-emerald-500',
  warning: 'text-amber-500'
}

interface StatCardProps {
  label: string
  value: string
  icon: LucideIcon
  highlight?: boolean
  hint?: string
  tone?: StatTone
}

export function StatCard({ label, value, icon: Icon, highlight, hint, tone = 'neutral' }: StatCardProps) {
  return (
    <div
      className={`rounded-2xl border p-5 shadow-sm transition-shadow hover:shadow-md ${
        highlight
          ? 'border-indigo-500 bg-gradient-to-br from-indigo-600 to-indigo-500 text-white shadow-indigo-600/10'
          : 'border-slate-200 bg-white text-slate-900'
      }`}
    >
      <div className="flex items-center justify-between gap-2">
        <span className={`text-sm font-medium ${highlight ? 'text-indigo-100' : 'text-slate-500'}`}>{label}</span>
        <Icon className={`h-5 w-5 shrink-0 ${highlight ? 'text-indigo-100' : iconTones[tone]}`} />
      </div>
      <p className="mt-2 text-2xl font-bold tracking-tight">{value}</p>
      {hint && <p className={`mt-1 text-xs ${highlight ? 'text-indigo-100' : 'text-slate-400'}`}>{hint}</p>}
    </div>
  )
}
