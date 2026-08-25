import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { API_URL, apiFetch, invalidateCsrfToken } from '../lib/api'

export interface User {
  id: string
  name: string
  email: string
  cpf?: string
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
  updateProfile: (data: { name: string; email: string }) => Promise<void>
  changePassword: (currentPassword: string, newPassword: string) => Promise<void>
  deleteAccount: (password: string) => Promise<void>
  loginWithProvider: (provider: OAuthProvider) => void
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoadingUser, setIsLoadingUser] = useState(true)
  const [isModalOpen, setIsModalOpen] = useState(false)

  useEffect(() => {
    apiFetch<User>('/auth/me')
      .then(setUser)
      .catch(() => setUser(null))
      .finally(() => setIsLoadingUser(false))
  }, [])

  const login = async (email: string, password: string) => {
    const loggedUser = await apiFetch<User>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    })
    invalidateCsrfToken()
    setUser(loggedUser)
    setIsModalOpen(false)
  }

  const register = async (data: { cpf: string; name: string; email: string; password: string }) => {
    const createdUser = await apiFetch<User>('/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    })
    invalidateCsrfToken()
    setUser(createdUser)
    setIsModalOpen(false)
  }

  const updateProfile = async (data: { name: string; email: string }) => {
    const updatedUser = await apiFetch<User>('/auth/me', {
      method: 'PATCH',
      body: JSON.stringify(data),
    })
    setUser(updatedUser)
  }

  const changePassword = async (currentPassword: string, newPassword: string) => {
    await apiFetch('/auth/change-password', {
      method: 'POST',
      body: JSON.stringify({ currentPassword, newPassword }),
    })
  }

  const deleteAccount = async (password: string) => {
    await apiFetch('/auth/me', {
      method: 'DELETE',
      body: JSON.stringify({ password }),
    })
    invalidateCsrfToken()
    setUser(null)
  }

  const loginWithProvider = (provider: OAuthProvider) => {
    window.location.href = `${API_URL}/api/auth/${provider}?returnTo=${encodeURIComponent(window.location.href)}`
  }

  const logout = async () => {
    try {
      await apiFetch('/auth/logout', { method: 'POST' })
    } catch {
      // segue limpando a sessão local mesmo se a chamada ao backend falhar
    }
    invalidateCsrfToken()
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
        updateProfile,
        changePassword,
        deleteAccount,
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
