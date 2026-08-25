import { useState, type FormEvent } from 'react'
import { RiLoader4Line, RiInformationLine } from 'react-icons/ri'
import { useAuth } from '../../context/AuthContext'
import { useToast } from '../../context/ToastContext'
import { getAuthErrorMessage } from '../../lib/authErrors'
import { formatCpf } from '../../lib/cpf'

export function PersonalData() {
  const { user, updateProfile } = useAuth()
  const { showToast } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const [name, setName] = useState(user?.name ?? '')
  const [email, setEmail] = useState(user?.email ?? '')

  if (!user) return null

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
    } catch (err) {
      showToast(getAuthErrorMessage(err))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <section>
      <h2 className="text-sm uppercase tracking-wider text-muted">Dados pessoais</h2>

      <form onSubmit={handleSubmit} noValidate className="mt-4 flex flex-col gap-4">
        <div className="grid grid-cols-2 gap-4">
          <label className="flex flex-col gap-1 text-sm text-muted">
            Nome
            <input
              type="text"
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
        </div>

        {user.cpf && (
          <div>
            <p className="text-sm text-muted">CPF</p>
            <p className="mt-0.5 text-sm text-foreground">{formatCpf(user.cpf)}</p>
            <p className="mt-1 flex items-center gap-1.5 text-xs text-muted">
              <RiInformationLine className="size-3.5 shrink-0" />
              O CPF não pode ser alterado.
            </p>
          </div>
        )}

        <button
          type="submit"
          disabled={isSaving}
          className="mt-2 flex w-fit cursor-pointer items-center gap-2 bg-moon-900 px-6 py-2.5 text-sm uppercase tracking-wider text-white transition-opacity disabled:cursor-not-allowed disabled:opacity-40"
        >
          {isSaving && <RiLoader4Line className="size-4 animate-spin" />}
          Salvar
        </button>
      </form>
    </section>
  )
}
