import { useEffect, useState } from 'react'
import { api } from '@/lib/api'
import BackButton from '@/components/BackButton';

export default function MyBookings() {
  const [data, setData] = useState<{ active: any[]; past: any[] }>({ active: [], past: [] })

  useEffect(() => {
    async function load() {
      const res = await api.get('/home/my-bookings')
      setData(res.data)
    }
    load()
  }, [])

  async function cancel(id: string) {
    await api.delete(`/rooms/booking/${id}`)
    const res = await api.get('/home/my-bookings')
    setData(res.data)
  }

  return (
    <div className="p-6 space-y-6">
      <BackButton />
      <section>
        <h2 className="text-xl font-bold mb-3">Текущиеee </h2>
        <div className="grid gap-3">
          {data.active.map((b: any) => (
            <div key={b.id} className="border rounded-lg p-3 flex justify-between items-center">
              <div>
                <div className="font-semibold">{b.roomName}</div>
                <div className="text-sm text-gray-600">{new Date(b.startAt).toLocaleString()} — {new Date(b.endAt).toLocaleString()}</div>
              </div>
              <button onClick={() => cancel(b.id)} className="px-3 py-2 rounded bg-red-600 text-white">Отменить</button>
            </div>
          ))}
        </div>
      </section>

      <hr />

      <section>
        <h2 className="text-xl font-bold mb-3">Прошедшие</h2>
        <div className="grid gap-3">
          {data.past.map((b: any) => (
            <div key={b.id} className="border rounded-lg p-3">
              <div className="font-semibold">{b.roomName}</div>
              <div className="text-sm text-gray-600">{new Date(b.startAt).toLocaleString()} — {new Date(b.endAt).toLocaleString()}</div>
            </div>
          ))}
        </div>
      </section>
    </div>
  )
}
