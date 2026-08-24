import { RiSettings3Line } from 'react-icons/ri'
import { EmptyState } from '../../components/EmptyState'

export function Settings() {
  return (
    <section>
      <h2 className="text-sm uppercase tracking-wider text-muted">Configurações</h2>

      <div className="mt-4">
        <EmptyState icon={RiSettings3Line} message="Em breve." />
      </div>
    </section>
  )
}
