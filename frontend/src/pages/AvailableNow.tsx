import { useEffect, useState } from 'react'
import { api } from '@/lib/api'
import { useNavigate } from 'react-router-dom'

type Room = { id: string; name: string; capacity: number; description: string }

export default function AvailableNow() {
  const [rooms, setRooms] = useState<Room[]>([])
  const nav = useNavigate()

  useEffect(() => {
    async function load() {
      const res = await api.get<Room[]>('/home/available-now')
      setRooms(res.data)
    }
    load()
  }, [])

  return (
    <div className="p-6">
      
      <h1 className="text-2xl font-bold mb-4">Свободные сейчас</h1>
      <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {rooms.map(r => (
          <button key={r.id} onClick={()=>nav(`/room/${r.id}`)} className="text-left rounded-xl border p-4 hover:shadow">
            <div className="font-semibold">{r.name}</div>
            <div className="text-sm text-gray-600">Вместимость: {r.capacity}</div>
            <div className="text-sm text-gray-600 line-clamp-2">{r.description}</div>
          </button>
        ))}
      </div>
    </div>
  )
}
