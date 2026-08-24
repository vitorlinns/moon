import { Link, useNavigate } from 'react-router-dom'
import { RiSearchLine, RiUserLine, RiShoppingBag3Line, RiLogoutBoxLine } from 'react-icons/ri'
import { CategoriesMenu } from './CategoriesMenu'
import { IconButton } from './IconButton'
import { useCart } from '../context/CartContext'
import { useAuth } from '../context/AuthContext'

export function Header() {
  const { itemCount, open } = useCart()
  const { isAuthenticated, openModal, logout } = useAuth()
  const navigate = useNavigate()

  return (
    <header className="sticky top-0 z-10 border-b border-border bg-surface">
      <div className="mx-auto flex max-w-6xl items-center justify-between gap-6 px-6 py-4">
        <Link to="/" className="text-xl font-light tracking-[0.3em] text-foreground">
          MOON
        </Link>

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
          <IconButton
            icon={<RiUserLine className="size-5" />}
            label={isAuthenticated ? 'Minha conta' : 'Entrar'}
            onClick={isAuthenticated ? () => navigate('/minha-conta') : openModal}
          />
          <IconButton
            icon={<RiShoppingBag3Line className="size-5" />}
            label="Sacola"
            onClick={open}
            badge={itemCount}
          />

          {isAuthenticated && (
            <button
              type="button"
              onClick={logout}
              className="flex cursor-pointer items-center gap-1.5 border-l border-border pl-5 text-sm text-danger transition-colors hover:opacity-80"
            >
              <RiLogoutBoxLine className="size-5" />
              Sair
            </button>
          )}
        </div>
      </div>
    </header>
  )
}
