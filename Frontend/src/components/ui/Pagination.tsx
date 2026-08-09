import { ChevronLeft, ChevronRight } from 'lucide-react'

interface PaginationProps {
  paginaAtual: number
  totalPaginas: number
  onChange: (pagina: number) => void
}

export function Pagination({ paginaAtual, totalPaginas, onChange }: PaginationProps) {
  if (totalPaginas <= 1) return null

  const paginas = Array.from({ length: totalPaginas }, (_, i) => i + 1).filter(
    (p) => p === 1 || p === totalPaginas || Math.abs(p - paginaAtual) <= 1
  )

  return (
    <div className="flex items-center justify-center gap-1 py-4">
      <button
        onClick={() => onChange(paginaAtual - 1)}
        disabled={paginaAtual <= 1}
        className="rounded-lg p-2 text-slate-500 hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-40"
        aria-label="Página anterior"
      >
        <ChevronLeft className="h-4 w-4" />
      </button>

      {paginas.map((p, idx) => (
        <span key={p} className="flex items-center">
          {idx > 0 && paginas[idx - 1] !== p - 1 && <span className="px-1 text-slate-300">…</span>}
          <button
            onClick={() => onChange(p)}
            className={`h-8 w-8 rounded-lg text-sm font-medium transition-colors ${
              p === paginaAtual ? 'bg-indigo-600 text-white' : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            {p}
          </button>
        </span>
      ))}

      <button
        onClick={() => onChange(paginaAtual + 1)}
        disabled={paginaAtual >= totalPaginas}
        className="rounded-lg p-2 text-slate-500 hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-40"
        aria-label="Próxima página"
      >
        <ChevronRight className="h-4 w-4" />
      </button>
    </div>
  )
}
