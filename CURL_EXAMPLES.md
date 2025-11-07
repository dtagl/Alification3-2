# Примеры правильных curl запросов

## ❌ НЕПРАВИЛЬНО (двойной Bearer):
```bash
-H 'Authorization: Bearer Bearer eyJhbGc...'
```

## ✅ ПРАВИЛЬНО:
```bash
-H 'Authorization: Bearer eyJhbGc...'
```

---

## Примеры запросов:

### 1. Получить список комнат компании
```bash
curl -X 'GET' \
  'http://localhost:5024/api/rooms/company' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjOGU1ZGFlNi0zZWZjLTRlOWItODYxYS0zNGNlYjU4NGZkOWYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiZCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiY29tcGFueUlkIjoiNTQyMmMxZjUtNDYxMC00ZWE2LTg5ZmUtYmYwYzU0ODBiOWI3IiwiZXhwIjoxNzYyNTQ2MjYzLCJpc3MiOiJBbGlmaWNhdGlvbjMiLCJhdWQiOiJBbGlmaWNhdGlvbjMifQ.qETPipO0emt79DVH8B1dS6hsulrwa9xF-rjYnekRGMI'
```

**Важно:** Только одно слово `Bearer`, затем пробел, затем токен!

---

### 2. Проверка пользователя (entrypage)
```bash
curl -X 'POST' \
  'http://localhost:5024/api/first/entrypage' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "telegramId": 123456789
}'
```

---

### 3. Получить таймслоты
```bash
curl -X 'GET' \
  'http://localhost:5024/api/rooms/{roomId}/timeslots?date=2024-11-07T00:00:00Z' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer YOUR_TOKEN_HERE'
```

---

### 4. Получить информацию о бронировании
```bash
curl -X 'GET' \
  'http://localhost:5024/api/rooms/{roomId}/booking-info?time=2024-11-07T09:00:00Z' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer YOUR_TOKEN_HERE'
```

---

### 5. Забронировать комнату
```bash
curl -X 'POST' \
  'http://localhost:5024/api/rooms/{roomId}/book' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer YOUR_TOKEN_HERE' \
  -H 'Content-Type: application/json' \
  -d '{
  "startAt": "2024-11-07T09:00:00Z",
  "endAt": "2024-11-07T10:00:00Z"
}'
```

---

## 🔍 Диагностика проблем:

### Ошибка: `invalid_token`
**Причины:**
1. ❌ Двойной `Bearer` в заголовке → исправить на один `Bearer`
2. ❌ Токен истек → получить новый токен через `/entrypage` или `/login-telegram`
3. ❌ Неверный формат токена → проверить, что токен полный и без пробелов
4. ❌ Неверный ключ подписи → проверить `Jwt:Key` в `appsettings.json`

### Ошибка: `Token-Expired`
**Решение:** Получить новый токен

### Ошибка: `401 Unauthorized`
**Решение:** Проверить заголовок Authorization

---

## 💡 Советы:

1. **Всегда используйте формат:** `Bearer {token}` (одно слово Bearer)
2. **Проверяйте срок действия токена** (по умолчанию 120 минут, но может быть изменено)
3. **Если токен истек** → вызвать `/api/first/entrypage` для получения нового
4. **В Postman/Swagger** → используйте вкладку "Authorization" → "Bearer Token" (там автоматически добавится правильный формат)

