import { useMemo, useState, useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { api } from '@/lib/api'

function daysAhead(n: number) {
  const arr: Date[] = []
  const today = new Date()
  today.setHours(0,0,0,0)
  for (let i=0;i<n;i++) {
    const d = new Date(today)
    d.setDate(today.getDate() + i)
    arr.push(d)
  }
  return arr
}

function groupByHour(slots: Record<string, boolean>) {
  const entries = Object.entries(slots).sort((a,b)=> new Date(a[0]).getTime() - new Date(b[0]).getTime())
  const map: Record<string, { time: string; free: boolean }[]> = {}
  for (const [iso, free] of entries) {
    const dt = new Date(iso)
    const hourKey = dt.toISOString().substring(11,13)
    const t = iso.substring(11,16)
    map[hourKey] ||= []
    map[hourKey].push({ time: t, free })
  }
  return map
}

export default function Room() {
  const { roomId } = useParams()
  const [selectedDate, setSelectedDate] = useState<Date>(new Date())
  const [slots, setSlots] = useState<Record<string, boolean>>({})
  const [info, setInfo] = useState<any | null>(null)

  const dates = useMemo(()=> daysAhead(14), [])

  useEffect(() => {
    async function load() {
      // send midnight UTC of the selected calendar day to the server
      const d = new Date(selectedDate)
      d.setUTCHours(0, 0, 0, 0)
      const dateIso = d.toISOString()
      const res = await api.get<Record<string, boolean>>(`/rooms/${roomId}/timeslots`, { params: { date: dateIso } })
      setSlots(res.data)
      setInfo(null)
    }
    if (roomId) load()
  }, [roomId, selectedDate])

  async function bookingInfo(iso: string) {
    const res = await api.get(`/rooms/${roomId}/booking-info`, { params: { time: iso } })
    setInfo(res.data)
  }

  async function book(startIso: string, endIso: string) {
    await api.post(`/rooms/${roomId}/book`, { startAt: startIso, endAt: endIso })
    const dateIso = new Date(selectedDate).toISOString()
    const res = await api.get<Record<string, boolean>>(`/rooms/${roomId}/timeslots`, { params: { date: dateIso } })
    setSlots(res.data)
  }

  const grouped = useMemo(()=> groupByHour(slots), [slots])

  return (
    <div className="p-6 space-y-6">
      <div className="flex gap-2 overflow-x-auto pb-2">
        {dates.map(d => {
          const label = d.toLocaleDateString('ru-RU', { day:'2-digit', month:'2-digit' })
          const sel = d.toDateString() === selectedDate.toDateString()
          return (
            <button key={d.toISOString()} onClick={()=>setSelectedDate(d)} className={`px-3 py-2 rounded-lg border ${sel? 'bg-sky-600 text-white' : 'bg-white'}`}>
              {label}
            </button>
          )
        })}
      </div>

      <div className="grid gap-4">
        {Object.entries(grouped).map(([hour, items]) => (
          <div key={hour} className="border rounded-lg p-4">
            <div className="font-semibold mb-2">{hour}:00</div>
            <div className="grid grid-cols-4 gap-2">
              {items.map((it, idx) => {
                // Build ISO using UTC hours to stay consistent with server (which operates in UTC baseline)
                const [h, m] = it.time.split(':').map(Number)
                const d = new Date(selectedDate)
                d.setUTCHours(h, m, 0, 0)
                const iso = d.toISOString()
                const isFree = it.free
                return (
                  <button
                    key={idx}
                    onClick={() => { bookingInfo(iso) }}
                    className={`px-2 py-2 rounded border text-sm ${isFree ? 'bg-emerald-50 border-emerald-300' : 'bg-red-50 border-red-300 line-through'}`}
                    // Allow clicking even if booked, so we can show who booked
                    title={isFree ? 'Свободно' : 'Занято'}
                  >
                    {it.time}
                  </button>
                )
              })}
            </div>
          </div>
        ))}
      </div>

      {info && (
        <div className="border rounded-xl p-4">
          <div className="font-semibold mb-1">Информация</div>
          {info.isBooked ? (
            <div className="text-sm">Забронировано: {info.userName}</div>
          ) : (
            <div className="text-sm">Свободно</div>
          )}
          <div className="text-sm text-gray-600">
            {new Date(info.startAt).toLocaleString()} — {new Date(info.endAt).toLocaleString()}
          </div>
          {!info.isBooked && (
            <div className="mt-3">
              <button className="px-3 py-2 rounded bg-sky-600 text-white" onClick={() => book(info.startAt, info.endAt)}>Забронировать</button>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
