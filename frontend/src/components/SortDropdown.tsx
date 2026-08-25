import { useEffect, useRef, useState } from 'react'
import { RiArrowDownSLine } from 'react-icons/ri'

export interface SortDropdownOption<T extends string> {
  value: T
  label: string
}

interface SortDropdownProps<T extends string> {
  options: SortDropdownOption<T>[]
  value: T
  onChange: (value: T) => void
  /** Classes de posicionamento/largura do menu. Padrão: alinhado à direita, largura fixa. */
  menuClassName?: string
  /** Se a borda do botão escurece ao passar o mouse. Padrão: true. */
  showHoverBorder?: boolean
}

export function SortDropdown<T extends string>({
  options,
  value,
  onChange,
  menuClassName,
  showHoverBorder = true,
}: SortDropdownProps<T>) {
  const [isOpen, setIsOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!isOpen) return

    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false)
      }
    }
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') setIsOpen(false)
    }

    document.addEventListener('mousedown', handleClickOutside)
    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [isOpen])

  const selected = options.find((option) => option.value === value)

  return (
    <div ref={containerRef} className="relative">
      <button
        type="button"
        onClick={() => setIsOpen((open) => !open)}
        aria-haspopup="listbox"
        aria-expanded={isOpen}
        className={`flex cursor-pointer items-center gap-2 border border-border bg-surface px-3 py-2 text-sm text-foreground outline-none transition-colors ${showHoverBorder ? 'hover:border-foreground' : ''}`}
      >
        {selected?.label}
        <RiArrowDownSLine className={`size-4 transition-transform ${isOpen ? 'rotate-180' : ''}`} />
      </button>

      {isOpen && (
        <ul
          role="listbox"
          className={`absolute z-10 mt-1 border border-border bg-surface py-1 shadow-md ${menuClassName ?? 'right-0 w-48'}`}
        >
          {options.map((option) => (
            <li key={option.value} role="option" aria-selected={option.value === value}>
              <button
                type="button"
                onClick={() => {
                  onChange(option.value)
                  setIsOpen(false)
                }}
                className={`block w-full cursor-pointer px-3 py-2 text-left text-sm outline-none transition-colors hover:bg-moon-100 ${
                  option.value === value ? 'text-foreground' : 'text-muted'
                }`}
              >
                {option.label}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
