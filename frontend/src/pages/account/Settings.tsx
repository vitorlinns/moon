import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { RiLoader4Line } from 'react-icons/ri'
import { useAuth } from '../../context/AuthContext'
import { useToast } from '../../context/ToastContext'
import { getAuthErrorMessage } from '../../lib/authErrors'
import { PasswordInput } from '../../components/PasswordInput'

export function Settings() {
  const { changePassword, deleteAccount } = useAuth()
  const { showToast } = useToast()
  const navigate = useNavigate()

  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmNewPassword, setConfirmNewPassword] = useState('')
  const [isChangingPassword, setIsChangingPassword] = useState(false)

  const [deletePassword, setDeletePassword] = useState('')
  const [isDeleting, setIsDeleting] = useState(false)

  const handleChangePassword = async (event: FormEvent) => {
    event.preventDefault()

    if (!currentPassword || !newPassword || !confirmNewPassword) {
      showToast('Preencha todos os campos.')
      return
    }

    if (newPassword.length < 8) {
      showToast('A nova senha precisa ter no mínimo 8 caracteres.')
      return
    }

    if (newPassword !== confirmNewPassword) {
      showToast('As senhas não coincidem.')
      return
    }

    setIsChangingPassword(true)
    try {
      await changePassword(currentPassword, newPassword)
      showToast('Senha alterada com sucesso.', 'success')
      setCurrentPassword('')
      setNewPassword('')
      setConfirmNewPassword('')
    } catch (err) {
      showToast(getAuthErrorMessage(err))
    } finally {
      setIsChangingPassword(false)
    }
  }

  const handleDeleteAccount = async (event: FormEvent) => {
    event.preventDefault()

    if (!deletePassword) {
      showToast('Informe sua senha pra confirmar.')
      return
    }

    if (!window.confirm('Tem certeza que quer excluir sua conta? Essa ação não pode ser desfeita.')) {
      return
    }

    setIsDeleting(true)
    try {
      await deleteAccount(deletePassword)
      navigate('/')
    } catch (err) {
      showToast(getAuthErrorMessage(err))
      setIsDeleting(false)
    }
  }

  return (
    <section className="flex flex-col gap-10">
      <div>
        <h2 className="text-sm uppercase tracking-wider text-muted">Trocar senha</h2>

        <form onSubmit={handleChangePassword} noValidate className="mt-4 flex max-w-sm flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm text-muted">
            Senha atual
            <PasswordInput value={currentPassword} onChange={setCurrentPassword} placeholder="Digite sua senha atual" />
          </label>

          <label className="flex flex-col gap-1 text-sm text-muted">
            Nova senha
            <PasswordInput value={newPassword} onChange={setNewPassword} placeholder="Mínimo de 8 caracteres" />
          </label>

          <label className="flex flex-col gap-1 text-sm text-muted">
            Confirmar nova senha
            <PasswordInput value={confirmNewPassword} onChange={setConfirmNewPassword} placeholder="Repita a nova senha" />
          </label>

          <button
            type="submit"
            disabled={isChangingPassword}
            className="mt-2 flex w-fit cursor-pointer items-center gap-2 bg-moon-900 px-6 py-2.5 text-sm uppercase tracking-wider text-white transition-opacity disabled:cursor-not-allowed disabled:opacity-40"
          >
            {isChangingPassword && <RiLoader4Line className="size-4 animate-spin" />}
            Salvar nova senha
          </button>
        </form>
      </div>

      <div className="border-t border-border pt-8">
        <h2 className="text-sm uppercase tracking-wider text-danger">Excluir conta</h2>
        <p className="mt-2 max-w-sm text-sm text-muted">
          Isso remove permanentemente sua conta, endereços e sessões ativas. Essa ação não pode ser desfeita.
        </p>

        <form onSubmit={handleDeleteAccount} noValidate className="mt-4 flex max-w-sm flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm text-muted">
            Confirme sua senha
            <PasswordInput value={deletePassword} onChange={setDeletePassword} placeholder="Digite sua senha" />
          </label>

          <button
            type="submit"
            disabled={isDeleting}
            className="flex w-fit cursor-pointer items-center gap-2 border border-danger px-6 py-2.5 text-sm uppercase tracking-wider text-danger transition-colors hover:bg-danger hover:text-white disabled:cursor-not-allowed disabled:opacity-40"
          >
            {isDeleting && <RiLoader4Line className="size-4 animate-spin" />}
            Excluir minha conta
          </button>
        </form>
      </div>
    </section>
  )
}
