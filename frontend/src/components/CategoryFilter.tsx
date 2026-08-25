import { useCategories } from '../context/CategoryContext'

export type CategoryValue = 'Todos' | string

interface CategoryFilterProps {
  value: CategoryValue
  onChange: (value: CategoryValue) => void
}

export function CategoryFilter({ value, onChange }: CategoryFilterProps) {
  const { categories } = useCategories()

  const options: { value: CategoryValue; label: string }[] = [
    { value: 'Todos', label: 'Todos' },
    ...categories.map((category) => ({ value: category.slug, label: category.name })),
  ]

  return (
    <nav className="flex flex-wrap items-center gap-6">
      {options.map((option) => (
        <button
          key={option.value}
          type="button"
          onClick={() => onChange(option.value)}
          className={`text-sm transition-colors ${
            value === option.value ? 'text-foreground' : 'text-muted hover:text-foreground'
          }`}
        >
          {option.label}
        </button>
      ))}
    </nav>
  )
}
