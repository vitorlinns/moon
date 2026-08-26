import { useAdminAuth } from '../context/AdminAuthContext'

export function Dashboard() {
  const { admin } = useAdminAuth()

  return (
    <section>
      <h2 className="text-2xl font-light tracking-wide text-foreground">Olá, {admin?.name}</h2>
      <p className="mt-2 text-sm text-muted">
        Painel administrativo em construção — gestão de produtos, categorias e pedidos vem nas
        próximas etapas.
      </p>
    </section>
  )
}
