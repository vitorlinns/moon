import { Routes, Route } from 'react-router-dom'
import { AdminAuthProvider } from './context/AdminAuthContext'
import { ProtectedRoute } from './components/ProtectedRoute'
import { Login } from './pages/Login'
import { AdminLayout } from './pages/AdminLayout'
import { Dashboard } from './pages/Dashboard'
import { Products } from './pages/Products'
import { Categories } from './pages/Categories'
import { Orders } from './pages/Orders'
import { Billing } from './pages/Billing'

function App() {
  return (
    <AdminAuthProvider>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <AdminLayout />
            </ProtectedRoute>
          }
        >
          <Route index element={<Dashboard />} />
          <Route path="produtos" element={<Products />} />
          <Route path="categorias" element={<Categories />} />
          <Route path="pedidos" element={<Orders />} />
          <Route path="faturamento" element={<Billing />} />
        </Route>
      </Routes>
    </AdminAuthProvider>
  )
}

export default App
