# WattsTap REST API Documentation

## 📋 Обзор

WattsTap REST API - это серверная часть tap-to-earn игры для Telegram Mini App. API следует принципу **Server Authoritative Architecture**, где весь игровой стейт и логика находятся на сервере, а клиент отвечает только за отображение данных и отправку пользовательских действий.

**Базовый URL**: `https://api.wattstap.com/v1`

## 🏗️ Архитектурные принципы

### Server-Side Authority
- ✅ Все расчёты происходят на сервере
- ✅ Клиент отправляет только намерения (тапы, действия)
- ✅ Сервер валидирует все действия
- ✅ Защита от читерства через rate limiting и античит системы

### Batching System
Тапы отправляются **пакетами** для оптимизации:
- Клиент накапливает N тапов (по умолчанию 5-10)
- Отправляет пакет на сервер
- Сервер валидирует тайминг и физические ограничения
- Возвращает актуальный стейт игрока

## 🔐 Аутентификация

Все запросы требуют Telegram Web App аутентификации через заголовок:

```
Authorization: tma <initDataRaw>
```

Где `initDataRaw` - данные из Telegram WebApp SDK (`window.Telegram.WebApp.initData`).

### Процесс аутентификации

1. Клиент получает `initData` от Telegram WebApp
2. Отправляет на `/auth/telegram` для валидации
3. Получает JWT токен
4. Использует JWT для всех последующих запросов

```
Authorization: Bearer <jwt_token>
```

## 📡 Основные эндпоинты

### Authentication

#### `POST /auth/telegram`
Аутентификация через Telegram WebApp

**Request:**
```json
{
  "initData": "query_id=AAH...&user=%7B%22id%22%3A123456789...",
  "initDataUnsafe": {
    "user": {
      "id": 123456789,
      "first_name": "John",
      "last_name": "Doe",
      "username": "johndoe",
      "language_code": "en",
      "photo_url": "https://..."
    }
  }
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 86400,
    "player": {
      "playerId": "uuid-here",
      "nickname": "John Doe",
      "level": 1,
      "isNewPlayer": true
    }
  }
}
```

---

### Player Data

#### `GET /player/me`
Получить полные данные текущего игрока

**Response:**
```json
{
  "success": true,
  "data": {
    "playerId": "550e8400-e29b-41d4-a716-446655440000",
    "nickname": "John Doe",
    "level": 15,
    "avatarUrl": "https://t.me/i/userpic/320/...",
    "telegramUserId": 123456789,
    "tonWalletAddress": "UQD...",
    "resources": {
      "watts": 1500000,
      "currentEnergy": 80,
      "maxEnergy": 100,
      "currentXP": 45000,
      "xpToNextLevel": 50000,
      "kiloWattTokens": "125.50",
      "currentHits": 18,
      "maxHits": 20
    },
    "stats": {
      "totalTaps": 15420,
      "totalPlayTimeSeconds": 36000,
      "incomePerHour": 5000,
      "incomePerTap": 100,
      "friendsCount": 12,
      "upgradesPurchased": 25,
      "chestsOpened": 8,
      "tournamentRank": 156,
      "bestTournamentRank": 42,
      "lastLoginTime": "2025-11-12T10:30:00Z"
    },
    "inventory": {
      "equippedWeaponId": "weapon-123",
      "equippedHelmetId": "helmet-456",
      "equippedArmorId": "armor-789",
      "equippedBootsId": "boots-012",
      "items": [...]
    },
    "dailyLoginStreak": 5,
    "lastDailyBonusDate": "2025-11-12T00:00:00Z",
    "createdAt": "2025-10-01T12:00:00Z",
    "updatedAt": "2025-11-12T10:30:15Z"
  },
  "serverTime": "2025-11-12T10:30:15Z"
}
```

#### `PATCH /player/me`
Обновить профиль игрока

**Request:**
```json
{
  "nickname": "NewNickname",
  "avatarUrl": "https://..."
}
```

---

### Tap System (Core Gameplay)

#### `POST /gameplay/tap-batch`
**Основной эндпоинт для тапов** - отправка пакета тапов на сервер

