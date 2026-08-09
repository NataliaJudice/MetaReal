type Listener = () => void

let accessToken: string | null = null
const sessionExpiredListeners = new Set<Listener>()

export const tokenStore = {
  get: () => accessToken,
  set: (token: string | null) => {
    accessToken = token
  },
  onSessionExpired: (fn: Listener) => {
    sessionExpiredListeners.add(fn)
    return () => sessionExpiredListeners.delete(fn)
  },
  notifySessionExpired: () => {
    accessToken = null
    sessionExpiredListeners.forEach((fn) => fn())
  }
}
