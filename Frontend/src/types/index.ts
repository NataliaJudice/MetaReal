export type Role = 'Gerente' | 'Vendedor'

export interface Usuario {
  id: string
  nome: string
  email: string
  role: Role
  vendedorId: string | null
}

export interface LoginResponse {
  accessToken: string
  expiraEmSegundos: number
  usuario: Usuario
}

export interface Vendedor {
  id: string
  nome: string
}

export interface RegistroVenda {
  id: string
  data: string
  pretasMistas: number
  garantia: number
  crediarioDujuca: number
  quantAtendimento: number
  numVendas: number
  valorTotalVendas: number
  vendedorId: string
  vendedorNome: string
  percentualServico: number
  aproveitamento: number
}

export interface RegistroVendaInput {
  data: string
  vendedorId: string
  pretasMistas: number
  garantia: number
  crediarioDujuca: number
  quantAtendimento: number
  numVendas: number
  valorTotalVendas: number
}

export interface Paginado<T> {
  totalRegistros: number
  paginaAtual: number
  totalPaginas: number
  // esses totais são do filtro inteiro, não só do que veio em items
  valorTotal?: number
  totalVendas?: number
  totalAtendimentos?: number
  conversaoMedia?: number
  items: T[]
}

export interface RankingVendedor {
  posicao: number
  vendedorId: string
  nome: string
  valorTotalVendas: number
  numVendas: number
  quantAtendimento: number
  aproveitamento: number
  participacaoPercentual: number
}

export interface EvolucaoTemporalItem {
  periodo: string
  valorTotalVendas: number
}

export interface DashboardResumo {
  dataInicio: string | null
  dataFim: string | null
  totalVendedores: number
  totalRegistros: number
  somaPretasMistas: number
  somaGarantia: number
  somaCrediarioDujuca: number
  somaQuantAtendimento: number
  somaNumVendas: number
  somaValorTotalVendas: number
  percentualServicoGeral: number
  aproveitamentoGeral: number
  ranking: RankingVendedor[]
  evolucaoTemporal?: EvolucaoTemporalItem[]
}

export interface PerfilVendedor {
  vendedor: Vendedor
  dataInicio: string | null
  dataFim: string | null
  somaPretasMistas: number
  somaGarantia: number
  somaCrediarioDujuca: number
  somaQuantAtendimento: number
  somaNumVendas: number
  somaValorTotalVendas: number
  percentualServico: number
  aproveitamento: number
  totalRegistrosPeriodo: number
  paginaAtual: number
  totalPaginas: number
  registros: RegistroVenda[]
}

export interface AuditLogItem {
  id: string
  usuarioNome: string | null
  acao: string
  entidade: string
  idEntidade: string | null
  dataHora: string
  detalhes: string | null
  ip: string | null
}

export interface MetaProgresso {
  id: string | null
  vendedorId: string
  vendedorNome: string
  mes: number
  ano: number
  valorMeta: number
  valorAtual: number
  percentualConcluido: number
  concluida: boolean
}

export interface MetaInput {
  vendedorId: string
  mes: number
  ano: number
  valorMeta: number
}

export type TipoColuna = 'Texto' | 'Moeda' | 'Numero' | 'Percentual' | 'Data'

export interface ColunaRelatorio {
  chave: string
  titulo: string
  tipo: TipoColuna
}

export interface ResumoRelatorio {
  rotulo: string
  valor: string
}

export interface Relatorio {
  chave: string
  titulo: string
  subtitulo: string
  geradoEm: string
  colunas: ColunaRelatorio[]
  linhas: Record<string, unknown>[]
  resumo: ResumoRelatorio[]
}

export interface ApiEnvelope<T> {
  success: boolean
  data: T
}

export interface ApiError {
  success: false
  error: string
  requestId?: string
  errors?: Record<string, string[]>
}
