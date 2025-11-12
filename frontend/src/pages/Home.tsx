import { useEffect, useState } from 'react'
import { api } from '@/lib/api'
import { Link, useNavigate } from 'react-router-dom'
import { isAdmin } from '@/lib/auth'

type Room = { id: string; name: string; capacity: number; description: string }

export default function Home() {
  const [rooms, setRooms] = useState<Room[]>([])
  const [loading, setLoading] = useState(true)
  const nav = useNavigate()

  useEffect(() => {
    async function load() {
      try {
        const res = await api.get<Room[]>('/rooms/company')
        setRooms(res.data)
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])

  return (
    <div className="min-h-screen p-6">
      <header className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Комнаты</h1>
        <div className="flex gap-2">
          <button onClick={()=>nav('/my-bookings')} className="px-3 py-2 rounded bg-sky-600 text-white">Мои брони</button>
          <button onClick={()=>nav('/available-now')} className="px-3 py-2 rounded bg-emerald-600 text-white">Доступные сейчас</button>
          <button onClick={()=>nav('/search')} className="px-3 py-2 rounded bg-gray-900 text-white">Поиск</button>
          {isAdmin() && (
            <button onClick={()=>nav('/admin')} className="px-3 py-2 rounded bg-amber-600 text-white">Админ</button>
          )}
        </div>
      </header>

      {loading ? (
        <div>Загрузка...</div>
      ) : (
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {rooms.map(r => (
            <Link key={r.id} to={`/room/${r.id}`} className="block rounded-xl border p-4 hover:shadow">
              <div className="font-semibold">{r.name}</div>
              <div className="text-sm text-gray-600">Вместимость: {r.capacity}</div>
              <div className="text-sm text-gray-600 line-clamp-2">{r.description}</div>
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}
