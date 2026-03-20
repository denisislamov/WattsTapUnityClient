# WattsTap API Migration Guide

## 📋 Обзор

Документ описывает различия между **текущим сервером** и **новым API-сервером**, а также шаги для миграции Unity-клиента.

**Все данные ниже верифицированы реальными запросами к новому серверу (2026-03-19).**

| | Текущий сервер (OLD) | Новый сервер (NEW) |
|---|---|---|
| **URL** | `https://wattstap-referral-service.onrender.com` | `https://api-dev.wattstap.energy` |
| **Стек** | Python / FastAPI / PostgreSQL | Новый бэкенд (Cloudflare-protected) |
| **Конфиг Unity** | `ReferralConfig.asset → BaseUrl` | Тот же `ReferralConfig.asset → BaseUrl` |
| **Сервис Unity** | `ReferralAPIService.cs` | Тот же (потребует доработки) |

---

## 1. Сопоставление эндпоинтов

### 1.1 Эндпоинты, которые ЕСТЬ в обоих серверах

| # | Метод | Path | Статус | Изменения |
|---|-------|------|--------|-----------|
| 1 | `POST` | `/auth/telegram` | ⚠️ Различия | `expiresIn: 900` вместо 86400. `playerId` = UUID. См. 2.1 |
| 2 | `GET` | `/progress` | ⚠️ Различия | Добавлены `playerState`, `nextRegenAt`, `boosters`. См. 2.2 |
| 3 | `POST` | `/progress` | 🔴 **Критическое** | `{ tapCount }` вместо `{ level, watts, xp, totalXp }`. См. 2.2 |
| 4 | `POST` | `/progress/reset` | ✅ Совпадает | Body: `{ "confirm": true }` → `{ success, progress, message: "Progress reset" }` |
| 5 | `GET` | `/balance/mining` | ✅ Совпадает | Формат идентичен. `dailyProgression: []` (пуст на dev) |
| 6 | `GET` | `/social/my-referral` | ⚠️ Различия | Добавлены `pendingBonus`, `availableToClaim`. См. 2.4 |
| 7 | `GET` | `/social/friends` | ⚠️ Различия | Добавлено `totalBonusPending`. См. 2.4 |
| 8 | `GET` | `/avatars` | 🔴 **Критическое** | Полный каталог вместо только ID. См. 2.3 |
| 9 | `POST` | `/avatars/purchase` | ⚠️ Различия | Убрано поле `price`. См. 2.3 |

### 1.2 Эндпоинты, которые есть ТОЛЬКО в OLD сервере

| # | Метод | Path | Описание | Статус миграции |
|---|-------|------|----------|-----------------|
| 1 | `POST` | `/avatars/unlock-by-level` | Разблокировка аватара по уровню | 🔄 Заменён на `POST /avatars/{id}/claim` |
| 2 | `POST` | `/dev/add-resources` | Debug: добавить watts/xp | ❌ Убран |
| 3 | `DELETE` | `/dev/reset-all` | Debug: удалить всех пользователей | ❌ Убран |
| 4 | `DELETE` | `/dev/reset-user/{telegram_id}` | Debug: удалить пользователя | ❌ Убран |
| 5 | `PUT` | `/balance/mining/admin` | Admin: обновить баланс | ❌ Убран |
| 6 | `POST` | `/balance/mining/admin/seed` | Admin: seed из CSV | ❌ Убран |

### 1.3 Эндпоинты, которые есть ТОЛЬКО в NEW сервере

| # | Метод | Path | Описание | Протестировано | Приоритет |
|---|-------|------|----------|----------------|-----------|
| 1 | `POST` | `/auth/refresh` | Обновление JWT через refresh token | ✅ HTTP 400 `"Missing refresh token"` (ожидаемо — refresh приходит в cookie) | 🔴 Высокий |
| 2 | `POST` | `/auth/logout` | Логаут | ✅ HTTP 200 `"Logged out successfully"` | 🔴 Высокий |
| 3 | `POST` | `/avatars/{id}/claim` | Получение бесплатного аватара | ✅ HTTP 200 `{ avatarId, currentAvatarId }` | 🟡 Средний |
| 4 | `GET` | `/orders/{id}` | Получить заказ по ID | ✅ HTTP 404 `"Order not found"` (ожидаемо) | 🟡 Средний |
| 5 | `GET` | `/game/items/catalog` | Каталог предметов | ✅ HTTP 200 (186KB JSON — items с variants, levels, bonuses) | 🟡 Средний |
| 6 | `GET` | `/game/inventory` | Инвентарь игрока | ✅ HTTP 200 `{ inventory, equipment, equippedStats, currencies }` | 🟡 Средний |
| 7 | `POST` | `/game/inventory/equip` | Экипировать предмет | ✅ HTTP 404 `"Inventory item not found"` (ожидаемо — нет предметов) | 🟡 Средний |
| 8 | `POST` | `/game/inventory/unequip` | Снять предмет | ✅ HTTP 200 `{ equipment: { WEAPON: null, ... } }` | 🟡 Средний |
| 9 | `POST` | `/game/inventory/upgrade` | Улучшить предмет | ✅ HTTP 404 `"Inventory item not found"` (ожидаемо) | 🟡 Средний |
| 10 | `GET` | `/wallet/my-wallet` | Получить кошелёк | ✅ HTTP 200 `{ walletAddress, jettonAddress, jettonBalance, wallets }` | 🟢 Низкий |
| 11 | `GET` | `/wallet/payload` | Сгенерировать payload | ✅ HTTP 200 `{ payload: "8a17bda1..." }` | 🟢 Низкий |
| 12 | `POST` | `/wallet/validate` | Валидация кошелька | ✅ HTTP 400 `"Invalid wallet proof"` (ожидаемо — тестовые данные) | 🟢 Низкий |
| 13 | `GET` | `/user/me` | Профиль пользователя + события | ✅ HTTP 200 `{ user, events[], unreadEventsCount }` | 🟡 Средний |
| 14 | `POST` | `/user/language` | Обновить язык пользователя | ✅ HTTP 200 `{ user }` (с currentAvatarId) | 🟢 Низкий |
| 15 | `POST` | `/user/events/read` | Отметить события прочитанными | ✅ HTTP 200 `{ success, updatedCount, unreadEventsCount }` | 🟢 Низкий |
| 16 | `POST` | `/social/bonus/claim` | Получить реферальные бонусы | ✅ HTTP 200 `{ success, claimedAmount, ... }` | 🟡 Средний |
| 17 | `POST` | `/referral/apply` | Применить реферальный код вручную | ✅ HTTP 200 `{ invitedBy, userReferralCode, activeStatus }` | 🟡 Средний |

