import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { useSearchParams } from 'react-router-dom'
import { NotebookPen, Pencil, Trash2, ClipboardList, Plus, TrendingUp, DollarSign, ShoppingBag, Users } from 'lucide-react'
import { api, extractErrorMessage } from '@/lib/api'
import { useAuth } from '@/context/AuthContext'
import { useToast } from '@/context/ToastContext'
import { Card, CardHeader } from '@/components/ui/Card'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { Button } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'
import { EmptyState } from '@/components/ui/EmptyState'
import { Pagination } from '@/components/ui/Pagination'
import { StatCard } from '@/components/ui/StatCard'
import { Modal } from '@/components/ui/Modal'
import { PeriodoFilter, type Periodo } from '@/components/ui/PeriodoFilter'
import { MetaProgressCard } from '@/components/MetaProgressCard'
import { formatCurrency, formatDate, formatPercent, todayInputDate } from '@/lib/format'
import type { ApiEnvelope, MetaProgresso, Paginado, RegistroVenda, RegistroVendaInput, Vendedor } from '@/types'

const DRAFT_KEY = 'metareal.lancamento.rascunho'

interface FormState {
  data: string
  vendedorId: string
  pretasMistas: string
  garantia: string
  crediarioDujuca: string
  quantAtendimento: string
  numVendas: string
  valorTotalVendas: string
}

function estadoInicial(vendedorId: string): FormState {
  return {
    data: todayInputDate(),
    vendedorId,
    pretasMistas: '0',
    garantia: '0',
    crediarioDujuca: '0',
    quantAtendimento: '0',
    numVendas: '0',
    valorTotalVendas: '0'
  }
}

function carregarRascunho(): FormState | null {
  try {
    const bruto = localStorage.getItem(DRAFT_KEY)
    return bruto ? (JSON.parse(bruto) as FormState) : null
  } catch {
    return null
  }
}

