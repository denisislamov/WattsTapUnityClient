# 📄 Отчёт: Swagger-документация API `api-dev.wattstap.energy/docs`

> **Дата снятия**: 27 марта 2026  
> **URL**: `https://api-dev.wattstap.energy/docs/`  
> **Формат**: Swagger UI (OpenAPI 3.0.3)  
> **Название API**: Wattstap Backend API v1.0.0  
> **Описание**: Interactive API documentation for Wattstap backend.  
> **Сервер**: `/` (относительный путь, т.е. `https://api-dev.wattstap.energy`)

---

## 📊 Общая статистика

| Параметр | Значение |
|----------|----------|
| Версия OpenAPI | 3.0.3 |
| Всего эндпоинтов | **65** |
| Тегов (разделов) | **13** |
| Схем (моделей данных) | **~70** |
| Аутентификация | Bearer JWT (+ ADMIN_TOKEN для `/admin/*`) |
| Размер спецификации | ~200 KB JSON |

---

## 🏷️ Теги (разделы API)

| # | Тег | Описание | Кол-во эндпоинтов |
|---|-----|----------|--------------------|
| 1 | **Health** | Проверка здоровья сервера | 3 |
| 2 | **Auth** | Аутентификация через Telegram | 3 |
| 3 | **Progress** | Прогресс игрока, тапы, баланс майнинга | 4 |
| 4 | **Dev** | Отладочные эндпоинты (non-production) | 3 |
| 5 | **Social** | Рефералы, друзья, бонусы | 4 |
| 6 | **Avatars** | Каталог аватаров, покупка, claim | 3 + admin |
| 7 | **Boosters** | Каталог бустеров, использование, покупка | 3 + admin |
| 8 | **Chests** | Сундуки: каталог, открытие, покупка | 4 + admin (~18) |
| 9 | **Admin** | Административное управление | ~20 |
| 10 | **Items** | Каталог игровых предметов | 1 |
| 11 | **Inventory** | Инвентарь, экипировка, улучшения | 4 + dev |
| 12 | **Wallet** | TON-кошелёк: подключение, валидация | 3 |
| 13 | **User** | Профиль пользователя, события, язык | 3 |

---

## 🔐 Аутентификация

```
Тип: HTTP Bearer (JWT)
Формат заголовка: Authorization: Bearer <access_token>
Admin routes: Authorization: Bearer <ADMIN_TOKEN>
```

- Access token выдаётся через `POST /auth/telegram`
- Время жизни: **900 секунд (15 минут)**
- Refresh через `POST /auth/refresh` (refresh token в HTTP-only cookie)
- Логаут через `POST /auth/logout`

---

## 📡 Полный список эндпоинтов

### 1. Health — Служебные

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `GET` | `/health` | Health check | ❌ |
| `GET` | `/openapi.json` | OpenAPI спецификация в JSON | ❌ |
| `GET` | `/docs` | Swagger UI | ❌ |

---

### 2. Auth — Аутентификация

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `POST` | `/auth/telegram` | Аутентификация через Telegram initData | ❌ |
| `POST` | `/auth/refresh` | Обновление access token (refresh cookie) | ❌ |
| `POST` | `/auth/logout` | Выход, очистка refresh token cookie | ✅ Bearer |

#### `POST /auth/telegram`

**Request Body:**
```json
{
  "initData": "string",       // * обязательное — Telegram WebApp initData
  "referralCode": "string"    // опционально — реферальный код
}
```

**Response 200:**
```json
{
  "token": "eyJ...",
  "expiresIn": 900,
  "player": {
    "playerId": "uuid",
    "nickname": "string",
    "level": 1,
    "isNewPlayer": true,
    "referralCode": "XXXX-XXXX"
  },
  "referral": null
}
```

**Ошибки:** `400` Bad request, `401` Unauthorized, `429` Too many requests

#### `POST /auth/refresh`

Использует refresh token из HTTP-only cookie. Тело запроса пустое.

**Response 200:** Новый access token  
**Ошибки:** `400` Missing refresh token, `401` Unauthorized, `429` Rate limit

#### `POST /auth/logout`

