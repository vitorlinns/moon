import { RiShoppingBag3Line } from 'react-icons/ri'

interface AddToCartButtonProps {
  onClick: () => void
}

export function AddToCartButton({ onClick }: AddToCartButtonProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="absolute inset-x-3 bottom-3 flex translate-y-2 cursor-pointer items-center justify-center gap-2 bg-moon-900 py-2.5 text-xs uppercase tracking-wider text-white opacity-0 transition-all group-hover:translate-y-0 group-hover:opacity-100"
    >
      <RiShoppingBag3Line className="size-4" />
      Comprar
    </button>
  )
}
