import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { LineChart, Line, PieChart, Pie, Cell, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend } from 'recharts'
import { Wallet, ShoppingCart, TrendingUp, Users2, ShieldCheck, Wrench, CreditCard, Package } from 'lucide-react'
import { api, extractErrorMessage } from '@/lib/api'
import { useToast } from '@/context/ToastContext'
import { StatCard } from '@/components/ui/StatCard'
import { Card, CardHeader } from '@/components/ui/Card'
import { Spinner } from '@/components/ui/Spinner'
import { EmptyState } from '@/components/ui/EmptyState'
import { PeriodoFilter } from '@/components/ui/PeriodoFilter'
import { formatCurrency, formatPercent } from '@/lib/format'
import type { ApiEnvelope, DashboardResumo } from '@/types'

const COLORS = ['#4f46e5', '#06b6d4', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899']

export function DashboardPage() {
  const { showToast } = useToast()
  const [resumo, setResumo] = useState<DashboardResumo | null>(null)
  const [carregando, setCarregando] = useState(true)

  const carregar = useCallback(
    async (inicio: string, fim: string) => {
      setCarregando(true)
      try {
        const res = await api.get<ApiEnvelope<DashboardResumo>>('/dashboard/resumo', {
          params: { dataInicio: inicio || undefined, dataFim: fim || undefined }
        })
        setResumo(res.data.data)
      } catch (err) {
        showToast(extractErrorMessage(err), 'error')
      } finally {
        setCarregando(false)
      }
    },
    [showToast]
  )

  useEffect(() => {
    carregar('', '')
  }, [carregar])

  const ticketMedio = resumo && resumo.somaNumVendas > 0 ? resumo.somaValorTotalVendas / resumo.somaNumVendas : 0

  return (
    <div className="space-y-6">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Dashboard</h1>
          <p className="text-sm text-slate-500">Visão geral do desempenho de todos os vendedores.</p>
        </div>
      </div>

      <PeriodoFilter onChange={(periodo) => carregar(periodo.dataInicio, periodo.dataFim)} />

      {carregando ? (
        <Spinner />
      ) : !resumo ? (
        <EmptyState icon={Users2} title="Não foi possível carregar o dashboard" />
      ) : (
        <>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <StatCard label="Valor Total Vendas" value={formatCurrency(resumo.somaValorTotalVendas)} icon={Wallet} highlight />
            <StatCard label="Ticket Médio" value={formatCurrency(ticketMedio)} icon={ShoppingCart} tone="success" />
            <StatCard label="Taxa de Conversão" value={formatPercent(resumo.aproveitamentoGeral)} icon={TrendingUp} tone="warning" />
            <StatCard label="Vendedores Ativos" value={String(resumo.totalVendedores)} icon={Users2} tone="primary" />
            <StatCard label="Núm. de Vendas" value={String(resumo.somaNumVendas)} icon={ShoppingCart} />
            <StatCard label="Quant. Atendimentos" value={String(resumo.somaQuantAtendimento)} icon={Users2} />
            <StatCard label="Garantia" value={formatCurrency(resumo.somaGarantia)} icon={ShieldCheck} tone="success" hint={`% Serviço: ${formatPercent(resumo.percentualServicoGeral)}`} />
            <StatCard label="Crediário Dujuca" value={formatCurrency(resumo.somaCrediarioDujuca)} icon={CreditCard} />
          </div>

          <div className="grid grid-cols-1 gap-6 xl:grid-cols-5 items-start">
            <Card className="xl:col-span-3 overflow-hidden">
              <CardHeader title="Ranking de Vendedores" />
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-slate-100 text-left text-xs font-semibold uppercase tracking-wide text-slate-400">
                      <th className="px-4 py-3 sm:px-5">#</th>
                      <th className="px-4 py-3 sm:px-5">Vendedor</th>
                      <th className="px-4 py-3 sm:px-5">Valor Total</th>
                      <th className="px-4 py-3 sm:px-5">Vendas</th>
                      <th className="px-4 py-3 sm:px-5" />
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {resumo.ranking.length === 0 ? (
                      <tr>
                        <td colSpan={5}>
                          <EmptyState icon={Users2} title="Nenhum vendedor cadastrado" />
                        </td>
                      </tr>
                    ) : (
                      resumo.ranking.map((r) => (
                        <tr key={r.vendedorId} className="hover:bg-slate-50">
                          <td className="px-4 py-3.5 sm:px-5 font-semibold text-slate-400">{r.posicao}º</td>
                          <td className="px-4 py-3.5 sm:px-5 font-semibold text-slate-900 whitespace-nowrap">{r.nome}</td>
                          <td className="px-4 py-3.5 sm:px-5 text-slate-700 whitespace-nowrap">{formatCurrency(r.valorTotalVendas)}</td>
                          <td className="px-4 py-3.5 sm:px-5 text-slate-700">{r.numVendas}</td>
                          <td className="px-4 py-3.5 sm:px-5 text-right whitespace-nowrap">
                            <Link to={`/vendedores/${r.vendedorId}/perfil`} className="text-sm font-semibold text-indigo-600 hover:text-indigo-700">
                              Perfil
                            </Link>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </Card>

            <Card className="xl:col-span-2 flex flex-col">
              <CardHeader title="Faturamento por Vendedor" />
              <div className="h-64 w-full px-2 py-2 flex items-center justify-center">
                {resumo.ranking.length === 0 ? (
                  <EmptyState icon={Package} title="Sem dados no período" />
                ) : (
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Tooltip formatter={(value) => formatCurrency(Number(value ?? 0))} />
                      <Pie
                        data={resumo.ranking}
                        dataKey="valorTotalVendas"
                        nameKey="nome"
                        cx="50%"
                        cy="45%"
                        outerRadius={60}
                        innerRadius={25}
                        label={false}
                      >
                        {resumo.ranking.map((_, index) => (
                          <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                        ))}
                      </Pie>
                      <Legend
                        verticalAlign="bottom"
                        height={36}
                        formatter={(value) => {
                          const item = resumo.ranking.find(r => r.nome === value)
                          const percentual = item ? item.participacaoPercentual.toFixed(0) : 0
                          return <span className="text-xs font-medium text-slate-700">{value} ({percentual}%)</span>
                        }}
                      />
                    </PieChart>
                  </ResponsiveContainer>
                )}
              </div>
            </Card>
          </div>

          <Card className="p-5">
            <CardHeader title="Evolução de Rendimento ao Longo do Tempo" />
            <div className="h-72 sm:h-80 w-full pt-4">
              {!resumo.evolucaoTemporal || resumo.evolucaoTemporal.length === 0 ? (
                <EmptyState icon={Package} title="Sem dados temporais no período" />
              ) : (
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={resumo.evolucaoTemporal} margin={{ top: 10, right: 10, left: -10, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                    <XAxis dataKey="periodo" tick={{ fontSize: 11, fill: '#64748b' }} />
                    <YAxis tickFormatter={(v: number) => formatCurrency(v)} tick={{ fontSize: 11, fill: '#64748b' }} width={80} />
                    <Tooltip formatter={(value) => formatCurrency(Number(value ?? 0))} labelStyle={{ fontWeight: 600 }} />
                    <Legend verticalAlign="top" height={36} />
                    <Line
                      type="monotone"
                      dataKey="valorTotalVendas"
                      name="Rendimento Total"
                      stroke="#4f46e5"
                      strokeWidth={3}
                      dot={{ r: 4, fill: '#4f46e5' }}
                      activeDot={{ r: 8 }}
                    />
                  </LineChart>
                </ResponsiveContainer>
              )}
            </div>
          </Card>

          <Card className="p-5">
            <div className="flex items-center gap-2 text-sm text-slate-500">
              <Wrench className="h-4 w-4" />
              Pretas Mistas no período: <span className="font-semibold text-slate-700">{formatCurrency(resumo.somaPretasMistas)}</span>
            </div>
          </Card>
        </>
      )}
    </div>
  )
}
