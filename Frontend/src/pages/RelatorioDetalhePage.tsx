import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, FileSpreadsheet, FileText, TableProperties } from 'lucide-react'
import { api, extractErrorMessage } from '@/lib/api'
import { useAuth } from '@/context/AuthContext'
import { useToast } from '@/context/ToastContext'
import { Card, CardHeader } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { Select } from '@/components/ui/Select'
import { Spinner } from '@/components/ui/Spinner'
import { EmptyState } from '@/components/ui/EmptyState'
import { PeriodoFilter, type Periodo } from '@/components/ui/PeriodoFilter'
import { buscarRelatorio, CORES_RELATORIO } from '@/lib/relatorios'
import { formatCurrency, formatPercent } from '@/lib/format'
import type { ApiEnvelope, Relatorio, TipoColuna, Vendedor } from '@/types'

function formatarCelula(valor: unknown, tipo: TipoColuna): string {
  if (valor === null || valor === undefined || valor === '') return '—'

  switch (tipo) {
    case 'Moeda':
      return formatCurrency(Number(valor))
    case 'Percentual':
      return formatPercent(Number(valor))
    case 'Numero': {
      const n = Number(valor)
      return Number.isInteger(n) ? String(n) : n.toLocaleString('pt-BR', { maximumFractionDigits: 2 })
    }
    default:
      return String(valor)
  }
}

