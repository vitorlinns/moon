import { RiShoppingBag3Line } from 'react-icons/ri'
import { EmptyState } from '../components/EmptyState'

export function Orders() {
  return (
    <section>
      <h2 className="text-sm uppercase tracking-wider text-muted">Pedidos</h2>

      <div className="mt-4">
        <EmptyState icon={RiShoppingBag3Line} message="Gestão de pedidos ainda não implementada." />
      </div>
    </section>
  )
}
