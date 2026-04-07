import { api } from '../api/axios'

export type Theme = 'light' | 'dark'

export async function getTheme(): Promise<Theme> {
  const { data } = await api.get<{ theme: Theme }>('/preferences/theme')
  return data.theme ?? 'light'
}

export async function setTheme(theme: Theme): Promise<void> {
  await api.post('/preferences/theme', { theme })
}
