import type { ReactNode } from 'react'
import type { IconType } from 'react-icons'

interface EmptyStateProps {
  icon: IconType
  message: string
  action?: ReactNode
}

export function EmptyState({ icon: Icon, message, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center gap-2 border border-border py-12 text-center">
      <Icon className="size-8 text-moon-400" />
      <p className="text-sm text-muted">{message}</p>
      {action}
    </div>
  )
}
