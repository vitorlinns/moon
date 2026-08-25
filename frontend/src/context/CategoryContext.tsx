import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { fetchCategories, type Category } from '../lib/catalog'

interface CategoryContextValue {
  categories: Category[]
  isLoading: boolean
}

const CategoryContext = createContext<CategoryContextValue | null>(null)

export function CategoryProvider({ children }: { children: ReactNode }) {
  const [categories, setCategories] = useState<Category[]>([])
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    fetchCategories()
      .then(setCategories)
      .catch(() => setCategories([]))
      .finally(() => setIsLoading(false))
  }, [])

  return (
    <CategoryContext.Provider value={{ categories, isLoading }}>{children}</CategoryContext.Provider>
  )
}

export function useCategories() {
  const context = useContext(CategoryContext)
  if (!context) {
    throw new Error('useCategories deve ser usado dentro de um CategoryProvider')
  }
  return context
}
