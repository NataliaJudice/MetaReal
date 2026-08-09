import * as signalR from '@microsoft/signalr'
import { API_BASE_URL } from './api'
import { tokenStore } from './tokenStore'

const HUB_URL = `${API_BASE_URL.replace(/\/api\/?$/, '')}/hubs/notificacoes`

export interface NotificacaoPayload {
  tipo: string
  mensagem: string
  dataHora: string
}

let connection: signalR.HubConnection | null = null

function obterConexao(): signalR.HubConnection {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, { accessTokenFactory: () => tokenStore.get() ?? '' })
      .withAutomaticReconnect()
      .build()
  }
  return connection
}

export function iniciarConexaoNotificacoes() {
  const conn = obterConexao()
  if (conn.state === signalR.HubConnectionState.Disconnected) {
    // engolindo o erro de propósito, notificação é extra. se o hub cair o app tem que continuar
    conn.start().catch(() => {
    })
  }
}

export async function pararConexaoNotificacoes() {
  if (connection) {
    try {
      await connection.stop()
    } catch {
    }
    connection = null
  }
}

export function onNotificacao(handler: (payload: NotificacaoPayload) => void) {
  const conn = obterConexao()
  conn.on('notificacao', handler)
  return () => conn.off('notificacao', handler)
}
