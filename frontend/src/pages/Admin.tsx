import { useEffect, useState } from 'react'
import { api } from '@/lib/api'

export default function Admin() {
  const [overview, setOverview] = useState<any | null>(null)
  const [util, setUtil] = useState<any[]>([])
  const [top, setTop] = useState<any[]>([])
  const [activity, setActivity] = useState<any[]>([])
  const [trend, setTrend] = useState<any[]>([])
  const [users, setUsers] = useState<any[]>([])

  const [roomForm, setRoomForm] = useState({ name: '', capacity: 1, description: '' })
  const [companyForm, setCompanyForm] = useState({ workingStart: '09:00:00', workingEnd: '18:00:00', name: '', password: '' })

  async function loadAll() {
    const [o, u, t, a, tr, us] = await Promise.all([
      api.get('/admin/overview'),
      api.get('/admin/room-utilization'),
      api.get('/admin/top-rooms'),
      api.get('/admin/user-activity'),
      api.get('/admin/bookings-trend'),
      api.get('/admin/all-users'),
    ])
    setOverview(o.data)
    setUtil(u.data)
    setTop(t.data)
    setActivity(a.data)
    setTrend(tr.data)
    setUsers(us.data)
  }

  useEffect(() => { loadAll() }, [])

  async function createRoom(e: React.FormEvent) {
    e.preventDefault()
    await api.post('/rooms/create', roomForm)
    setRoomForm({ name: '', capacity: 1, description: '' })
    await loadAll()
  }

  async function changeHours(e: React.FormEvent) {
    e.preventDefault()
    await api.put('/admin/change-working-hours', { workingStart: companyForm.workingStart, workingEnd: companyForm.workingEnd })
    await loadAll()
  }

  async function changeCompanyName(e: React.FormEvent) {
    e.preventDefault()
    await api.put('/admin/change-company-name', companyForm.name, { headers: { 'Content-Type': 'application/json' } })
    await loadAll()
  }

  async function changePassword(e: React.FormEvent) {
    e.preventDefault()
    await api.put('/admin/change-password', companyForm.password, { headers: { 'Content-Type': 'application/json' } })
  }

  async function makeAdmin(id: string) {
    await api.put(`/admin/make-admin/${id}`)
    await loadAll()
  }
  async function revokeAdmin(id: string) {
    await api.put(`/admin/revoke-admin/${id}`)
    await loadAll()
  }
  async function deleteUser(id: string) {
    await api.delete(`/admin/delete-user/${id}`)
    await loadAll()
  }

  return (
    <div className="p-6 space-y-8">
      <h1 className="text-2xl font-bold">Админ панель</h1>

      <section className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="rounded-xl border p-4">
          <div className="text-sm text-gray-600">Комнат</div>
          <div className="text-2xl font-semibold">{overview?.totalRooms ?? '-'}</div>
        </div>
        <div className="rounded-xl border p-4">
          <div className="text-sm text-gray-600">Пользователей</div>
          <div className="text-2xl font-semibold">{overview?.totalUsers ?? '-'}</div>
        </div>
        <div className="rounded-xl border p-4">
          <div className="text-sm text-gray-600">Всего броней</div>
          <div className="text-2xl font-semibold">{overview?.totalBookings ?? '-'}</div>
        </div>
        <div className="rounded-xl border p-4">
          <div className="text-sm text-gray-600">Активно сейчас</div>
          <div className="text-2xl font-semibold">{overview?.activeBookings ?? '-'}</div>
        </div>
      </section>

      <section className="grid lg:grid-cols-2 gap-6">
        <div className="rounded-xl border p-4">
          <h2 className="font-semibold mb-3">Создать комнату</h2>
          <form onSubmit={createRoom} className="grid gap-3">
            <input required placeholder="Название" className="border rounded px-3 py-2" value={roomForm.name} onChange={e=>setRoomForm(f=>({...f, name:e.target.value}))} />
            <input type="number" min={1} required placeholder="Вместимость" className="border rounded px-3 py-2" value={roomForm.capacity} onChange={e=>setRoomForm(f=>({...f, capacity:Number(e.target.value)}))} />
            <input placeholder="Описание" className="border rounded px-3 py-2" value={roomForm.description} onChange={e=>setRoomForm(f=>({...f, description:e.target.value}))} />
            <button className="px-3 py-2 rounded bg-sky-600 text-white w-fit">Создать</button>
          </form>
        </div>

        <div className="rounded-xl border p-4 grid gap-4">
          <h2 className="font-semibold">Настройки компании</h2>
          <form onSubmit={changeHours} className="grid gap-2">
            <div className="text-sm text-gray-600">Рабочее время</div>
            <div className="grid grid-cols-2 gap-2">
              <input placeholder="Начало (HH:mm:ss)" className="border rounded px-3 py-2" value={companyForm.workingStart} onChange={e=>setCompanyForm(f=>({...f, workingStart:e.target.value}))} />
              <input placeholder="Конец (HH:mm:ss)" className="border rounded px-3 py-2" value={companyForm.workingEnd} onChange={e=>setCompanyForm(f=>({...f, workingEnd:e.target.value}))} />
            </div>
            <button className="px-3 py-2 rounded border w-fit">Обновить</button>
          </form>
          <form onSubmit={changeCompanyName} className="grid gap-2">
            <div className="text-sm text-gray-600">Название компании</div>
            <input placeholder="Новое название" className="border rounded px-3 py-2" value={companyForm.name} onChange={e=>setCompanyForm(f=>({...f, name:e.target.value}))} />
            <button className="px-3 py-2 rounded border w-fit">Сохранить</button>
          </form>
          <form onSubmit={changePassword} className="grid gap-2">
            <div className="text-sm text-gray-600">Пароль</div>
            <input type="password" placeholder="Новый пароль" className="border rounded px-3 py-2" value={companyForm.password} onChange={e=>setCompanyForm(f=>({...f, password:e.target.value}))} />
            <button className="px-3 py-2 rounded border w-fit">Сменить пароль</button>
          </form>
        </div>
      </section>

      <section className="grid lg:grid-cols-2 gap-6">
        <div className="rounded-xl border p-4">
          <h2 className="font-semibold mb-3">Утилизация комнат</h2>
          <div className="grid gap-2">
            {util.map((x,i)=> (
              <div key={i} className="flex justify-between text-sm">
                <div>{x.room}</div>
                <div>{x.utilizationPercent?.toFixed?.(1) ?? x.utilizationPercent}%</div>
              </div>
            ))}
          </div>
        </div>
        <div className="rounded-xl border p-4">
          <h2 className="font-semibold mb-3">Топ комнат</h2>
          <div className="grid gap-2">
            {top.map((x,i)=> (
              <div key={i} className="flex justify-between text-sm">
                <div>{x.room}</div>
                <div>{x.count}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="grid lg:grid-cols-2 gap-6">
        <div className="rounded-xl border p-4">
          <h2 className="font-semibold mb-3">Активность пользователей</h2>
          <div className="grid gap-2">
            {activity.map((x,i)=> (
              <div key={i} className="flex justify-between text-sm">
                <div>{x.user}</div>
                <div>{x.bookings} брони, {x.totalHours} ч</div>
              </div>
            ))}
          </div>
        </div>
        <div className="rounded-xl border p-4">
          <h2 className="font-semibold mb-3">Тренд за 7 дней</h2>
          <div className="grid gap-2">
            {trend.map((x,i)=> (
              <div key={i} className="flex justify-between text-sm">
                <div>{x.date}</div>
                <div>{x.count}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="rounded-xl border p-4">
        <h2 className="font-semibold mb-3">Пользователи</h2>
        <div className="grid gap-2">
          {users.map((u:any) => (
            <div key={u.id} className="flex items-center justify-between text-sm">
              <div>
                <div className="font-medium">{u.userName}</div>
                <div className="text-gray-600">{u.role}</div>
              </div>
              <div className="flex gap-2">
                <button onClick={()=>makeAdmin(u.id)} className="px-2 py-1 rounded border">Сделать админом</button>
                <button onClick={()=>revokeAdmin(u.id)} className="px-2 py-1 rounded border">Убрать админа</button>
                <button onClick={()=>deleteUser(u.id)} className="px-2 py-1 rounded bg-red-600 text-white">Удалить</button>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  )
}