---

## 2. Детальное сравнение запросов и ответов (верифицировано)

### 2.1 Аутентификация — `POST /auth/telegram`

#### Request — ✅ Совместим

Формат запроса **идентичен**:
```json
{
  "initData": "query_id=AAH...&user=...",
  "referralCode": "ABC123"
}
```

#### Response — ⚠️ Есть различия

**OLD Response:**
```json
{
  "token": "eyJ...",
  "expiresIn": 86400,
  "player": {
    "playerId": "42",
    "nickname": "John Doe",
    "level": 1,
    "isNewPlayer": true,
    "referralCode": "ABC123"
  },
  "referral": { "applied": true, "referrer": {...}, "bonusForReferrer": 5000, "message": "..." }
}
```

**NEW Response (реальный):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 900,
  "player": {
    "playerId": "cee9a4b4-8ed7-42f0-bcdc-b70f9abb8d1a",
    "nickname": "disadisa",
    "level": 1,
    "isNewPlayer": false,
    "referralCode": "MI6M-IIUP"
  },
  "referral": null
}
```

| Поле | OLD | NEW (реальное) | Влияние на клиент |
|------|-----|----------------|-------------------|
| `token` | JWT string | JWT string | ✅ Совместим |
| `expiresIn` | `86400` (24ч) | `900` (15 мин!) | 🔴 **Критическое** — нужен refresh-token механизм |
| `player.playerId` | `"42"` (числовой ID) | `"cee9a4b4-..."` (UUID) | ✅ Тип `string` — совместим |
| `player.nickname` | display_name | username | ✅ Совместим |
| `player.level` | int | int | ✅ Совместим |
| `player.isNewPlayer` | bool | bool | ✅ Совместим |
| `player.referralCode` | `"ABC123"` | `"MI6M-IIUP"` (с дефисом) | ✅ Тип `string` — совместим |
| `referral` | ReferralResult / null | ReferralResult / null | ✅ Совместим |

> 🔴 **КРИТИЧЕСКИ**: `expiresIn: 900` (15 минут) вместо 86400 (24 часа). Клиент **обязательно** должен поддерживать `/auth/refresh` для обновления токена, иначе пользователь будет разлогинен каждые 15 минут.

> ✅ **C# DTO `AuthResponse`**: Полностью совместим. Все поля совпадают. `JsonUtility.FromJson<AuthResponse>()` работает без изменений.

---

### 2.2 Прогресс — 🔴 КРИТИЧЕСКОЕ ИЗМЕНЕНИЕ

#### `GET /progress` — Response расширен, но обратно совместим

**OLD Response:**
```json
{
  "success": true,
  "progress": { "level": 5, "watts": 15000, "currentXp": 4500, "totalXp": 25000 },
  "isNewPlayer": false
}
```

**NEW Response (реальный):**
```json
{
  "success": true,
  "progress": {
    "level": 1,
    "watts": 8,
    "currentXp": 8,
    "totalXp": 8
  },
  "isNewPlayer": false,
  "playerState": {
    "id": "019d0635-184f-7563-8af2-202bddc0f2b8",
    "userId": "cee9a4b4-8ed7-42f0-bcdc-b70f9abb8d1a",
    "gameConfigId": "019d055a-187d-7cc3-8aab-00a436d6bdb2",
    "level": 1,
    "experience": 8,
    "coins": 8,
    "energy": 1500,
    "energyUpdatedAt": "2026-03-19T13:54:26.174Z",
    "createdAt": "2026-03-19T13:07:16.175Z",
    "updatedAt": "2026-03-19T13:43:31.662Z"
  },
  "nextRegenAt": null,
  "boosters": {
    "active": [],
    "tapMode": "MULTIPLY_TAPS_AND_COST"
  }
}
```

| Поле | OLD | NEW | Влияние |
|------|-----|-----|---------|
| `success` | ✅ | ✅ | Без изменений |
| `progress` | `{ level, watts, currentXp, totalXp }` | **Тот же формат** | ✅ `PlayerProgressDTO` **совместим** |
| `isNewPlayer` | ✅ | ✅ | Без изменений |
| `playerState` | ❌ Нет | ✅ Новое | JsonUtility проигнорирует |
| `nextRegenAt` | ❌ Нет | ✅ Новое | JsonUtility проигнорирует |
| `boosters` | ❌ Нет | ✅ Новое | JsonUtility проигнорирует |

> ✅ **`LoadProgressResponse` DTO**: **Совместим без изменений.** `JsonUtility` проигнорирует новые поля.
>
> 🟡 **Рекомендация**: Добавить `PlayerStateDTO`, `BoostersDTO` позже для использования energy/boosters в UI.

#### `POST /progress` — 🔴 Полностью изменён

**OLD Request:** `{ "level": 5, "watts": 15000, "currentXp": 4500, "totalXp": 25000 }`

**NEW Request:** `{ "tapCount": 2 }`

**NEW Response (реальный):**
```json
{
  "success": true,
  "message": "Progress updated",
  "progress": {
    "level": 1,
    "watts": 10,
    "currentXp": 10,
    "totalXp": 10
  },
  "isNewPlayer": false,
  "playerState": {
    "id": "019d0635-...",
    "userId": "cee9a4b4-...",
    "level": 1,
    "experience": 10,
    "coins": 10,
    "energy": 1498,
    "energyUpdatedAt": "2026-03-19T13:54:28.174Z",
    "createdAt": "2026-03-19T13:07:16.175Z",
    "updatedAt": "2026-03-19T13:54:29.937Z"
  },
  "nextRegenAt": "2026-03-19T13:54:30.174Z",
  "boosters": {
    "active": [],
    "tapMode": "MULTIPLY_TAPS_AND_COST",
    "tapConversion": {
      "validatedTapCount": 2,
      "effectiveTapCount": 2,
      "energyTapCostCount": 2,
      "baseAppliedCount": 2,
      "appliedMultipliers": []
    }
  },
  "tap": {
    "energyBefore": 1500,
    "energyAfter": 1498,
    "xpEarned": 2,
    "coinsEarned": 2,
    "appliedTapCount": 2,
    "tapMeta": {
      "requestedTapCount": 2,
      "appliedTapCount": 2,
      "droppedTapCount": 0
    }
  }
}
```

| Аспект | Детали |
|--------|--------|
| **Request** | 🔴 Полностью изменён: `{ tapCount: N }` вместо `{ level, watts, currentXp, totalXp }` |
| **Response `progress`** | ✅ `{ level, watts, currentXp, totalXp }` — **совместим** с `PlayerProgressDTO` |
| **Response `success`, `message`** | ✅ Совместим с `SaveProgressResponse` |
| **Новое: `tap`** | Детализация: energyBefore/After, xpEarned, coinsEarned, tapMeta |
| **Новое: `tap.tapMeta.droppedTapCount`** | ⚠️ Если > 0 — тапы были отброшены (нет energy) |
| **Новое: `boosters.tapConversion`** | Информация о валидации тапов, мультипликаторы |
| **Новое: `playerState`** | Полный стейт (energy, coins, experience...) |

> 🔴 **Действие**:
> 1. **Заменить `SaveProgressRequest`** → `TapProgressRequest { tapCount }`
> 2. **`SaveProgressResponse`** — базово **совместим**: поля `success`, `progress`, `message` есть
> 3. Добавить `TapResultDTO` для обработки `tap.droppedTapCount`

---

### 2.3 Аватары — 🔴 Расширен, но обратно совместим

#### `GET /avatars`

**OLD Response:**
```json
{
  "success": true,
  "unlockedAvatars": ["avatar_01", "avatar_02"],
  "currentAvatar": "avatar_01"
}
```

**NEW Response (реальный):**
```json
{
  "success": true,
  "unlockedAvatars": [
    "019d055a-2a7c-7120-8da4-b33ed0263a18",
    "019d055a-2a81-7ca1-95fa-f05476567952"
  ],
  "currentAvatar": "019d055a-2a7c-7120-8da4-b33ed0263a18",
  "avatars": [
    {
      "id": "019d055a-2a7c-7120-8da4-b33ed0263a18",
      "code": "default",
      "title": "Default",
      "imageUrl": "https://cdn.wattstap.com/avatars/default.png",
      "previewUrl": "https://cdn.wattstap.com/avatars/previews/default.png",
      "isActive": true,
      "unlockType": "FREE",
      "unlockLevel": null,
      "price": "0",
      "isOwned": true,
      "isCurrent": true,
      "canPurchase": false
    },
    {
      "id": "019d055a-2a85-7d10-9520-360c0d37838b",
      "code": "paid_neon",
      "title": "Neon",
      "imageUrl": "https://cdn.wattstap.com/avatars/paid-neon.png",
      "previewUrl": "https://cdn.wattstap.com/avatars/previews/paid-neon.png",
      "isActive": true,
      "unlockType": "COINS",
      "unlockLevel": null,
      "price": "10",
      "isOwned": false,
      "isCurrent": false,
      "canPurchase": true
    }
  ]
}
```

> ✅ **`GetAvatarsResponse` DTO**: Базово **совместим** — `success`, `unlockedAvatars`, `currentAvatar` совпадают. `avatars[]` будет проигнорирован `JsonUtility`.
>
> 🟡 **Рекомендация**: Добавить `AvatarCatalogItem[] avatars` в DTO для полноценного UI каталога.

> ⚠️ **Замечание**: `price` — тип `string` (не int): `"0"`, `"10"`. Нужно `int.Parse()` при использовании.

#### `POST /avatars/purchase` — убрано `price`

**OLD Request:** `{ "avatarId": "...", "price": 1000, "currency": "watts" }`
**NEW Request:** `{ "avatarId": "...", "currency": "watts" }` — сервер знает цену сам из каталога.

---

### 2.4 Социальные функции — ⚠️ Расширены, обратно совместимы

#### `GET /social/my-referral` (реальный ответ)

```json
{
  "referralCode": "MI6M-IIUP",
  "inviteLink": "https://t.me/WattsTapDevTemp_bot/app?startapp=REF_MI6M-IIUP",
  "bonusPerFriend": 300,
  "totalFriendsInvited": 0,
  "totalBonusEarned": 0,
  "pendingBonus": 0,
  "availableToClaim": false
}
```

> ✅ `MyReferralResponse` совместим. Новые поля `pendingBonus`, `availableToClaim` игнорируются. Рекомендуется добавить.

#### `GET /social/friends` (реальный ответ)

```json
{
  "friends": [],
  "totalFriends": 0,
  "totalBonusEarned": 0,
  "totalBonusPending": 0
}
```

> ✅ `FriendsListResponse` совместим. Новое поле `totalBonusPending` игнорируется.

#### НОВОЕ: `POST /social/bonus/claim` (реальный ответ)

**Request:** `{}` (пустое тело)

```json
{
  "success": true,
  "claimedAmount": 0,
  "claimedFriendsCount": 0,
  "newWattsBalance": 10,
  "totalBonusEarned": 0,
  "totalBonusPending": 0
}
```

---

### 2.5 Mining Balance — `GET /balance/mining` — ✅ Полностью совместим

**NEW Response (реальный):**
```json
{
  "success": true,
  "balance": {
    "coinsPerTap": 1,
    "expPerTap": 1,
    "energyCostPerTap": 1,
    "startCapacityHits": 1500,
    "cooldownPerHitSec": 2,
    "critMultiplier": 1.2,
    "chanceCritPercent": 1,
    "avgPlaytimeMinutes": 15,
    "tapsPerSecond": 10,
    "profitPerHour": 500,
    "maxHoursOffline": 3,
    "sessionsPerDay": 2,
    "dailyProgression": []
  }
}
```

> ✅ **Идентичен** `MiningBalancePublicResponse` / `MiningBalanceDTO`. Никаких изменений не требуется.

---

### 2.6 Новые endpoints — Реальные схемы

#### `GET /user/me` (реальный)

```json
{
  "user": {
    "id": "cee9a4b4-8ed7-42f0-bcdc-b70f9abb8d1a",
    "username": "disadisa",
    "firstName": "Denis",
    "lastName": "Islamov",
    "telegramId": "184586563",
    "photoUrl": "https://t.me/i/userpic/320/...",
    "languageCode": "en",
    "createdAt": "2026-03-19T13:07:16.175Z"
  },
  "events": [],
  "unreadEventsCount": 0
}
```

#### `POST /user/language` (реальный)

**Request:** `{ "newLangCode": "en" }`

**Response:**
```json
{
  "user": {
    "id": "cee9a4b4-...",
    "telegramId": "184586563",
    "firstName": "Denis",
    "lastName": "Islamov",
    "username": "disadisa",
    "photoUrl": "https://t.me/i/userpic/320/...",
    "languageCode": "en",
    "currentAvatarId": "019d055a-2a7c-7120-8da4-b33ed0263a18",
    "createdAt": "2026-03-19T13:07:16.175Z"
  }
}
```

> ⚠️ **Замечание:** `/user/me` возвращает `{ user, events, unreadEventsCount }`, а `/user/language` возвращает `{ user }` (с доп. полем `currentAvatarId`). Разные view одного объекта.

#### `POST /user/events/read` (реальный)

**Request:** `{ "readAll": true }`

**Response:**
```json
{
  "success": true,
  "updatedCount": 0,
  "unreadEventsCount": 0
}
```

#### `GET /wallet/my-wallet` (реальный)

```json
{
  "walletAddress": "",
  "jettonAddress": "EQCKk1x74PGx7TDS2Lr5JN8RXum-IFkYmuvy9EFOSgu4yg66",
  "jettonBalance": 0,
  "wallets": []
}
```

#### `GET /game/inventory` (реальный)

```json
{
  "inventory": [],
  "equipment": {
    "WEAPON": null,
    "ARMS": null,
    "BODY": null,
    "FEET": null
  },
  "equippedStats": {
    "COINS_PER_TAP":           { "value": 0, "valuePercent": 0 },
    "XP_PER_TAP":              { "value": 0, "valuePercent": 0 },
    "CAPACITY_HITS":           { "value": 0, "valuePercent": 0 },
    "RECOVER_HITS_PER_SECOND": { "value": 0, "valuePercent": 0 },
    "CRIT_CHANCE":             { "value": 0, "valuePercent": 0 },
    "CRIT_MULTIPLIER":         { "value": 0, "valuePercent": 0 },
    "OFFLINE_BONUS_PERCENT":   { "value": 0, "valuePercent": 0 }
  },
  "currencies": {
    "coins": 10,
    "drawings": 0
  }
}
```

---

### 2.7 Auth — Refresh, Logout (NEW, верифицировано)

#### `POST /auth/refresh`

**Request:** `{}` (пустое тело, refresh token передаётся через cookie)

**Response (HTTP 400 — ожидаемо, т.к. нет cookie):**
```json
{
  "detail": "Missing refresh token"
}
```

> ⚠️ **Важно**: Refresh token приходит в **HTTP-only cookie** при авторизации через `/auth/telegram`. Python urllib не сохраняет cookies, поэтому получили 400. В Unity нужно реализовать поддержку cookies или передавать refresh token иным способом (уточнить с бэкендом).

#### `POST /auth/logout`

**Request:** `{}` (пустое тело, с Bearer token)

**Response (HTTP 200):**
```json
{
  "message": "Logged out successfully"
}
```

---

### 2.8 Avatars — Claim, Purchase (NEW, верифицировано)

#### `POST /avatars/{id}/claim`

**Response (HTTP 200):**
```json
{
  "avatarId": "019d055a-2a7c-7120-8da4-b33ed0263a18",
  "currentAvatarId": "019d055a-2a7c-7120-8da4-b33ed0263a18"
}
```

> ⚠️ **Заметка**: Ответ **НЕ содержит** `success`, `unlockedAvatars[]`, `message` как было в OLD `UnlockAvatarByLevelResponse`. Нужен новый DTO `ClaimAvatarResponse { avatarId, currentAvatarId }`.

#### `POST /avatars/purchase` — ошибка (HTTP 409)

**Response (недостаточно средств):**
```json
{
  "detail": "Not enough watts balance"
}
```

> ✅ Ошибки возвращаются в стандартном формате `{ "detail": "..." }`. `ApiErrorResponse` совместим.

---

### 2.9 Referral Apply (NEW, верифицировано)

#### `POST /referral/apply`

**Request:**
```json
{
  "referralCode": "TEST-CODE",
  "source": "miniapp"
}
```

**Response (HTTP 200):**
```json
{
  "invitedBy": "",
  "userReferralCode": "",
  "activeStatus": false
}
```

> ⚠️ **Формат неожиданный**: Не `{ success, message }`, а `{ invitedBy, userReferralCode, activeStatus }`. Нужен новый DTO.

---

### 2.10 Items Catalog (NEW, верифицировано)

#### `GET /game/items/catalog`

**Response (HTTP 200, 186KB JSON):**
```json
{
  "items": [
    {
      "id": "019d055a-1a7d-...",
      "code": "weapon_amp_thumper",
      "name": "Amp-Thumper",
      "description": "Each swing generates enough electricity...",
      "slot": "WEAPON",
      "variants": [
        {
          "id": "019d055a-1a81-...",
          "rarity": "COMMON",
          "maxLevel": 5,
          "mainStatType": "CAPACITY_HITS",
          "levels": [
            { "level": 1, "value": 10, "valuePercent": null },
            { "level": 2, "value": 20, "valuePercent": null },
            ...
          ],
          "bonuses": []
        },
        {
          "id": "...",
          "rarity": "UNCOMMON",
          "maxLevel": 10,
          "mainStatType": "CAPACITY_HITS",
          "levels": [...],
          "bonuses": [
            {
              "statType": "COINS_PER_TAP",
              "value": null,
              "valuePercent": 1,
              "description": "Profit per tap 1 %"
            }
          ]
        }
      ]
    }
  ]
}
```

**Слоты:** `WEAPON`, `ARMS`, `BODY`, `FEET`
**Редкости:** `COMMON` (maxLevel=5), `UNCOMMON` (maxLevel=10), `RARE` (maxLevel=15), `LEGENDARY` (maxLevel=20)
**Типы статов:** `CAPACITY_HITS`, `COINS_PER_TAP`, `XP_PER_TAP`, `RECOVER_HITS_PER_SECOND`, `CRIT_CHANCE`, `CRIT_MULTIPLIER`, `OFFLINE_BONUS_PERCENT`

> ⚠️ **186KB JSON** — рекомендуется кешировать каталог на клиенте и обновлять редко.

---

### 2.11 Inventory — Equip, Unequip, Upgrade (NEW, верифицировано)

#### `POST /game/inventory/equip`

**Request:** `{ "playerItemId": "..." }`

**Response (HTTP 404 — нет предметов):**
```json
{
  "detail": "Inventory item not found"
}
```

#### `POST /game/inventory/unequip`

**Request:** `{ "slot": "WEAPON" }`

**Response (HTTP 200):**
```json
{
  "equipment": {
    "WEAPON": null,
    "ARMS": null,
    "BODY": null,
    "FEET": null
  }
}
```

> ⚠️ **Формат ответа**: Возвращает только `{ equipment }`, а не полный inventory. Нужен DTO `UnequipResponse { equipment }`.

#### `POST /game/inventory/upgrade`

**Request:** `{ "playerItemId": "..." }`

**Response (HTTP 404 — нет предметов):**
```json
{
  "detail": "Inventory item not found"
}
```

---

### 2.12 Wallet — Payload, Validate (NEW, верифицировано)

#### `GET /wallet/payload`

**Response (HTTP 200):**
```json
{
  "payload": "8a17bda113267a827413a4dc909caa10ab77c24a85179c2289756929070d6931"
}
```

> ✅ Простой формат. Нужен DTO `WalletPayloadResponse { payload }`.

#### `POST /wallet/validate`

**Request:**
```json
{
  "address": "UQTest",
  "network": "-239",
  "public_key": "test",
  "proof": {}
}
```

**Response (HTTP 400 — невалидные данные):**
```json
{
  "detail": "Invalid wallet proof"
}
```

---

### 2.13 Orders (NEW, верифицировано)

#### `GET /orders/{id}`

**Response (HTTP 404):**
```json
{
  "detail": "Order not found"
}
```

> Формат ошибки стандартный. Формат успешного ответа не протестирован (нет реальных заказов).

---

## 3. Архитектурные изменения

### 3.1 Server-Authoritative Architecture

| Аспект | OLD | NEW |
|--------|-----|-----|
| Расчёт прогресса | Клиент считает, сервер сохраняет | Сервер считает, клиент отображает |
| Тапы | Клиент: +watts, +xp → sync full state | Клиент: `{ tapCount }` → сервер возвращает new state |
| Цены аватаров | Клиент указывает `price` | Сервер знает цены (`canPurchase` в каталоге) |
| Energy | Нет на сервере | `playerState.energy` — отслеживается сервером, тапы расходуют энергию |
| Inventory | Нет | Полная система: equip/unequip/upgrade + суммарные stats |
| Wallet | Нет | TON wallet: address, jetton balance |

### 3.2 Система токенов 🔴

| Аспект | OLD | NEW |
|--------|-----|-----|
| Auth token | JWT, `expiresIn: 86400` (24ч) | JWT, `expiresIn: 900` **(15 мин!)** |
| Refresh | Нет | `POST /auth/refresh` (обязателен) |
| Logout | Нет | `POST /auth/logout` |

> 🔴 **КРИТИЧЕСКОЕ**: Токен живёт 15 минут. Без refresh-механизма игра перестанет работать через 15 минут после логина.

### 3.3 Формат ответов

| Аспект | OLD | NEW |
|--------|-----|-----|
| Успешный ответ | Данные напрямую (без обёртки) | Данные напрямую (без обёртки) ✅ |
| Ошибка | FastAPI: `{ "detail": "..." }` | `{ "detail": "..." }` ✅ |
| Cloudflare | Нет | ✅ Защита (требует User-Agent заголовок) |

> ⚠️ **Cloudflare**: Запросы без User-Agent блокируются с error 1010. Unity `UnityWebRequest` отправляет UA по умолчанию — проблем быть не должно.

---

## 4. Необходимые изменения в Unity-клиенте

### 4.1 Файлы, требующие изменений

| # | Файл | Тип изменения | Приоритет |
|---|------|---------------|-----------|
| 1 | `ReferralConfig.asset` | Сменить `BaseUrl` на `https://api-dev.wattstap.energy` | 🔴 |
| 2 | `ProgressSyncService.cs` | **Переработка**: `SetProgressData(level,watts,xp,totalXp)` → `AddTaps(tapCount)` | 🔴 |
| 3 | `ProgressDTOs.cs` | Новый `TapProgressRequest { tapCount }`. Response DTOs **совместимы**. | 🔴 |
| 4 | `ReferralAPIService.cs` | Обновить `SaveProgress` → `SendTaps`, добавить RefreshToken, новые методы | 🔴 |
| 5 | `IReferralAPIService.cs` | Добавить `SendTaps`, `RefreshToken`, `Logout` в интерфейс | 🔴 |
| 6 | `AvatarDTOs.cs` | Убрать `price` из `PurchaseAvatarRequest`, добавить `AvatarCatalogItem` | 🟡 |
| 7 | `ReferralDTOs.cs` | Добавить `pendingBonus`, `availableToClaim`; `ClaimBonusResponse` | 🟡 |
| 8 | `MiningBalanceRemoteLoader.cs` | Обновить hardcoded URL в логах (строка 115) | 🟢 |
| 9 | *НОВЫЙ* `WalletDTOs.cs` | DTO для wallet endpoints | 🟡 |
| 10 | *НОВЫЙ* `InventoryDTOs.cs` | DTO для inventory endpoints | 🟡 |
| 11 | *НОВЫЙ* `UserDTOs.cs` | DTO для user endpoints | 🟡 |

