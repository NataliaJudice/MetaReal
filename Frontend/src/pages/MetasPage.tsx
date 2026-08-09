import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react'
import { Target, Users2, Trophy, TrendingUp, Wallet, Pencil, History, Sparkles } from 'lucide-react'
import { api, extractErrorMessage } from '@/lib/api'
import { useAuth } from '@/context/AuthContext'
import { useToast } from '@/context/ToastContext'
import { Card, CardHeader } from '@/components/ui/Card'
import { StatCard } from '@/components/ui/StatCard'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Modal } from '@/components/ui/Modal'
import { Badge } from '@/components/ui/Badge'
import { ProgressBar } from '@/components/ui/ProgressBar'
import { MonthNavigator } from '@/components/ui/MonthNavigator'
import { Spinner } from '@/components/ui/Spinner'
import { EmptyState } from '@/components/ui/EmptyState'
import { MetaProgressCard } from '@/components/MetaProgressCard'
import { formatCurrency, formatMesAno, NOMES_MES } from '@/lib/format'
import type { ApiEnvelope, MetaProgresso } from '@/types'

function statusDaMeta(meta: MetaProgresso) {
  if (meta.valorMeta <= 0) return { tone: 'neutral' as const, label: 'Sem meta' }
  if (meta.concluida) return { tone: 'success' as const, label: 'Batida' }
  if (meta.percentualConcluido >= 0.75) return { tone: 'info' as const, label: 'Quase lá' }
  if (meta.percentualConcluido >= 0.4) return { tone: 'warning' as const, label: 'Em andamento' }
  return { tone: 'neutral' as const, label: 'Começando' }
}

