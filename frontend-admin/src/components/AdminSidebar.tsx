import { NavLink } from 'react-router-dom'
import type { IconType } from 'react-icons'
import { RiDashboardLine, RiPriceTag3Line, RiListCheck2, RiShoppingBag3Line } from 'react-icons/ri'

const links: { to: string; label: string; icon: IconType; end?: boolean }[] = [
  { to: '/', label: 'Dashboard', icon: RiDashboardLine, end: true },
  { to: '/produtos', label: 'Produtos', icon: RiPriceTag3Line },
  { to: '/categorias', label: 'Categorias', icon: RiListCheck2 },
  { to: '/pedidos', label: 'Pedidos', icon: RiShoppingBag3Line },
]

export function AdminSidebar() {
  return (
    <nav className="flex w-56 shrink-0 flex-col gap-1 border-r border-border bg-surface px-4 py-6">
      <p className="px-3 pb-6 text-sm uppercase tracking-wider text-foreground">Moon Admin</p>

      {links.map((link) => (
        <NavLink
          key={link.to}
          to={link.to}
          end={link.end}
          className={({ isActive }) =>
            `flex items-center gap-3 px-3 py-2 text-sm transition-colors ${
              isActive ? 'bg-moon-100 text-foreground' : 'text-muted hover:text-foreground'
            }`
          }
        >
          <link.icon className="size-4" />
          {link.label}
        </NavLink>
      ))}
    </nav>
  )
}