### 4.2 Подробные изменения по файлам

#### `ProgressDTOs.cs`

```csharp
// НОВЫЙ: запрос тапов (заменяет SaveProgressRequest для нового сервера)
[Serializable]
public class TapProgressRequest
{
    public int tapCount;
}

// SaveProgressResponse — СОВМЕСТИМ (поля success, progress, message совпадают)
// LoadProgressResponse — СОВМЕСТИМ (поля success, progress, isNewPlayer совпадают)
// PlayerProgressDTO   — СОВМЕСТИМ (поля level, watts, currentXp, totalXp совпадают)

// ОПЦИОНАЛЬНО: расширенный стейт игрока от нового сервера
[Serializable]
public class PlayerStateDTO
{
    public string id;
    public string userId;
    public string gameConfigId;
    public int level;
    public long experience;
    public long coins;
    public int energy;
    public string energyUpdatedAt;
    public string createdAt;
    public string updatedAt;
}

// ОПЦИОНАЛЬНО: результат тапа
[Serializable]
public class TapResultDTO
{
    public int energyBefore;
    public int energyAfter;
    public int xpEarned;
    public int coinsEarned;
    public int appliedTapCount;
}

// ОПЦИОНАЛЬНО: мета-данные тапа
[Serializable]
public class TapMetaDTO
{
    public int requestedTapCount;
    public int appliedTapCount;
    public int droppedTapCount;   // > 0 = тапы отброшены (нет energy!)
}
```

