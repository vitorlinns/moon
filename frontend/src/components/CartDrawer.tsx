import { RiCloseLine } from 'react-icons/ri'
import { useCart } from '../context/CartContext'
import { CartItemCard } from './CartItemCard'

const currencyFormatter = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

export function CartDrawer() {
  const { items, isOpen, total, close, removeItem, updateQuantity } = useCart()

  return (
    <div
      className={`fixed inset-0 z-20 ${isOpen ? '' : 'pointer-events-none'}`}
      aria-hidden={!isOpen}
    >
      <div
        onClick={close}
        className={`absolute inset-0 bg-moon-900/40 transition-opacity ${
          isOpen ? 'opacity-100' : 'opacity-0'
        }`}
      />

      <aside
        className={`absolute right-0 top-0 flex h-full w-full max-w-sm flex-col bg-surface transition-transform duration-300 ${
          isOpen ? 'translate-x-0' : 'translate-x-full'
        }`}
      >
        <div className="flex items-center justify-between border-b border-border px-6 py-5">
          <h2 className="text-sm uppercase tracking-wider text-foreground">Sacola</h2>
          <button type="button" aria-label="Fechar sacola" onClick={close} className="cursor-pointer text-foreground">
            <RiCloseLine className="size-5" />
          </button>
        </div>

        {items.length === 0 ? (
          <p className="flex flex-1 items-center justify-center px-6 text-sm text-muted">
            Sua sacola está vazia.
          </p>
        ) : (
          <ul className="flex-1 divide-y divide-border overflow-y-auto px-6">
            {items.map((item) => (
              <CartItemCard
                key={item.product.name}
                item={item}
                onIncrease={() => updateQuantity(item.product.name, item.quantity + 1)}
                onDecrease={() => updateQuantity(item.product.name, item.quantity - 1)}
                onRemove={() => removeItem(item.product.name)}
              />
            ))}
          </ul>
        )}

        <div className="border-t border-border px-6 py-5">
          <div className="flex items-center justify-between text-sm text-foreground">
            <span>Total</span>
            <span>{currencyFormatter.format(total)}</span>
          </div>
          <button
            type="button"
            disabled={items.length === 0}
            className="mt-4 w-full cursor-not-allowed bg-moon-900 py-3 text-sm uppercase tracking-wider text-white transition-opacity disabled:opacity-40"
          >
            Finalizar compra
          </button>
        </div>
      </aside>
    </div>
  )
}
