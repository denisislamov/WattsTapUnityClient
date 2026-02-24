## 1. Общее описание проекта

- Клиент: Unity 6 (C#), WebGL build
- Платформа: Telegram Mini App (основная), WeChat Mini Program (планируется)
- Формат данных: JSON
- Архитектура: **server-authoritative** — сервер является источником истины для всех данных

---

## 2. Аутентификация

### Telegram Auth Flow
1. Unity-клиент получает `initData` из Telegram WebApp JavaScript bridge
2. Клиент извлекает `referralCode` из `start_param` (формат: `REF_ABC123`)
3. Клиент отправляет `POST /auth/telegram`
4. **Сервер валидирует `initData`** через HMAC-SHA256 с использованием Telegram Bot Token
5. Сервер создаёт/находит игрока, возвращает JWT
6. Все последующие запросы: `Authorization: Bearer <jwt_token>`
7. Время жизни токена: ~24 часа

### Telegram Bot
- Bot username: `wattstap_eu_bot`
- Mini App short name: `app`
- Реферальная ссылка: `https://t.me/{BotUsername}/{MiniAppShortName}?startapp=REF_{code}`

---

## 3. Формат ответов API

### Успешный ответ
Данные возвращаются напрямую (без обёртки `{ success, data }`):
```json
{
    "token": "jwt...",
    "expiresIn": 86400,
    "player": { ... }
}
```

### Ошибка (FastAPI стандарт)
```json
{
    "detail": "Error message here"
}
```

### HTTP-коды
| Код | Значение |
|-----|----------|
| 200 | Успех |
| 400 | Ошибка валидации |
| 401 | Не авторизован |
| 403 | Доступ запрещён |
| 404 | Не найдено |
| 429 | Rate limit |
| 500 | Серверная ошибка |

---

## 4. API Endpoints — РЕАЛИЗОВАННЫЕ В КЛИЕНТЕ

Эти эндпоинты **уже вызываются из Unity-клиента**.

### 4.1 Аутентификация

```
POST /auth/telegram
  Auth: Нет
  Body: {
      "initData": string,       // Telegram WebApp initData
      "referralCode": string?   // Опционально, формат "REF_ABC123"
  }
  Response: {
      "token": string,          // JWT Bearer token
      "expiresIn": int,         // Время жизни в секундах
      "player": {
          "playerId": UUID,
          "nickname": string,
          "level": int,
          "isNewPlayer": bool,
          "referralCode": string // Уникальный реферальный код игрока
      },
      "referral": {             // null если без реферала
          "applied": bool,
          "referrer": {
              "userId": long,
              "nickname": string,
              "avatarUrl": string,
              "level": int
          },
          "bonusForReferrer": int,
          "message": string
      }
  }
```

**Логика:**
- Валидировать `initData` через HMAC-SHA256
- Если игрок не существует — создать нового
- Если передан `referralCode` и игрок новый — привязать реферала, начислить бонусы
- Вернуть JWT токен

### 4.2 Прогресс игрока

```
GET /progress
  Auth: Bearer
  Response: {
      "success": bool,
      "progress": {
          "level": int,
          "watts": long,
          "currentXp": long,
          "totalXp": long
      },
      "isNewPlayer": bool
  }
```

```
POST /progress
  Auth: Bearer
  Body: {
      "level": int,
      "watts": long,
      "currentXp": long,
      "totalXp": long
  }
  Response: {
      "success": bool,
      "progress": {
          "level": int,
          "watts": long,
          "currentXp": long,
          "totalXp": long
      },
      "message": string
  }
```

**Важно:** Клиент синхронизирует прогресс каждые 5-N секунд (dirty flag — только при изменениях). Сервер должен **валидировать** входящие данные и отклонять подозрительные изменения (например, резкий скачок watts).

```
POST /progress/reset
  Auth: Bearer
  Body: { "confirm": true }
  Response: {
      "success": bool,
      "progress": { ... },
      "message": string
  }
```
*Debug endpoint — доступен только в dev-окружении.*

### 4.3 Mining Balance (публичный)

```
GET /balance/mining
  Auth: Нет
  Response: {
      "success": bool,
      "balance": {
          "coinsPerTap": float,           // 1.0
          "expPerTap": float,             // 1.0
          "energyCostPerTap": float,      // 1.0
          "startCapacityHits": int,       // 1500
          "cooldownPerHitSec": float,     // 2.0
          "critMultiplier": float,        // 1.2
          "chanceCritPercent": float,     // 1.0
          "avgPlaytimeMinutes": float,    // 15.0
          "tapsPerSecond": float,         // 10.0
          "profitPerHour": float,         // 500.0
          "maxHoursOffline": int,         // 3
          "sessionsPerDay": int,          // 2
          "dailyProgression": [           // 30 дней
              {
                  "day": int,
                  "playtimeSec": float,
                  "tapsPerSession": float,
                  "tapsPerDay": float,
                  "expPerDay": float,
                  "coinsFromTaps": float,
                  "coinsFromOfflineBonus": float,
                  "profitCoins": float,
                  "cumulativeProfitCoins": float,
                  "cumulativeExp": float
              }
          ]
      }
  }
```

**Назначение:** Клиент загружает баланс с сервера при старте. Если сервер недоступен — использует локальные значения. Это позволяет менять баланс игры без обновления клиента.

### 4.4 Социальные функции

```
GET /social/my-referral
  Auth: Bearer
  Response: {
      "referralCode": string,
      "inviteLink": string,
      "bonusPerFriend": int,         // 5000
      "totalFriendsInvited": int,
      "totalBonusEarned": long
  }
```

```
GET /social/friends
  Auth: Bearer
  Response: {
      "friends": [
          {
              "playerId": UUID,
              "nickname": string,
              "level": int,
              "avatarUrl": string,
              "totalEarnings": long,
              "yourBonus": long,
              "invitedAt": datetime
          }
      ],
      "totalFriends": int,
      "totalBonusEarned": long
  }
```

### 4.5 Аватары

```
GET /avatars
  Auth: Bearer
  Response: {
      "success": bool,
      "unlockedAvatars": [string],   // Массив avatarId
      "currentAvatar": string        // Текущий активный avatarId
  }
```

```
POST /avatars/purchase
  Auth: Bearer
  Body: {
      "avatarId": string,
      "price": long,
      "currency": string             // "watts" | "btn"
  }
  Response: {
      "success": bool,
      "avatarId": string,
      "newWattsBalance": long,
      "unlockedAvatars": [string],
      "message": string
  }
```

```
POST /avatars/unlock-by-level
  Auth: Bearer
  Body: { "avatarId": string }
  Response: {
      "success": bool,
      "avatarId": string,
      "unlockedAvatars": [string],
      "message": string
  }
```

**Данные аватаров:** 72 аватара с разными типами разблокировки:
- `Free` — доступен по умолчанию
- `Level` — разблокируется при достижении уровня
- `Coins` — покупается за Watts
- `BTN` — покупается за BTN-токены (будущее)

### 4.6 Debug (только dev)

```
POST /dev/add-resources
  Auth: Bearer
  Body: {
      "watts": long,
      "xp": long
  }
  Response: {
      "success": bool,
      "progress": { ... },
      "addedWatts": long,
      "addedXp": long,
      "message": string
  }
```