#### `AvatarDTOs.cs`

```csharp
// ИЗМЕНИТЬ: убрать price
[Serializable]
public class PurchaseAvatarRequest
{
    public string avatarId;
    public string currency;  // "watts" | "btn"
    // price — УБРАТЬ, сервер знает цену
}

// НОВЫЙ: элемент каталога аватаров
[Serializable]
public class AvatarCatalogItem
{
    public string id;
    public string code;
    public string title;
    public string imageUrl;
    public string previewUrl;
    public bool isActive;
    public string unlockType;    // "FREE", "COINS", "LEVEL"
    public int unlockLevel;
    public string price;         // ⚠️ string! "0", "10" — не int
    public bool isOwned;
    public bool isCurrent;
    public bool canPurchase;
}

// РАСШИРИТЬ: добавить каталог
[Serializable]
public class GetAvatarsResponse
{
    public bool success;
    public string[] unlockedAvatars;
    public string currentAvatar;
    public AvatarCatalogItem[] avatars;  // НОВОЕ
}
```

#### `ReferralDTOs.cs`

```csharp
// РАСШИРИТЬ MyReferralResponse
[Serializable]
public class MyReferralResponse
{
    public string referralCode;
    public string inviteLink;
    public int bonusPerFriend;
    public int totalFriendsInvited;
    public long totalBonusEarned;
    public long pendingBonus;          // НОВОЕ
    public bool availableToClaim;      // НОВОЕ
}

// РАСШИРИТЬ FriendsListResponse
[Serializable]
public class FriendsListResponse
{
    public List<FriendInfo> friends;
    public int totalFriends;
    public long totalBonusEarned;
    public long totalBonusPending;     // НОВОЕ
}

// НОВЫЙ: ответ на claim бонуса
[Serializable]
public class ClaimBonusResponse
{
    public bool success;
    public long claimedAmount;
    public int claimedFriendsCount;
    public long newWattsBalance;
    public long totalBonusEarned;
    public long totalBonusPending;
}

// НОВЫЙ: запрос /referral/apply
[Serializable]
public class ReferralApplyRequest
{
    public string referralCode;
    public string source;  // "web" | "miniapp" | "system"
}

// НОВЫЙ: ответ /referral/apply
[Serializable]
public class ReferralApplyResponse
{
    public string invitedBy;          // "" если не применён
    public string userReferralCode;   // "" если не применён
    public bool activeStatus;
}
```

