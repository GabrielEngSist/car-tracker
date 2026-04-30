# CarTracker

Track your car expenses and market price

## Layout

- **`src/Car.Tracker.Api`** — ASP.NET Core host (`Program.cs`), EF Core, migrations, REST API (namespaces under `Car.Tracker.Api.*`).
- **`src/Car.Tracker.Presentation`** — Vite + React SPA (root of this folder: `package.json`, `src/`, `public/`). Builds to `dist/` and is wired into publish via `Car.Tracker.Api.csproj` (`SpaRoot` → Presentation).
