import { Link } from 'react-router-dom'
import { Compass } from 'lucide-react'

export function NotFoundPage() {
  return (
    <div className="flex h-screen flex-col items-center justify-center gap-4 bg-slate-50 text-center px-6">
      <div className="flex h-14 w-14 items-center justify-center rounded-full bg-indigo-100">
        <Compass className="h-7 w-7 text-indigo-600" />
      </div>
      <h1 className="text-2xl font-bold text-slate-900">Página não encontrada</h1>
      <p className="text-sm text-slate-500">O endereço acessado não existe ou foi movido.</p>
      <Link to="/" className="rounded-xl bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-indigo-500">
        Voltar ao início
      </Link>
    </div>
  )
}
