import { RiDiamondLine, RiSubtractLine, RiAddLine, RiDeleteBinLine } from 'react-icons/ri'
import type { CartItem } from '../context/CartContext'

const currencyFormatter = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

interface CartItemCardProps {
  item: CartItem
  onIncrease: () => void
  onDecrease: () => void
  onRemove: () => void
}

export function CartItemCard({ item, onIncrease, onDecrease, onRemove }: CartItemCardProps) {
  const { product, quantity } = item

  return (
    <li className="flex gap-4 py-5">
      <div className="flex size-16 shrink-0 items-center justify-center bg-moon-100">
        <RiDiamondLine className="size-6 text-moon-400" />
      </div>

      <div className="flex flex-1 flex-col">
        <p className="text-xs uppercase tracking-wider text-muted">{product.category}</p>
        <h3 className="mt-1 text-sm text-foreground">{product.name}</h3>
        <p className="mt-1 text-sm text-foreground">{currencyFormatter.format(product.price)}</p>

        <div className="mt-3 flex items-center justify-between">
          <div className="flex items-center border border-border">
            <button
              type="button"
              aria-label="Diminuir quantidade"
              onClick={onDecrease}
              className="flex size-7 cursor-pointer items-center justify-center text-foreground hover:bg-moon-100"
            >
              <RiSubtractLine className="size-3.5" />
            </button>
            <span className="w-7 text-center text-sm text-foreground">{quantity}</span>
            <button
              type="button"
              aria-label="Aumentar quantidade"
              onClick={onIncrease}
              className="flex size-7 cursor-pointer items-center justify-center text-foreground hover:bg-moon-100"
            >
              <RiAddLine className="size-3.5" />
            </button>
          </div>

          <button
            type="button"
            aria-label="Remover item"
            onClick={onRemove}
            className="cursor-pointer text-muted transition-colors hover:text-foreground"
          >
            <RiDeleteBinLine className="size-4" />
          </button>
        </div>
      </div>
    </li>
  )
}
