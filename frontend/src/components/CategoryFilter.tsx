import { categories } from '../data/categories'

export type CategoryValue = 'Todos' | (typeof categories)[number]

interface CategoryFilterProps {
  value: CategoryValue
  onChange: (value: CategoryValue) => void
}

const options: CategoryValue[] = ['Todos', ...categories]

export function CategoryFilter({ value, onChange }: CategoryFilterProps) {
  return (
    <nav className="flex flex-wrap items-center gap-6">
      {options.map((option) => (
        <button
          key={option}
          type="button"
          onClick={() => onChange(option)}
          className={`text-sm transition-colors ${
            value === option ? 'text-foreground' : 'text-muted hover:text-foreground'
          }`}
        >
          {option}
        </button>
      ))}
    </nav>
  )
}
