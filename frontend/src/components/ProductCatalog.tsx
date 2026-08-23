import { useEffect, useMemo, useState } from 'react'
import { ProductCard } from './ProductCard'
import { CategoryFilter, type CategoryValue } from './CategoryFilter'
import { SortDropdown } from './SortDropdown'
import { Pagination } from './Pagination'
import { products } from '../data/products'

type SortOption = '' | 'price-asc' | 'price-desc' | 'launch' | 'sales'

const sortOptions: { value: SortOption; label: string }[] = [
  { value: '', label: 'Ordenar por' },
  { value: 'price-asc', label: 'Menor preço' },
  { value: 'price-desc', label: 'Maior preço' },
  { value: 'launch', label: 'Lançamentos' },
  { value: 'sales', label: 'Mais vendidos' },
]

const PAGE_SIZE = 16

export function ProductCatalog() {
  const [category, setCategory] = useState<CategoryValue>('Todos')
  const [sort, setSort] = useState<SortOption>('')
  const [page, setPage] = useState(1)

  const sortedProducts = useMemo(() => {
    const filtered =
      category === 'Todos' ? products : products.filter((product) => product.category === category)

    switch (sort) {
      case 'price-asc':
        return [...filtered].sort((a, b) => a.price - b.price)
      case 'price-desc':
        return [...filtered].sort((a, b) => b.price - a.price)
      case 'launch':
        return [...filtered].sort((a, b) => b.launchedAt.localeCompare(a.launchedAt))
      case 'sales':
        return [...filtered].sort((a, b) => b.sales - a.sales)
      default:
        return filtered
    }
  }, [category, sort])

  const totalPages = Math.max(1, Math.ceil(sortedProducts.length / PAGE_SIZE))

  useEffect(() => {
    setPage(1)
  }, [category, sort])

  const visibleProducts = sortedProducts.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)

  return (
    <section className="mx-auto max-w-6xl px-6 py-16">
      <h2 className="text-2xl font-light tracking-wide text-foreground">Todos os produtos</h2>

      <div className="mt-6 flex flex-wrap items-center justify-between gap-4 border-b border-border pb-4">
        <CategoryFilter value={category} onChange={setCategory} />
        <SortDropdown options={sortOptions} value={sort} onChange={setSort} />
      </div>

      <div className="mt-8 grid grid-cols-2 gap-8 sm:grid-cols-4">
        {visibleProducts.map((product) => (
          <ProductCard key={product.name} {...product} />
        ))}
      </div>

      <Pagination page={page} totalPages={totalPages} onChange={setPage} />
    </section>
  )
}
