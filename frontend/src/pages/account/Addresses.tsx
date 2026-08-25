import { useEffect, useState, type FormEvent } from 'react'
import { RiMapPinLine, RiLoader4Line, RiPencilLine, RiDeleteBinLine, RiStarFill, RiStarLine } from 'react-icons/ri'
import { apiFetch, ApiError } from '../../lib/api'
import { useToast } from '../../context/ToastContext'
import { formatCep } from '../../lib/cep'
import { BRAZILIAN_STATES } from '../../lib/states'
import { EmptyState } from '../../components/EmptyState'

interface Address {
  id: string
  label: string
  recipient: string
  cep: string
  street: string
  number: string
  complement: string | null
  neighborhood: string
  city: string
  state: string
  isDefault: boolean
}

type AddressFormData = Omit<Address, 'id' | 'isDefault'>

const emptyForm: AddressFormData = {
  label: '',
  recipient: '',
  cep: '',
  street: '',
  number: '',
  complement: '',
  neighborhood: '',
  city: '',
  state: '',
}

function getErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    return err.message || 'Não foi possível completar a solicitação.'
  }
  return 'Não foi possível conectar ao servidor. Tente novamente.'
}

export function Addresses() {
  const { showToast } = useToast()
  const [addresses, setAddresses] = useState<Address[] | null>(null)
  const [isFormOpen, setIsFormOpen] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState<AddressFormData>(emptyForm)
  const [isSaving, setIsSaving] = useState(false)
  const [pendingActionId, setPendingActionId] = useState<string | null>(null)

  const loadAddresses = () => {
    apiFetch<Address[]>('/addresses')
      .then(setAddresses)
      .catch(() => setAddresses([]))
  }

  useEffect(() => {
    loadAddresses()
  }, [])

  const startCreate = () => {
    setEditingId(null)
    setForm(emptyForm)
    setIsFormOpen(true)
  }

  const startEdit = (address: Address) => {
    setEditingId(address.id)
    setForm({
      label: address.label,
      recipient: address.recipient,
      cep: formatCep(address.cep),
      street: address.street,
      number: address.number,
      complement: address.complement ?? '',
      neighborhood: address.neighborhood,
      city: address.city,
      state: address.state,
    })
    setIsFormOpen(true)
  }

  const cancelForm = () => {
    setIsFormOpen(false)
    setEditingId(null)
    setForm(emptyForm)
  }

  const updateField = <K extends keyof AddressFormData>(field: K, value: AddressFormData[K]) => {
    setForm((current) => ({ ...current, [field]: value }))
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()

    if (
      !form.label ||
      !form.recipient ||
      !form.cep ||
      !form.street ||
      !form.number ||
      !form.neighborhood ||
      !form.city ||
      !form.state
    ) {
      showToast('Preencha todos os campos obrigatórios.')
      return
    }

    const editingAddress = addresses?.find((address) => address.id === editingId)
    const payload = { ...form, isDefault: editingAddress?.isDefault ?? false }

    setIsSaving(true)
    try {
      if (editingId) {
        await apiFetch<Address>(`/addresses/${editingId}`, { method: 'PUT', body: JSON.stringify(payload) })
        showToast('Endereço atualizado.', 'success')
      } else {
        await apiFetch<Address>('/addresses', { method: 'POST', body: JSON.stringify(payload) })
        showToast('Endereço adicionado.', 'success')
      }
      cancelForm()
      loadAddresses()
    } catch (err) {
      showToast(getErrorMessage(err))
    } finally {
      setIsSaving(false)
    }
  }

  const handleDelete = async (id: string) => {
    if (!window.confirm('Remover este endereço?')) return

    setPendingActionId(id)
    try {
      await apiFetch(`/addresses/${id}`, { method: 'DELETE' })
      showToast('Endereço removido.', 'success')
      loadAddresses()
    } catch (err) {
      showToast(getErrorMessage(err))
    } finally {
      setPendingActionId(null)
    }
  }

  const handleSetDefault = async (id: string) => {
    setPendingActionId(id)
    try {
      await apiFetch(`/addresses/${id}/default`, { method: 'POST' })
      loadAddresses()
    } catch (err) {
      showToast(getErrorMessage(err))
    } finally {
      setPendingActionId(null)
    }
  }

  return (
    <section>
      <div className="flex items-center justify-between">
        <h2 className="text-sm uppercase tracking-wider text-muted">Endereços</h2>
        {!isFormOpen && addresses && addresses.length > 0 && (
          <button
            type="button"
            onClick={startCreate}
            className="cursor-pointer border border-border px-4 py-2 text-sm text-foreground transition-colors hover:border-foreground"
          >
            Adicionar endereço
          </button>
        )}
      </div>

      {isFormOpen && (
        <form onSubmit={handleSubmit} noValidate className="mt-4 flex flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm text-muted">
            Nome do endereço
            <input
              type="text"
              placeholder="Casa, trabalho..."
              autoFocus
              value={form.label}
              onChange={(event) => updateField('label', event.target.value)}
              className="border border-border px-3 py-2 text-sm text-foreground outline-none"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-muted">
            Quem recebe
            <input
              type="text"
              value={form.recipient}
              onChange={(event) => updateField('recipient', event.target.value)}
              className="border border-border px-3 py-2 text-sm text-foreground outline-none"
            />
          </label>

          <div className="grid grid-cols-2 gap-4">
            <label className="flex flex-col gap-1 text-sm text-muted">
              CEP
              <input
                type="text"
                inputMode="numeric"
                placeholder="00000-000"
                maxLength={9}
                value={form.cep}
                onChange={(event) => updateField('cep', formatCep(event.target.value))}
                className="border border-border px-3 py-2 text-sm text-foreground outline-none"
              />
            </label>

            <label className="flex flex-col gap-1 text-sm text-muted">
              Estado
              <select
                value={form.state}
                onChange={(event) => updateField('state', event.target.value)}
                className="border border-border bg-surface px-3 py-2 text-sm text-foreground outline-none"
              >
                <option value="">Selecione</option>
                {BRAZILIAN_STATES.map(([code, name]) => (
                  <option key={code} value={code}>
                    {code} — {name}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <label className="flex flex-col gap-1 text-sm text-muted">
            Rua
            <input
              type="text"
              value={form.street}
              onChange={(event) => updateField('street', event.target.value)}
              className="border border-border px-3 py-2 text-sm text-foreground outline-none"
            />
          </label>

          <div className="grid grid-cols-2 gap-4">
            <label className="flex flex-col gap-1 text-sm text-muted">
              Número
              <input
                type="text"
                value={form.number}
                onChange={(event) => updateField('number', event.target.value)}
                className="border border-border px-3 py-2 text-sm text-foreground outline-none"
              />
            </label>

            <label className="flex flex-col gap-1 text-sm text-muted">
              Complemento
              <input
                type="text"
                placeholder="Opcional"
                value={form.complement ?? ''}
                onChange={(event) => updateField('complement', event.target.value)}
                className="border border-border px-3 py-2 text-sm text-foreground outline-none"
              />
            </label>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <label className="flex flex-col gap-1 text-sm text-muted">
              Bairro
              <input
                type="text"
                value={form.neighborhood}
                onChange={(event) => updateField('neighborhood', event.target.value)}
                className="border border-border px-3 py-2 text-sm text-foreground outline-none"
              />
            </label>

            <label className="flex flex-col gap-1 text-sm text-muted">
              Cidade
              <input
                type="text"
                value={form.city}
                onChange={(event) => updateField('city', event.target.value)}
                className="border border-border px-3 py-2 text-sm text-foreground outline-none"
              />
            </label>
          </div>

          <div className="mt-2 flex items-center gap-3">
            <button
              type="submit"
              disabled={isSaving}
              className="flex cursor-pointer items-center gap-2 bg-moon-900 px-6 py-2.5 text-sm uppercase tracking-wider text-white transition-opacity disabled:cursor-not-allowed disabled:opacity-40"
            >
              {isSaving && <RiLoader4Line className="size-4 animate-spin" />}
              Salvar
            </button>
            <button
              type="button"
              onClick={cancelForm}
              disabled={isSaving}
              className="cursor-pointer text-sm text-muted transition-colors hover:text-foreground disabled:cursor-not-allowed"
            >
              Cancelar
            </button>
          </div>
        </form>
      )}

      {!isFormOpen && addresses === null && (
        <p className="mt-4 text-sm text-muted">Carregando...</p>
      )}

      {!isFormOpen && addresses !== null && addresses.length === 0 && (
        <div className="mt-4">
          <EmptyState
            icon={RiMapPinLine}
            message="Nenhum endereço cadastrado ainda."
            action={
              <button
                type="button"
                onClick={startCreate}
                className="mt-2 cursor-pointer bg-moon-900 px-6 py-3 text-sm uppercase tracking-wider text-white transition-opacity hover:opacity-90"
              >
                Adicionar endereço
              </button>
            }
          />
        </div>
      )}

      {!isFormOpen && addresses !== null && addresses.length > 0 && (
        <ul className="mt-4 flex flex-col gap-3">
          {addresses.map((address) => (
            <li key={address.id} className="border border-border p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <div className="flex items-center gap-2">
                    <p className="text-sm text-foreground">{address.label}</p>
                    {address.isDefault && (
                      <span className="border border-border px-1.5 py-0.5 text-xs uppercase tracking-wider text-muted">
                        Padrão
                      </span>
                    )}
                  </div>
                  <p className="mt-1 text-sm text-muted">{address.recipient}</p>
                  <p className="mt-1 text-sm text-muted">
                    {address.street}, {address.number}
                    {address.complement ? ` — ${address.complement}` : ''}
                  </p>
                  <p className="text-sm text-muted">
                    {address.neighborhood}, {address.city} - {address.state}
                  </p>
                  <p className="text-sm text-muted">{formatCep(address.cep)}</p>
                </div>

                <div className="flex shrink-0 items-center gap-1">
                  {!address.isDefault && (
                    <button
                      type="button"
                      aria-label="Tornar padrão"
                      title="Tornar padrão"
                      disabled={pendingActionId === address.id}
                      onClick={() => handleSetDefault(address.id)}
                      className="cursor-pointer p-2 text-muted transition-colors hover:text-foreground disabled:cursor-not-allowed"
                    >
                      <RiStarLine className="size-4" />
                    </button>
                  )}
                  {address.isDefault && (
                    <span className="p-2 text-moon-400" title="Endereço padrão">
                      <RiStarFill className="size-4" />
                    </span>
                  )}
                  <button
                    type="button"
                    aria-label="Editar"
                    onClick={() => startEdit(address)}
                    className="cursor-pointer p-2 text-muted transition-colors hover:text-foreground"
                  >
                    <RiPencilLine className="size-4" />
                  </button>
                  <button
                    type="button"
                    aria-label="Remover"
                    disabled={pendingActionId === address.id}
                    onClick={() => handleDelete(address.id)}
                    className="cursor-pointer p-2 text-muted transition-colors hover:text-danger disabled:cursor-not-allowed"
                  >
                    <RiDeleteBinLine className="size-4" />
                  </button>
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
