import { NavLink } from 'react-router-dom'
import type { IconType } from 'react-icons'
import { RiUserLine, RiMapPinLine, RiBankCardLine, RiShoppingBag3Line, RiSettings3Line } from 'react-icons/ri'

const links: { to: string; label: string; icon: IconType; end?: boolean }[] = [
  { to: '/minha-conta', label: 'Dados pessoais', icon: RiUserLine, end: true },
  { to: '/minha-conta/enderecos', label: 'Endereços', icon: RiMapPinLine },
  { to: '/minha-conta/pagamento', label: 'Pagamento', icon: RiBankCardLine },
  { to: '/minha-conta/pedidos', label: 'Pedidos', icon: RiShoppingBag3Line },
  { to: '/minha-conta/configuracoes', label: 'Configurações', icon: RiSettings3Line },
]

export function AccountSidebar() {
  return (
    <nav className="flex shrink-0 flex-col gap-1 md:w-56 md:border-r md:border-border md:pr-6">
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
