import { Link } from 'react-router-dom'
import { RiShoppingBag3Line } from 'react-icons/ri'
import { EmptyState } from '../../components/EmptyState'

export function Orders() {
  return (
    <section>
      <h2 className="text-sm uppercase tracking-wider text-muted">Meus pedidos</h2>

      <div className="mt-4">
        <EmptyState
          icon={RiShoppingBag3Line}
          message="Você ainda não fez nenhum pedido."
          action={
            <Link
              to="/"
              className="mt-2 cursor-pointer bg-moon-900 px-6 py-3 text-sm uppercase tracking-wider text-white transition-opacity hover:opacity-90"
            >
              Ver catálogo
            </Link>
          }
        />
      </div>
    </section>
  )
}
