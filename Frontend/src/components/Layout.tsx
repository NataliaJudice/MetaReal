import { useState, type ReactNode } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { LayoutDashboard, NotebookPen, Users, ShieldCheck, LogOut, Menu, X, TrendingUp, Target, FileText } from 'lucide-react'
import { useAuth } from '@/context/AuthContext'
import { useToast } from '@/context/ToastContext'

interface NavItem {
  to: string
  label: string
  icon: typeof LayoutDashboard
}

export function Layout({ children }: { children: ReactNode }) {
  const { usuario, logout } = useAuth()
  const { showToast } = useToast()
  const navigate = useNavigate()
  const [menuAberto, setMenuAberto] = useState(false)

  const navItems: NavItem[] =
    usuario?.role === 'Gerente'
      ? [
          { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
          { to: '/lancamento', label: 'Lançamento', icon: NotebookPen },
          { to: '/metas', label: 'Metas', icon: Target },
          { to: '/relatorios', label: 'Relatórios', icon: FileText },
          { to: '/vendedores', label: 'Vendedores', icon: Users },
          { to: '/auditoria', label: 'Auditoria', icon: ShieldCheck }
        ]
      : [
          { to: '/lancamento', label: 'Lançamento', icon: NotebookPen },
          { to: '/metas', label: 'Metas', icon: Target },
          { to: `/vendedores/${usuario?.vendedorId}/perfil`, label: 'Meu Desempenho', icon: TrendingUp }
        ]

  const handleLogout = async () => {
    await logout()
    showToast('Sessão encerrada.', 'info')
    navigate('/login', { replace: true })
  }

  return (
    <div className="flex min-h-screen bg-slate-50">
      <aside className="sticky top-0 hidden h-screen w-64 shrink-0 flex-col border-r border-slate-200 bg-slate-900 text-slate-100 md:flex">
        <SidebarContent navItems={navItems} usuarioNome={usuario?.nome} role={usuario?.role} onLogout={handleLogout} />
      </aside>

      {menuAberto && (
        <div className="fixed inset-0 z-40 md:hidden">
          <div className="absolute inset-0 bg-black/40" onClick={() => setMenuAberto(false)} />
          <aside className="fixed inset-y-0 left-0 flex h-full w-64 flex-col bg-slate-900 text-slate-100 shadow-xl">
            <div className="flex justify-end p-3">
              <button onClick={() => setMenuAberto(false)} className="rounded-lg p-1.5 hover:bg-white/10" aria-label="Fechar menu">
                <X className="h-5 w-5" />
              </button>
            </div>
            <SidebarContent
              navItems={navItems}
              usuarioNome={usuario?.nome}
              role={usuario?.role}
              onLogout={handleLogout}
              onNavigate={() => setMenuAberto(false)}
            />
          </aside>
        </div>
      )}

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-30 flex items-center justify-between border-b border-slate-200 bg-white px-4 py-3 md:hidden">
          <button onClick={() => setMenuAberto(true)} className="rounded-lg p-2 text-slate-600 hover:bg-slate-100" aria-label="Abrir menu">
            <Menu className="h-5 w-5" />
          </button>
          <span className="text-sm font-bold text-slate-900">MetaReal</span>
          <div className="w-9" />
        </header>

        <main className="flex-1 px-4 py-6 sm:px-6 lg:px-10 lg:py-8">
          <div className="mx-auto max-w-7xl">{children}</div>
        </main>
      </div>
    </div>
  )
}

function SidebarContent({
  navItems,
  usuarioNome,
  role,
  onLogout,
  onNavigate
}: {
  navItems: NavItem[]
  usuarioNome?: string
  role?: string
  onLogout: () => void
  onNavigate?: () => void
}) {
  return (
    <div className="flex h-full flex-col">
      <div className="flex items-center gap-2.5 px-5 py-6">
        <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-indigo-500 font-bold text-white">MR</div>
        <div className="leading-tight">
          <p className="text-sm font-bold text-white">MetaReal</p>
          <p className="text-xs text-slate-400"></p>
        </div>
      </div>

      <nav className="flex-1 space-y-1 px-3 overflow-y-auto">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            onClick={onNavigate}
            className={({ isActive }) =>
              `flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-colors ${
                isActive ? 'bg-indigo-600 text-white shadow-sm' : 'text-slate-300 hover:bg-white/5 hover:text-white'
              }`
            }
          >
            <item.icon className="h-5 w-5 shrink-0" />
            {item.label}
          </NavLink>
        ))}
      </nav>

      <div className="border-t border-white/10 p-3 mt-auto">
        <div className="flex items-center gap-3 rounded-xl px-3 py-2.5">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-slate-700 text-sm font-bold text-white">
            {usuarioNome?.charAt(0)?.toUpperCase() ?? '?'}
          </div>
          <div className="min-w-0 leading-tight">
            <p className="truncate text-sm font-semibold text-white">{usuarioNome}</p>
            <p className="text-xs text-slate-400">{role === 'Gerente' ? 'Gerente' : 'Vendedor'}</p>
          </div>
        </div>
        <button
          onClick={onLogout}
          className="mt-2 flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium text-slate-300 transition-colors hover:bg-white/5 hover:text-white"
        >
          <LogOut className="h-5 w-5" />
          Sair
        </button>
      </div>
    </div>
  )
}