#### `AvatarDTOs.cs` — Дополнения

```csharp
// НОВЫЙ: ответ /avatars/{id}/claim (отличается от OLD UnlockAvatarByLevelResponse!)
[Serializable]
public class ClaimAvatarResponse
{
    public string avatarId;
    public string currentAvatarId;
    // ⚠️ НЕТ полей: success, unlockedAvatars[], message (как было в OLD)
}
```

#### `AuthDTOs.cs` — Дополнения

```csharp
// НОВЫЙ: ответ /auth/logout
[Serializable]
public class LogoutResponse
{
    public string message;  // "Logged out successfully"
}
```

#### Новые DTO файлы

**`UserDTOs.cs`:**
```csharp
[Serializable]
public class UserProfile
{
    public string id;
    public string username;
    public string firstName;
    public string lastName;
    public string telegramId;
    public string photoUrl;
    public string languageCode;
    public string currentAvatarId;  // может быть null в /user/me
    public string createdAt;
}

[Serializable]
public class UserMeResponse
{
    public UserProfile user;
    public UserEvent[] events;
    public int unreadEventsCount;
}

[Serializable]
public class UserEvent
{
    public string id;
    public string type;
    public string title;
    public string message;
    public bool isRead;
    public string createdAt;
}

[Serializable]
public class UpdateLanguageRequest { public string newLangCode; }
[Serializable]
public class UpdateLanguageResponse { public UserProfile user; }

[Serializable]
public class MarkEventsReadRequest { public bool readAll; }
[Serializable]
public class MarkEventsReadResponse
{
    public bool success;
    public int updatedCount;
    public int unreadEventsCount;
}
```

