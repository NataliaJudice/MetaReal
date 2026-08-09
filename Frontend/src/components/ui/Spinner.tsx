import { Loader2 } from 'lucide-react'

export function Spinner({ label = 'Carregando…' }: { label?: string }) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 py-16 text-slate-400">
      <Loader2 className="h-8 w-8 animate-spin" />
      <p className="text-sm font-medium">{label}</p>
    </div>
  )
}

export function FullScreenSpinner() {
  return (
    <div className="flex h-screen w-full items-center justify-center bg-slate-50">
      <Spinner label="Carregando aplicação…" />
    </div>
  )
}
