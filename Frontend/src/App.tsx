import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider, useAuth } from '@/context/AuthContext'
import { ToastProvider } from '@/context/ToastContext'
import { ProtectedRoute } from '@/components/ProtectedRoute'
import { Layout } from '@/components/Layout'
import { LoginPage } from '@/pages/LoginPage'
import { DashboardPage } from '@/pages/DashboardPage'
import { LancamentoPage } from '@/pages/LancamentoPage'
import { VendedoresPage } from '@/pages/VendedoresPage'
import { VendedorPerfilPage } from '@/pages/VendedorPerfilPage'
import { MetasPage } from '@/pages/MetasPage'
import { RelatoriosPage } from '@/pages/RelatoriosPage'
import { RelatorioDetalhePage } from '@/pages/RelatorioDetalhePage'
import { AuditoriaPage } from '@/pages/AuditoriaPage'
import { NotFoundPage } from '@/pages/NotFoundPage'

function HomeRedirect() {
  const { usuario } = useAuth()
  if (!usuario) return <Navigate to="/login" replace />
  return <Navigate to={usuario.role === 'Gerente' ? '/dashboard' : '/lancamento'} replace />
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route path="/" element={<HomeRedirect />} />

      <Route
        path="/dashboard"
        element={
          <ProtectedRoute roles={['Gerente']}>
            <Layout>
              <DashboardPage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/lancamento"
        element={
          <ProtectedRoute>
            <Layout>
              <LancamentoPage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/vendedores"
        element={
          <ProtectedRoute roles={['Gerente']}>
            <Layout>
              <VendedoresPage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/vendedores/:id/perfil"
        element={
          <ProtectedRoute>
            <Layout>
              <VendedorPerfilPage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/metas"
        element={
          <ProtectedRoute>
            <Layout>
              <MetasPage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/relatorios"
        element={
          <ProtectedRoute>
            <Layout>
              <RelatoriosPage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/relatorios/:chave"
        element={
          <ProtectedRoute>
            <Layout>
              <RelatorioDetalhePage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/auditoria"
        element={
          <ProtectedRoute roles={['Gerente']}>
            <Layout>
              <AuditoriaPage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}

export default function App() {
  return (
    <BrowserRouter>
      <ToastProvider>
        <AuthProvider>
          <AppRoutes />
        </AuthProvider>
      </ToastProvider>
    </BrowserRouter>
  )
}
