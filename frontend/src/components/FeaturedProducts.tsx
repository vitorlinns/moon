import { ProductCard } from './ProductCard'
import { products } from '../data/products'

const featuredProducts = products.filter((product) => product.featured)

export function FeaturedProducts() {
  return (
    <section className="mx-auto max-w-6xl px-6 py-16">
      <h2 className="text-2xl font-light tracking-wide text-foreground">Em destaque</h2>
      <p className="mt-1 text-sm text-muted">Seleção da nossa curadoria</p>

      <div className="mt-8 grid grid-cols-2 gap-8 sm:grid-cols-4">
        {featuredProducts.map((product) => (
          <ProductCard key={product.name} {...product} />
        ))}
      </div>
    </section>
  )
}
