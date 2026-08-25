import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { apiFetch, invalidateCsrfToken } from '../lib/adminApi'

export interface AdminUser {
  id: string
  name: string
  email: string
}

interface AdminAuthContextValue {
  admin: AdminUser | null
  isAuthenticated: boolean
  isLoadingAdmin: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

const AdminAuthContext = createContext<AdminAuthContextValue | null>(null)

export function AdminAuthProvider({ children }: { children: ReactNode }) {
  const [admin, setAdmin] = useState<AdminUser | null>(null)
  const [isLoadingAdmin, setIsLoadingAdmin] = useState(true)

  useEffect(() => {
    apiFetch<AdminUser>('/admin/auth/me')
      .then(setAdmin)
      .catch(() => setAdmin(null))
      .finally(() => setIsLoadingAdmin(false))
  }, [])

  const login = async (email: string, password: string) => {
    const loggedAdmin = await apiFetch<AdminUser>('/admin/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    })
    invalidateCsrfToken()
    setAdmin(loggedAdmin)
  }

  const logout = async () => {
    await apiFetch('/admin/auth/logout', { method: 'POST' })
    invalidateCsrfToken()
    setAdmin(null)
  }

  return (
    <AdminAuthContext.Provider
      value={{ admin, isAuthenticated: admin !== null, isLoadingAdmin, login, logout }}
    >
      {children}
    </AdminAuthContext.Provider>
  )
}

export function useAdminAuth() {
  const context = useContext(AdminAuthContext)
  if (!context) {
    throw new Error('useAdminAuth deve ser usado dentro de um AdminAuthProvider')
  }
  return context
}
