import { useEffect, useRef, useState } from 'react'
import { RiArrowDownSLine } from 'react-icons/ri'
import { categories } from '../data/categories'

export function CategoriesMenu() {
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
      <button
        type="button"
        onClick={() => setIsOpen((open) => !open)}
        aria-haspopup="true"
        aria-expanded={isOpen}
        className="flex cursor-pointer items-center gap-1 text-sm text-muted transition-colors hover:text-foreground"
      >
        Categorias
        <RiArrowDownSLine className={`size-4 transition-transform ${isOpen ? 'rotate-180' : ''}`} />
      </button>

      {isOpen && (
        <ul className="absolute right-0 z-10 mt-2 w-48 border border-border bg-surface py-1 shadow-md">
          {categories.map((category) => (
            <li key={category}>
              <a
                href="#"
                onClick={() => setIsOpen(false)}
                className="block px-3 py-2 text-sm text-muted transition-colors hover:bg-moon-100 hover:text-foreground"
              >
                {category}
              </a>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
