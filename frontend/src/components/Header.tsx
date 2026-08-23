import { RiSearchLine, RiUserLine, RiShoppingBag3Line } from 'react-icons/ri'
import { categories } from '../data/categories'

export function Header() {
  return (
    <header className="sticky top-0 z-10 border-b border-border bg-surface">
      <div className="mx-auto flex max-w-6xl items-center justify-between gap-6 px-6 py-4">
        <a href="/" className="text-xl font-light tracking-[0.3em] text-foreground">
          MOON
        </a>

        <nav className="hidden md:flex items-center gap-8">
          {categories.map((category) => (
            <a
              key={category}
              href="#"
              className="text-sm text-muted transition-colors hover:text-foreground"
            >
              {category}
            </a>
          ))}
        </nav>

        <div className="flex items-center gap-5 text-foreground">
          <button type="button" aria-label="Buscar" className="cursor-not-allowed">
            <RiSearchLine className="size-5" />
          </button>
          <button type="button" aria-label="Minha conta" className="cursor-not-allowed">
            <RiUserLine className="size-5" />
          </button>
          <button type="button" aria-label="Sacola" className="cursor-not-allowed">
            <RiShoppingBag3Line className="size-5" />
          </button>
        </div>
      </div>
    </header>
  )
}
