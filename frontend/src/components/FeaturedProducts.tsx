import { useEffect, useState } from 'react'
import { ProductCard } from './ProductCard'
import { fetchProducts, type Product } from '../lib/catalog'

export function FeaturedProducts() {
  const [products, setProducts] = useState<Product[]>([])

  useEffect(() => {
    fetchProducts({ featured: true, pageSize: 50 }).then((result) => setProducts(result.items))
  }, [])

  return (
    <section className="mx-auto max-w-6xl px-6 py-16">
      <h2 className="text-2xl font-light tracking-wide text-foreground">Em destaque</h2>
      <p className="mt-1 text-sm text-muted">Seleção da nossa curadoria</p>

      <div className="mt-8 grid grid-cols-2 gap-8 sm:grid-cols-4">
        {products.map((product) => (
          <ProductCard key={product.id} {...product} />
        ))}
      </div>
    </section>
  )
}
