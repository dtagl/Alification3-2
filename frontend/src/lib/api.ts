import axios from 'axios'
import { getToken, clearToken } from './auth'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE || '/api',
})

api.interceptors.request.use((config) => {
  const token = getToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      clearToken()
      if (location.pathname !== '/entry') location.href = '/entry'
    }
    return Promise.reject(err)
  }
)
