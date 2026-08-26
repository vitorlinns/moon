import { RiPriceTag3Line } from 'react-icons/ri'
import { EmptyState } from '../components/EmptyState'

export function Products() {
  return (
    <section>
      <h2 className="text-sm uppercase tracking-wider text-muted">Produtos</h2>

      <div className="mt-4">
        <EmptyState icon={RiPriceTag3Line} message="Gestão de produtos ainda não implementada." />
      </div>
    </section>
  )
}
