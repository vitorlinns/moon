import { Header } from './components/Header'
import { Footer } from './components/Footer'
import { CartDrawer } from './components/CartDrawer'
import { HomeStore } from './pages/HomeStore'
import { CartProvider } from './context/CartContext'

function App() {
  return (
    <CartProvider>
      <div className="min-h-svh bg-background text-foreground">
        <Header />
        <HomeStore />
        <Footer />
        <CartDrawer />
      </div>
    </CartProvider>
  )
}

export default App
