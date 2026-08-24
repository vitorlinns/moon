import { Routes, Route } from 'react-router-dom'
import { Header } from './components/Header'
import { Footer } from './components/Footer'
import { CartDrawer } from './components/CartDrawer'
import { AuthModal } from './components/AuthModal'
import { ToastViewport } from './components/ToastViewport'
import { HomeStore } from './pages/HomeStore'
import { AccountLayout } from './pages/account/AccountLayout'
import { PersonalData } from './pages/account/PersonalData'
import { Addresses } from './pages/account/Addresses'
import { Payment } from './pages/account/Payment'
import { Orders } from './pages/account/Orders'
import { Settings } from './pages/account/Settings'
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
            <Routes>
              <Route path="/" element={<HomeStore />} />
              <Route path="/minha-conta" element={<AccountLayout />}>
                <Route index element={<PersonalData />} />
                <Route path="enderecos" element={<Addresses />} />
                <Route path="pagamento" element={<Payment />} />
                <Route path="pedidos" element={<Orders />} />
                <Route path="configuracoes" element={<Settings />} />
              </Route>
            </Routes>
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
