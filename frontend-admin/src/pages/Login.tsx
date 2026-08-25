import { useState, type FormEvent } from 'react'
import { RiLoader4Line } from 'react-icons/ri'
import { Navigate } from 'react-router-dom'
import { useAdminAuth } from '../context/AdminAuthContext'
import { ApiError } from '../lib/adminApi'
import { PasswordInput } from '../components/PasswordInput'

export function Login() {
  const { login, isAuthenticated, isLoadingAdmin } = useAdminAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  if (!isLoadingAdmin && isAuthenticated) {
    return <Navigate to="/" replace />
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)

    if (!email || !password) {
      setError('Preencha e-mail e senha.')
      return
    }

    setIsSubmitting(true)
    try {
      await login(email, password)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Não foi possível entrar. Tente novamente.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="flex min-h-svh items-center justify-center bg-background px-6">
      <div className="w-full max-w-sm bg-surface p-6">
        <h1 className="text-sm uppercase tracking-wider text-foreground">Moon Admin</h1>

        <form onSubmit={handleSubmit} noValidate className="mt-6 flex flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm text-muted">
            E-mail
            <input
              type="email"
              placeholder="Digite seu e-mail"
              autoComplete="username"
              autoFocus
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              className="border border-border px-3 py-2 text-sm text-foreground outline-none"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm text-muted">
            Senha
            <PasswordInput
              value={password}
              onChange={setPassword}
              placeholder="Digite sua senha"
              autoComplete="current-password"
            />
          </label>

          {error && <p className="text-sm text-danger">{error}</p>}

          <button
            type="submit"
            disabled={isSubmitting}
            className="mt-2 flex cursor-pointer items-center justify-center gap-2 bg-moon-900 py-3 text-sm uppercase tracking-wider text-white transition-opacity disabled:cursor-not-allowed disabled:opacity-40"
          >
            {isSubmitting && <RiLoader4Line className="size-4 animate-spin" />}
            Entrar
          </button>
        </form>
      </div>
    </div>
  )
}
