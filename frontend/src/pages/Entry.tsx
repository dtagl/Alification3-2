import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '@/lib/api'
import { setToken } from '@/lib/auth'
import BackButton from '@/components/BackButton'



export default function Entry() {
  const navigate = useNavigate()

  useEffect(() => {
    const tg = window.Telegram?.WebApp
    tg?.ready?.()
    tg?.expand?.()
    const user = tg?.initDataUnsafe?.user
    console.log('Telegram WebApp detected:', !!tg, 'User:', user)

    async function run() {
      try {
        if (!user) {
          // If not inside Telegram, show join
          navigate('/join', { replace: true })
          return
        }
        const payload = { telegramId: user.id }
        const res = await api.post<string>('/first/entrypage', payload)
        if (res.status === 200 && res.data) {
          setToken(res.data)
          navigate('/home', { replace: true })
        }
      } catch (e: any) {
        if (e?.response?.status === 404) {
          navigate('/join', { replace: true })
        }
      }
    }

    run()
  }, [navigate])

  return (
    <div className="min-h-screen grid place-items-center p-6">
      <BackButton />
      <div className="max-w-md w-full text-center space-y-4">
        <h1 className="text-2xl font-bold">Добро пожаловать</h1>
        <p className="text-gray-600">Проверяем ваш аккаунт...</p>
      </div>
    </div>
  )
}
