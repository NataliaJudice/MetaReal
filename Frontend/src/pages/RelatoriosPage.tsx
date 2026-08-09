import { Link } from 'react-router-dom'
import { ArrowRight, FileSpreadsheet, FileText, Lightbulb } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'
import { CORES_RELATORIO, relatoriosDoPapel } from '@/lib/relatorios'

export function RelatoriosPage() {
  const { usuario } = useAuth()
  const relatorios = relatoriosDoPapel(usuario?.role)

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-indigo-100">
          <FileText className="h-5 w-5 text-indigo-600" />
        </div>
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Relatórios</h1>
          <p className="text-sm text-slate-500">
            Escolha um relatório, ajuste os filtros e exporte para Excel ou PDF.
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
        {relatorios.map((relatorio) => {
          const cores = CORES_RELATORIO[relatorio.cor] ?? CORES_RELATORIO.indigo
          const Icone = relatorio.icone

          return (
            <Link
              key={relatorio.chave}
              to={`/relatorios/${relatorio.chave}`}
              className={`group flex flex-col rounded-2xl border border-slate-200 bg-white p-5 shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md ${cores.hover}`}
            >
              <div className="flex items-start justify-between gap-3">
                <div className={`flex h-11 w-11 shrink-0 items-center justify-center rounded-xl ${cores.chip}`}>
                  <Icone className={`h-5 w-5 ${cores.icone}`} />
                </div>
                <ArrowRight className="h-4 w-4 shrink-0 text-slate-300 transition-transform group-hover:translate-x-1 group-hover:text-slate-500" />
              </div>

              <h2 className="mt-4 text-base font-bold text-slate-900">{relatorio.nome}</h2>
              <p className="mt-1 text-sm leading-relaxed text-slate-500">{relatorio.descricao}</p>

              <p className="mt-3 flex items-start gap-1.5 rounded-lg bg-slate-50 p-2.5 text-xs leading-relaxed text-slate-500">
                <Lightbulb className="mt-0.5 h-3.5 w-3.5 shrink-0 text-amber-500" />
                {relatorio.paraQue}
              </p>

              <div className="mt-4 flex items-center gap-3 border-t border-slate-100 pt-3 text-xs font-medium text-slate-400">
                <span className="inline-flex items-center gap-1">
                  <FileSpreadsheet className="h-3.5 w-3.5" /> Excel
                </span>
                <span className="inline-flex items-center gap-1">
                  <FileText className="h-3.5 w-3.5" /> PDF
                </span>
              </div>
            </Link>
          )
        })}
      </div>
    </div>
  )
}
