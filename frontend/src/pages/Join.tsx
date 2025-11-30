import BackButton from '@/components/BackButton'
import { Link } from 'react-router-dom'

export default function Join() {
  return (
    <div className="min-h-screen grid place-items-center p-6">
           <BackButton/>
      <div className="max-w-md w-full space-y-6">
        <h1 className="text-2xl font-bold text-center">Присоединение</h1>
        <div className="grid gap-3">
          <Link to="/create-company" className="px-4 py-3 rounded-lg bg-sky-600 text-white text-center font-medium hover:bg-sky-700">Создать компанию</Link>
          <Link to="/login" className="px-4 py-3 rounded-lg bg-gray-900 text-white text-center font-medium hover:bg-black">Войти</Link>
        </div>
      </div>
    </div>
  )
}