**`WalletDTOs.cs`:**
```csharp
[Serializable]
public class WalletResponse
{
    public string walletAddress;    // "" если не подключён
    public string jettonAddress;    // "EQCKk1x74PGx7TDS..."
    public long jettonBalance;
    public WalletInfo[] wallets;    // [] если не подключён
}

[Serializable]
public class WalletInfo
{
    public string address;
    public string network;
    public string publicKey;
}

// НОВЫЙ: ответ на /wallet/payload
[Serializable]
public class WalletPayloadResponse
{
    public string payload;   // "8a17bda1..." hex-строка
}

// НОВЫЙ: запрос валидации кошелька
[Serializable]
public class WalletValidateRequest
{
    public string address;
    public string network;      // "-239" для mainnet
    public string public_key;   // ⚠️ snake_case!
    public string proof;        // JSON-строка
}
```

**`InventoryDTOs.cs`:**
```csharp
[Serializable]
public class InventoryResponse
{
    public PlayerItem[] inventory;
    public EquipmentSlots equipment;
    // equippedStats — сложная структура с string-ключами, 
    // JsonUtility не поддерживает Dictionary — нужен кастомный парсинг или Newtonsoft
    public CurrenciesDTO currencies;
}

[Serializable]
public class EquipmentSlots
{
    public PlayerItem WEAPON;  // null если пуст
    public PlayerItem ARMS;
    public PlayerItem BODY;
    public PlayerItem FEET;
}

// НОВЫЙ: ответ на unequip (отличается от полного inventory!)
[Serializable]
public class UnequipResponse
{
    public EquipmentSlots equipment;
}

[Serializable]
public class CurrenciesDTO
{
    public long coins;
    public int drawings;
}

[Serializable]
public class PlayerItem
{
    public string id;
    public string itemVariantId;
    public int level;
    public string slot;     // "WEAPON", "ARMS", "BODY", "FEET"
    public string rarity;   // "COMMON", "UNCOMMON", "RARE", "LEGENDARY"
    public bool isEquipped;
}

[Serializable]
public class EquipItemRequest { public string playerItemId; }
[Serializable]
public class UnequipSlotRequest { public string slot; }
[Serializable]
public class UpgradeItemRequest { public string playerItemId; }

// НОВЫЙ: каталог предметов (GET /game/items/catalog)
[Serializable]
public class ItemsCatalogResponse
{
    public CatalogItem[] items;
}

[Serializable]
public class CatalogItem
{
    public string id;
    public string code;
    public string name;
    public string description;
    public string slot;          // "WEAPON", "ARMS", "BODY", "FEET"
    public CatalogItemVariant[] variants;
}

[Serializable]
public class CatalogItemVariant
{
    public string id;
    public string rarity;        // "COMMON", "UNCOMMON", "RARE", "LEGENDARY"
    public int maxLevel;
    public string mainStatType;  // "CAPACITY_HITS", "COINS_PER_TAP", etc.
    public CatalogItemLevel[] levels;
    public CatalogItemBonus[] bonuses;
}

[Serializable]
public class CatalogItemLevel
{
    public int level;
    public float value;          // nullable → 0
    public float valuePercent;   // nullable → 0
}

[Serializable]
public class CatalogItemBonus
{
    public string statType;
    public float value;          // nullable → 0
    public float valuePercent;   // nullable → 0
    public string description;
}
```

