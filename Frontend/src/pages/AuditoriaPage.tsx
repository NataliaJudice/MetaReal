import { useCallback, useEffect, useState } from 'react'
import { ShieldCheck, History } from 'lucide-react'
import { api, extractErrorMessage } from '@/lib/api'
import { useToast } from '@/context/ToastContext'
import { Card, CardHeader } from '@/components/ui/Card'
import { Select } from '@/components/ui/Select'
import { Spinner } from '@/components/ui/Spinner'
import { EmptyState } from '@/components/ui/EmptyState'
import { Pagination } from '@/components/ui/Pagination'
import type { ApiEnvelope, AuditLogItem, Paginado } from '@/types'

function badgeCor(acao: string) {
  if (acao.includes('Criado')) return 'bg-emerald-50 text-emerald-700 border-emerald-200'
  if (acao.includes('Editado')) return 'bg-amber-50 text-amber-700 border-amber-200'
  if (acao.includes('Excluido')) return 'bg-red-50 text-red-700 border-red-200'
  return 'bg-indigo-50 text-indigo-700 border-indigo-200'
}

export function AuditoriaPage() {
  const { showToast } = useToast()
  const [itens, setItens] = useState<AuditLogItem[]>([])
  const [carregando, setCarregando] = useState(true)
  const [pagina, setPagina] = useState(1)
  const [totalPaginas, setTotalPaginas] = useState(1)
  const [entidade, setEntidade] = useState('')

  const carregar = useCallback(
    async (paginaAlvo: number, entidadeAlvo: string) => {
      setCarregando(true)
      try {
        const res = await api.get<ApiEnvelope<Paginado<AuditLogItem>>>('/auditoria', {
          params: { pagina: paginaAlvo, tamanhoPagina: 20, entidade: entidadeAlvo || undefined }
        })
        setItens(res.data.data.items)
        setTotalPaginas(res.data.data.totalPaginas)
        setPagina(res.data.data.paginaAtual)
      } catch (err) {
        showToast(extractErrorMessage(err), 'error')
      } finally {
        setCarregando(false)
      }
    },
    [showToast]
  )

  useEffect(() => {
    carregar(1, entidade)
  }, [carregar, entidade])

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-indigo-100">
          <ShieldCheck className="h-5 w-5 text-indigo-600" />
        </div>
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Auditoria</h1>
          <p className="text-sm text-slate-500">Trilha de ações críticas: quem alterou, o quê e quando.</p>
        </div>
      </div>

      <Card>
        <CardHeader
          title="Eventos registrados"
          action={
            <Select value={entidade} onChange={(e) => setEntidade(e.target.value)} className="w-48">
              <option value="">Todas as entidades</option>
              <option value="RegistroVenda">Registros de venda</option>
              <option value="Vendedor">Vendedores</option>
            </Select>
          }
        />

        {carregando ? (
          <Spinner />
        ) : itens.length === 0 ? (
          <EmptyState icon={History} title="Nenhum evento encontrado" />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs font-semibold uppercase tracking-wide text-slate-400">
                  <th className="px-5 py-3">Data/Hora</th>
                  <th className="px-5 py-3">Usuário</th>
                  <th className="px-5 py-3">Ação</th>
                  <th className="px-5 py-3">Detalhes</th>
                  <th className="px-5 py-3">IP</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {itens.map((item) => (
                  <tr key={item.id} className="hover:bg-slate-50">
                    <td className="whitespace-nowrap px-5 py-3.5 text-slate-500">
                      {new Date(item.dataHora).toLocaleString('pt-BR')}
                    </td>
                    <td className="px-5 py-3.5 font-medium text-slate-800">{item.usuarioNome ?? '—'}</td>
                    <td className="px-5 py-3.5">
                      <span className={`inline-flex rounded-full border px-2.5 py-1 text-xs font-semibold ${badgeCor(item.acao)}`}>
                        {item.acao}
                      </span>
                    </td>
                    <td className="px-5 py-3.5 text-slate-600">{item.detalhes ?? '—'}</td>
                    <td className="px-5 py-3.5 text-slate-400">{item.ip ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <Pagination paginaAtual={pagina} totalPaginas={totalPaginas} onChange={(p) => carregar(p, entidade)} />
      </Card>
    </div>
  )
}
