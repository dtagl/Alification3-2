import { Route, Routes, Navigate } from 'react-router-dom'
import Entry from '@/pages/Entry'
import Join from '@/pages/Join'
import CreateCompany from '@/pages/CreateCompany'
import Login from '@/pages/Login'
import Home from '@/pages/Home'
import Room from '@/pages/Room'
import MyBookings from '@/pages/MyBookings'
import AvailableNow from '@/pages/AvailableNow'
import Search from '@/pages/Search'
import Admin from '@/pages/Admin'
import { getToken } from '@/lib/auth'

export default function App() {
  const isAuthed = !!getToken()
  return (
    <Routes>
      <Route path="/entry" element={<Entry />} />
      <Route path="/join" element={<Join />} />
      <Route path="/create-company" element={<CreateCompany />} />
      <Route path="/login" element={<Login />} />

      <Route path="/home" element={isAuthed ? <Home /> : <Navigate to="/entry" replace />} />
      <Route path="/room/:roomId" element={isAuthed ? <Room /> : <Navigate to="/entry" replace />} />
      <Route path="/my-bookings" element={isAuthed ? <MyBookings /> : <Navigate to="/entry" replace />} />
      <Route path="/available-now" element={isAuthed ? <AvailableNow /> : <Navigate to="/entry" replace />} />
      <Route path="/search" element={isAuthed ? <Search /> : <Navigate to="/entry" replace />} />
      <Route path="/admin" element={isAuthed ? <Admin /> : <Navigate to="/entry" replace />} />

      <Route path="*" element={<Navigate to="/entry" replace />} />
    </Routes>
  )
}
