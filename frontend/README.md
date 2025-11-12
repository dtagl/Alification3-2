# Room Booking Frontend (React + Vite + Tailwind)

A Telegram WebApp-ready frontend for the Room Booking API.

## Prerequisites
- Node.js 18+
- API running locally on http://localhost:8080 (or your Railway URL)

## Configure
- Copy env example and set your API URL if not using the dev proxy:
```
cp .env.example .env
# set VITE_API_BASE to your backend URL in production (Railway)
```

During local dev, `vite.config.ts` proxies `/api` to `http://localhost:8080`. In production builds, set `VITE_API_BASE`.

## Install
```
npm install
```

## Run (development)
```
npm run dev
```
App will be available on http://localhost:5173

## Build (production)
```
npm run build
npm run preview
```

## Pages
- /entry: Telegram user check → redirects to /home or /join
- /join: choose create or login
- /create-company: POST /api/first/create-company
- /login: POST /api/first/login-telegram
- /home: GET /api/rooms/company
- /room/:roomId: timeslots, booking info, booking
- /my-bookings: GET /api/home/my-bookings; cancel via DELETE /api/rooms/booking/:id
- /available-now: GET /api/home/available-now
- /search: GET /api/rooms/findroom
- /admin: Admin dashboard and management endpoints

## Telegram WebApp
The app reads Telegram user via `window.Telegram.WebApp.initDataUnsafe.user` inside /entry. If absent (non-Telegram browser), it falls back to /join.

## Styling
TailwindCSS configured via PostCSS.
