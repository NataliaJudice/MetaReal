import { useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { UserPlus, Pencil, Trash2, Users, TrendingUp } from 'lucide-react'
import { api, extractErrorMessage } from '@/lib/api'
import { useToast } from '@/context/ToastContext'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Modal } from '@/components/ui/Modal'
import { Spinner } from '@/components/ui/Spinner'
import { EmptyState } from '@/components/ui/EmptyState'
import type { ApiEnvelope, Vendedor } from '@/types'

export function VendedoresPage() {
  const { showToast } = useToast()
  const [vendedores, setVendedores] = useState<Vendedor[]>([])
  const [carregando, setCarregando] = useState(true)
  const [modalAberto, setModalAberto] = useState(false)
  const [editando, setEditando] = useState<Vendedor | null>(null)
  const [nome, setNome] = useState('')
  const [erro, setErro] = useState<string | null>(null)
  const [salvando, setSalvando] = useState(false)

  const carregar = async () => {
    setCarregando(true)
    try {
      const res = await api.get<ApiEnvelope<Vendedor[]>>('/vendedores')
      setVendedores(res.data.data)
    } catch (err) {
      showToast(extractErrorMessage(err), 'error')
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    carregar()
  }, [])

  const abrirNovo = () => {
    setEditando(null)
    setNome('')
    setErro(null)
    setModalAberto(true)
  }

  const abrirEdicao = (vendedor: Vendedor) => {
    setEditando(vendedor)
    setNome(vendedor.nome)
    setErro(null)
    setModalAberto(true)
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setErro(null)
    setSalvando(true)
    try {
      if (editando) {
        await api.put(`/vendedores/${editando.id}`, { nome })
        showToast('Vendedor atualizado.', 'success')
      } else {
        await api.post('/vendedores', { nome })
        showToast('Vendedor cadastrado.', 'success')
      }
      setModalAberto(false)
      carregar()
    } catch (err) {
      setErro(extractErrorMessage(err))
    } finally {
      setSalvando(false)
    }
  }

  const excluir = async (vendedor: Vendedor) => {
    if (!confirm(`Excluir ${vendedor.nome}? Todos os registros de venda dela(e) também serão excluídos.`)) return
    try {
      await api.delete(`/vendedores/${vendedor.id}`)
      showToast('Vendedor excluído.', 'success')
      carregar()
    } catch (err) {
      showToast(extractErrorMessage(err), 'error')
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Vendedores</h1>
          <p className="text-sm text-slate-500">Gerencie a equipe de vendas.</p>
        </div>
        <Button onClick={abrirNovo}>
          <UserPlus className="h-4 w-4" /> Novo vendedor
        </Button>
      </div>

      <Card>
        {carregando ? (
          <Spinner />
        ) : vendedores.length === 0 ? (
          <EmptyState icon={Users} title="Nenhum vendedor cadastrado" action={<Button onClick={abrirNovo}>Cadastrar o primeiro</Button>} />
        ) : (
          <div className="divide-y divide-slate-100">
            {vendedores.map((v) => (
              <div key={v.id} className="flex items-center justify-between gap-3 px-5 py-4">
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-indigo-100 font-bold text-indigo-700">
                    {v.nome.charAt(0).toUpperCase()}
                  </div>
                  <p className="font-semibold text-slate-900">{v.nome}</p>
                </div>
                <div className="flex items-center gap-1">
                  <Link
                    to={`/vendedores/${v.id}/perfil`}
                    className="flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-sm font-semibold text-indigo-600 hover:bg-indigo-50"
                  >
                    <TrendingUp className="h-4 w-4" /> Perfil
                  </Link>
                  <button onClick={() => abrirEdicao(v)} className="rounded-lg p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-700" aria-label="Editar">
                    <Pencil className="h-4 w-4" />
                  </button>
                  <button onClick={() => excluir(v)} className="rounded-lg p-2 text-slate-400 hover:bg-red-50 hover:text-red-600" aria-label="Excluir">
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </Card>

      <Modal open={modalAberto} title={editando ? 'Editar vendedor' : 'Novo vendedor'} onClose={() => setModalAberto(false)}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input label="Nome" value={nome} onChange={(e) => setNome(e.target.value)} autoFocus required error={erro ?? undefined} />
          <div className="flex justify-end gap-2">
            <Button type="button" variant="secondary" onClick={() => setModalAberto(false)}>
              Cancelar
            </Button>
            <Button type="submit" loading={salvando}>
              Salvar
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
