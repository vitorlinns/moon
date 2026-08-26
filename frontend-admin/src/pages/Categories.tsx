import { RiListCheck2 } from 'react-icons/ri'
import { EmptyState } from '../components/EmptyState'

export function Categories() {
  return (
    <section>
      <h2 className="text-sm uppercase tracking-wider text-muted">Categorias</h2>

      <div className="mt-4">
        <EmptyState icon={RiListCheck2} message="Gestão de categorias ainda não implementada." />
      </div>
    </section>
  )
}
