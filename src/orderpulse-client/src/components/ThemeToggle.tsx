import { useEffect, useState } from 'react'
import * as preferencesService from '../services/preferencesService'

export function ThemeToggle() {
  const [isDark, setIsDark] = useState(
    () => document.documentElement.classList.contains('dark')
  )

  useEffect(() => {
    if (isDark) {
      document.documentElement.classList.add('dark')
    } else {
      document.documentElement.classList.remove('dark')
    }
  }, [isDark])

  function handleToggle() {
    const next = !isDark
    setIsDark(next)
    preferencesService.setTheme(next ? 'dark' : 'light').catch(() => {})
  }

  return (
    <button
      onClick={handleToggle}
      className="rounded-md p-2 text-sm transition-colors hover:bg-gray-100 dark:hover:bg-gray-700"
      aria-label="Toggle theme"
      title={isDark ? 'Switch to light mode' : 'Switch to dark mode'}
    >
      {isDark ? '☀️' : '🌙'}
    </button>
  )
}
