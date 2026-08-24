import { Outlet } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { AccountSidebar } from '../../components/AccountSidebar'

export function AccountLayout() {
  const { isAuthenticated, isLoadingUser, openModal } = useAuth()

  if (isLoadingUser) {
    return null
  }

  if (!isAuthenticated) {
    return (
      <main className="mx-auto flex max-w-6xl flex-col items-center gap-4 px-6 py-24 text-center">
        <p className="text-sm text-muted">Você precisa entrar para ver sua conta.</p>
        <button
          type="button"
          onClick={openModal}
          className="cursor-pointer bg-moon-900 px-6 py-3 text-sm uppercase tracking-wider text-white"
        >
          Entrar
        </button>
      </main>
    )
  }

  return (
    <main className="mx-auto max-w-6xl px-6 py-16">
      <h1 className="text-2xl font-light tracking-wide text-foreground">Minha conta</h1>

      <div className="mt-8 flex flex-col gap-8 md:flex-row">
        <AccountSidebar />
        <div className="flex-1">
          <Outlet />
        </div>
      </div>
    </main>
  )
}
