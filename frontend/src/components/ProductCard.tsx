import { RiDiamondLine } from 'react-icons/ri'
import type { Product } from '../data/products'
import { useCart } from '../context/CartContext'
import { AddToCartButton } from './AddToCartButton'

const currencyFormatter = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

export function ProductCard(product: Product) {
  const { name, category, price } = product
  const { addItem } = useCart()

  return (
    <div className="group">
      <div className="relative flex aspect-square items-center justify-center bg-moon-100">
        <RiDiamondLine className="size-10 text-moon-400 transition-colors group-hover:text-accent" />

        <AddToCartButton onClick={() => addItem(product)} />
      </div>
      <p className="mt-3 text-xs uppercase tracking-wider text-muted">{category}</p>
      <h3 className="mt-1 text-sm text-foreground">{name}</h3>
      <p className="mt-1 text-sm text-foreground">{currencyFormatter.format(price)}</p>
    </div>
  )
}
