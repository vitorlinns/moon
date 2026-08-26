import { useNavigate } from 'react-router-dom'
import { RiBillLine, RiGlobalLine } from 'react-icons/ri'
import { IconButton } from './IconButton'
import { NotificationsMenu } from './NotificationsMenu'
import { AdminAvatarMenu } from './AdminAvatarMenu'

const STORE_URL = import.meta.env.VITE_STORE_URL ?? 'http://localhost:5001'

export function AdminHeader() {
  const navigate = useNavigate()

  return (
    <header className="flex items-center justify-end gap-5 border-b border-border bg-surface px-6 py-4 text-foreground">
      <NotificationsMenu />

      <IconButton
        icon={<RiBillLine className="size-5" />}
        label="Faturamento"
        onClick={() => navigate('/faturamento')}
      />

      <a
        href={STORE_URL}
        target="_blank"
        rel="noopener noreferrer"
        aria-label="Ver loja"
        title="Ver loja"
        className="relative cursor-pointer text-foreground"
      >
        <RiGlobalLine className="size-5" />
      </a>

      <div className="border-l border-border pl-5">
        <AdminAvatarMenu />
      </div>
    </header>
  )
}