**Request:**
```json
{
  "taps": [
    {
      "clientTimestamp": 1699786215000,
      "screenPosition": {"x": 540, "y": 960}
    },
    {
      "clientTimestamp": 1699786215250,
      "screenPosition": {"x": 545, "y": 965}
    },
    {
      "clientTimestamp": 1699786215500,
      "screenPosition": {"x": 538, "y": 958}
    }
  ],
  "clientTime": 1699786215500,
  "sessionId": "session-uuid"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "validTapsCount": 3,
    "invalidTapsCount": 0,
    "wattsEarned": 300,
    "xpEarned": 15,
    "resources": {
      "watts": 1500300,
      "currentHits": 15,
      "maxHits": 20,
      "currentEnergy": 77,
      "maxEnergy": 100,
      "currentXP": 45015,
      "xpToNextLevel": 50000
    },
    "effects": [
      {
        "type": "tap_success",
        "position": {"x": 540, "y": 960},
        "value": 100
      },
      {
        "type": "combo",
        "multiplier": 1.2
      }
    ],
    "levelUp": null
  },
  "serverTime": "2025-11-12T10:30:15Z",
  "antiCheat": {
    "flagged": false,
    "trustScore": 0.98
  }
}
```

**Валидация на сервере:**
- Проверка временных меток (макс. отклонение ±5 сек)
- Проверка физической возможности тапов (макс. 10/сек)
- Проверка доступных ударов (currentHits)
- Расчёт износа энергии
- Античит детекция аномалий

#### `POST /gameplay/sync`
Синхронизация состояния (для восстановления соединения)

**Request:**
```json
{
  "lastKnownServerTime": "2025-11-12T10:25:00Z",
  "clientTime": 1699786515000
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "resources": {...},
    "offlineRewards": {
      "watts": 5000,
      "duration": 300,
      "multiplier": 1.5
    },
    "hitsRecovered": 5
  },
  "serverTime": "2025-11-12T10:30:15Z"
}
```

---

### Resources & Economy

#### `GET /player/resources`
Получить только ресурсы (легковесный запрос)

**Response:**
```json
{
  "success": true,
  "data": {
    "watts": 1500000,
    "currentEnergy": 80,
    "maxEnergy": 100,
    "currentXP": 45000,
    "xpToNextLevel": 50000,
    "kiloWattTokens": "125.50",
    "currentHits": 18,
    "maxHits": 20
  },
  "serverTime": "2025-11-12T10:30:15Z"
}
```

#### `POST /player/claim-offline-bonus`
Получить оффлайн награды (макс. 4 часа)

**Response:**
```json
{
  "success": true,
  "data": {
    "wattsEarned": 20000,
    "offlineDuration": 7200,
    "multiplier": 1.5,
    "maxDuration": 14400,
    "resources": {...}
  }
}
```

#### `POST /player/claim-daily-bonus`
Получить ежедневный бонус

**Response:**
```json
{
  "success": true,
  "data": {
    "streakDay": 6,
    "rewards": {
      "watts": 5000,
      "xp": 500,
      "items": []
    },
    "nextBonusAvailableAt": "2025-11-13T00:00:00Z",
    "resources": {...}
  }
}
```

---

### Upgrades

#### `GET /upgrades/available`
Получить список доступных улучшений

**Response:**
```json
{
  "success": true,
  "data": {
    "upgrades": [
      {
        "upgradeId": "tap_income_1",
        "category": "income",
        "name": "Увеличить доход за тап",
        "description": "+50 Watts за тап",
        "currentLevel": 5,
        "maxLevel": 100,
        "cost": 10000,
        "effect": {
          "type": "tap_income_boost",
          "value": 50
        },
        "requirements": {
          "level": 5
        },
        "available": true
      },
      {
        "upgradeId": "max_hits_1",
        "category": "capacity",
        "name": "Увеличить макс. удары",
        "description": "+5 максимальных ударов",
        "currentLevel": 2,
        "maxLevel": 50,
        "cost": 25000,
        "effect": {
          "type": "max_hits_boost",
          "value": 5
        },
        "requirements": {
          "level": 10
        },
        "available": false
      }
    ]
  }
}
```

#### `POST /upgrades/purchase`
Купить улучшение

**Request:**
```json
{
  "upgradeId": "tap_income_1"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "upgrade": {
      "upgradeId": "tap_income_1",
      "newLevel": 6,
      "costPaid": 10000
    },
    "resources": {...},
    "stats": {
      "incomePerTap": 350
    }
  }
}
```

---

### Inventory & Items

#### `GET /inventory`
Получить инвентарь игрока