**Response 200:**
```json
{ "message": "Logged out successfully" }
```

---

### 3. Progress — Игровой прогресс

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `GET` | `/progress` | Получить прогресс с детальным состоянием | ✅ Bearer |
| `POST` | `/progress` | Отправить тапы (server-side расчёт) | ✅ Bearer |
| `GET` | `/balance/mining` | Конфигурация майнинга | ❌ |

#### `GET /progress`

**Response 200 — `ProgressStateResponse`:**
```json
{
  "success": true,
  "progress": {
    "level": 1,
    "watts": 100,
    "currentXp": 100,
    "totalXp": 100
  },
  "isNewPlayer": false,
  "playerState": {
    "id": "uuid",
    "userId": "uuid",
    "gameConfigId": "uuid",
    "level": 1,
    "experience": 100,
    "coins": 100,
    "energy": 1500,
    "energyUpdatedAt": "ISO8601",
    "createdAt": "ISO8601",
    "updatedAt": "ISO8601"
  },
  "nextRegenAt": "ISO8601 | null",
  "boosters": {
    "active": [],
    "tapMode": "MULTIPLY_TAPS_AND_COST"
  }
}
```

**Ошибки:** `429` Rate limit

#### `POST /progress`

**Request Body — `ProgressTapPayload`:**
```json
{
  "tapCount": 5    // * обязательное — количество тапов в пакете
}
```

**Response 200 — `ProgressStateResponse`:**
```json
{
  "success": true,
  "progress": { "level": 1, "watts": 105, "currentXp": 105, "totalXp": 105 },
  "isNewPlayer": false,
  "playerState": { ... },
  "nextRegenAt": "ISO8601",
  "message": "Progress updated",
  "tap": {
    "energyBefore": 1500,
    "energyAfter": 1495,
    "xpEarned": 5,
    "coinsEarned": 5,
    "appliedTapCount": 5,
    "tapMeta": {
      "requestedTapCount": 5,
      "appliedTapCount": 5,
      "droppedTapCount": 0
    }
  },
  "boosters": {
    "active": [],
    "tapMode": "MULTIPLY_TAPS_AND_COST",
    "tapConversion": {
      "validatedTapCount": 5,
      "effectiveTapCount": 5,
      "energyTapCostCount": 5,
      "baseAppliedCount": 5,
      "appliedMultipliers": []
    }
  }
}
```

**Ошибки:** `400` Invalid payload, `401` Unauthorized, `404` Player/config not found, `429` Rate limit, `500` Internal

#### `GET /balance/mining`

**Response 200:**
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

---

### 4. Social — Рефералы и друзья

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `GET` | `/social/my-referral` | Реферальный код и статистика | ✅ Bearer |
| `GET` | `/social/friends` | Список приглашённых друзей | ✅ Bearer |
| `POST` | `/social/bonus/claim` | Получить реферальные бонусы | ✅ Bearer |
| `POST` | `/referral/apply` | Применить реферальный код вручную | ✅ Bearer |

#### `GET /social/my-referral`

**Response 200:**
```json
{
  "referralCode": "XXXX-XXXX",
  "inviteLink": "https://t.me/WattsTapDevTemp_bot/app?startapp=REF_XXXX-XXXX",
  "bonusPerFriend": 300,
  "totalFriendsInvited": 0,
  "totalBonusEarned": 0,
  "pendingBonus": 0,
  "availableToClaim": false
}
```

#### `GET /social/friends`

**Response 200:**
```json
{
  "friends": [
    {
      "playerId": "uuid",
      "nickname": "string",
      "level": 5,
      "avatarUrl": "string | null",
      "totalEarnings": 1000,
      "yourBonus": 300,
      "yourBonusCollected": 300,
      "yourBonusPending": 0,
      "yourBonusTotal": 300,
      "relationType": "string",
      "invitedAt": "ISO8601"
    }
  ],
  "totalFriends": 1,
  "totalBonusEarned": 300,
  "totalBonusPending": 0
}
```

#### `POST /social/bonus/claim`

**Request Body:** `{}` (пустое)