export function RelatorioDetalhePage() {
  const { chave } = useParams<{ chave: string }>()
  const navigate = useNavigate()
  const { showToast } = useToast()
  const { usuario } = useAuth()
  const ehGerente = usuario?.role === 'Gerente'

  const definicao = buscarRelatorio(chave)

  const [relatorio, setRelatorio] = useState<Relatorio | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [baixando, setBaixando] = useState<'excel' | 'pdf' | null>(null)

  const [periodo, setPeriodo] = useState<Periodo>({ dataInicio: '', dataFim: '' })
  const [vendedorId, setVendedorId] = useState('')
  const [agrupamento, setAgrupamento] = useState('dia')
  const [vendedores, setVendedores] = useState<Vendedor[]>([])

  const montarParams = useCallback(
    () => ({
      dataInicio: periodo.dataInicio || undefined,
      dataFim: periodo.dataFim || undefined,
      vendedorId: ehGerente && vendedorId ? vendedorId : undefined,
      agrupamento: definicao?.filtros.includes('agrupamento') ? agrupamento : undefined
    }),
    [periodo, vendedorId, agrupamento, ehGerente, definicao]
  )

  const carregar = useCallback(async () => {
    if (!chave) return
    setCarregando(true)
    try {
      const res = await api.get<ApiEnvelope<Relatorio>>(`/relatorios/${chave}`, { params: montarParams() })
      setRelatorio(res.data.data)
    } catch (err) {
      showToast(extractErrorMessage(err), 'error')
      setRelatorio(null)
    } finally {
      setCarregando(false)
    }
  }, [chave, montarParams, showToast])

  useEffect(() => {
    carregar()
  }, [carregar])

  useEffect(() => {
    if (ehGerente && definicao?.filtros.includes('vendedor')) {
      api
        .get<ApiEnvelope<Vendedor[]>>('/vendedores')
        .then((res) => setVendedores(res.data.data))
        .catch(() => setVendedores([]))
    }
  }, [ehGerente, definicao])

  const baixar = async (formato: 'excel' | 'pdf') => {
    if (!chave) return
    setBaixando(formato)
    try {
      const res = await api.get(`/relatorios/${chave}/${formato}`, {
        params: montarParams(),
        responseType: 'blob'
      })
      const extensao = formato === 'excel' ? 'xlsx' : 'pdf'
      const url = URL.createObjectURL(res.data as Blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `${chave}-${new Date().toISOString().slice(0, 10)}.${extensao}`
      document.body.appendChild(link)
      link.click()
      link.remove()
      URL.revokeObjectURL(url)
      showToast(`Download do ${formato === 'excel' ? 'Excel' : 'PDF'} iniciado.`, 'success')
    } catch (err) {
      showToast(extractErrorMessage(err), 'error')
    } finally {
      setBaixando(null)
    }
  }
  if (!definicao) {
    return (
      <EmptyState
        icon={TableProperties}
        title="Relatório não encontrado"
        description="O relatório que você tentou abrir não existe."
        action={
          <Button variant="secondary" className="mt-2" onClick={() => navigate('/relatorios')}>
            Ver relatórios
          </Button>
        }
      />
    )
  }
  const cores = CORES_RELATORIO[definicao.cor] ?? CORES_RELATORIO.indigo
  const Icone = definicao.icone

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div className="flex items-start gap-3">
          <button
            onClick={() => navigate('/relatorios')}
            className="mt-1 rounded-lg p-2 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-700"
            aria-label="Voltar"
          >
            <ArrowLeft className="h-5 w-5" />
          </button>
          <div className={`flex h-11 w-11 shrink-0 items-center justify-center rounded-xl ${cores.chip}`}>
            <Icone className={`h-5 w-5 ${cores.icone}`} />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">{definicao.nome}</h1>
            <p className="text-sm text-slate-500">{definicao.descricao}</p>
          </div>
        </div>
        <div className="flex shrink-0 gap-2">
          <Button
            variant="secondary"
            onClick={() => baixar('excel')}
            loading={baixando === 'excel'}
            disabled={baixando !== null || carregando}
          >
            <FileSpreadsheet className="h-4 w-4" /> Excel
          </Button>
          <Button
            onClick={() => baixar('pdf')}
            loading={baixando === 'pdf'}
            disabled={baixando !== null || carregando}
          >
            <FileText className="h-4 w-4" /> PDF
          </Button>
        </div>
      </div>
      <PeriodoFilter
        onChange={setPeriodo}
        action={
          <div className="flex flex-wrap items-center gap-2">
            {ehGerente && definicao.filtros.includes('vendedor') && (
              <Select value={vendedorId} onChange={(e) => setVendedorId(e.target.value)} className="w-52">
                <option value="">Todos os vendedores</option>
                {vendedores.map((v) => (
                  <option key={v.id} value={v.id}>
                    {v.nome}
                  </option>
                ))}
              </Select>
            )}
            {definicao.filtros.includes('agrupamento') && (
              <Select value={agrupamento} onChange={(e) => setAgrupamento(e.target.value)} className="w-40">
                <option value="dia">Por dia</option>
                <option value="mes">Por mês</option>
              </Select>
            )}
          </div>
        }
      />
      {carregando ? (
        <Spinner label="Gerando relatório…" />
      ) : !relatorio ? (
        <EmptyState icon={TableProperties} title="Não foi possível gerar o relatório" />
      ) : (
        <>
          {relatorio.resumo.length > 0 && (
            <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
              {relatorio.resumo.map((item) => (
                <div key={item.rotulo} className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                  <p className="text-sm font-medium text-slate-500">{item.rotulo}</p>
                  <p className="mt-2 text-xl font-bold tracking-tight text-slate-900">{item.valor}</p>
                </div>
              ))}
            </div>
          )}
          <Card>
            <CardHeader
              title={relatorio.titulo}
              action={<span className="text-xs font-medium text-slate-400">{relatorio.subtitulo}</span>}
            />
            {relatorio.linhas.length === 0 ? (
              <EmptyState
                icon={TableProperties}
                title="Nenhum dado no período"
                description="Ajuste os filtros acima para ampliar o intervalo."
              />
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-slate-100 text-xs font-semibold uppercase tracking-wide text-slate-400">
                      {relatorio.colunas.map((coluna) => (
                        <th
                          key={coluna.chave}
                          className={`px-5 py-3 ${coluna.tipo === 'Texto' ? 'text-left' : 'text-right'}`}
                        >
                          {coluna.titulo}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {relatorio.linhas.map((linha, i) => (
                      <tr key={i} className="hover:bg-slate-50">
                        {relatorio.colunas.map((coluna) => (
                          <td
                            key={coluna.chave}
                            className={`px-5 py-3.5 ${
                              coluna.tipo === 'Texto'
                                ? 'text-left font-medium text-slate-800'
                                : 'text-right tabular-nums text-slate-600'
                            }`}
                          >
                            {formatarCelula(linha[coluna.chave], coluna.tipo)}
                          </td>
                        ))}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </Card>
        </>
      )}
    </div>
  )
}
