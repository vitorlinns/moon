import { useEffect, useRef, useState } from 'react'
import { RiNotification3Line } from 'react-icons/ri'
import { IconButton } from './IconButton'

// Sem backend ainda — quando pedidos existirem de verdade, isso passa a buscar/realtime
// notificar pedidos novos e alimentar o badge com a contagem não vista.
export function NotificationsMenu() {
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

  return (
    <div ref={containerRef} className="relative">
      <IconButton
        icon={<RiNotification3Line className="size-5" />}
        label="Notificações"
        onClick={() => setIsOpen((open) => !open)}
      />

      {isOpen && (
        <ul className="absolute right-0 z-10 mt-2 w-56 border border-border bg-surface py-1">
          <li className="px-3 py-2 text-sm text-muted">Nenhum pedido novo por enquanto.</li>
        </ul>
      )}
    </div>
  )
}
