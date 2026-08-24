import { useEffect, useState } from 'react'
import { RiCheckLine, RiErrorWarningLine } from 'react-icons/ri'
import { useToast, type ToastVariant } from '../context/ToastContext'

const VISIBLE_DURATION_MS = 4000
const TRANSITION_MS = 250

export function ToastViewport() {
  const { toasts, dismissToast } = useToast()

  return (
    <div className="pointer-events-none fixed inset-x-0 bottom-6 z-40 flex flex-col items-center gap-2 px-6">
      {toasts.map((toast) => (
        <ToastItem
          key={toast.id}
          message={toast.message}
          variant={toast.variant}
          onDismiss={() => dismissToast(toast.id)}
        />
      ))}
    </div>
  )
}

type Phase = 'enter' | 'visible' | 'exit'

interface ToastItemProps {
  message: string
  variant: ToastVariant
  onDismiss: () => void
}

function ToastItem({ message, variant, onDismiss }: ToastItemProps) {
  const [phase, setPhase] = useState<Phase>('enter')

  useEffect(() => {
    const enterFrame = requestAnimationFrame(() => setPhase('visible'))
    const hideTimer = setTimeout(() => setPhase('exit'), VISIBLE_DURATION_MS)
    return () => {
      cancelAnimationFrame(enterFrame)
      clearTimeout(hideTimer)
    }
  }, [])

  useEffect(() => {
    if (phase !== 'exit') return
    const removeTimer = setTimeout(onDismiss, TRANSITION_MS)
    return () => clearTimeout(removeTimer)
  }, [phase, onDismiss])

  const Icon = variant === 'error' ? RiErrorWarningLine : RiCheckLine

  return (
    <div
      role="status"
      onClick={() => setPhase('exit')}
      className={`pointer-events-auto flex max-w-sm cursor-pointer items-center gap-2 border border-moon-700 bg-moon-900 px-4 py-3 text-sm text-white shadow-lg transition-all duration-[250ms] ${
        phase === 'visible' ? 'translate-y-0 opacity-100' : 'translate-y-3 opacity-0'
      }`}
    >
      <Icon className={`size-4 shrink-0 ${variant === 'error' ? 'text-red-400' : 'text-emerald-400'}`} />
      {message}
    </div>
  )
}
