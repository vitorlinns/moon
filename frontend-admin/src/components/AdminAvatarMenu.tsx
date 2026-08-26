import { useEffect, useRef, useState } from 'react'
import { useAdminAuth } from '../context/AdminAuthContext'

export function AdminAvatarMenu() {
  const { admin, logout } = useAdminAuth()
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

  const initial = admin?.name.trim().charAt(0).toUpperCase() ?? '?'

  return (
    <div ref={containerRef} className="relative">
      <button
        type="button"
        onClick={() => setIsOpen((open) => !open)}
        aria-haspopup="true"
        aria-expanded={isOpen}
        aria-label="Menu do administrador"
        className="flex size-9 cursor-pointer items-center justify-center rounded-full bg-moon-900 text-sm text-white"
      >
        {initial}
      </button>

      {isOpen && (
        <ul className="absolute right-0 z-10 mt-2 w-40 border border-border bg-surface py-1">
          <li>
            <button
              type="button"
              onClick={() => {
                setIsOpen(false)
                void logout()
              }}
              className="block w-full cursor-pointer px-3 py-2 text-left text-sm text-muted transition-colors hover:bg-moon-100 hover:text-foreground"
            >
              Sair
            </button>
          </li>
        </ul>
      )}
    </div>
  )
}
