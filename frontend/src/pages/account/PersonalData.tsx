import { useState, type FormEvent } from 'react'
import { RiLoader4Line, RiInformationLine } from 'react-icons/ri'
import { useAuth } from '../../context/AuthContext'
import { useToast } from '../../context/ToastContext'
import { getAuthErrorMessage } from '../../lib/authErrors'

export function PersonalData() {
  const { user, updateProfile } = useAuth()
  const { showToast } = useToast()
  const [isEditing, setIsEditing] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [name, setName] = useState(user?.name ?? '')
  const [email, setEmail] = useState(user?.email ?? '')

  if (!user) return null

  const startEditing = () => {
    setName(user.name)
    setEmail(user.email)
    setIsEditing(true)
  }

  const cancelEditing = () => {
    setIsEditing(false)
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()

    if (!name || !email) {
      showToast('Preencha nome e e-mail.')
      return
    }

    setIsSaving(true)
    try {
      await updateProfile({ name, email })
      showToast('Dados atualizados com sucesso.', 'success')
      setIsEditing(false)
    } catch (err) {
      showToast(getAuthErrorMessage(err))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <section>
      <div className="flex items-center justify-between">
        <h2 className="text-sm uppercase tracking-wider text-muted">Dados pessoais</h2>
        {!isEditing && (
          <button
            type="button"
            onClick={startEditing}
            className="cursor-pointer border border-border px-4 py-2 text-sm text-foreground transition-colors hover:border-foreground"
          >
            Editar
          </button>
        )}
      </div>

      {isEditing ? (
        <form onSubmit={handleSubmit} noValidate className="mt-4 flex flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm text-muted">
            Nome
            <input
              type="text"
              autoFocus
              value={name}
              onChange={(event) => setName(event.target.value)}
              className="border border-border px-3 py-2 text-sm text-foreground outline-none"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-muted">
            E-mail
            <input
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              className="border border-border px-3 py-2 text-sm text-foreground outline-none"
            />
          </label>

          {user.cpf && (
            <div>
              <p className="text-sm text-muted">CPF</p>
              <p className="mt-0.5 text-sm text-foreground">{user.cpf}</p>
              <p className="mt-1 flex items-center gap-1.5 text-xs text-muted">
                <RiInformationLine className="size-3.5 shrink-0" />
                O CPF não pode ser alterado.
              </p>
            </div>
          )}

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
              onClick={cancelEditing}
              disabled={isSaving}
              className="cursor-pointer text-sm text-muted transition-colors hover:text-foreground disabled:cursor-not-allowed"
            >
              Cancelar
            </button>
          </div>
        </form>
      ) : (
        <dl className="mt-4 flex flex-col gap-3 text-sm">
          <div>
            <dt className="text-muted">Nome</dt>
            <dd className="mt-0.5 text-foreground">{user.name}</dd>
          </div>
          <div>
            <dt className="text-muted">E-mail</dt>
            <dd className="mt-0.5 text-foreground">{user.email}</dd>
          </div>
          {user.cpf && (
            <div>
              <dt className="text-muted">CPF</dt>
              <dd className="mt-0.5 text-foreground">{user.cpf}</dd>
            </div>
          )}
        </dl>
      )}
    </section>
  )
}