**Response 200:**
```json
{
  "success": true,
  "claimedAmount": 0,
  "claimedFriendsCount": 0,
  "newWattsBalance": 100,
  "totalBonusEarned": 0,
  "totalBonusPending": 0
}
```

**Ошибки:** `429` Rate limit, `500` Failed to claim

#### `POST /referral/apply`

**Request Body:**
```json
{
  "source": "miniapp",       // * обязательное — "web" | "miniapp" | "system"
  "referralCode": "XXXX-XXXX"  // опционально
}
```

**Response 200:**
```json
{
  "invitedBy": "",
  "userReferralCode": "",
  "activeStatus": false
}
```

**Ошибки:** `400` Self referral is not allowed, `429` Rate limit

---

### 5. Avatars — Аватары

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `GET` | `/avatars` | Каталог аватаров для текущего пользователя | ✅ Bearer |
| `POST` | `/avatars/purchase` | Покупка аватара за watts или btn (TON) | ✅ Bearer |
| `POST` | `/avatars/{id}/claim` | Получение бесплатного аватара | ✅ Bearer |

#### `GET /avatars`

**Response 200:**
```json
{
  "success": true,
  "unlockedAvatars": ["uuid-1", "uuid-2"],
  "currentAvatar": "uuid-1",
  "avatars": [
    {
      "id": "uuid",
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
    }
  ]
}
```

#### `POST /avatars/purchase`

**Request Body:**
```json
{
  "avatarId": "uuid",    // * обязательное
  "currency": "watts"    // * обязательное — "watts" | "btn"
}
```

**Response 200:**
```json
{
  "success": true,
  "avatarId": "uuid",
  "currency": "watts",
  "status": "string",
  "nextAction": "string",
  "message": "string",
  "purchase": null,
  "payment": { ... }     // ShopPaymentPayload (для btn оплаты)
}
```

**Ошибки:** `400` Bad request, `401` Unauthorized, `404` Avatar not found, `409` Not enough watts

#### `POST /avatars/{id}/claim`

**Path параметр:** `id` — UUID аватара

**Response 200:**
```json
{
  "avatarId": "uuid",
  "currentAvatarId": "uuid"
}
```

**Ошибки:** `400` Avatar is not free, `404` Avatar not found

---

### 6. Boosters — Бустеры 🆕

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `GET` | `/game/boosters` | Каталог, инвентарь и активные бустеры | ✅ Bearer |
| `POST` | `/game/boosters/use` | Использовать купленный бустер | ✅ Bearer |
| `POST` | `/game/boosters/purchase` | Создать заказ на покупку бустера | ✅ Bearer |

#### `GET /game/boosters`

**Response 200 — `BoosterListResponse`:**
```json
{
  "catalog": [
    {
      "id": "uuid",
      "code": "string",
      "type": "string",
      "title": "string",
      "description": "string",
      "price": "100",
      "paymentAssetType": "string",
      "activated": true,
      "sortOrder": 1,
      "config": { ... }
    }
  ],
  "inventory": [
    {
      "playerBoosterId": "uuid",
      "boosterCode": "string",
      "boosterType": "string",
      "acquiredAt": "ISO8601",
      "consumedAt": "ISO8601 | null"
    }
  ],
  "active": [
    {
      "id": "uuid",
      "boosterCode": "string",
      "boosterType": "string",
      "startedAt": "ISO8601",
      "expiresAt": "ISO8601 | null",
      "secondsLeft": 300
    }
  ]
}
```

**Ошибки:** `401` Unauthorized, `404` Player state not found, `500` Internal

#### `POST /game/boosters/use`

**Request Body:**
```json
{
  "playerBoosterId": "uuid"    // * обязательное
}
```

**Response 200 — `BoosterUseResponse`:**
```json
{
  "success": true,
  "boosterType": "string",
  "consumedAt": "ISO8601",
  "effect": { ... }
}
```

**Ошибки:** `400` Invalid ID, `401` Unauthorized, `404` Not found, `409` Cannot use in current state, `500` Internal

#### `POST /game/boosters/purchase`

**Request Body:**
```json
{
  "boosterCode": "string"    // * обязательное
}
```

