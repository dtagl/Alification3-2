import { useEffect, useState } from 'react'
import { api } from '@/lib/api'
import { Link, useNavigate } from 'react-router-dom'
import { isAdmin } from '@/lib/auth'
import BackButton from '@/components/BackButton'

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


    <div className="min-h-screen p-8 bg-gradient-to-br from-sky-50 via-white to-emerald-50">
      <header className="flex flex-col sm:flex-row items-start sm:items-center justify-between mb-10">
        <BackButton />
        <h1 className="text-3xl font-extrabold tracking-tight text-gray-900 drop-shadow-sm">
          🏠 Комнаты
        </h1>

        <div className="flex flex-wrap gap-3 mt-4 sm:mt-0">
          <button
            onClick={() => nav('/my-bookings')}
            className="px-4 py-2.5 rounded-xl bg-sky-600 text-white font-medium 
                   shadow-md hover:bg-sky-700 hover:shadow-lg 
                   active:scale-[0.98] transition-all duration-150"
          >
            Мои брони
          </button>

          <button
            onClick={() => nav('/available-now')}
            className="px-4 py-2.5 rounded-xl bg-emerald-600 text-white font-medium 
                   shadow-md hover:bg-emerald-700 hover:shadow-lg 
                   active:scale-[0.98] transition-all duration-150"
          >
            Доступные сейчас
          </button>

          <button
            onClick={() => nav('/search')}
            className="px-4 py-2.5 rounded-xl bg-gray-900 text-white font-medium 
                   shadow-md hover:bg-gray-800 hover:shadow-lg 
                   active:scale-[0.98] transition-all duration-150"
          >
            Поиск
          </button>

          {isAdmin() && (
            <button
              onClick={() => nav('/admin')}
              className="px-4 py-2.5 rounded-xl bg-gradient-to-r from-amber-500 to-orange-600 
                     text-white font-semibold shadow-md hover:from-amber-600 hover:to-orange-700 
                     hover:shadow-lg active:scale-[0.98] transition-all duration-150"
            >
              👑 Админ
            </button>
          )}
        </div>
      </header>



      {loading ? (
        <div className="flex items-center justify-center h-64 text-gray-600 text-lg animate-pulse">
          Загрузка...
        </div>
      ) : (
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {rooms.map(r => (
            <Link
              key={r.id}
              to={`/room/${r.id}`}
              className="group block rounded-2xl border border-gray-200 bg-white/80 backdrop-blur-sm 
                     p-5 shadow-sm hover:shadow-xl transition-all duration-200 hover:-translate-y-1"
            >
              <div className="font-semibold text-lg text-gray-900 flex items-center justify-between">
                {r.name}
                <span className="text-xs px-2 py-1 rounded-full bg-sky-100 text-sky-700 font-medium">
                  {r.capacity} мест
                </span>
              </div>
              <div className="mt-2 text-sm text-gray-600 line-clamp-2">
                {r.description}
              </div>
              <div className="mt-4 flex items-center justify-between text-sm text-gray-500">
                <span className="group-hover:text-sky-600 transition-colors">
                  Подробнее →
                </span>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>

  )
}



// я тут изменил то что было в header 👇 это ваш , 👆это мой


// {/* <header className="flex flex-col sm:flex-row items-start sm:items-center justify-between mb-10">
//       <div className="flex items-center gap-3">
//         <BackButton />
//         <h1 className="text-3xl font-extrabold tracking-tight text-gray-900 drop-shadow-sm">
//           🏠 Комнаты
//         </h1>
//       </div>

//       <div className="flex flex-wrap gap-3 mt-4 sm:mt-0">
//         <button
//           onClick={() => nav('/my-bookings')}
//           className="px-4 py-2.5 rounded-xl bg-sky-600 text-white font-medium
//                shadow-md hover:bg-sky-700 hover:shadow-lg
//                active:scale-[0.98] transition-all duration-150"
//         >
//           Мои брони
//         </button>

//         <button
//           onClick={() => nav('/available-now')}
//           className="px-4 py-2.5 rounded-xl bg-emerald-600 text-white font-medium
//                shadow-md hover:bg-emerald-700 hover:shadow-lg
//                active:scale-[0.98] transition-all duration-150"
//         >
//           Доступные сейчас
//         </button>

//         <button
//           onClick={() => nav('/search')}
//           className="px-4 py-2.5 rounded-xl bg-gray-900 text-white font-medium
//                shadow-md hover:bg-gray-800 hover:shadow-lg
//                active:scale-[0.98] transition-all duration-150"
//         >
//           Поиск
//         </button>

//         {isAdmin() && (
//           <button
//             onClick={() => nav('/admin')}
//             className="px-4 py-2.5 rounded-xl bg-gradient-to-r from-amber-500 to-orange-600
//                  text-white font-semibold shadow-md hover:from-amber-600 hover:to-orange-700
//                  hover:shadow-lg active:scale-[0.98] transition-all duration-150"
//           >
//             👑 Админ
//           </button>
//         )}
//       </div>
//     </header> */}