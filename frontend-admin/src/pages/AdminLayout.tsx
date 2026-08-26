import { Outlet } from 'react-router-dom'
import { AdminSidebar } from '../components/AdminSidebar'
import { AdminHeader } from '../components/AdminHeader'

export function AdminLayout() {
  return (
    <div className="flex min-h-svh bg-background">
      <AdminSidebar />

      <div className="flex flex-1 flex-col">
        <AdminHeader />

        <main className="flex-1 px-6 py-10">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
