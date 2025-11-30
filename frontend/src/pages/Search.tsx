import { useState } from 'react'
import { api } from '@/lib/api'
import { useNavigate } from 'react-router-dom'
import BackButton from '@/components/BackButton';

type Room = { id: string; name: string; capacity: number; description: string; isAvailable?: boolean }

export default function Search() {
  const [filters, setFilters] = useState({
    MinCapacity: '',
    MaxCapacity: '',
    Name: '',
    Description: '',
    StartAt: '',
    EndAt: '',
  })
  const [results, setResults] = useState<Room[]>([])
  const [loading, setLoading] = useState(false)
  const nav = useNavigate()

  async function submit(e: React.FormEvent) {
    e.preventDefault()
    setLoading(true)
    try {
      const params: Record<string, string> = {}
      Object.entries(filters).forEach(([k, v]) => { if (v) params[k] = v })
      const res = await api.get<Room[]>('/rooms/findroom', { params })
      setResults(res.data)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="p-6 space-y-6">
      <BackButton />
      <form onSubmit={submit} className="grid gap-3 bg-white rounded-xl border p-4">
        <h2 className="font-semibold">Поиск комнат</h2>
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-3">
          <input placeholder="Минимальная вместимость" className="border rounded px-3 py-2" value={filters.MinCapacity} onChange={e => setFilters(f => ({ ...f, MinCapacity: e.target.value }))} />
          <input placeholder="Максимальная вместимость" className="border rounded px-3 py-2" value={filters.MaxCapacity} onChange={e => setFilters(f => ({ ...f, MaxCapacity: e.target.value }))} />
          <input placeholder="Название" className="border rounded px-3 py-2" value={filters.Name} onChange={e => setFilters(f => ({ ...f, Name: e.target.value }))} />
          <input placeholder="Описание" className="border rounded px-3 py-2" value={filters.Description} onChange={e => setFilters(f => ({ ...f, Description: e.target.value }))} />
          <input placeholder="Начало (ISO UTC)" className="border rounded px-3 py-2" value={filters.StartAt} onChange={e => setFilters(f => ({ ...f, StartAt: e.target.value }))} />
          <input placeholder="Конец (ISO UTC)" className="border rounded px-3 py-2" value={filters.EndAt} onChange={e => setFilters(f => ({ ...f, EndAt: e.target.value }))} />
        </div>
        <button disabled={loading} className="px-4 py-2 rounded bg-gray-900 text-white w-fit">{loading ? 'Поиск...' : 'Найти'}</button>
      </form>

      <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {results.map(r => (
          <div key={r.id} className="rounded-xl border p-4">
            <div className="font-semibold">{r.name}</div>
            <div className="text-sm text-gray-600">Вместимость: {r.capacity}</div>
            <div className="text-sm text-gray-600 line-clamp-2">{r.description}</div>
            {r.isAvailable ? (
              <button onClick={() => nav(`/room/${r.id}`)} className="mt-3 px-3 py-2 rounded bg-sky-600 text-white">Забронировать</button>
            ) : (
              <button onClick={() => nav(`/room/${r.id}`)} className="mt-3 px-3 py-2 rounded border">Открыть</button>
            )}
          </div>
        ))}
      </div>
    </div>
  )
}
