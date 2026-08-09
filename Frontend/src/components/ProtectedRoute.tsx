import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '@/context/AuthContext'
import { FullScreenSpinner } from '@/components/ui/Spinner'
import type { Role } from '@/types'

export function ProtectedRoute({ children, roles }: { children: ReactNode; roles?: Role[] }) {
  const { usuario, carregando } = useAuth()

  if (carregando) {
    return <FullScreenSpinner />
  }

  if (!usuario) {
    return <Navigate to="/login" replace />
  }

  if (roles && !roles.includes(usuario.role)) {
    return <Navigate to="/" replace />
  }

  return <>{children}</>
}
