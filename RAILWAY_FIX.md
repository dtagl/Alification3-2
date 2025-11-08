# Исправление проблемы с Railway Healthcheck

## Проблема:
Healthcheck не проходил, потому что:
1. Swagger был включен только в Development режиме
2. Swagger требовал авторизацию из-за глобальной политики

## Исправления:

### 1. Включен Swagger в Production
- Swagger теперь доступен во всех окружениях
- Полезно для Railway healthcheck и документации API

### 2. Создан HealthController
- Новый endpoint `/api/health` с `[AllowAnonymous]`
- Простой endpoint для проверки работоспособности
- Не требует авторизацию

### 3. Обновлен railway.json
- Healthcheck path изменен с `/swagger/index.html` на `/api/health`
- Более надежный вариант для healthcheck

## Что нужно сделать:

1. **Закоммитьте изменения:**
   ```bash
   git add .
   git commit -m "Fix Railway healthcheck: enable Swagger in production and add health endpoint"
   git push
   ```

2. **Railway автоматически пересоберет проект**

3. **Проверьте логи Railway** после деплоя:
   - Должны быть сообщения о подключении к БД
   - Healthcheck должен пройти успешно

## Проверка:

После деплоя проверьте:
- `https://your-app.up.railway.app/api/health` - должен вернуть `{"status":"healthy"}`
- `https://your-app.up.railway.app/swagger/index.html` - должен открыться Swagger UI

## Если проблема сохраняется:

1. Проверьте переменные окружения на Railway:
   - `DATABASE_URL` должен быть подключен
   - `Telegram__BotToken` должен быть установлен
   - `Jwt__Key` должен быть установлен

2. Проверьте логи Railway на наличие ошибок:
   - Ошибки подключения к БД
   - Ошибки миграций
   - Другие исключения

3. Убедитесь, что PostgreSQL и Redis сервисы запущены и подключены к API