export function LancamentoPage() {
  const { usuario } = useAuth()
  const { showToast } = useToast()
  const ehGerente = usuario?.role === 'Gerente'
  const [searchParams] = useSearchParams()
  const vendedorIdPreSelecionado = searchParams.get('vendedorId')

  const [vendedores, setVendedores] = useState<Vendedor[]>([])
  const [form, setForm] = useState<FormState>(() => {
    if (ehGerente && vendedorIdPreSelecionado) {
      return estadoInicial(vendedorIdPreSelecionado)
    }
    return carregarRascunho() ?? estadoInicial(usuario?.vendedorId ?? '')
  })
  const [editandoId, setEditandoId] = useState<string | null>(null)
  const [modalAberta, setModalAberta] = useState(false)
  const [salvando, setSalvando] = useState(false)
  const [erros, setErros] = useState<Record<string, string>>({})

  const [registros, setRegistros] = useState<RegistroVenda[]>([])
  const [carregandoLista, setCarregandoLista] = useState(true)
  const [pagina, setPagina] = useState(1)
  const [totalPaginas, setTotalPaginas] = useState(1)
  const [filtroVendedor, setFiltroVendedor] = useState(ehGerente ? vendedorIdPreSelecionado ?? '' : '')
  const [totais, setTotais] = useState({ valorTotal: 0, totalVendas: 0, totalAtendimentos: 0, conversaoMedia: 0 })
  const [periodo, setPeriodo] = useState<Periodo>({ dataInicio: '', dataFim: '' })

  const vendedorParaMeta = ehGerente ? form.vendedorId : usuario?.vendedorId ?? ''
  const [progressoMeta, setProgressoMeta] = useState<MetaProgresso | null>(null)

  // salva rascunho enquanto digita, só pra lançamento novo. se a internet cair no meio
  // a vendedora não perde o que já tinha preenchido. na edição não faz sentido
  useEffect(() => {
    if (!editandoId) {
      localStorage.setItem(DRAFT_KEY, JSON.stringify(form))
    }
  }, [form, editandoId])

  useEffect(() => {
    if (ehGerente) {
      api
        .get<ApiEnvelope<Vendedor[]>>('/vendedores')
        .then((res) => setVendedores(res.data.data))
        .catch((err) => showToast(extractErrorMessage(err), 'error'))
    }
  }, [ehGerente, showToast])

  const carregarRegistros = useCallback(
    async (paginaAlvo: number, vendedorId: string, periodoAlvo: Periodo = periodo) => {
      setCarregandoLista(true)
      try {
        const res = await api.get<ApiEnvelope<Paginado<RegistroVenda>>>('/vendas', {
          params: {
            pagina: paginaAlvo,
            tamanhoPagina: 8,
            vendedorId: vendedorId || undefined,
            dataInicio: periodoAlvo.dataInicio || undefined,
            dataFim: periodoAlvo.dataFim || undefined
          }
        })
        setRegistros(res.data.data.items)
        setTotalPaginas(res.data.data.totalPaginas)
        setPagina(res.data.data.paginaAtual)
        setTotais({
          valorTotal: res.data.data.valorTotal ?? 0,
          totalVendas: res.data.data.totalVendas ?? 0,
          totalAtendimentos: res.data.data.totalAtendimentos ?? 0,
          conversaoMedia: res.data.data.conversaoMedia ?? 0
        })
      } catch (err) {
        showToast(extractErrorMessage(err), 'error')
      } finally {
        setCarregandoLista(false)
      }
    },
    [showToast, periodo]
  )

  useEffect(() => {
    carregarRegistros(1, filtroVendedor)
  }, [carregarRegistros, filtroVendedor])

  useEffect(() => {
    if (!vendedorParaMeta) {
      setProgressoMeta(null)
      return
    }
    api
      .get<ApiEnvelope<MetaProgresso>>(`/metas/vendedor/${vendedorParaMeta}`)
      .then((res) => setProgressoMeta(res.data.data))
      .catch(() => setProgressoMeta(null))
  }, [vendedorParaMeta])

  const atualizarCampo = (campo: keyof FormState, valor: string) => {
    setForm((prev) => ({ ...prev, [campo]: valor }))
    setErros((prev) => ({ ...prev, [campo]: '' }))
  }

  const fecharModal = () => {
    setModalAberta(false)
    setEditandoId(null)
    setForm(estadoInicial(usuario?.vendedorId ?? ''))
  }

  const abrirModalNovo = () => {
    setEditandoId(null)
    setForm(carregarRascunho() ?? estadoInicial(usuario?.vendedorId ?? ''))
    setModalAberta(true)
  }

  const iniciarEdicao = (registro: RegistroVenda) => {
    setEditandoId(registro.id)
    setForm({
      data: registro.data.slice(0, 10),
      vendedorId: registro.vendedorId,
      pretasMistas: String(registro.pretasMistas),
      garantia: String(registro.garantia),
      crediarioDujuca: String(registro.crediarioDujuca),
      quantAtendimento: String(registro.quantAtendimento),
      numVendas: String(registro.numVendas),
      valorTotalVendas: String(registro.valorTotalVendas)
    })
    setModalAberta(true)
  }

  const validarLocalmente = (payload: RegistroVendaInput): boolean => {
    const novosErros: Record<string, string> = {}
    if (!payload.vendedorId) novosErros.vendedorId = 'Selecione um vendedor.'
    if (!payload.data) novosErros.data = 'Informe a data.'
    if (payload.numVendas > payload.quantAtendimento) {
      novosErros.numVendas = 'Não pode ser maior que a quantidade de atendimentos.'
    }
    for (const [campo, valor] of Object.entries(payload)) {
      if (typeof valor === 'number' && valor < 0) {
        novosErros[campo] = 'O valor não pode ser negativo.'
      }
    }
    setErros(novosErros)
    return Object.keys(novosErros).length === 0
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()

    const payload: RegistroVendaInput = {
      data: form.data,
      vendedorId: ehGerente ? form.vendedorId : usuario?.vendedorId ?? '',
      pretasMistas: Number(form.pretasMistas) || 0,
      garantia: Number(form.garantia) || 0,
      crediarioDujuca: Number(form.crediarioDujuca) || 0,
      quantAtendimento: Number(form.quantAtendimento) || 0,
      numVendas: Number(form.numVendas) || 0,
      valorTotalVendas: Number(form.valorTotalVendas) || 0
    }

    if (!validarLocalmente(payload)) {
      return
    }

    setSalvando(true)
    try {
      if (editandoId) {
        await api.put(`/vendas/${editandoId}`, payload)
        showToast('Lançamento atualizado com sucesso.', 'success')
      } else {
        await api.post('/vendas', payload)
        showToast('Lançamento registrado com sucesso.', 'success')
        localStorage.removeItem(DRAFT_KEY)
      }
      fecharModal()
      carregarRegistros(editandoId ? pagina : 1, filtroVendedor)
    } catch (err) {
      showToast(extractErrorMessage(err), 'error')
    } finally {
      setSalvando(false)
    }
  }

  const excluir = async (id: string) => {
    if (!confirm('Excluir este lançamento? Essa ação não pode ser desfeita.')) return
    try {
      await api.delete(`/vendas/${id}`)
      showToast('Lançamento excluído.', 'success')
      carregarRegistros(pagina, filtroVendedor)
    } catch (err) {
      showToast(extractErrorMessage(err), 'error')
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Lançamento diário</h1>
          <p className="text-sm text-slate-500">Registre atendimentos, vendas, garantias e crediário do dia.</p>
        </div>
        <Button onClick={abrirModalNovo}>
          <Plus className="h-4 w-4 mr-2" /> Novo lançamento
        </Button>
      </div>

      <PeriodoFilter
        onChange={(novoPeriodo) => {
          setPeriodo(novoPeriodo)
          carregarRegistros(1, filtroVendedor, novoPeriodo)
        }}
      />

      {vendedorParaMeta && <MetaProgressCard progresso={progressoMeta} variant="compact" />}

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard label="Valor Total" value={formatCurrency(totais.valorTotal)} icon={DollarSign} tone="success" />
        <StatCard label="Total de Vendas" value={String(totais.totalVendas)} icon={ShoppingBag} tone="primary" />
        <StatCard label="Atendimentos" value={String(totais.totalAtendimentos)} icon={Users} />
        <StatCard label="Conv. Média" value={formatPercent(totais.conversaoMedia)} icon={TrendingUp} tone="warning" />
      </div>

      <Card>
        <CardHeader
          title="Lançamentos"
          action={
            ehGerente ? (
              <Select value={filtroVendedor} onChange={(e) => setFiltroVendedor(e.target.value)} className="w-48">
                <option value="">Todos os vendedores</option>
                {vendedores.map((v) => (
                  <option key={v.id} value={v.id}>
                    {v.nome}
                  </option>
                ))}
              </Select>
            ) : undefined
          }
        />

        {carregandoLista ? (
          <Spinner />
        ) : registros.length === 0 ? (
          <EmptyState icon={ClipboardList} title="Nenhum lançamento encontrado" description="Clique em 'Novo lançamento' acima para começar." />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs font-semibold uppercase tracking-wide text-slate-400">
                  <th className="px-5 py-3">Data</th>
                  {ehGerente && <th className="px-5 py-3">Vendedor</th>}
                  <th className="px-5 py-3">Atend.</th>
                  <th className="px-5 py-3">Vendas</th>
                  <th className="px-5 py-3">Conversão</th>
                  <th className="px-5 py-3">Valor Total</th>
                  <th className="px-5 py-3">Garantia</th>
                  <th className="px-5 py-3">Crediário</th>
                  <th className="px-5 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {registros.map((r) => (
                  <tr key={r.id} className="hover:bg-slate-50">
                    <td className="px-5 py-3.5 font-medium text-slate-700">{formatDate(r.data)}</td>
                    {ehGerente && <td className="px-5 py-3.5 text-slate-700">{r.vendedorNome}</td>}
                    <td className="px-5 py-3.5 text-slate-700">{r.quantAtendimento}</td>
                    <td className="px-5 py-3.5 text-slate-700">{r.numVendas}</td>
                    <td className="px-5 py-3.5 text-slate-700">{formatPercent(r.aproveitamento)}</td>
                    <td className="px-5 py-3.5 font-semibold text-slate-900">{formatCurrency(r.valorTotalVendas)}</td>
                    <td className="px-5 py-3.5 text-slate-700">{formatCurrency(r.garantia)}</td>
                    <td className="px-5 py-3.5 text-slate-700">{formatCurrency(r.crediarioDujuca)}</td>
                    <td className="px-5 py-3.5">
                      <div className="flex justify-end gap-1">
                        <button
                          onClick={() => iniciarEdicao(r)}
                          className="rounded-lg p-2 text-slate-400 hover:bg-indigo-50 hover:text-indigo-600"
                          aria-label="Editar"
                        >
                          <Pencil className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => excluir(r.id)}
                          className="rounded-lg p-2 text-slate-400 hover:bg-red-50 hover:text-red-600"
                          aria-label="Excluir"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <Pagination paginaAtual={pagina} totalPaginas={totalPaginas} onChange={(p) => carregarRegistros(p, filtroVendedor)} />
      </Card>

      <Modal
        open={modalAberta}
        onClose={fecharModal}
        size="lg"
        title={editandoId ? 'Editar lançamento' : 'Novo lançamento'}
      >
            <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <Input
                label="Data"
                type="date"
                value={form.data}
                onChange={(e) => atualizarCampo('data', e.target.value)}
                error={erros.data}
                required
              />

              {ehGerente ? (
                <Select
                  label="Vendedor"
                  value={form.vendedorId}
                  onChange={(e) => atualizarCampo('vendedorId', e.target.value)}
                  error={erros.vendedorId}
                  required
                >
                  <option value="">Selecione...</option>
                  {vendedores.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.nome}
                    </option>
                  ))}
                </Select>
              ) : (
                <Input label="Vendedor" value={usuario?.nome ?? ''} disabled />
              )}

              <Input
                label="Quant. Atendimento"
                type="number"
                min={0}
                value={form.quantAtendimento}
                onChange={(e) => atualizarCampo('quantAtendimento', e.target.value)}
                error={erros.quantAtendimento}
              />
              <Input
                label="Núm. Vendas"
                type="number"
                min={0}
                value={form.numVendas}
                onChange={(e) => atualizarCampo('numVendas', e.target.value)}
                error={erros.numVendas}
              />
              {/* type number + step 0.01 nos campos de dinheiro. com input de texto o valor
                  ia com vírgula (0,00) e o backend não conseguia converter */}
              <Input
                label="Valor Total Vendas"
                type="number"
                min={0}
                step="0.01"
                value={form.valorTotalVendas}
                onChange={(e) => atualizarCampo('valorTotalVendas', e.target.value)}
                error={erros.valorTotalVendas}
              />
              <Input
                label="Garantia"
                type="number"
                min={0}
                step="0.01"
                value={form.garantia}
                onChange={(e) => atualizarCampo('garantia', e.target.value)}
                error={erros.garantia}
              />
              <Input
                label="Crediário Dujuca"
                type="number"
                min={0}
                step="0.01"
                value={form.crediarioDujuca}
                onChange={(e) => atualizarCampo('crediarioDujuca', e.target.value)}
                error={erros.crediarioDujuca}
              />
              <Input
                label="Pretas Mistas"
                type="number"
                min={0}
                step="0.01"
                value={form.pretasMistas}
                onChange={(e) => atualizarCampo('pretasMistas', e.target.value)}
                error={erros.pretasMistas}
              />

              <div className="col-span-full mt-4 flex justify-end gap-3 border-t border-slate-100 pt-4">
                <Button type="button" variant="ghost" onClick={fecharModal}>
                  Cancelar
                </Button>
                <Button type="submit" loading={salvando}>
                  <NotebookPen className="h-4 w-4" />
                  {editandoId ? 'Salvar alterações' : 'Registrar lançamento'}
                </Button>
              </div>
            </form>
      </Modal>
    </div>
  )
}
