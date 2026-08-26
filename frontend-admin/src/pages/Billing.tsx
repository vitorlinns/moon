import { RiBillLine } from 'react-icons/ri'
import { EmptyState } from '../components/EmptyState'

export function Billing() {
  return (
    <section>
      <h2 className="text-sm uppercase tracking-wider text-muted">Faturamento</h2>

      <div className="mt-4">
        <EmptyState icon={RiBillLine} message="Faturamento ainda não implementado." />
      </div>
    </section>
  )
}