**Response 200 — `BoosterPurchaseResponse`:**
```json
{
  "success": true,
  "boosterCode": "string",
  "boosterType": "string",
  "status": "string",
  "nextAction": "string",
  "message": "string",
  "purchase": null,
  "payment": {
    "orderId": "uuid",
    "price": "100",
    "paymentAssetType": "string",
    "walletAddress": "string",
    "expiresAt": "ISO8601",
    "comment": "string",
    "paymentLink": "string | null",
    "tonConnectArgs": {
      "address": "string",
      "amount": "string",
      "payload": "string"
    }
  }
}
```

**Ошибки:** `400` Bad request, `401` Unauthorized, `404` Booster not found

---

### 7. Chests — Сундуки 🆕

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `GET` | `/game/chests/catalog` | Каталог сундуков | ✅ Bearer |
| `GET` | `/game/chests/state` | Прогресс по сундукам | ✅ Bearer |
| `POST` | `/game/chests/open` | Открыть сундук из инвентаря | ✅ Bearer |
| `POST` | `/game/chests/purchase` | Создать заказ на покупку сундука | ✅ Bearer |

#### `GET /game/chests/catalog`

**Response 200 — `ChestCatalogResponse`:**
```json
{
  "chests": [
    {
      "chestType": "string",
      "displayName": "string",
      "containsItemsMin": 1,
      "containsItemsMax": 5,
      "nextOpenItemCount": 3,
      "ownedCount": 0,
      "canOpen": false,
      "canPurchase": true,
      "price": "100",
      "paymentAssetType": "string",
      "rarityChances": {
        "common": 0.6,
        "uncommon": 0.25,
        "rare": 0.12,
        "legendary": 0.03
      },
      "itemsPerOpenStats": {
        "min": 1,
        "max": 5,
        "avg": 2.5
      },
      "preview": {
        "imageUrl": "string | null",
        "backgroundStyle": "string | null"
      }
    }
  ]
}
```

**Ошибки:** `401`, `404` Player not found, `409` Config not active, `500` Internal

#### `GET /game/chests/state`

**Response 200 — `ChestStateResponse`:**
```json
{
  "state": [
    {
      "chestType": "string",
      "bagCode": "string",
      "slotIndex": 0,
      "opensInCurrentCycle": 0
    }
  ]
}
```

#### `POST /game/chests/open`

**Request Body:**
```json
{
  "chestType": "string",     // * обязательное
  "requestId": "string"      // * обязательное — идемпотентность
}
```

**Response 200 — `ChestOpenResponse`:**
```json
{
  "openId": "uuid",
  "chestType": "string",
  "configVersion": 1,
  "bagCode": "string",
  "slotIndex": 0,
  "remainingOwnedCount": 2,
  "rewards": [
    {
      "itemVariantId": "uuid",
      "rarity": "COMMON",
      "quantity": 1,
      "revealOrder": 0
    }
  ],
  "newProgress": {
    "bagCode": "string",
    "slotIndex": 1
  }
}
```

**Ошибки:** `400` Invalid payload, `401`, `404` Not found, `409` Out of stock / conflict, `422` Config validation error, `500` Internal

#### `POST /game/chests/purchase`

**Request Body:**
```json
{
  "chestType": "string",    // * обязательное
  "quantity": 1,            // опционально (default=1)
  "requestId": "string"     // * обязательное — идемпотентность
}
```

**Response 200 — `ChestPurchaseResponse`:**
```json
{
  "success": true,
  "orderId": "uuid",
  "chestType": "string",
  "quantity": 1,
  "status": "string",
  "nextAction": "string",
  "payment": { ... }        // ShopPaymentPayload
}
```

**Ошибки:** `400`, `401`, `404`, `409` Config not active, `422` Unprocessable, `500` Internal

---

### 8. Items — Каталог предметов

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `GET` | `/game/items/catalog` | Полный каталог игровых предметов | ✅ Bearer |

#### `GET /game/items/catalog`

