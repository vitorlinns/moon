import { Header } from './components/Header'
import { Footer } from './components/Footer'
import { CartDrawer } from './components/CartDrawer'
import { AuthModal } from './components/AuthModal'
import { ToastViewport } from './components/ToastViewport'
import { HomeStore } from './pages/HomeStore'
import { CartProvider } from './context/CartContext'
import { AuthProvider } from './context/AuthContext'
import { ToastProvider } from './context/ToastContext'

function App() {
  return (
    <ToastProvider>
      <AuthProvider>
        <CartProvider>
          <div className="min-h-svh bg-background text-foreground">
            <Header />
            <HomeStore />
            <Footer />
            <CartDrawer />
            <AuthModal />
            <ToastViewport />
          </div>
        </CartProvider>
      </AuthProvider>
    </ToastProvider>
  )
}

export default App
