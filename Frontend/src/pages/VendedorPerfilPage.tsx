import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, Wallet, ShoppingCart, TrendingUp, ShieldCheck, CreditCard, Users2, Lock, ClipboardList, NotebookPen, Plus } from 'lucide-react'
import { api, extractErrorMessage } from '@/lib/api'
import { useToast } from '@/context/ToastContext'
import { useAuth } from '@/context/AuthContext'
import { Card, CardHeader } from '@/components/ui/Card'
import { StatCard } from '@/components/ui/StatCard'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { Button } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'
import { EmptyState } from '@/components/ui/EmptyState'
import { Pagination } from '@/components/ui/Pagination'
import { Modal } from '@/components/ui/Modal'
import { PeriodoFilter } from '@/components/ui/PeriodoFilter'
import { MetaProgressCard } from '@/components/MetaProgressCard'
import { formatCurrency, formatDate, formatPercent, todayInputDate, NOMES_MES } from '@/lib/format'
import type { ApiEnvelope, MetaProgresso, PerfilVendedor, RegistroVendaInput } from '@/types'
import axios from 'axios'

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

export function VendedorPerfilPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { showToast } = useToast()
  const { usuario } = useAuth()
  const ehGerente = usuario?.role === 'Gerente'

  const [perfil, setPerfil] = useState<PerfilVendedor | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [semPermissao, setSemPermissao] = useState(false)
  const [dataInicio, setDataInicio] = useState('')
  const [dataFim, setDataFim] = useState('')

  const [modalAberta, setModalAberta] = useState(false)
  const [salvando, setSalvando] = useState(false)
  const [form, setForm] = useState<FormState>(() => estadoInicial(id ?? ''))
  const [erros, setErros] = useState<Record<string, string>>({})

  const hoje = new Date()
  const [progressoMeta, setProgressoMeta] = useState<MetaProgresso | null>(null)
  const [metaModalAberta, setMetaModalAberta] = useState(false)
  const [salvandoMeta, setSalvandoMeta] = useState(false)
  const [metaMes, setMetaMes] = useState(hoje.getMonth() + 1)
  const [metaAno, setMetaAno] = useState(hoje.getFullYear())
  const [metaValor, setMetaValor] = useState('')

  const carregarMeta = useCallback(async () => {
    if (!id) return
    try {
      const res = await api.get<ApiEnvelope<MetaProgresso>>(`/metas/vendedor/${id}`)
      setProgressoMeta(res.data.data)
    } catch {
    }
  }, [id])

  useEffect(() => {
    carregarMeta()
  }, [carregarMeta])

  const abrirModalMeta = () => {
    setMetaMes(progressoMeta?.mes ?? hoje.getMonth() + 1)
    setMetaAno(progressoMeta?.ano ?? hoje.getFullYear())
    setMetaValor(progressoMeta?.valorMeta ? String(progressoMeta.valorMeta) : '')
    setMetaModalAberta(true)
  }

  const salvarMeta = async (event: FormEvent) => {
    event.preventDefault()
    if (!id) return

    setSalvandoMeta(true)
    try {
      await api.post('/metas', {
        vendedorId: id,
        mes: metaMes,
        ano: metaAno,
        valorMeta: Number(metaValor) || 0
      })
      showToast('Meta definida com sucesso.', 'success')
      setMetaModalAberta(false)
      carregarMeta()
    } catch (err) {
      showToast(extractErrorMessage(err), 'error')
    } finally {
      setSalvandoMeta(false)
    }
  }

  const carregar = useCallback(
    async (pagina: number, inicio: string, fim: string) => {
      if (!id) return
      setCarregando(true)
      setSemPermissao(false)
      try {
        const res = await api.get<ApiEnvelope<PerfilVendedor>>(`/vendedores/${id}/perfil`, {
          params: { pagina, dataInicio: inicio || undefined, dataFim: fim || undefined }
        })
        setPerfil(res.data.data)
      } catch (err) {
        if (axios.isAxiosError(err) && err.response?.status === 403) {
          setSemPermissao(true)
        } else {
          showToast(extractErrorMessage(err), 'error')
        }
      } finally {
        setCarregando(false)
      }
    },
    [id, showToast]
  )

  useEffect(() => {
    carregar(1, '', '')
  }, [carregar])

  const abrirModalNovo = () => {
    setForm(estadoInicial(id ?? ''))
    setErros({})
    setModalAberta(true)
  }

  const fecharModal = () => {
    setModalAberta(false)
    setForm(estadoInicial(id ?? ''))
  }

  const atualizarCampo = (campo: keyof FormState, valor: string) => {
    setForm((prev) => ({ ...prev, [campo]: valor }))
    setErros((prev) => ({ ...prev, [campo]: '' }))
  }

  const validarLocalmente = (payload: RegistroVendaInput): boolean => {
    const novosErros: Record<string, string> = {}
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
      vendedorId: id ?? '',
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
      await api.post('/vendas', payload)
      showToast('Lançamento registrado com sucesso.', 'success')
      fecharModal()
      carregar(1, dataInicio, dataFim)
    } catch (err) {
      showToast(extractErrorMessage(err), 'error')
    } finally {
      setSalvando(false)
    }
  }

  if (carregando) return <Spinner />

  if (semPermissao) {
    return (
      <EmptyState
        icon={Lock}
        title="Sem permissão"
        description="Você não tem acesso ao desempenho de outro vendedor."
        action={
          <Button variant="secondary" onClick={() => navigate(-1)} className="mt-2">
            Voltar
          </Button>
        }
      />
    )
  }

  if (!perfil) {
    return <EmptyState icon={Users2} title="Vendedor não encontrado" />
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => navigate(-1)} className="rounded-lg p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-700" aria-label="Voltar">
          <ArrowLeft className="h-5 w-5" />
        </button>
        <div>
          <h1 className="text-2xl font-bold text-slate-900">{perfil.vendedor.nome}</h1>
          <p className="text-sm text-slate-500">Desempenho individual</p>
        </div>
      </div>

      <PeriodoFilter
        onChange={(periodo) => {
          setDataInicio(periodo.dataInicio)
          setDataFim(periodo.dataFim)
          carregar(1, periodo.dataInicio, periodo.dataFim)
        }}
        action={
          <Button type="button" size="sm" onClick={abrirModalNovo}>
            <Plus className="h-4 w-4" /> Novo lançamento
          </Button>
        }
      />

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-3">
        <div className="xl:col-span-1">
          <MetaProgressCard
            progresso={progressoMeta}
            variant="full"
            onDefinirMeta={ehGerente ? abrirModalMeta : undefined}
          />
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:col-span-2 xl:content-start">
          <StatCard label="Valor Total Vendas" value={formatCurrency(perfil.somaValorTotalVendas)} icon={Wallet} highlight />
          <StatCard label="Núm. Vendas" value={String(perfil.somaNumVendas)} icon={ShoppingCart} tone="primary" />
          <StatCard label="Aproveitamento" value={formatPercent(perfil.aproveitamento)} icon={TrendingUp} tone="warning" />
          <StatCard label="Quant. Atendimento" value={String(perfil.somaQuantAtendimento)} icon={Users2} />
          <StatCard label="Garantia" value={formatCurrency(perfil.somaGarantia)} icon={ShieldCheck} tone="success" hint={`% Serviço: ${formatPercent(perfil.percentualServico)}`} />
          <StatCard label="Crediário Dujuca" value={formatCurrency(perfil.somaCrediarioDujuca)} icon={CreditCard} />
        </div>
      </div>

      <Card>
        <CardHeader title={`Histórico de lançamentos (${perfil.totalRegistrosPeriodo})`} />
        {perfil.registros.length === 0 ? (
          <EmptyState icon={ClipboardList} title="Nenhum lançamento no período" />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs font-semibold uppercase tracking-wide text-slate-400">
                  <th className="px-5 py-3">Data</th>
                  <th className="px-5 py-3">Atend.</th>
                  <th className="px-5 py-3">Vendas</th>
                  <th className="px-5 py-3">Conversão</th>
                  <th className="px-5 py-3">Valor Total</th>
                  <th className="px-5 py-3">Garantia</th>
                  <th className="px-5 py-3">% Serviço</th>
                  <th className="px-5 py-3">Crediário</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {perfil.registros.map((r) => (
                  <tr key={r.id} className="hover:bg-slate-50">
                    <td className="px-5 py-3.5 font-medium text-slate-700">{formatDate(r.data)}</td>
                    <td className="px-5 py-3.5 text-slate-700">{r.quantAtendimento}</td>
                    <td className="px-5 py-3.5 text-slate-700">{r.numVendas}</td>
                    <td className="px-5 py-3.5 text-slate-700">{formatPercent(r.aproveitamento)}</td>
                    <td className="px-5 py-3.5 font-semibold text-slate-900">{formatCurrency(r.valorTotalVendas)}</td>
                    <td className="px-5 py-3.5 text-slate-700">{formatCurrency(r.garantia)}</td>
                    <td className="px-5 py-3.5 text-slate-700">{formatPercent(r.percentualServico)}</td>
                    <td className="px-5 py-3.5 text-slate-700">{formatCurrency(r.crediarioDujuca)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination paginaAtual={perfil.paginaAtual} totalPaginas={perfil.totalPaginas} onChange={(p) => carregar(p, dataInicio, dataFim)} />
      </Card>

      <Modal
        open={modalAberta}
        onClose={fecharModal}
        size="lg"
        title={`Novo lançamento para ${perfil.vendedor.nome}`}
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

              <Input label="Vendedor" value={perfil.vendedor.nome} disabled />

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
                  Registrar lançamento
                </Button>
              </div>
            </form>
      </Modal>

      <Modal open={metaModalAberta} title={`Meta de ${perfil.vendedor.nome}`} onClose={() => setMetaModalAberta(false)}>
        <form onSubmit={salvarMeta} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <Select label="Mês" value={metaMes} onChange={(e) => setMetaMes(Number(e.target.value))}>
              {NOMES_MES.map((nome, idx) => (
                <option key={nome} value={idx + 1}>
                  {nome}
                </option>
              ))}
            </Select>
            <Input label="Ano" type="number" value={metaAno} onChange={(e) => setMetaAno(Number(e.target.value))} />
          </div>
          <Input
            label="Valor da meta (R$)"
            type="number"
            min={0}
            step="0.01"
            value={metaValor}
            onChange={(e) => setMetaValor(e.target.value)}
            placeholder="Ex.: 30000"
            required
            autoFocus
          />
          <div className="flex justify-end gap-2">
            <Button type="button" variant="secondary" onClick={() => setMetaModalAberta(false)}>
              Cancelar
            </Button>
            <Button type="submit" loading={salvandoMeta}>
              Salvar meta
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
