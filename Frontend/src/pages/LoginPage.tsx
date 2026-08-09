import { useState, type FormEvent } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { Lock, Mail, TrendingUp, ShieldCheck, BarChart3 } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'
import { useToast } from '@/context/ToastContext'
import { Input } from '@/components/ui/Input'
import { Button } from '@/components/ui/Button'
import { extractErrorMessage } from '@/lib/api'
import { FullScreenSpinner } from '@/components/ui/Spinner'

export function LoginPage() {
  const { usuario, carregando, login } = useAuth()
  const { showToast } = useToast()
  const navigate = useNavigate()

  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [enviando, setEnviando] = useState(false)
  const [erro, setErro] = useState<string | null>(null)

  if (carregando) {
    return <FullScreenSpinner />
  }

  if (usuario) {
    return <Navigate to="/" replace />
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setErro(null)
    setEnviando(true)
    try {
      await login(email, senha)
      showToast('Bem-vindo(a) de volta!', 'success')
      navigate('/', { replace: true })
    } catch (err) {
      setErro(extractErrorMessage(err))
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="flex min-h-screen">
      <div className="relative hidden w-1/2 flex-col justify-between overflow-hidden bg-gradient-to-br from-indigo-700 via-indigo-600 to-violet-700 p-12 text-white lg:flex">
        <div className="absolute -right-24 -top-24 h-96 w-96 rounded-full bg-white/10 blur-3xl" />
        <div className="absolute -bottom-32 -left-10 h-96 w-96 rounded-full bg-violet-400/20 blur-3xl" />

        <div className="relative flex items-center gap-2.5">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-white/15 font-bold backdrop-blur">MR</div>
          <span className="text-lg font-bold">MetaReal</span>
        </div>

        <div className="relative space-y-8">
          <h1 className="text-4xl font-bold leading-tight">

            Controle suas vendas de verdade.
          </h1>
          <p className="max-w-md text-indigo-100">
            Acompanhe o dia a dia da loja, veja quem está batendo meta e gerencie os números da equipe sem dor de cabeça.
          </p>

          <div className="space-y-4 pt-4">
            <Feature icon={BarChart3} text="Faturamento, ticket médio e conversão em tempo real" />
            <Feature icon={TrendingUp} text="Acompanhamento de metas e desempenho por vendedor" />
            <Feature icon={ShieldCheck} text="Acesso seguro, organizado por cargos e com histórico de ações" />
          </div>
        </div>

        <p className="relative text-xs text-indigo-200">© 2026 MetaReal</p>
      </div>

      <div className="flex w-full items-center justify-center bg-slate-50 px-6 py-12 lg:w-1/2">
        <div className="w-full max-w-sm">
          <div className="mb-8 lg:hidden">
            <div className="mb-3 flex h-10 w-10 items-center justify-center rounded-xl bg-indigo-600 font-bold text-white">MR</div>
            <h1 className="text-xl font-bold text-slate-900">MetaReal</h1>
          </div>

          <h2 className="text-2xl font-bold text-slate-900">Entrar</h2>
          <p className="mt-1 text-sm text-slate-500">Digite seu e-mail e senha para continuar.</p>

          <form onSubmit={handleSubmit} className="mt-8 space-y-4" noValidate>
            <div className="relative">
              <Mail className="pointer-events-none absolute left-3 top-[38px] h-4 w-4 text-slate-400" />
              <Input
                label="E-mail"
                type="email"
                autoComplete="username"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="pl-10"
                placeholder="voce@dujuca.com"
              />
            </div>

            <div className="relative">
              <Lock className="pointer-events-none absolute left-3 top-[38px] h-4 w-4 text-slate-400" />
              <Input
                label="Senha"
                type="password"
                autoComplete="current-password"
                required
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
                className="pl-10"
                placeholder="••••••••"
              />
            </div>

            {erro && (
              <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm font-medium text-red-700">
                {erro}
              </div>
            )}

            <Button type="submit" loading={enviando} className="w-full">
              Entrar
            </Button>
          </form>

          <div className="mt-8 rounded-xl border border-slate-200 bg-white p-4 text-xs text-slate-500">
            <p className="mb-1.5 font-semibold text-slate-600">Contas de teste</p>
            <p>Gerente: gerente@gmail.com / Gerente@123</p>
            <p>Vendedor: luciana@gmail.com / Vendedor@123</p>
          </div>
        </div>
      </div>
    </div>
  )
}

function Feature({ icon: Icon, text }: { icon: typeof BarChart3; text: string }) {
  return (
    <div className="flex items-center gap-3">
      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-white/15">
        <Icon className="h-4 w-4" />
      </div>
      <p className="text-sm text-indigo-50">{text}</p>
    </div>
  )
}
