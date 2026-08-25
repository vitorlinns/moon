import { useAdminAuth } from '../context/AdminAuthContext'

export function Dashboard() {
  const { admin, logout } = useAdminAuth()

  return (
    <div className="min-h-svh bg-background">
      <header className="flex items-center justify-between border-b border-border bg-surface px-6 py-4">
        <p className="text-sm uppercase tracking-wider text-foreground">Moon Admin</p>
        <div className="flex items-center gap-4">
          <span className="text-sm text-muted">{admin?.name}</span>
          <button
            type="button"
            onClick={() => void logout()}
            className="cursor-pointer text-sm text-muted transition-colors hover:text-foreground"
          >
            Sair
          </button>
        </div>
      </header>

      <main className="mx-auto max-w-3xl px-6 py-16">
        <h2 className="text-2xl font-light tracking-wide text-foreground">Olá, {admin?.name}</h2>
        <p className="mt-2 text-sm text-muted">
          Painel administrativo em construção — gestão de produtos, categorias e pedidos vem nas
          próximas etapas.
        </p>
      </main>
    </div>
  )
}
