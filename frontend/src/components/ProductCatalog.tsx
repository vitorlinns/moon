import { useEffect, useState } from 'react'
import { ProductCard } from './ProductCard'
import { CategoryFilter, type CategoryValue } from './CategoryFilter'
import { SortDropdown } from './SortDropdown'
import { Pagination } from './Pagination'
import { fetchProducts, type Product } from '../lib/catalog'

type SortOption = '' | 'price-asc' | 'price-desc' | 'launch' | 'sales'

const sortOptions: { value: SortOption; label: string }[] = [
  { value: '', label: 'Ordenar por' },
  { value: 'price-asc', label: 'Menor preço' },
  { value: 'price-desc', label: 'Maior preço' },
  { value: 'launch', label: 'Lançamentos' },
  { value: 'sales', label: 'Mais vendidos' },
]

export function ProductCatalog() {
  const [category, setCategory] = useState<CategoryValue>('Todos')
  const [sort, setSort] = useState<SortOption>('')
  const [page, setPage] = useState(1)
  const [products, setProducts] = useState<Product[]>([])
  const [totalPages, setTotalPages] = useState(1)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    setPage(1)
  }, [category, sort])

  useEffect(() => {
    let cancelled = false
    setIsLoading(true)

    fetchProducts({
      category: category === 'Todos' ? undefined : category,
      sort: sort || undefined,
      page,
    })
      .then((result) => {
        if (cancelled) return
        setProducts(result.items)
        setTotalPages(result.totalPages)
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [category, sort, page])

  return (
    <section className="mx-auto max-w-6xl px-6 py-16">
      <h2 className="text-2xl font-light tracking-wide text-foreground">Todos os produtos</h2>

      <div className="mt-6 flex flex-wrap items-center justify-between gap-4 border-b border-border pb-4">
        <CategoryFilter value={category} onChange={setCategory} />
        <SortDropdown options={sortOptions} value={sort} onChange={setSort} />
      </div>

      <div className={`mt-8 grid grid-cols-2 gap-8 sm:grid-cols-4 ${isLoading ? 'opacity-50' : ''}`}>
        {products.map((product) => (
          <ProductCard key={product.id} {...product} />
        ))}
      </div>

      <Pagination page={page} totalPages={totalPages} onChange={setPage} />
    </section>
  )
}
