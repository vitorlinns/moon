import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { API_URL, apiFetch, ApiError } from '../lib/api'

export interface User {
  id: string
  name: string
  email: string
  cpf?: string
}

// TODO: remover quando o backend de auth existir — só pra pré-visualizar a área logada sem servidor.
const PREVIEW_USER: User = {
  id: 'preview',
  name: 'Ana Beatriz',
  email: 'ana.beatriz@exemplo.com',
  cpf: '123.456.789-00',
}

export type OAuthProvider = 'google' | 'facebook' | 'apple'

interface AuthContextValue {
  user: User | null
  isAuthenticated: boolean
  isLoadingUser: boolean
  isModalOpen: boolean
  openModal: () => void
  closeModal: () => void
  login: (email: string, password: string) => Promise<void>
  register: (data: { cpf: string; name: string; email: string; password: string }) => Promise<void>
  loginWithProvider: (provider: OAuthProvider) => void
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(PREVIEW_USER)
  const [isLoadingUser, setIsLoadingUser] = useState(true)
  const [isModalOpen, setIsModalOpen] = useState(false)

  useEffect(() => {
    apiFetch<User>('/auth/me')
      .then(setUser)
      .catch((err) => {
        // erro de rede (sem backend ainda) mantém o usuário de pré-visualização
        if (err instanceof ApiError) setUser(null)
      })
      .finally(() => setIsLoadingUser(false))
  }, [])

  const login = async (email: string, password: string) => {
    const loggedUser = await apiFetch<User>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    })
    setUser(loggedUser)
    setIsModalOpen(false)
  }

  const register = async (data: { cpf: string; name: string; email: string; password: string }) => {
    const createdUser = await apiFetch<User>('/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    })
    setUser(createdUser)
    setIsModalOpen(false)
  }

  const loginWithProvider = (provider: OAuthProvider) => {
    window.location.href = `${API_URL}/auth/${provider}?returnTo=${encodeURIComponent(window.location.href)}`
  }

  const logout = async () => {
    try {
      await apiFetch('/auth/logout', { method: 'POST' })
    } catch {
      // sem backend ainda: segue limpando a sessão local mesmo assim
    }
    setUser(null)
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: user !== null,
        isLoadingUser,
        isModalOpen,
        openModal: () => setIsModalOpen(true),
        closeModal: () => setIsModalOpen(false),
        login,
        register,
        loginWithProvider,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth deve ser usado dentro de um AuthProvider')
  }
  return context
}
