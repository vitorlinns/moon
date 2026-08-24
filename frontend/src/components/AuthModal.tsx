import { useState, type FormEvent } from 'react'
import type { IconType } from 'react-icons'
import { RiCloseLine, RiGoogleFill, RiFacebookFill, RiAppleFill, RiLoader4Line } from 'react-icons/ri'
import { useAuth, type OAuthProvider } from '../context/AuthContext'
import { useToast } from '../context/ToastContext'
import { formatCpf } from '../lib/cpf'
import { getAuthErrorMessage } from '../lib/authErrors'

type Mode = 'login' | 'register'
type RegisterStep = 'cpf' | 'details'

const socialProviders: { id: OAuthProvider; label: string; icon: IconType }[] = [
  { id: 'google', label: 'Google', icon: RiGoogleFill },
  { id: 'facebook', label: 'Facebook', icon: RiFacebookFill },
  { id: 'apple', label: 'Apple', icon: RiAppleFill },
]

export function AuthModal() {
  const { isModalOpen, closeModal, login, register, loginWithProvider } = useAuth()
  const { showToast } = useToast()
  const [mode, setMode] = useState<Mode>('login')
  const [registerStep, setRegisterStep] = useState<RegisterStep>('cpf')
  const [cpf, setCpf] = useState('')
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const resetForm = () => {
    setRegisterStep('cpf')
    setCpf('')
    setName('')
    setEmail('')
    setPassword('')
  }

  const switchMode = (nextMode: Mode) => {
    setMode(nextMode)
    resetForm()
  }

  const handleClose = () => {
    closeModal()
    resetForm()
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()

    if (mode === 'register' && registerStep === 'cpf') {
      if (cpf.replace(/\D/g, '').length !== 11) {
        showToast('Informe um CPF válido.')
        return
      }
      setRegisterStep('details')
      return
    }

    if (mode === 'register' && (!name || !email || !password)) {
      showToast('Preencha nome, e-mail e senha.')
      return
    }

    if (mode === 'login' && (!email || !password)) {
      showToast('Preencha e-mail e senha.')
      return
    }

    if (password && password.length < 8) {
      showToast('A senha precisa ter no mínimo 8 caracteres.')
      return
    }

    setIsSubmitting(true)
    try {
      if (mode === 'login') {
        await login(email, password)
      } else {
        await register({ cpf, name, email, password })
      }
      resetForm()
    } catch (err) {
      showToast(getAuthErrorMessage(err))
    } finally {
      setIsSubmitting(false)
    }
  }

  const submitLabel =
    mode === 'login' ? 'Entrar' : registerStep === 'cpf' ? 'Continuar' : 'Criar conta'

  return (
    <div
      className={`fixed inset-0 z-30 flex items-center justify-center px-6 ${
        isModalOpen ? '' : 'pointer-events-none'
      }`}
      aria-hidden={!isModalOpen}
    >
      <div
        onClick={handleClose}
        className={`absolute inset-0 bg-moon-900/40 transition-opacity ${
          isModalOpen ? 'opacity-100' : 'opacity-0'
        }`}
      />

      <div
        className={`relative w-full max-w-sm bg-surface p-6 transition-all duration-200 ${
          isModalOpen ? 'translate-y-0 opacity-100' : 'translate-y-2 opacity-0'
        }`}
      >
        <div className="flex items-center justify-between">
          <h2 className="text-sm uppercase tracking-wider text-foreground">
            {mode === 'login' ? 'Entrar' : 'Criar conta'}
          </h2>
          <button
            type="button"
            aria-label="Fechar"
            onClick={handleClose}
            className="cursor-pointer text-foreground"
          >
            <RiCloseLine className="size-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} noValidate className="mt-6 flex flex-col gap-4">
          {mode === 'register' && registerStep === 'cpf' && (
            <label className="flex flex-col gap-1 text-sm text-muted">
              CPF
              <input
                type="text"
                inputMode="numeric"
                placeholder="000.000.000-00"
                autoFocus
                maxLength={14}
                value={cpf}
                onChange={(event) => setCpf(formatCpf(event.target.value))}
                className="border border-border px-3 py-2 text-sm text-foreground outline-none"
              />
            </label>
          )}

          {mode === 'register' && registerStep === 'details' && (
            <>
              <button
                type="button"
                onClick={() => setRegisterStep('cpf')}
                className="-mb-2 w-fit cursor-pointer text-xs text-muted transition-colors hover:text-foreground"
              >
                CPF {cpf} · alterar
              </button>

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
            </>
          )}

          {(mode === 'login' || registerStep === 'details') && (
            <>
              <label className="flex flex-col gap-1 text-sm text-muted">
                E-mail
                <input
                  type="email"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  className="border border-border px-3 py-2 text-sm text-foreground outline-none"
                />
              </label>

              <label className="flex flex-col gap-1 text-sm text-muted">
                Senha
                <input
                  type="password"
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  className="border border-border px-3 py-2 text-sm text-foreground outline-none"
                />
              </label>
            </>
          )}

          <button
            type="submit"
            disabled={isSubmitting}
            className="mt-2 flex cursor-pointer items-center justify-center gap-2 bg-moon-900 py-3 text-sm uppercase tracking-wider text-white transition-opacity disabled:cursor-not-allowed disabled:opacity-40"
          >
            {isSubmitting && <RiLoader4Line className="size-4 animate-spin" />}
            {submitLabel}
          </button>
        </form>

        {mode === 'login' && (
          <div className="mt-6 flex flex-col gap-4">
            <div className="flex items-center gap-3 text-xs uppercase tracking-wider text-muted">
              <span className="h-px flex-1 bg-border" />
              ou
              <span className="h-px flex-1 bg-border" />
            </div>

            <div className="flex items-center justify-center gap-3">
              {socialProviders.map((provider) => (
                <button
                  key={provider.id}
                  type="button"
                  aria-label={`Continuar com ${provider.label}`}
                  onClick={() => loginWithProvider(provider.id)}
                  className="flex size-11 cursor-pointer items-center justify-center border border-border text-foreground transition-colors hover:border-foreground"
                >
                  <provider.icon className="size-5" />
                </button>
              ))}
            </div>
          </div>
        )}

        <button
          type="button"
          onClick={() => switchMode(mode === 'login' ? 'register' : 'login')}
          className="mt-4 w-full cursor-pointer text-center text-sm text-muted transition-colors hover:text-foreground"
        >
          {mode === 'login' ? 'Não tem conta? Criar conta' : 'Já tem conta? Entrar'}
        </button>
      </div>
    </div>
  )
}
