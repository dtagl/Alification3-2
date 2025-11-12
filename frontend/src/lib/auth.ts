import jwtDecode from 'jwt-decode'

const STORAGE_KEY = 'token'

export function setToken(token: string) {
  localStorage.setItem(STORAGE_KEY, token)
}
export function getToken() {
  return localStorage.getItem(STORAGE_KEY) || ''
}
export function clearToken() {
  localStorage.removeItem(STORAGE_KEY)
}

export type Decoded = {
  sub: string
  name: string
  role: 'Admin' | 'User'
  companyId: string
  exp: number
}

export function getUser() {
  const t = getToken()
  if (!t) return null
  try {
    return jwtDecode<Decoded>(t)
  } catch {
    return null
  }
}

export function isAdmin() {
  return getUser()?.role === 'Admin'
}
export function getCompanyId() {
  return getUser()?.companyId
}
export function getUserId() {
  return getUser()?.sub
}
