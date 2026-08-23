import type { ReactNode } from 'react'

interface IconButtonProps {
  icon: ReactNode
  label: string
  onClick?: () => void
  badge?: number
  disabled?: boolean
}

export function IconButton({ icon, label, onClick, badge, disabled }: IconButtonProps) {
  return (
    <button
      type="button"
      aria-label={label}
      onClick={onClick}
      disabled={disabled}
      className={`relative text-foreground ${disabled ? 'cursor-not-allowed' : 'cursor-pointer'}`}
    >
      {icon}
      {!!badge && badge > 0 && (
        <span className="absolute -right-2 -top-2 flex size-4 items-center justify-center rounded-full bg-accent text-[10px] text-white">
          {badge}
        </span>
      )}
    </button>
  )
}
