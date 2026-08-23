import { createContext, useContext, useMemo, useState, type ReactNode } from 'react'
import type { Product } from '../data/products'

export interface CartItem {
  product: Product
  quantity: number
}

interface CartContextValue {
  items: CartItem[]
  isOpen: boolean
  itemCount: number
  total: number
  addItem: (product: Product) => void
  removeItem: (name: string) => void
  updateQuantity: (name: string, quantity: number) => void
  open: () => void
  close: () => void
}

const CartContext = createContext<CartContextValue | null>(null)

export function CartProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<CartItem[]>([])
  const [isOpen, setIsOpen] = useState(false)

  const addItem = (product: Product) => {
    setItems((current) => {
      const existing = current.find((item) => item.product.name === product.name)
      if (existing) {
        return current.map((item) =>
          item.product.name === product.name ? { ...item, quantity: item.quantity + 1 } : item,
        )
      }
      return [...current, { product, quantity: 1 }]
    })
    setIsOpen(true)
  }

  const removeItem = (name: string) => {
    setItems((current) => current.filter((item) => item.product.name !== name))
  }

  const updateQuantity = (name: string, quantity: number) => {
    if (quantity < 1) {
      removeItem(name)
      return
    }
    setItems((current) =>
      current.map((item) => (item.product.name === name ? { ...item, quantity } : item)),
    )
  }

  const itemCount = useMemo(() => items.reduce((sum, item) => sum + item.quantity, 0), [items])
  const total = useMemo(
    () => items.reduce((sum, item) => sum + item.product.price * item.quantity, 0),
    [items],
  )

  return (
    <CartContext.Provider
      value={{
        items,
        isOpen,
        itemCount,
        total,
        addItem,
        removeItem,
        updateQuantity,
        open: () => setIsOpen(true),
        close: () => setIsOpen(false),
      }}
    >
      {children}
    </CartContext.Provider>
  )
}

export function useCart() {
  const context = useContext(CartContext)
  if (!context) {
    throw new Error('useCart deve ser usado dentro de um CartProvider')
  }
  return context
}
