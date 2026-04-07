import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Toaster } from 'react-hot-toast'
import { App } from './App'
import { AuthProvider } from './contexts/AuthContext'
import './index.css'

// Apply saved theme before first paint to avoid flash
;(function applyThemeFromCookie() {
  const match = document.cookie.match(/(?:^|;\s*)theme=([^;]*)/)
  if (match?.[1] === 'dark') {
    document.documentElement.classList.add('dark')
  }
})()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider>
      <App />
      <Toaster
        position="top-right"
        toastOptions={{
          style: {
            background: 'var(--color-surface)',
            color: 'var(--color-text)',
            border: '1px solid var(--color-border)',
          },
        }}
      />
    </AuthProvider>
  </StrictMode>
)
