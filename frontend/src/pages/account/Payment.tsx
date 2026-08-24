import { RiBankCardLine } from 'react-icons/ri'
import { EmptyState } from '../../components/EmptyState'

export function Payment() {
  return (
    <section>
      <h2 className="text-sm uppercase tracking-wider text-muted">Pagamento</h2>

      <div className="mt-4">
        <EmptyState
          icon={RiBankCardLine}
          message="Nenhum cartão cadastrado ainda."
          action={
            <button
              type="button"
              disabled
              className="mt-2 cursor-not-allowed bg-moon-900 px-6 py-3 text-sm uppercase tracking-wider text-white opacity-40"
            >
              Adicionar cartão
            </button>
          }
        />
      </div>
    </section>
  )
}
