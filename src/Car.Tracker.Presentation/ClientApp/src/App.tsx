import { Navigate, Route, Routes } from 'react-router-dom'
import { CarsPage } from './pages/CarsPage'
import { CarDetailsPage } from './pages/CarDetailsPage'
import { MaintenancePage } from './pages/MaintenancePage'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<CarsPage />} />
      <Route path="/cars/:carId" element={<CarDetailsPage />} />
      <Route path="/cars/:carId/maintenance" element={<MaintenancePage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
