import { createContext, useCallback, useContext, useState, type ReactNode } from 'react'

export type ToastVariant = 'error' | 'success'

interface Toast {
  id: number
  message: string
  variant: ToastVariant
}

interface ToastContextValue {
  toasts: Toast[]
  showToast: (message: string, variant?: ToastVariant) => void
  dismissToast: (id: number) => void
}

const ToastContext = createContext<ToastContextValue | null>(null)

let nextToastId = 0

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([])

  const dismissToast = useCallback((id: number) => {
    setToasts((current) => current.filter((toast) => toast.id !== id))
  }, [])

  const showToast = useCallback((message: string, variant: ToastVariant = 'error') => {
    const id = nextToastId++
    setToasts((current) => [...current, { id, message, variant }])
  }, [])

  return (
    <ToastContext.Provider value={{ toasts, showToast, dismissToast }}>
      {children}
    </ToastContext.Provider>
  )
}

export function useToast() {
  const context = useContext(ToastContext)
  if (!context) {
    throw new Error('useToast deve ser usado dentro de um ToastProvider')
  }
  return context
}
