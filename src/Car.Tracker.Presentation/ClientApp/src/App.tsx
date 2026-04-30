import { Navigate, Route, Routes } from 'react-router-dom'
import { AppShell } from './components/AppShell'
import { CarsPage } from './pages/CarsPage'
import { CarDetailsPage } from './pages/CarDetailsPage'
import { MaintenancePage } from './pages/MaintenancePage'
import { FuelingsPage } from './pages/FuelingsPage'
import { CarFuelingsPage } from './pages/CarFuelingsPage'
import { MaintenanceServicesPage } from './pages/MaintenanceServicesPage'
import { SettingsPage } from './pages/SettingsPage'

export default function App() {
  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route path="/" element={<CarsPage />} />
        <Route path="/fuelings" element={<FuelingsPage />} />
        <Route path="/maintenance-services" element={<MaintenanceServicesPage />} />
        <Route path="/cars/:carId" element={<CarDetailsPage />} />
        <Route path="/cars/:carId/maintenance" element={<MaintenancePage />} />
        <Route path="/cars/:carId/fuelings" element={<CarFuelingsPage />} />
        <Route path="/settings" element={<SettingsPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
