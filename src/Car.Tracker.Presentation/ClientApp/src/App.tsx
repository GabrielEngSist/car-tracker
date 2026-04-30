import { Navigate, Route, Routes } from 'react-router-dom'
import { AppShell } from './components/AppShell'
import { CarsPage } from './pages/CarsPage'
import { CarDetailsPage } from './pages/CarDetailsPage'
import { MaintenancePage } from './pages/MaintenancePage'
import { SettingsPage } from './pages/SettingsPage'

export default function App() {
  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route path="/" element={<CarsPage />} />
        <Route path="/cars/:carId" element={<CarDetailsPage />} />
        <Route path="/cars/:carId/maintenance" element={<MaintenancePage />} />
        <Route path="/settings" element={<SettingsPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
