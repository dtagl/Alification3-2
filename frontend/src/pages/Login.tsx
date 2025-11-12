import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '@/lib/api'
import { setToken } from '@/lib/auth'

export default function Login() {
  const nav = useNavigate()
  const [form, setForm] = useState({
    companyName: '',
    companyPassword: '',
    userName: '',
  })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function submit(e: React.FormEvent) {
    e.preventDefault()
    setLoading(true)
    setError(null)
    try {
      const tgid = (window as any).Telegram?.WebApp?.initDataUnsafe?.user?.id as number | undefined
      const payload = { ...form, telegramId: tgid ?? 0 }
      const res = await api.post<string>('/first/login-telegram', payload)
      setToken(res.data)
      nav('/home', { replace: true })
    } catch (err: any) {
      setError(err?.response?.data || 'Ошибка входа')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen p-6 grid place-items-center">
      <form onSubmit={submit} className="max-w-lg w-full bg-white rounded-xl shadow p-6 grid gap-4">
        <h1 className="text-xl font-bold">Вход</h1>
        {error && <div className="text-red-600 text-sm">{error}</div>}
        <input placeholder="Название компании (для новых)" className="border rounded px-3 py-2" value={form.companyName} onChange={e=>setForm(f=>({...f, companyName:e.target.value}))} />
        <input placeholder="Пароль компании (для новых)" className="border rounded px-3 py-2" value={form.companyPassword} onChange={e=>setForm(f=>({...f, companyPassword:e.target.value}))} />
        <input placeholder="Ваше имя (опционально)" className="border rounded px-3 py-2" value={form.userName} onChange={e=>setForm(f=>({...f, userName:e.target.value}))} />
        <button disabled={loading} className="px-4 py-2 rounded-lg bg-gray-900 text-white font-medium disabled:opacity-50">
          {loading ? 'Входим...' : 'Войти'}
        </button>
      </form>
    </div>
  )
}
