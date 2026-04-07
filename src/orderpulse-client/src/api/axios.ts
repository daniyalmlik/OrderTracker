import axios, { type InternalAxiosRequestConfig } from 'axios'
import * as authService from '../services/authService'

export const api = axios.create({
  baseURL: 'http://localhost:5098/api',
  withCredentials: true,
})

// Token accessor — set by AuthProvider after login/refresh
let currentToken: string | null = null
let clearAuth: (() => void) | null = null

export function setApiToken(token: string | null) {
  currentToken = token
}

export function setApiClearAuth(fn: () => void) {
  clearAuth = fn
}

// Attach Bearer token to every request
api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  if (currentToken && config.headers) {
    config.headers.Authorization = `Bearer ${currentToken}`
  }
  return config
})

// Silent refresh on 401
let isRefreshing = false
let failedQueue: Array<{
  resolve: (token: string) => void
  reject: (err: unknown) => void
}> = []

function flushQueue(token: string | null, error: unknown) {
  failedQueue.forEach(({ resolve, reject }) => {
    if (token) resolve(token)
    else reject(error)
  })
  failedQueue = []
}

api.interceptors.response.use(
  (response) => response,
  async (error: unknown) => {
    if (!axios.isAxiosError(error)) return Promise.reject(error)

    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean
    }

    const is401 =
      error.response?.status === 401 &&
      !originalRequest._retry &&
      !originalRequest.url?.includes('/auth/')

    if (!is401) return Promise.reject(error)

    if (isRefreshing) {
      return new Promise<string>((resolve, reject) => {
        failedQueue.push({ resolve, reject })
      }).then((token) => {
        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${token}`
        }
        return api(originalRequest)
      })
    }

    originalRequest._retry = true
    isRefreshing = true

    try {
      const res = await authService.refresh()
      const newToken = res.accessToken
      currentToken = newToken
      flushQueue(newToken, null)
      if (originalRequest.headers) {
        originalRequest.headers.Authorization = `Bearer ${newToken}`
      }
      return api(originalRequest)
    } catch (refreshError) {
      flushQueue(null, refreshError)
      currentToken = null
      clearAuth?.()
      window.location.href = '/login'
      return Promise.reject(refreshError)
    } finally {
      isRefreshing = false
    }
  }
)
