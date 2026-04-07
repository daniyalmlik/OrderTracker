import { Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { ThemeToggle } from './ThemeToggle'

export function AppLayout() {
  const { userEmail, logout } = useAuth()
  const navigate = useNavigate()

  async function handleLogout() {
    await logout()
    navigate('/login')
  }

  return (
    <div className="flex min-h-screen flex-col" style={{ backgroundColor: 'var(--color-bg)' }}>
      <header
        className="sticky top-0 z-10 border-b px-6 py-3 shadow-sm"
        style={{
          backgroundColor: 'var(--color-surface)',
          borderColor: 'var(--color-border)',
        }}
      >
        <div className="mx-auto flex max-w-6xl items-center justify-between">
          <div className="flex items-center gap-2">
            <span className="text-2xl">📦</span>
            <span className="text-lg font-semibold" style={{ color: 'var(--color-text)' }}>
              OrderPulse
            </span>
          </div>
          <div className="flex items-center gap-3">
            {userEmail && (
              <span className="text-sm" style={{ color: 'var(--color-text-muted)' }}>
                {userEmail}
              </span>
            )}
            <ThemeToggle />
            <button
              onClick={handleLogout}
              className="rounded-md bg-red-500 px-3 py-1.5 text-sm font-medium text-white transition-colors hover:bg-red-600"
            >
              Logout
            </button>
          </div>
        </div>
      </header>
      <main className="mx-auto w-full max-w-6xl flex-1 px-6 py-6">
        <Outlet />
      </main>
    </div>
  )
}
