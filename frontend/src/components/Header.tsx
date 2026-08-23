import { RiSearchLine, RiUserLine, RiShoppingBag3Line } from 'react-icons/ri'
import { CategoriesMenu } from './CategoriesMenu'
import { IconButton } from './IconButton'
import { useCart } from '../context/CartContext'

export function Header() {
  const { itemCount, open } = useCart()

  return (
    <header className="sticky top-0 z-10 border-b border-border bg-surface">
      <div className="mx-auto flex max-w-6xl items-center justify-between gap-6 px-6 py-4">
        <a href="/" className="text-xl font-light tracking-[0.3em] text-foreground">
          MOON
        </a>

        <div className="hidden md:flex items-center gap-8">
          <label className="flex items-center gap-2 border border-border px-3 py-2 text-muted">
            <RiSearchLine className="size-4 shrink-0" />
            <input
              type="search"
              placeholder="Buscar produtos..."
              className="w-96 bg-transparent text-sm text-foreground outline-none placeholder:text-muted"
            />
          </label>

          <CategoriesMenu />
        </div>

        <div className="flex items-center gap-5 text-foreground">
          <IconButton icon={<RiUserLine className="size-5" />} label="Minha conta" disabled />
          <IconButton
            icon={<RiShoppingBag3Line className="size-5" />}
            label="Sacola"
            onClick={open}
            badge={itemCount}
          />
        </div>
      </div>
    </header>
  )
}