export function MetasPage() {
  const { usuario } = useAuth()
  const ehGerente = usuario?.role === 'Gerente'

  const hoje = new Date()
  const [mes, setMes] = useState(hoje.getMonth() + 1)
  const [ano, setAno] = useState(hoje.getFullYear())

  const [carregando, setCarregando] = useState(true)
  const [metas, setMetas] = useState<MetaProgresso[]>([])
  const [historico, setHistorico] = useState<MetaProgresso[]>([])

  const [loteAberto, setLoteAberto] = useState(false)
  const [loteValor, setLoteValor] = useState('')
  const [salvandoLote, setSalvandoLote] = useState(false)

  const [individualAlvo, setIndividualAlvo] = useState<MetaProgresso | null>(null)
  const [individualValor, setIndividualValor] = useState('')
  const [salvandoIndividual, setSalvandoIndividual] = useState(false)

  const { showToast } = useToast()

  const carregar = useCallback(async () => {
    setCarregando(true)
    try {
      if (ehGerente) {
        const res = await api.get<ApiEnvelope<MetaProgresso[]>>('/metas', { params: { mes, ano } })
        setMetas(res.data.data)
      } else if (usuario?.vendedorId) {
        const [progresso, hist] = await Promise.all([
          api.get<ApiEnvelope<MetaProgresso>>(`/metas/vendedor/${usuario.vendedorId}`, { params: { mes, ano } }),
          api.get<ApiEnvelope<MetaProgresso[]>>(`/metas/vendedor/${usuario.vendedorId}/historico`, {
            params: { meses: 6 }
          })
        ])
        setMetas([progresso.data.data])
        setHistorico(hist.data.data)
      }
    } catch (err) {
      showToast(extractErrorMessage(err), 'error')
    } finally {
      setCarregando(false)
    }
  }, [ehGerente, usuario?.vendedorId, mes, ano, showToast])

  useEffect(() => {
    carregar()
  }, [carregar])

  const resumo = useMemo(() => {
    const comMeta = metas.filter((m) => m.valorMeta > 0)
    return {
      total: metas.length,
      bateram: comMeta.filter((m) => m.concluida).length,
      comMeta: comMeta.length,
      somaMetas: comMeta.reduce((acc, m) => acc + m.valorMeta, 0),
      somaRealizado: metas.reduce((acc, m) => acc + m.valorAtual, 0)
    }
  }, [metas])

  const salvarLote = async (event: FormEvent) => {
    event.preventDefault()
    setSalvandoLote(true)
    try {
      await api.post('/metas/lote', { mes, ano, valorMeta: Number(loteValor) || 0 })
      showToast(`Meta de ${formatMesAno(mes, ano)} definida para toda a equipe.`, 'success')
      setLoteAberto(false)
      setLoteValor('')
      carregar()
    } catch (err) {
      showToast(extractErrorMessage(err), 'error')
    } finally {
      setSalvandoLote(false)
    }
  }

  const salvarIndividual = async (event: FormEvent) => {
    event.preventDefault()
    if (!individualAlvo) return

    setSalvandoIndividual(true)
    try {
      await api.post('/metas', {
        vendedorId: individualAlvo.vendedorId,
        mes,
        ano,
        valorMeta: Number(individualValor) || 0
      })
      showToast(`Meta de ${individualAlvo.vendedorNome} atualizada.`, 'success')
      setIndividualAlvo(null)
      carregar()
    } catch (err) {
      showToast(extractErrorMessage(err), 'error')
    } finally {
      setSalvandoIndividual(false)
    }
  }

  const abrirIndividual = (meta: MetaProgresso) => {
    setIndividualAlvo(meta)
    setIndividualValor(meta.valorMeta > 0 ? String(meta.valorMeta) : '')
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-indigo-100">
            <Target className="h-5 w-5 text-indigo-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">Metas</h1>
            <p className="text-sm text-slate-500">
              {ehGerente ? 'Defina e acompanhe as metas da equipe.' : 'Acompanhe seu progresso do mês.'}
            </p>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-3">
          <MonthNavigator mes={mes} ano={ano} onChange={(m, a) => { setMes(m); setAno(a) }} />
          {ehGerente && (
            <Button onClick={() => setLoteAberto(true)}>
              <Sparkles className="h-4 w-4" /> Definir meta para todos
            </Button>
          )}
        </div>
      </div>

      {carregando ? (
        <Spinner />
      ) : ehGerente ? (
        <>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <StatCard
              label="Bateram a meta"
              value={`${resumo.bateram} de ${resumo.comMeta || 0}`}
              icon={Trophy}
              highlight
              hint={resumo.comMeta === 0 ? 'Nenhuma meta definida' : undefined}
            />
            <StatCard label="Realizado da equipe" value={formatCurrency(resumo.somaRealizado)} icon={Wallet} />
            <StatCard label="Soma das metas" value={formatCurrency(resumo.somaMetas)} icon={Target} />
            <StatCard
              label="Progresso geral"
              value={
                resumo.somaMetas > 0
                  ? `${((resumo.somaRealizado / resumo.somaMetas) * 100).toFixed(0)}%`
                  : '—'
              }
              icon={TrendingUp}
            />
          </div>

          <Card>
            <CardHeader title={`Metas de ${formatMesAno(mes, ano)}`} />
            {metas.length === 0 ? (
              <EmptyState icon={Users2} title="Nenhum vendedor cadastrado" />
            ) : (
              <div className="divide-y divide-slate-100">
                {metas.map((meta) => {
                  const status = statusDaMeta(meta)
                  return (
                    <div key={meta.vendedorId} className="flex flex-col gap-3 p-5 sm:flex-row sm:items-center sm:gap-5">
                      <div className="flex min-w-0 flex-1 items-center gap-3">
                        <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-indigo-100 font-bold text-indigo-700">
                          {meta.vendedorNome.charAt(0).toUpperCase()}
                        </div>
                        <div className="min-w-0 flex-1">
                          <div className="flex flex-wrap items-center gap-2">
                            <p className="truncate font-semibold text-slate-900">{meta.vendedorNome}</p>
                            <Badge tone={status.tone}>{status.label}</Badge>
                          </div>
                          <p className="mt-0.5 text-sm text-slate-500">
                            {formatCurrency(meta.valorAtual)}
                            {meta.valorMeta > 0 && (
                              <span className="text-slate-400"> de {formatCurrency(meta.valorMeta)}</span>
                            )}
                          </p>
                        </div>
                      </div>

                      <div className="w-full sm:w-56">
                        <ProgressBar percentual={meta.valorMeta > 0 ? meta.percentualConcluido : 0} />
                        <p className="mt-1.5 text-right text-xs font-semibold text-slate-500">
                          {meta.valorMeta > 0 ? `${(meta.percentualConcluido * 100).toFixed(0)}%` : 'sem meta'}
                        </p>
                      </div>

                      <Button size="sm" variant="secondary" onClick={() => abrirIndividual(meta)}>
                        <Pencil className="h-4 w-4" /> Editar
                      </Button>
                    </div>
                  )
                })}
              </div>
            )}
          </Card>
        </>
      ) : (
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          <div className="lg:col-span-1">
            <MetaProgressCard progresso={metas[0] ?? null} variant="full" />
          </div>

          <Card className="lg:col-span-2">
            <CardHeader title="Histórico dos últimos 6 meses" />
            {historico.length === 0 ? (
              <EmptyState icon={History} title="Sem histórico ainda" />
            ) : (
              <div className="divide-y divide-slate-100">
                {historico.map((h) => {
                  const status = statusDaMeta(h)
                  return (
                    <div key={`${h.ano}-${h.mes}`} className="flex items-center gap-4 px-5 py-4">
                      <div className="w-28 shrink-0">
                        <p className="text-sm font-semibold text-slate-900">{NOMES_MES[h.mes - 1]}</p>
                        <p className="text-xs text-slate-400">{h.ano}</p>
                      </div>

                      <div className="min-w-0 flex-1">
                        <div className="flex flex-wrap items-baseline justify-between gap-x-3 gap-y-1">
                          <p className="text-sm text-slate-600">
                            {formatCurrency(h.valorAtual)}
                            {h.valorMeta > 0 && (
                              <span className="text-slate-400"> de {formatCurrency(h.valorMeta)}</span>
                            )}
                          </p>
                          <Badge tone={status.tone}>{status.label}</Badge>
                        </div>
                        <ProgressBar
                          percentual={h.valorMeta > 0 ? h.percentualConcluido : 0}
                          className="mt-2"
                        />
                      </div>
                    </div>
                  )
                })}
              </div>
            )}
          </Card>
        </div>
      )}

      <Modal open={loteAberto} title="Definir meta para todos" onClose={() => setLoteAberto(false)}>
        <form onSubmit={salvarLote} className="space-y-4">
          <div className="rounded-xl border border-amber-200 bg-amber-50 p-3 text-sm text-amber-800">
            O mesmo valor vira a meta individual de <strong>cada vendedor</strong> em{' '}
            <strong>{formatMesAno(mes, ano)}</strong>, substituindo as metas já definidas para esse mês. Depois dá para
            ajustar caso a caso.
          </div>

          <Input
            label="Valor da meta por vendedor (R$)"
            type="number"
            min={0}
            step="0.01"
            value={loteValor}
            onChange={(e) => setLoteValor(e.target.value)}
            placeholder="Ex.: 30000"
            required
            autoFocus
          />

          <div className="flex justify-end gap-2">
            <Button type="button" variant="secondary" onClick={() => setLoteAberto(false)}>
              Cancelar
            </Button>
            <Button type="submit" loading={salvandoLote} disabled={Number(loteValor) <= 0}>
              Aplicar a todos
            </Button>
          </div>
        </form>
      </Modal>

      <Modal
        open={individualAlvo !== null}
        title={`Meta de ${individualAlvo?.vendedorNome ?? ''}`}
        onClose={() => setIndividualAlvo(null)}
      >
        <form onSubmit={salvarIndividual} className="space-y-4">
          <p className="text-sm text-slate-500">
            Referente a <strong className="text-slate-700">{formatMesAno(mes, ano)}</strong>.
          </p>
          <Input
            label="Valor da meta (R$)"
            type="number"
            min={0}
            step="0.01"
            value={individualValor}
            onChange={(e) => setIndividualValor(e.target.value)}
            placeholder="Ex.: 30000"
            required
            autoFocus
          />
          <div className="flex justify-end gap-2">
            <Button type="button" variant="secondary" onClick={() => setIndividualAlvo(null)}>
              Cancelar
            </Button>
            <Button type="submit" loading={salvandoIndividual} disabled={Number(individualValor) <= 0}>
              Salvar meta
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
