import { useEffect, useState, type FormEvent } from 'react'
import { RiBankCardLine, RiLoader4Line, RiDeleteBinLine, RiStarFill, RiStarLine } from 'react-icons/ri'
import { apiFetch, ApiError } from '../../lib/api'
import { useToast } from '../../context/ToastContext'
import { formatCardNumber, isValidCardNumber, detectCardBrand } from '../../lib/card'
import { EmptyState } from '../../components/EmptyState'

interface PaymentMethod {
  id: string
  brand: string
  lastFourDigits: string
  holderName: string
  expiryMonth: number
  expiryYear: number
  isDefault: boolean
}

function getErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    return err.message || 'Não foi possível completar a solicitação.'
  }
  return 'Não foi possível conectar ao servidor. Tente novamente.'
}

function formatExpiryInput(value: string) {
  return value
    .replace(/\D/g, '')
    .slice(0, 4)
    .replace(/(\d{2})(?=\d)/, '$1/')
}

export function Payment() {
  const { showToast } = useToast()
  const [cards, setCards] = useState<PaymentMethod[] | null>(null)
  const [isFormOpen, setIsFormOpen] = useState(false)
  const [cardNumber, setCardNumber] = useState('')
  const [holderName, setHolderName] = useState('')
  const [cvv, setCvv] = useState('')
  const [expiry, setExpiry] = useState('')
  const [isSaving, setIsSaving] = useState(false)
  const [pendingActionId, setPendingActionId] = useState<string | null>(null)

  const loadCards = () => {
    apiFetch<PaymentMethod[]>('/payment-methods')
      .then(setCards)
      .catch(() => setCards([]))
  }

  useEffect(() => {
    loadCards()
  }, [])

  const resetForm = () => {
    setCardNumber('')
    setHolderName('')
    setCvv('')
    setExpiry('')
  }

  const cancelForm = () => {
    setIsFormOpen(false)
    resetForm()
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()

    const digits = cardNumber.replace(/\D/g, '')

    if (!isValidCardNumber(digits)) {
      showToast('Número de cartão inválido.')
      return
    }

    if (!holderName) {
      showToast('Informe o nome impresso no cartão.')
      return
    }

    if (!/^\d{3,4}$/.test(cvv)) {
      showToast('Informe um CVV válido.')
      return
    }

    const expiryMatch = /^(\d{2})\/(\d{2})$/.exec(expiry)
    if (!expiryMatch) {
      showToast('Informe a validade no formato MM/AA.')
      return
    }

    const expiryMonth = Number(expiryMatch[1])
    const expiryYear = 2000 + Number(expiryMatch[2])

    if (expiryMonth < 1 || expiryMonth > 12) {
      showToast('Mês de validade inválido.')
      return
    }

    setIsSaving(true)
    try {
      // só a bandeira e os últimos 4 dígitos saem do navegador — o número completo nunca é enviado
      await apiFetch<PaymentMethod>('/payment-methods', {
        method: 'POST',
        body: JSON.stringify({
          brand: detectCardBrand(digits),
          lastFourDigits: digits.slice(-4),
          holderName,
          expiryMonth,
          expiryYear,
          isDefault: false,
        }),
      })
      showToast('Cartão adicionado.', 'success')
      cancelForm()
      loadCards()
    } catch (err) {
      showToast(getErrorMessage(err))
    } finally {
      setIsSaving(false)
    }
  }

  const handleDelete = async (id: string) => {
    if (!window.confirm('Remover este cartão?')) return

    setPendingActionId(id)
    try {
      await apiFetch(`/payment-methods/${id}`, { method: 'DELETE' })
      showToast('Cartão removido.', 'success')
      loadCards()
    } catch (err) {
      showToast(getErrorMessage(err))
    } finally {
      setPendingActionId(null)
    }
  }

  const handleSetDefault = async (id: string) => {
    setPendingActionId(id)
    try {
      await apiFetch(`/payment-methods/${id}/default`, { method: 'POST' })
      loadCards()
    } catch (err) {
      showToast(getErrorMessage(err))
    } finally {
      setPendingActionId(null)
    }
  }

  return (
    <section>
      <div className="flex items-center justify-between">
        <h2 className="text-sm uppercase tracking-wider text-muted">Pagamento</h2>
        {!isFormOpen && cards && cards.length > 0 && (
          <button
            type="button"
            onClick={() => setIsFormOpen(true)}
            className="cursor-pointer border border-border px-4 py-2 text-sm text-foreground transition-colors hover:border-foreground"
          >
            Adicionar cartão
          </button>
        )}
      </div>

      {isFormOpen && (
        <form onSubmit={handleSubmit} noValidate className="mt-4 flex flex-col gap-4">
          <div className="grid grid-cols-2 gap-4">
            <label className="flex flex-col gap-1 text-sm text-muted">
              Número do cartão
              <input
                type="text"
                inputMode="numeric"
                placeholder="0000 0000 0000 0000"
                autoFocus
                maxLength={23}
                value={cardNumber}
                onChange={(event) => setCardNumber(formatCardNumber(event.target.value))}
                className="border border-border px-3 py-2 text-sm text-foreground outline-none"
              />
            </label>

            <label className="flex flex-col gap-1 text-sm text-muted">
              Nome impresso no cartão
              <input
                type="text"
                value={holderName}
                onChange={(event) => setHolderName(event.target.value)}
                className="border border-border px-3 py-2 text-sm text-foreground outline-none"
              />
            </label>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <label className="flex flex-col gap-1 text-sm text-muted">
              CVV
              <input
                type="text"
                inputMode="numeric"
                placeholder="000"
                maxLength={4}
                value={cvv}
                onChange={(event) => setCvv(event.target.value.replace(/\D/g, '').slice(0, 4))}
                className="border border-border px-3 py-2 text-sm text-foreground outline-none"
              />
            </label>

            <label className="flex flex-col gap-1 text-sm text-muted">
              Validade (MM/AA)
              <input
                type="text"
                inputMode="numeric"
                placeholder="MM/AA"
                maxLength={5}
                value={expiry}
                onChange={(event) => setExpiry(formatExpiryInput(event.target.value))}
                className="border border-border px-3 py-2 text-sm text-foreground outline-none"
              />
            </label>
          </div>

          <p className="text-sm text-muted">
            Por segurança, o número do cartão e o CVV não são armazenados: ficam só no seu
            navegador o tempo da validação e nunca chegam aos nossos servidores.
          </p>

          <div className="mt-2 flex items-center gap-3">
            <button
              type="submit"
              disabled={isSaving}
              className="flex cursor-pointer items-center gap-2 bg-moon-900 px-6 py-2.5 text-sm uppercase tracking-wider text-white transition-opacity disabled:cursor-not-allowed disabled:opacity-40"
            >
              {isSaving && <RiLoader4Line className="size-4 animate-spin" />}
              Salvar
            </button>
            <button
              type="button"
              onClick={cancelForm}
              disabled={isSaving}
              className="cursor-pointer text-sm text-muted transition-colors hover:text-foreground disabled:cursor-not-allowed"
            >
              Cancelar
            </button>
          </div>
        </form>
      )}

      {!isFormOpen && cards === null && <p className="mt-4 text-sm text-muted">Carregando...</p>}

      {!isFormOpen && cards !== null && cards.length === 0 && (
        <div className="mt-4">
          <EmptyState
            icon={RiBankCardLine}
            message="Nenhum cartão cadastrado ainda."
            action={
              <button
                type="button"
                onClick={() => setIsFormOpen(true)}
                className="mt-2 cursor-pointer bg-moon-900 px-6 py-3 text-sm uppercase tracking-wider text-white transition-opacity hover:opacity-90"
              >
                Adicionar cartão
              </button>
            }
          />
        </div>
      )}

      {!isFormOpen && cards !== null && cards.length > 0 && (
        <ul className="mt-4 flex flex-col gap-3">
          {cards.map((card) => (
            <li key={card.id} className="border border-border p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <div className="flex items-center gap-2">
                    <p className="text-sm text-foreground">{card.brand}</p>
                    {card.isDefault && (
                      <span className="border border-border px-1.5 py-0.5 text-xs uppercase tracking-wider text-muted">
                        Padrão
                      </span>
                    )}
                  </div>
                  <p className="mt-1 text-sm text-muted">•••• •••• •••• {card.lastFourDigits}</p>
                  <p className="text-sm text-muted">{card.holderName}</p>
                  <p className="text-sm text-muted">
                    Validade {String(card.expiryMonth).padStart(2, '0')}/{String(card.expiryYear).slice(-2)}
                  </p>
                </div>

                <div className="flex shrink-0 items-center gap-1">
                  {!card.isDefault && (
                    <button
                      type="button"
                      aria-label="Tornar padrão"
                      title="Tornar padrão"
                      disabled={pendingActionId === card.id}
                      onClick={() => handleSetDefault(card.id)}
                      className="cursor-pointer p-2 text-muted transition-colors hover:text-foreground disabled:cursor-not-allowed"
                    >
                      <RiStarLine className="size-4" />
                    </button>
                  )}
                  {card.isDefault && (
                    <span className="p-2 text-moon-400" title="Cartão padrão">
                      <RiStarFill className="size-4" />
                    </span>
                  )}
                  <button
                    type="button"
                    aria-label="Remover"
                    disabled={pendingActionId === card.id}
                    onClick={() => handleDelete(card.id)}
                    className="cursor-pointer p-2 text-muted transition-colors hover:text-danger disabled:cursor-not-allowed"
                  >
                    <RiDeleteBinLine className="size-4" />
                  </button>
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
