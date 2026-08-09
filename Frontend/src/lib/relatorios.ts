import {
  Trophy,
  Target,
  LineChart,
  Gauge,
  CalendarCheck,
  ShieldCheck,
  CreditCard,
  type LucideIcon
} from 'lucide-react'
import type { Role } from '@/types'

export type FiltroRelatorio = 'periodo' | 'vendedor' | 'agrupamento'

export interface DefinicaoRelatorio {
  chave: string
  nome: string
  descricao: string
  paraQue: string
  icone: LucideIcon
  cor: string
  filtros: FiltroRelatorio[]
  papeis: Role[]
}

export const RELATORIOS: DefinicaoRelatorio[] = [
  {
    chave: 'desempenho-vendedor',
    nome: 'Desempenho por Vendedor',
    descricao: 'Ranking consolidado com faturamento, vendas, conversão, ticket médio, garantia e crediário.',
    paraQue: 'Ver quem puxa o resultado da loja e quanto cada um representa do total.',
    icone: Trophy,
    cor: 'indigo',
    filtros: ['periodo', 'vendedor'],
    papeis: ['Gerente', 'Vendedor']
  },
  {
    chave: 'cumprimento-metas',
    nome: 'Cumprimento de Metas',
    descricao: 'Meta x realizado por vendedor e competência, com diferença e situação.',
    paraQue: 'Saber quem bateu a meta, por quanto passou ou faltou, e a taxa de cumprimento do time.',
    icone: Target,
    cor: 'emerald',
    filtros: ['periodo', 'vendedor'],
    papeis: ['Gerente', 'Vendedor']
  },
  {
    chave: 'evolucao-vendas',
    nome: 'Evolução de Vendas',
    descricao: 'Série temporal do faturamento, agrupável por dia ou por mês.',
    paraQue: 'Identificar tendência, sazonalidade e os melhores e piores períodos.',
    icone: LineChart,
    cor: 'sky',
    filtros: ['periodo', 'vendedor', 'agrupamento'],
    papeis: ['Gerente', 'Vendedor']
  },
  {
    chave: 'produtividade-conversao',
    nome: 'Produtividade e Conversão',
    descricao: 'Funil de atendimento até a venda, com taxa de conversão e atendimentos por dia.',
    paraQue: 'Descobrir quem atende muito e converte pouco — onde treinamento rende mais.',
    icone: Gauge,
    cor: 'amber',
    filtros: ['periodo', 'vendedor'],
    papeis: ['Gerente', 'Vendedor']
  },
  {
    chave: 'consistencia-lancamentos',
    nome: 'Consistência de Lançamentos',
    descricao: 'Dias úteis do período contra os dias efetivamente lançados, com as datas em falta.',
    paraQue: 'Achar furo de digitação antes que ele contamine todos os outros relatórios.',
    icone: CalendarCheck,
    cor: 'rose',
    filtros: ['periodo', 'vendedor'],
    papeis: ['Gerente']
  },
  {
    chave: 'garantias-servico',
    nome: 'Garantias e % Serviço',
    descricao: 'Volume de garantia por vendedor e quanto representa sobre o valor vendido.',
    paraQue: 'Acompanhar a venda de serviço, que é margem alta, vendedor a vendedor.',
    icone: ShieldCheck,
    cor: 'violet',
    filtros: ['periodo', 'vendedor'],
    papeis: ['Gerente', 'Vendedor']
  },
  {
    chave: 'crediario-dujuca',
    nome: 'Crediário Dujuca',
    descricao: 'Volume do crediário próprio e a fatia que ele ocupa no faturamento de cada um.',
    paraQue: 'Medir a exposição da loja ao crédito próprio e quem mais usa essa condição.',
    icone: CreditCard,
    cor: 'teal',
    filtros: ['periodo', 'vendedor'],
    papeis: ['Gerente']
  }
]
export const buscarRelatorio = (chave?: string) => RELATORIOS.find((r) => r.chave === chave)
export const relatoriosDoPapel = (papel: Role | undefined) =>
  papel ? RELATORIOS.filter((r) => r.papeis.includes(papel)) : []

// escrito por extenso porque o tailwind varre o código procurando o nome da classe.
// se montar tipo `bg-${cor}-100` ele não acha e a cor some no build de produção
export const CORES_RELATORIO: Record<string, { chip: string; icone: string; hover: string }> = {
  indigo: { chip: 'bg-indigo-100', icone: 'text-indigo-600', hover: 'group-hover:border-indigo-300' },
  emerald: { chip: 'bg-emerald-100', icone: 'text-emerald-600', hover: 'group-hover:border-emerald-300' },
  sky: { chip: 'bg-sky-100', icone: 'text-sky-600', hover: 'group-hover:border-sky-300' },
  amber: { chip: 'bg-amber-100', icone: 'text-amber-600', hover: 'group-hover:border-amber-300' },
  rose: { chip: 'bg-rose-100', icone: 'text-rose-600', hover: 'group-hover:border-rose-300' },
  violet: { chip: 'bg-violet-100', icone: 'text-violet-600', hover: 'group-hover:border-violet-300' },
  teal: { chip: 'bg-teal-100', icone: 'text-teal-600', hover: 'group-hover:border-teal-300' }
}
