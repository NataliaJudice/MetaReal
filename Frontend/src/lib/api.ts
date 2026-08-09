import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { tokenStore } from './tokenStore'
import type { ApiEnvelope, LoginResponse } from '@/types'

export const API_BASE_URL = (import.meta.env.VITE_API_URL as string | undefined) ?? 'http://localhost:5157/api'

export const api = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
  timeout: 15000
})

api.interceptors.request.use((config) => {
  const token = tokenStore.get()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// o token fica só em memória, não em localStorage, pra não ficar exposto a XSS

interface RetryableConfig extends InternalAxiosRequestConfig {
  _retry?: boolean
}

// NÃO chamar /auth/refresh direto em outro lugar, usar sempre essa função aqui.
// o refresh token é de uso único (o backend revoga o antigo e devolve um novo), então duas
// chamadas ao mesmo tempo = a segunda chega com um token que já foi queimado e toma 401,
// derrubando a sessão que a primeira tinha acabado de validar.
// isso me pegou no StrictMode do react, que monta o efeito duas vezes em dev e eu ficava
// deslogado sozinho toda vez que abria o app. essa promise compartilhada resolve.
let refreshPromise: Promise<LoginResponse | null> | null = null

export async function refreshAccessToken(): Promise<LoginResponse | null> {
  if (!refreshPromise) {
    refreshPromise = axios
      .post<ApiEnvelope<LoginResponse>>(`${API_BASE_URL}/auth/refresh`, {}, { withCredentials: true })
      .then((res) => {
        tokenStore.set(res.data.data.accessToken)
        return res.data.data
      })
      .catch(() => null)
      .finally(() => {
        refreshPromise = null
      })
  }
  return refreshPromise
}
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as RetryableConfig | undefined
    const isAuthRoute = original?.url?.includes('/auth/login') || original?.url?.includes('/auth/refresh')

    if (error.response?.status === 401 && original && !original._retry && !isAuthRoute) {
      original._retry = true
      const resultado = await refreshAccessToken()
      if (resultado) {
        original.headers.Authorization = `Bearer ${resultado.accessToken}`
        return api(original)
      }
      tokenStore.notifySessionExpired()
    }
    return Promise.reject(error)
  }
)
export function extractErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as { error?: string; errors?: Record<string, string[]> } | undefined
    if (data?.errors) {
      return Object.values(data.errors).flat().join(' ')
    }
    if (data?.error) {
      return data.error
    }
    if (error.code === 'ECONNABORTED') {
      return 'A requisição demorou demais para responder. Verifique sua conexão e tente novamente.'
    }
    if (!error.response) {
      return 'Não foi possível conectar ao servidor. Verifique sua internet e tente novamente.'
    }
  }
  return 'Ocorreu um erro inesperado. Tente novamente.'
}
