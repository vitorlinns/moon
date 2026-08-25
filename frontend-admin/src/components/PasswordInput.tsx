import { useState } from 'react'
import { RiEyeLine, RiEyeOffLine } from 'react-icons/ri'

interface PasswordInputProps {
  value: string
  onChange: (value: string) => void
  placeholder?: string
  autoFocus?: boolean
  autoComplete?: string
}

export function PasswordInput({ value, onChange, placeholder, autoFocus, autoComplete }: PasswordInputProps) {
  const [isVisible, setIsVisible] = useState(false)

  return (
    <div className="flex items-center border border-border px-3 py-2">
      <input
        type={isVisible ? 'text' : 'password'}
        placeholder={placeholder}
        autoFocus={autoFocus}
        autoComplete={autoComplete}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="w-full bg-transparent text-sm text-foreground outline-none"
      />
      <button
        type="button"
        aria-label={isVisible ? 'Ocultar senha' : 'Mostrar senha'}
        onClick={() => setIsVisible((current) => !current)}
        className="cursor-pointer text-muted transition-colors hover:text-foreground"
      >
        {isVisible ? <RiEyeOffLine className="size-4" /> : <RiEyeLine className="size-4" />}
      </button>
    </div>
  )
}
