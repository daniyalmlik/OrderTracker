import axios from 'axios'
import type { AuthResponse } from '../api/types'

const BASE_URL = 'http://localhost:5098/api'

// Plain axios for auth calls to avoid circular dependency with the interceptor
const authAxios = axios.create({ baseURL: BASE_URL, withCredentials: true })

export async function register(
  email: string,
  password: string,
  fullName: string
): Promise<AuthResponse> {
  const { data } = await authAxios.post<AuthResponse>('/auth/register', {
    email,
    password,
    fullName,
  })
  return data
}

export async function login(
  email: string,
  password: string
): Promise<AuthResponse> {
  const { data } = await authAxios.post<AuthResponse>('/auth/login', {
    email,
    password,
  })
  return data
}

export async function refresh(): Promise<AuthResponse> {
  const { data } = await authAxios.post<AuthResponse>('/auth/refresh')
  return data
}

export async function logout(): Promise<void> {
  await authAxios.post('/auth/logout')
}
