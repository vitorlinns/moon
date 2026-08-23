import { Hero } from '../components/Hero'
import { FeaturedProducts } from '../components/FeaturedProducts'
import { ProductCatalog } from '../components/ProductCatalog'

export function HomeStore() {
  return (
    <main>
      <Hero />
      <FeaturedProducts />
      <ProductCatalog />
    </main>
  )
}