**Response:**
```json
{
  "success": true,
  "data": {
    "equipped": {
      "weapon": {...},
      "helmet": {...},
      "armor": {...},
      "boots": {...}
    },
    "items": [
      {
        "itemId": "item-uuid-123",
        "type": "weapon",
        "rarity": "epic",
        "level": 5,
        "tapIncomeBonus": 500,
        "passiveIncomeBonus": 100,
        "energyBonus": 0,
        "isEquipped": true
      }
    ],
    "totalSlots": 100,
    "usedSlots": 45
  }
}
```

#### `POST /inventory/equip`
Экипировать предмет

**Request:**
```json
{
  "itemId": "item-uuid-123"
}
```

#### `POST /inventory/merge`
Объединить предметы (система мержа)

**Request:**
```json
{
  "sourceItemIds": ["item-1", "item-2", "item-3"]
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "resultItem": {
      "itemId": "new-item-uuid",
      "type": "weapon",
      "rarity": "legendary",
      "level": 1,
      "tapIncomeBonus": 1000
    },
    "consumedItems": ["item-1", "item-2", "item-3"]
  }
}
```

#### `POST /inventory/upgrade-item`
Улучшить предмет

**Request:**
```json
{
  "itemId": "item-uuid-123",
  "costInWatts": 50000
}
```

#### `POST /inventory/sell`
Продать предмет

**Request:**
```json
{
  "itemId": "item-uuid-123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "wattsEarned": 5000,
    "resources": {...}
  }
}
```

---

### Social & Referrals

#### `GET /social/friends`
Получить список друзей

**Response:**
```json
{
  "success": true,
  "data": {
    "friends": [
      {
        "playerId": "friend-uuid",
        "nickname": "Friend Name",
        "level": 12,
        "avatarUrl": "https://...",
        "totalEarnings": 500000,
        "yourBonus": 5000,
        "invitedAt": "2025-11-01T10:00:00Z"
      }
    ],
    "totalFriends": 12,
    "totalBonusEarned": 60000
  }
}
```

#### `POST /social/invite`
Создать реферальную ссылку

**Response:**
```json
{
  "success": true,
  "data": {
    "referralCode": "ABC123XYZ",
    "inviteLink": "https://t.me/WattsTapBot/game?startapp=ref_ABC123XYZ",
    "bonusPerFriend": 1000
  }
}
```

#### `GET /social/leaderboard`
Получить таблицу лидеров

**Query params:**
- `type`: `global` | `friends` | `tournament`
- `limit`: int (default 100)
- `offset`: int (default 0)

**Response:**
```json
{
  "success": true,
  "data": {
    "type": "global",
    "entries": [
      {
        "rank": 1,
        "playerId": "top-player-uuid",
        "nickname": "TopPlayer",
        "level": 50,
        "totalWatts": 10000000,
        "avatarUrl": "https://..."
      }
    ],
    "yourRank": 156,
    "totalPlayers": 10000
  }
}
```

---

### Tournaments

#### `GET /tournaments/active`
Получить активные турниры

**Response:**
```json
{
  "success": true,
  "data": {
    "tournaments": [
      {
        "tournamentId": "tournament-uuid",
        "name": "Weekly Championship",
        "description": "Compete for top rewards!",
        "startTime": "2025-11-11T00:00:00Z",
        "endTime": "2025-11-18T00:00:00Z",
        "rewards": [
          {
            "rank": 1,
            "watts": 100000,
            "items": ["legendary-chest"]
          }
        ],
        "yourRank": 156,
        "yourScore": 50000
      }
    ]
  }
}
```

#### `GET /tournaments/:tournamentId/leaderboard`
Таблица лидеров турнира

---

### Shop

#### `GET /shop/items`
Получить товары магазина

**Response:**
```json
{
  "success": true,
  "data": {
    "categories": [
      {
        "categoryId": "chests",
        "name": "Chests",
        "items": [
          {
            "itemId": "common-chest",
            "name": "Common Chest",
            "price": 5000,
            "currency": "watts",
            "contents": {
              "guaranteedRarity": "common",
              "itemCount": 3
            }
          }
        ]
      },
      {
        "categoryId": "boosters",
        "name": "Boosters",
        "items": [...]
      }
    ]
  }
}
```

#### `POST /shop/purchase`
Купить товар

**Request:**
```json
{
  "itemId": "common-chest",
  "quantity": 1,
  "currency": "watts"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "purchase": {
      "itemId": "common-chest",
      "quantity": 1,
      "totalCost": 5000
    },
    "rewards": [
      {
        "type": "item",
        "itemId": "weapon-123",
        "rarity": "uncommon"
      }
    ],
    "resources": {...}
  }
}
```

---

### Wallet Integration

