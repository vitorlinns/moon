import { Header } from './components/Header'
import { Footer } from './components/Footer'
import { HomeStore } from './pages/HomeStore'

function App() {
  return (
    <div className="min-h-svh bg-background text-foreground">
      <Header />
      <HomeStore />
      <Footer />
    </div>
  )
}

export default App
