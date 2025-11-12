import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '@/lib/api'
import { setToken } from '@/lib/auth'

export default function CreateCompany() {
  const nav = useNavigate()
  const [form, setForm] = useState({
    companyName: '',
    password: '',
    workingStart: '09:00:00',
    workingEnd: '18:00:00',
    userName: '',
  })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function submit(e: React.FormEvent) {
    e.preventDefault()
    setLoading(true)
    setError(null)
    try {
      const tg = (window as any).Telegram?.WebApp
      const tgid = tg?.initDataUnsafe?.user?.id as number | undefined
      const payload = { ...form, telegramId: tgid ?? null }
      const res = await api.post<string>('/first/create-company', payload)
      setToken(res.data)
      nav('/home', { replace: true })
    } catch (err: any) {
      setError(err?.response?.data || 'Ошибка создания компании')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen p-6 grid place-items-center">
      <form onSubmit={submit} className="max-w-lg w-full bg-white rounded-xl shadow p-6 grid gap-4">
        <h1 className="text-xl font-bold">Создать компанию</h1>
        {error && <div className="text-red-600 text-sm">{error}</div>}
        <input required placeholder="Название компании" className="border rounded px-3 py-2" value={form.companyName} onChange={e=>setForm(f=>({...f, companyName:e.target.value}))} />
        <input required type="password" placeholder="Пароль компании" className="border rounded px-3 py-2" value={form.password} onChange={e=>setForm(f=>({...f, password:e.target.value}))} />
        <input placeholder="Ваше имя (опционально)" className="border rounded px-3 py-2" value={form.userName} onChange={e=>setForm(f=>({...f, userName:e.target.value}))} />
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="text-sm text-gray-600">Начало рабочего дня</label>
            <input required placeholder="09:00:00" className="border rounded px-3 py-2 w-full" value={form.workingStart} onChange={e=>setForm(f=>({...f, workingStart:e.target.value}))} />
          </div>
          <div>
            <label className="text-sm text-gray-600">Конец рабочего дня</label>
            <input required placeholder="18:00:00" className="border rounded px-3 py-2 w-full" value={form.workingEnd} onChange={e=>setForm(f=>({...f, workingEnd:e.target.value}))} />
          </div>
        </div>
        <button disabled={loading} className="px-4 py-2 rounded-lg bg-sky-600 text-white font-medium disabled:opacity-50">
          {loading ? 'Создаем...' : 'Создать'}
        </button>
      </form>
    </div>
  )
}