**Response 200** (≈186 KB JSON):
```json
{
  "items": [
    {
      "id": "uuid",
      "code": "weapon_amp_thumper",
      "name": "Amp-Thumper",
      "description": "Each swing generates enough electricity...",
      "slot": "WEAPON",
      "variants": [
        {
          "id": "uuid",
          "rarity": "COMMON",
          "maxLevel": 5,
          "mainStatType": "CAPACITY_HITS",
          "levels": [
            { "level": 1, "value": 10, "valuePercent": null },
            { "level": 2, "value": 20, "valuePercent": null }
          ],
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

---

### 9. Inventory — Инвентарь

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `GET` | `/game/inventory` | Получить инвентарь игрока | ✅ Bearer |
| `POST` | `/game/inventory/equip` | Экипировать предмет | ✅ Bearer |
| `POST` | `/game/inventory/unequip` | Снять предмет со слота | ✅ Bearer |
| `POST` | `/game/inventory/upgrade` | Улучшить предмет | ✅ Bearer |

#### `GET /game/inventory`

**Response 200:**
```json
{
  "inventory": [
    {
      "id": "uuid",
      "itemVariantId": "uuid",
      "level": 1,
      "slot": "WEAPON",
      "rarity": "COMMON",
      "isEquipped": false
    }
  ],
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
    "coins": 100,
    "drawings": 0
  }
}
```

**Ошибки:** `401`, `404` Player state not found

#### `POST /game/inventory/equip`

**Request Body:**
```json
{ "playerItemId": "uuid" }    // * обязательное
```

**Ошибки:** `400` Invalid ID, `401`, `404` Item not found, `409` Inconsistent equipment state

#### `POST /game/inventory/unequip`

**Request Body:**
```json
{ "slot": "WEAPON" }          // * обязательное — "WEAPON" | "ARMS" | "BODY" | "FEET"
```

**Response 200:**
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

**Ошибки:** `400` Invalid slot, `401`, `404` Player not found

#### `POST /game/inventory/upgrade`

**Request Body:**
```json
{ "playerItemId": "uuid" }    // * обязательное
```

**Ошибки:** `400` Invalid ID, `401`, `404` Item not found, `409` Not enough resources, `422` Already at max level

---

### 10. Wallet — TON-кошелёк

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `GET` | `/wallet/my-wallet` | Получить кошелёк пользователя | ✅ Bearer |
| `GET` | `/wallet/payload` | Сгенерировать payload для подключения | ✅ Bearer |
| `POST` | `/wallet/validate` | Валидация proof подключённого кошелька | ✅ Bearer |

#### `GET /wallet/my-wallet`

**Response 200:**
```json
{
  "walletAddress": "",
  "jettonAddress": "EQCKk1x74PGx7TDS2Lr5JN8RXum-IFkYmuvy9EFOSgu4yg66",
  "jettonBalance": 0,
  "wallets": []
}
```

#### `GET /wallet/payload`

**Response 200:**
```json
{
  "payload": "8a17bda113267a827413a4dc909caa10ab77c24a85179c2289756929070d6931"
}
```

**Ошибки:** `401`, `429` Rate limit

#### `POST /wallet/validate`

**Request Body:**
```json
{
  "address": "UQ...",        // * обязательное
  "network": "-239",         // * обязательное — mainnet
  "public_key": "hex...",    // * обязательное ⚠️ snake_case!
  "proof": { ... }           // * обязательное — TON Connect proof object
}
```

**Ошибки:** `400` Invalid wallet proof, `429` Rate limit

---

### 11. User — Профиль пользователя

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `GET` | `/user/me` | Профиль и события пользователя | ✅ Bearer |
| `POST` | `/user/language` | Обновить язык пользователя | ✅ Bearer |
| `POST` | `/user/events/read` | Пометить события прочитанными | ✅ Bearer |

#### `GET /user/me`

**Response 200:**
```json
{
  "user": {
    "id": "uuid",
    "username": "string",
    "firstName": "string",
    "lastName": "string",
    "telegramId": "string",
    "photoUrl": "string",
    "languageCode": "en",
    "createdAt": "ISO8601"
  },
  "events": [],
  "unreadEventsCount": 0
}
```

#### `POST /user/language`

**Request Body:**
```json
{ "newLangCode": "en" }    // * обязательное
```

**Response 200:**
```json
{
  "user": {
    "id": "uuid",
    "telegramId": "string",
    "firstName": "string",
    "lastName": "string",
    "username": "string",
    "photoUrl": "string",
    "languageCode": "en",
    "currentAvatarId": "uuid",
    "createdAt": "ISO8601"
  }
}
```

#### `POST /user/events/read`

**Request Body:**
```json
{
  "readAll": true,           // опционально
  "eventIds": ["uuid"]       // опционально — список конкретных событий
}
```

**Response 200:**
```json
{
  "success": true,
  "updatedCount": 0,
  "unreadEventsCount": 0
}
```

---

### 12. Orders — Заказы (общий для аватаров, бустеров, сундуков)

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `GET` | `/orders/{id}` | Получить детали заказа по ID | ✅ Bearer |

#### `GET /orders/{id}`

**Response 200 — `OrderDetailsResponse`:**
```json
{
  "id": "uuid",
  "status": "string",
  "price": "100",
  "paymentAssetType": "string",
  "expiresAt": "ISO8601",
  "createdAt": "ISO8601",
  "avatar": {                    // null если не аватар
    "id": "uuid",
    "code": "string",
    "title": "string"
  },
  "booster": {                   // null если не бустер
    "id": "uuid",
    "code": "string",
    "type": "string",
    "title": "string"
  },
  "payment": {                   // null если оплата не прошла
    "id": "uuid",
    "status": "string",
    "amount": "100",
    "paymentAssetType": "string",
    "paidAt": "ISO8601",
    "transaction": {
      "txHash": "string",
      "timestamp": "ISO8601",
      "fromAddress": "string | null",
      "toAddress": "string",
      "amount": "100",
      "paymentAssetType": "string",
      "comment": "string | null"
    }
  }
}
```

**Ошибки:** `401`, `404` Order not found

---

### 13. Dev — Отладочные эндпоинты (non-production)

| Метод | Path | Описание | Auth |
|-------|------|----------|------|
| `POST` | `/dev/add-resources` | Добавить watts и XP | ✅ Bearer |
| `POST` | `/game/dev/inventory/grant` | Выдать предмет в инвентарь | ✅ Bearer |
| `POST` | `/game/dev/boosters/grant` | Выдать бустер по коду | ✅ Bearer |

#### `POST /dev/add-resources`

**Request Body — `DevAddResourcesRequest`:**
```json
{
  "watts": 1000,     // опционально
  "xp": 500          // опционально
}
```

**Response 200 — `DevAddResourcesResponse`:**
```json
{
  "success": true,
  "progress": { "level": 2, "watts": 1100, "currentXp": 600, "totalXp": 600 },
  "addedWatts": 1000,
  "addedXp": 500,
  "message": "Resources added"
}
```

**Ошибки:** `400`, `401`, `403` Forbidden in production, `404`, `500`

#### `POST /game/dev/inventory/grant`

**Request Body — `DevInventoryGrantRequest`:**
```json
{
  "itemVariantId": "uuid",    // * обязательное
  "level": 1                   // опционально (default = 1)
}
```

**Response 200 — `DevInventoryGrantResponse`:**
```json
{
  "playerItemId": "uuid",
  "itemVariantId": "uuid",
  "level": 1
}
```

**Ошибки:** `400`, `401`, `403`, `404`, `500`

#### `POST /game/dev/boosters/grant`

**Request Body — `DevBoosterGrantRequest`:**
```json
{
  "code": "string"    // * обязательное — код бустера из каталога
}
```

**Response 200 — `DevBoosterGrantResponse`:**
```json
{
  "playerBoosterId": "uuid",
  "boosterCode": "string",
  "boosterType": "string",
  "acquiredAt": "ISO8601",
  "consumedAt": null
}
```

**Ошибки:** `400`, `401`, `403`, `404`, `500`

---

### 14. Admin — Административные эндпоинты

> ⚠️ Все admin-эндпоинты требуют `Authorization: Bearer <ADMIN_TOKEN>`

| Метод | Path | Описание |
|-------|------|----------|
| `POST` | `/admin/test/{userId}` | Тестовое admin-действие |
| `POST` | `/admin/game/config` | Создать конфиг игры |
| `GET` | `/admin/game/config/{id}` | Получить конфиг по ID |
| `PUT` | `/admin/game/config/{id}` | Обновить конфиг |
| `GET` | `/admin/game/configs` | Список конфигов |
| `POST` | `/admin/user` | Создать пользователя |
| `POST` | `/admin/avatars` | Создать/обновить аватар |
| `PATCH` | `/admin/avatars/{id}` | Обновить аватар |
| `POST` | `/admin/orders/{id}/fulfill` | Выполнить оплаченный заказ |
| `POST` | `/admin/orders/{id}/cancel` | Отменить заказ |
| `GET` | `/admin/boosters` | Список бустеров |
| `POST` | `/admin/boosters` | Создать бустер |
| `PATCH` | `/admin/boosters/{id}` | Обновить бустер |
| `DELETE` | `/admin/boosters/{id}` | Soft-delete бустер |
| `POST` | `/admin/boosters/{id}/activate` | Активировать бустер |
| `POST` | `/admin/boosters/{id}/deactivate` | Деактивировать бустер |
| `GET` | `/admin/chests/configs` | Список конфигов сундуков |
| `POST` | `/admin/chests/configs` | Создать черновик конфига |
| `GET` | `/admin/chests/configs/{id}` | Получить конфиг |
| `PATCH` | `/admin/chests/configs/{id}` | Обновить черновик |
| `POST` | `/admin/chests/configs/{id}/clone` | Клонировать конфиг |
| `POST` | `/admin/chests/configs/{id}/validate` | Валидировать конфиг |
| `POST` | `/admin/chests/configs/{id}/publish` | Опубликовать конфиг |
| `POST` | `/admin/chests/configs/{id}/archive` | Архивировать конфиг |
| `POST` | `/admin/chests/configs/{id}/load-default-v1` | Загрузить пресет |
| `GET` | `/admin/chests/configs/{id}/bags/{bagCode}/slots` | Слоты мешка |
| `PUT` | `/admin/chests/configs/{id}/bags/{bagCode}/slots` | Заменить слоты |
| `GET` | `/admin/chests/configs/{id}/item-pools` | Пулы предметов |
| `POST` | `/admin/chests/configs/{id}/item-pools` | Создать пул |
| `PATCH` | `/admin/chests/item-pools/{poolId}` | Обновить пул |
| `POST` | `/admin/chests/item-pools/{poolId}/entries` | Upsert запись пула |
| `PATCH` | `/admin/chests/item-pool-entries/{entryId}` | Обновить запись |
| `DELETE` | `/admin/chests/item-pool-entries/{entryId}` | Удалить запись |
| `POST` | `/admin/chests/configs/{id}/simulate` | Симуляция дропа |
| `GET` | `/admin/chests/grants` | Список выданных сундуков |
| `POST` | `/admin/chests/grants` | Выдать сундук игроку |
| `GET` | `/admin/chests/players/{playerId}/stock-ledger` | Лог инвентаря сундуков |

#### Game Config — `GameConfig` schema:
```json
{
  "id": "uuid",
  "name": "string",
  "xpPerTap": 1,
  "coinsPerTap": 1,
  "maxEnergy": 1500,
  "regenPeriodSeconds": 2,
  "regenAmount": 1,
  "critMultiplier": 1.2,
  "critChancePercent": 1,
  "profitPerHour": 500,
  "maxOfflineHours": 3,
  "levelUpExpBasis": 100,
  "maxTapsPerRequest": 50,
  "tapWindowSeconds": 10,
  "maxTapsPerWindow": 100,
  "multitapMode": "string",
  "bonusPerFriend": 300,
  "referralRewardUnlockLevel": 1,
  "createdAt": "ISO8601",
  "updatedAt": "ISO8601"
}
```

---

## 🔄 Формат ошибок

Все ошибки возвращаются в едином формате:

```json
{
  "detail": "Human-readable error message"
}
```

HTTP-коды:
- `400` — Невалидный запрос
- `401` — Не авторизован / токен невалиден
- `403` — Запрещено (admin в production, недостаточно прав)
- `404` — Ресурс не найден
- `409` — Конфликт (недостаточно средств, дубликат, невозможное действие)
- `422` — Unprocessable entity (валидация, max level)
- `429` — Rate limit
- `500` — Внутренняя ошибка сервера

---

## 🆕 Что появилось нового (по сравнению с предыдущей документацией)

По сравнению с `API_MIGRATION_GUIDE.md` (от 19.03.2026), в текущей Swagger-документации появились **значительные дополнения**:

### Новые системы

| Система | Эндпоинты | Статус |
|---------|-----------|--------|
| **Boosters** (бустеры) | `GET /game/boosters`, `POST /game/boosters/use`, `POST /game/boosters/purchase` | 🆕 Полностью новое |
| **Chests** (сундуки) | `GET /game/chests/catalog`, `GET /game/chests/state`, `POST /game/chests/open`, `POST /game/chests/purchase` | 🆕 Полностью новое |
| **Dev inventory/boosters grant** | `POST /game/dev/inventory/grant`, `POST /game/dev/boosters/grant` | 🆕 Dev-утилиты |
| **Admin Boosters CRUD** | 6 эндпоинтов полного lifecycle | 🆕 Полностью новое |
| **Admin Chests** | ~18 эндпоинтов (configs, bags, slots, item-pools, simulate, grants, ledger) | 🆕 Массивная система |
| **Shop Payment** | `ShopPaymentPayload` с TON Connect args | 🆕 Единая платёжная модель |

### Возвращённые эндпоинты

| Эндпоинт | Примечание |
|-----------|------------|
| `POST /dev/add-resources` | ❗ Ранее в `API_MIGRATION_GUIDE.md` отмечен как "убран". Теперь **присутствует** в Swagger (non-production only) |

### Подробности по новым моделям

**`BoosterCatalogItem`** — предмет в каталоге бустеров:
- `id`, `code`, `type`, `title`, `description`
- `price` (string!), `paymentAssetType`
- `activated`, `sortOrder`, `config` (object)

**`ChestCatalogItem`** — предмет в каталоге сундуков:
- `chestType`, `displayName`
- `containsItemsMin/Max`, `nextOpenItemCount`, `ownedCount`
- `canOpen`, `canPurchase`
- `price`, `paymentAssetType`
- `rarityChances` (common/uncommon/rare/legendary — вероятности)
- `preview` (imageUrl, backgroundStyle)

**`ShopPaymentPayload`** — унифицированная платёжная модель:
- `orderId`, `price`, `paymentAssetType`
- `walletAddress`, `expiresAt`, `comment`
- `paymentLink` (nullable)
- `tonConnectArgs` → `{ address, amount, payload }`

**`ChestOpenResponse`** — результат открытия сундука:
- `rewards[]` → `{ itemVariantId, rarity, quantity, revealOrder }`
- `newProgress` → `{ bagCode, slotIndex }`
- `remainingOwnedCount`

---

## 📐 Архитектурные наблюдения

1. **Server-Authoritative**: Вся логика на сервере. Клиент отправляет `tapCount`, сервер считает результат.

2. **Единая система заказов**: Аватары, бустеры и сундуки используют общую модель `Orders` с `ShopPaymentPayload` для TON-оплаты.

3. **Chest Bag/Slot система**: Сложная конфигурируемая система дропа из сундуков с bags, slots, item pools, весами и симуляцией.

4. **Energy как ограничитель**: Тапы тратят energy (default: 1500). Регенерация по `regenPeriodSeconds`. При нулевой energy тапы дропаются.

5. **Idempotency**: Сундуки и покупки используют `requestId` для предотвращения дублирования.

6. **Платёжная интеграция**: Поддержка оплаты через TON wallet с `tonConnectArgs` для нативного TON Connect flow.

7. **Admin CMS**: Полноценная админка для управления конфигами игры, аватарами, бустерами, сундуками, пулами предметов и симуляцией.

---

*Документ сгенерирован автоматически из OpenAPI спецификации `https://api-dev.wattstap.energy/docs/` (27 марта 2026)*

