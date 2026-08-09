import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { api, refreshAccessToken } from '@/lib/api'
import { tokenStore } from '@/lib/tokenStore'
import { iniciarConexaoNotificacoes, onNotificacao, pararConexaoNotificacoes } from '@/lib/signalr'
import { useToast } from '@/context/ToastContext'
import type { ApiEnvelope, LoginResponse, Usuario } from '@/types'

interface AuthContextValue {
  usuario: Usuario | null
  carregando: boolean
  login: (email: string, senha: string) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const { showToast } = useToast()
  const [usuario, setUsuario] = useState<Usuario | null>(null)
  const [carregando, setCarregando] = useState(true)

  const aplicarSessao = useCallback((resposta: LoginResponse) => {
    tokenStore.set(resposta.accessToken)
    setUsuario(resposta.usuario)
  }, [])

  const limparSessao = useCallback(() => {
    tokenStore.set(null)
    setUsuario(null)
    void pararConexaoNotificacoes()
  }, [])

  useEffect(() => {
    let ativo = true
    refreshAccessToken()
      .then((resultado) => {
        if (!ativo) return
        if (resultado) {
          setUsuario(resultado.usuario)
        } else {
          limparSessao()
        }
      })
      .finally(() => {
        if (ativo) {
          setCarregando(false)
        }
      })
    return () => {
      ativo = false
    }
  }, [limparSessao])

  useEffect(() => {
    const cancelar = tokenStore.onSessionExpired(limparSessao)
    return () => {
      cancelar()
    }
  }, [limparSessao])

  useEffect(() => {
    if (!usuario) {
      return
    }

    iniciarConexaoNotificacoes()
    const cancelar = onNotificacao((payload) => {
      showToast(payload.mensagem, 'success')
    })

    return () => {
      cancelar()
    }
  }, [usuario, showToast])

  const login = useCallback(
    async (email: string, senha: string) => {
      const res = await api.post<ApiEnvelope<LoginResponse>>('/auth/login', { email, senha })
      aplicarSessao(res.data.data)
    },
    [aplicarSessao]
  )

  const logout = useCallback(async () => {
    try {
      await api.post('/auth/logout')
    } finally {
      limparSessao()
    }
  }, [limparSessao])

  const value = useMemo(
    () => ({ usuario, carregando, login, logout }),
    [usuario, carregando, login, logout]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth deve ser usado dentro de <AuthProvider>')
  }
  return ctx
}