#### `POST /wallet/connect`
Подключить TON кошелёк

**Request:**
```json
{
  "walletAddress": "UQD...",
  "proof": {
    "timestamp": 1699786215,
    "domain": "wattstap.com",
    "signature": "..."
  }
}
```

#### `POST /wallet/withdraw`
Вывести KiloWatt токены

**Request:**
```json
{
  "amount": "100.00",
  "walletAddress": "UQD..."
}
```

---

## 📊 Response Format

Все ответы следуют единому формату:

### Success Response
```json
{
  "success": true,
  "data": {...},
  "serverTime": "2025-11-12T10:30:15Z"
}
```

### Error Response
```json
{
  "success": false,
  "error": {
    "code": "INSUFFICIENT_RESOURCES",
    "message": "Not enough Watts to complete this action",
    "details": {
      "required": 10000,
      "available": 5000
    }
  },
  "serverTime": "2025-11-12T10:30:15Z"
}
```

## 🚨 Error Codes

| Code | Description |
|------|-------------|
| `INVALID_TOKEN` | Неверный или истекший JWT токен |
| `UNAUTHORIZED` | Требуется аутентификация |
| `RATE_LIMIT_EXCEEDED` | Превышен лимит запросов |
| `INSUFFICIENT_RESOURCES` | Недостаточно ресурсов |
| `INVALID_ACTION` | Некорректное действие |
| `PLAYER_NOT_FOUND` | Игрок не найден |
| `ITEM_NOT_FOUND` | Предмет не найден |
| `ANTI_CHEAT_VIOLATION` | Обнаружена подозрительная активность |
| `SERVER_ERROR` | Внутренняя ошибка сервера |
| `MAINTENANCE` | Сервер на обслуживании |

## 🔒 Rate Limiting

| Endpoint | Limit |
|----------|-------|
| `POST /gameplay/tap-batch` | 20 requests/minute |
| `GET /player/*` | 60 requests/minute |
| `POST /shop/purchase` | 10 requests/minute |
| Other endpoints | 100 requests/minute |

Headers:
```
X-RateLimit-Limit: 20
X-RateLimit-Remaining: 15
X-RateLimit-Reset: 1699786800
```

## 🛡️ Anti-Cheat System

Сервер автоматически отслеживает:
- Аномальную частоту тапов
- Невозможные временные последовательности
- Модификацию client-side данных
- Подозрительные паттерны поведения

При обнаружении подозрительной активности:
1. Снижается trust score игрока
2. Применяются дополнительные проверки
3. При повторных нарушениях - временный бан

## 📱 WebSocket Events (опционально)

Для real-time обновлений можно использовать WebSocket:

```
wss://api.wattstap.com/v1/ws
```

**Events:**
- `player:resources_updated` - обновление ресурсов
- `tournament:rank_changed` - изменение позиции в турнире
- `social:friend_online` - друг вошёл в игру
- `maintenance:scheduled` - запланировано обслуживание

## 🔧 SDK Examples

### Unity C# Example

```csharp
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class WattsTapAPI
{
    private const string BASE_URL = "https://api.wattstap.com/v1";
    private string authToken;

    public IEnumerator SendTapBatch(List<TapData> taps, System.Action<TapBatchResponse> callback)
    {
        var request = new TapBatchRequest {
            taps = taps,
            clientTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            sessionId = SessionManager.SessionId
        };

        string json = JsonUtility.ToJson(request);
        
        using (UnityWebRequest www = UnityWebRequest.Post($"{BASE_URL}/gameplay/tap-batch", json, "application/json"))
        {
            www.SetRequestHeader("Authorization", $"Bearer {authToken}");
            
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ApiResponse<TapBatchResponse>>(www.downloadHandler.text);
                callback?.Invoke(response.data);
            }
        }
    }
}
```

## 🌐 CORS Configuration

Allowed origins:
- `https://game.wattstap.com`
- `https://wattstap.com`
- Telegram WebApp origins

## 📈 Monitoring & Analytics

Server-side события для аналитики:
- Player login/logout
- Tap batches processed
- Purchases completed
- Anti-cheat triggers
- API errors

## 🚀 Environment URLs

- **Production**: `https://api.wattstap.com/v1`
- **Staging**: `https://api-staging.wattstap.com/v1`
- **Development**: `https://api-dev.wattstap.com/v1`

---

## 📞 Support

- **Email**: api-support@wattstap.com
- **Telegram**: @WattsTapSupport
- **Status Page**: https://status.wattstap.com