> ⚠️ **Замечание**: Поле `equippedStats` в InventoryResponse использует string-ключи (`"COINS_PER_TAP"` и т.д.). `JsonUtility` не десериализует `Dictionary`. Варианты: (1) Newtonsoft.Json, (2) кастомный парсинг, (3) фиксированная структура с полями для каждого стата.

---

## 5. Пошаговый план миграции

### Фаза 1: Подготовка ✅ ВЫПОЛНЕНО

- [x] **1.1** Изучить точные форматы ответов нового API
- [x] **1.2** Задокументировать реальные response JSON для каждого endpoint
- [x] **1.3** Определить какие поля добавились/изменились

### Фаза 2: Обновление DTO и моделей

- [ ] **2.1** Создать `TapProgressRequest { tapCount }` (заменяет `SaveProgressRequest`)
- [ ] **2.2** Добавить `PlayerStateDTO`, `TapResultDTO`, `TapMetaDTO` (опционально)
- [ ] **2.3** Обновить `PurchaseAvatarRequest` — убрать `price`
- [ ] **2.4** Добавить `AvatarCatalogItem` и расширить `GetAvatarsResponse`
- [ ] **2.5** Добавить `pendingBonus`, `availableToClaim` в `MyReferralResponse`
- [ ] **2.6** Добавить `totalBonusPending` в `FriendsListResponse`
- [ ] **2.7** Создать `ClaimBonusResponse`, `UserDTOs.cs`, `WalletDTOs.cs`, `InventoryDTOs.cs`

### Фаза 3: Обновление API сервиса

- [ ] **3.1** Обновить `SaveProgress` → `SendTaps(int tapCount, ...)`
- [ ] **3.2** Добавить token refresh механизм (перехватчик 401 → `/auth/refresh` → retry)
- [ ] **3.3** Добавить таймер auto-refresh за N сек до истечения (expiresIn: 900)
- [ ] **3.4** Добавить новые методы: `RefreshToken`, `Logout`, `ClaimReferralBonus`, `GetUserMe`, `UpdateLanguage`, `MarkEventsRead`, `GetMyWallet`, `GetInventory`, `EquipItem`, `UnequipSlot`, `UpgradeItem`, `ClaimAvatar`

### Фаза 4: Переработка ProgressSyncService

- [ ] **4.1** Заменить `SetProgressData(level, watts, xp, totalXp)` → `AddTaps(int tapCount)`
- [ ] **4.2** Реализовать накопление тапов и batch-отправку
- [ ] **4.3** Обработать `tap.tapMeta.droppedTapCount` (если > 0 — нет energy, показать UI)
- [ ] **4.4** Обновить `PlayerService.cs` и другие потребители

