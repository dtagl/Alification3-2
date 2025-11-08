# Railway Environment Variables

Скопируйте эти переменные окружения в настройках вашего Railway проекта.

## Обязательные переменные:

```env
# Environment
ASPNETCORE_ENVIRONMENT=Production
DOTNET_RUNNING_IN_CONTAINER=true

# Database (Railway автоматически предоставит DATABASE_URL, но можно переопределить)
# ConnectionStrings__Default будет автоматически создан из DATABASE_URL
# Если нужно переопределить, используйте формат:
# ConnectionStrings__Default=Host=hostname;Port=5432;Database=dbname;Username=user;Password=pass;SSL Mode=Require

# Redis (Railway автоматически предоставит REDIS_URL)
# ConnectionStrings__Redis будет автоматически создан из REDIS_URL
# Или укажите вручную:
# ConnectionStrings__Redis=hostname:6379

# Telegram Bot Token
Telegram__BotToken=8394190016:AAFcwClcmcVDs7d6xIPKdP_BJyyj-66uiyc

# JWT Configuration
Jwt__Issuer=Alification3
Jwt__Audience=Alification3
Jwt__Key=wA3X9p5Z6eN4dG7jKlP9xQ1a8vH2b3zR5yY6tU9oM0cP1nD2hF3sG4rH5tJ6kL7=
Jwt__ExpiryMinutes=120
```

## Как добавить в Railway:

1. Откройте ваш проект на Railway
2. Выберите сервис API
3. Перейдите в "Variables"
4. Нажмите "+ New Variable"
5. Добавьте каждую переменную

## Автоматические переменные Railway:

Railway автоматически создаст:
- `DATABASE_URL` - для PostgreSQL (формат: `postgresql://user:pass@host:port/db`)
- `REDIS_URL` - для Redis (формат: `redis://host:port`)
- `PORT` - порт для приложения

Приложение автоматически использует эти переменные, если они доступны.

## Важно:

- **Двойное подчеркивание** (`__`) используется для вложенных настроек в .NET
- Railway автоматически конвертирует `DATABASE_URL` в формат Npgsql
- `REDIS_URL` используется напрямую
- `PORT` используется автоматически

## Проверка:

После деплоя проверьте логи Railway - должны быть сообщения о подключении к базе данных и Redis.

