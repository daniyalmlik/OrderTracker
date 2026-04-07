import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from 'react'
import * as authService from '../services/authService'
import { setApiClearAuth, setApiToken } from '../api/axios'

interface AuthContextValue {
  accessToken: string | null
  userId: number | null
  userEmail: string | null
  isLoading: boolean
  login: (token: string) => void
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

function parseJwtPayload(token: string): Record<string, unknown> {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
    return JSON.parse(atob(base64)) as Record<string, unknown>
  } catch {
    return {}
  }
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [accessToken, setAccessToken] = useState<string | null>(null)
  const [userId, setUserId] = useState<number | null>(null)
  const [userEmail, setUserEmail] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const applyToken = useCallback((token: string) => {
    const payload = parseJwtPayload(token)
    setAccessToken(token)
    setUserId(payload.sub ? Number(payload.sub) : null)
    setUserEmail(typeof payload.email === 'string' ? payload.email : null)
    setApiToken(token)
  }, [])

  const clearTokenState = useCallback(() => {
    setAccessToken(null)
    setUserId(null)
    setUserEmail(null)
    setApiToken(null)
  }, [])

  // Register the clear function so the axios interceptor can trigger a logout
  useEffect(() => {
    setApiClearAuth(clearTokenState)
  }, [clearTokenState])

  const login = useCallback(
    (token: string) => {
      applyToken(token)
    },
    [applyToken]
  )

  const logout = useCallback(async () => {
    try {
      await authService.logout()
    } catch {
      // ignore network errors on logout
    }
    clearTokenState()
  }, [clearTokenState])

  // Attempt silent refresh on mount using the HTTP-only cookie
  useEffect(() => {
    authService
      .refresh()
      .then((res) => applyToken(res.accessToken))
      .catch(() => {
        // No valid cookie — user needs to log in
      })
      .finally(() => setIsLoading(false))
  }, [applyToken])

  return (
    <AuthContext.Provider
      value={{ accessToken, userId, userEmail, isLoading, login, logout }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