### Фаза 5: Переключение на новый сервер

- [ ] **5.1** Изменить `ReferralConfig.asset`: `BaseUrl` → `https://api-dev.wattstap.energy`
- [ ] **5.2** Обновить hardcoded URL в `MiningBalanceRemoteLoader.cs` (строка 115)
- [ ] **5.3** Smoke-тест через `WattsTapAPITesterWindow`

### Фаза 6: Интеграция новых фич

- [ ] **6.1** Интегрировать Inventory систему (equip/unequip/upgrade + stats overlay)
- [ ] **6.2** Интегрировать Wallet (TON connect)
- [ ] **6.3** Интегрировать User events
- [ ] **6.4** Интегрировать social bonus claim

### Фаза 7: Тестирование

- [ ] **7.1** Полный цикл: auth → tap → check progress → avatar → referral
- [ ] **7.2** Тест token refresh (15 мин expiry!)
- [ ] **7.3** Тест edge cases: offline, reconnect, rate limiting, Cloudflare
- [ ] **7.4** Тест droppedTapCount > 0 (нет energy)

---

## 6. Риски и замечания

### 🔴 Критические

1. **Token expiry 15 минут** — `expiresIn: 900`. Без `/auth/refresh` механизма игра сломается через 15 мин. **Блокирующее изменение.** ⚠️ **Refresh token передаётся через HTTP-only cookie** (не в теле ответа). `UnityWebRequest` по умолчанию не хранит cookies между запросами — **нужна кастомная обработка cookies** или обсуждение с бэкендом альтернативного способа передачи refresh token.

2. **Progress endpoint — полная смена парадигмы.** `{ level, watts, xp }` → `{ tapCount }`. Затронет `ProgressSyncService`, `PlayerService`, `TapControllerService` и все UI.

3. **Energy лимит.** Сервер отслеживает energy (1500 по умолчанию). Тапы расходуют 1 energy каждый. При energy=0 тапы будут дропаться (`droppedTapCount > 0`). Клиент должен отслеживать energy и блокировать тапы.

### 🟡 Средние

4. **Аватары — ID стали UUID.** Вместо `"avatar_01"` → `"019d055a-2a7c-..."`. Проверить все места где avatarId хардкодится.

5. **`price` = string** в аватарах (не int). `"0"`, `"10"` — нужен `int.Parse()`.

6. **`equippedStats` — Dictionary-формат.** `JsonUtility` не умеет десериализовать `Dictionary<string, StatValue>`. Нужен обходной путь.

7. **Cloudflare protection.** Проверить что `UnityWebRequest` не блокируется.

8. **`/avatars/{id}/claim` — другой формат ответа.** OLD возвращал `{ success, avatarId, unlockedAvatars[], message }`. NEW возвращает `{ avatarId, currentAvatarId }`. Нужен новый DTO.

9. **`/referral/apply` — неожиданный формат.** Возвращает `{ invitedBy, userReferralCode, activeStatus }` вместо `{ success, message }`.

10. **`/game/inventory/unequip` — ответ содержит только `{ equipment }`.** Не полный inventory, а только слоты. Нужен отдельный DTO `UnequipResponse`.

11. **Items catalog — 186KB JSON.** Рекомендуется кешировать. Каталог содержит все предметы с вариантами (4 редкости × N уровней × бонусы).

12. **`wallet/validate` — поле `public_key` в snake_case.** Все остальные поля в API в camelCase, но `public_key` — исключение. `JsonUtility` корректно сериализует, но нужно именовать поле `public_key` в C# DTO.

### 🟢 Низкие

8. **Новые поля в social responses.** `pendingBonus`, `availableToClaim`, `totalBonusPending` — можно добавить позже.

9. **Hardcoded URL в логах** `MiningBalanceRemoteLoader.cs` — косметическое.

---

## 7. Матрица совместимости DTO (верифицировано реальными запросами)

| C# DTO класс | Совместим с NEW? | Детали |
|---------------|------------------|--------|
| `TelegramAuthRequest` | ✅ Да | Формат идентичен |
| `AuthResponse` | ✅ Да | Все поля совпадают. `expiresIn=900` — работает, просто короче. |
| `PlayerInfo` | ✅ Да | `playerId` = UUID (тип string — ОК) |
| `ReferralResult` | ✅ Да | `null` когда нет реферала — ОК |
| **`SaveProgressRequest`** | 🔴 **Нет** | **Заменить** на `TapProgressRequest { tapCount }` |
| `SaveProgressResponse` | ✅ Да | `success`, `progress`, `message` — совпадают |
| `LoadProgressResponse` | ✅ Да | `success`, `progress`, `isNewPlayer` — совпадают |
| `PlayerProgressDTO` | ✅ Да | `level`, `watts`, `currentXp`, `totalXp` — все присутствуют |
| `ResetProgressRequest` | ✅ Да | `{ "confirm": true }` |
| `MiningBalancePublicResponse` | ✅ Да | Идентичен |
| `MiningBalanceDTO` | ✅ Да | Все поля совпадают |
| `MyReferralResponse` | ✅ Да (базово) | Новые поля игнорируются |
| `FriendsListResponse` | ✅ Да (базово) | Новое поле игнорируется |
| `GetAvatarsResponse` | ✅ Да (базово) | `avatars[]` — новое, игнорируется |
| **`PurchaseAvatarRequest`** | ⚠️ Частично | **Убрать `price`** |
| **`UnlockAvatarByLevelRequest`** | 🔴 Нет | Endpoint убран → `/avatars/{id}/claim` |
| **`AddResourcesRequest`** | 🔴 Нет | Endpoint убран |
| `ApiErrorResponse` | ✅ Да | `{ "detail": "..." }` |

---

*Документ создан: 2026-03-19*
*Верифицирован реальными запросами к `https://api-dev.wattstap.energy` (2026-03-19)*
*Полный тест: 26 запросов → ✅ 20 OK | ⚠️ 6 ожидаемых ошибок (4xx) | ❌ 0 серверных ошибок*









