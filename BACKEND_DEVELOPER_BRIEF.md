# WattsTap — Backend Developer Brief

## О проекте

**WattsTap** — tap-to-earn 2D игра с RPG-элементами, работающая как Telegram Mini App (WebGL).
Клиент написан на Unity 6 (C#). Бэкенд нужно разработать с нуля.

**Платформы запуска:**
- Telegram Mini App (основная платформа)
- WeChat Mini Program (планируется)

**Оплата:**
- Telegram: через BTN (Telegram Stars / TON blockchain)
- WeChat: WeChat Pay (планируется)

---

## Игровая механика (что должен поддерживать бэкенд)

### Core Loop
1. **Тапы** — игрок тапает по персонажу, получает Watts (монеты) и XP
2. **Энергия/Хиты** — ограниченное количество тапов за сессию, восстанавливается со временем (cooldown per hit)
3. **Критические удары** — шанс на 2x множитель
4. **Оффлайн-бонус** — пассивный доход до 4 часов отсутствия
5. **Ежедневный бонус** — награды за серию входов (streak)
6. **Прогрессия** — XP → уровни → разблокировка аватаров и наград

### Апгрейды (12 типов)
| Тип | Описание |
|-----|----------|
| GoldHammer | Увеличивает прибыль за тап |
| FastTime | Уменьшает кулдаун хита |
| Endurance | Увеличивает ёмкость хитов |
| CriticalChance | Увеличивает шанс крита |
| CriticalMultiplier | Увеличивает множитель крита |
| WorkExperience | Увеличивает XP за тап |
| Economist | Уменьшает стоимость апгрейдов |
| StrongFriendship | Увеличивает бонус от друзей |
| Investor | Увеличивает часовой доход |
| ItemMaster | Уменьшает стоимость апгрейда предметов |
| ShareProfit | Увеличивает долю прибыли от друзей |
| GoldFriends | Увеличивает реферальные награды |

### Инвентарь и экипировка
- **Типы предметов:** Оружие, Броня (тело, ноги, руки, голова)
- **Редкость:** Common → Uncommon → Rare → Epic → Legendary
- **Слоты:** Weapon, Helmet, Armor, Boots
- **Прокачка:** улучшение за Watts + чертежи
- **Мерж:** объединение предметов для повышения редкости
- **Бонусы:** бонус к тапу от экипированных предметов

### Магазин
- **Сундуки** — лутбоксы с предметами (по редкости)
- **Бустеры:**
  - Полная перезарядка хитов
  - Multi-tap (2x множитель прибыли)
  - Мгновенная прибыль (120 минут на базе часового дохода)

### Турниры
- Активные турниры с таблицами лидеров
- Награды по рангу (монеты, бустеры, предметы)

### Социальные функции
- Реферальная система (уникальный код, бонусы за приглашённых)
- Список друзей с отслеживанием заработка
- Лидерборды (глобальный, друзья, турнирный)

### Аватары
- Разблокировка по уровню
- Покупка за Watts
- Выбор активного аватара

---

## API Endpoints (клиент уже реализован)

### Аутентификация
```
POST /auth/telegram
  Body: { initData: string, referralCode?: string }
  Response: { token, expiresIn, player: PlayerInfo, referral?: ReferralResult }
```

### Игрок
```
GET  /player/me              — Полные данные игрока
PATCH /player/me             — Обновление профиля
GET  /player/resources       — Только ресурсы
POST /player/claim-offline-bonus  — Забрать оффлайн-награду
POST /player/claim-daily-bonus    — Забрать ежедневный бонус
```

### Геймплей
```
POST /gameplay/tap-batch     — Пакет тапов (5-10 за раз)
  Body: { taps: TapData[], clientTime: long, sessionId: string }
  Response: { validTapsCount, invalidTapsCount, wattsEarned, xpEarned, resources, effects, levelUp }

POST /gameplay/sync          — Синхронизация состояния
```

### Прогресс
```
GET  /progress               — Загрузка прогресса
POST /progress               — Сохранение прогресса
POST /progress/reset         — Сброс (debug)
```

### Апгрейды
```
GET  /upgrades/available     — Доступные апгрейды
POST /upgrades/purchase      — Покупка апгрейда
```

### Инвентарь
```
GET  /inventory              — Инвентарь игрока
POST /inventory/equip        — Экипировать предмет
POST /inventory/merge        — Мерж предметов
POST /inventory/upgrade-item — Улучшить предмет
POST /inventory/sell         — Продать предмет
```

### Социальные функции
```
GET  /social/my-referral     — Реферальная информация
GET  /social/friends         — Список друзей
POST /social/invite          — Создать реферальную ссылку
GET  /social/leaderboard?type=global|friends|tournament  — Лидерборд
```

### Аватары
```
GET  /avatars                — Разблокированные аватары
POST /avatars/purchase       — Покупка аватара
POST /avatars/unlock-by-level — Разблокировка по уровню
```

### Магазин
```
GET  /shop/items             — Товары магазина
POST /shop/purchase          — Покупка товара
```

### Турниры
```
GET  /tournaments/active                      — Активные турниры
GET  /tournaments/:tournamentId/leaderboard   — Таблица лидеров турнира
```

### Баланс (публичный)
```
GET  /balance/mining         — Конфигурация баланса игры (без авторизации)
```

### Debug (только dev)
```
POST /dev/add-resources      — Добавить ресурсы
```

---

## Модели данных

### Player
```
PlayerData {
    playerId: UUID
    nickname: string
    level: int
    avatarUrl: string
    telegramUserId: long
    resources: {
        watts: long           // основная валюта
        currentXP: long
        xpToNextLevel: long
        sumExp: long
        kiloWattTokens: decimal  // крипто-валюта
        currentHits: int
    }
}
```

### Inventory Item
```
InventoryItem {
    instanceId: UUID
    itemId: string           // ссылка на каталог
    isEquipped: bool
    acquiredAt: datetime
    level: int
}

ItemData (каталог) {
    id: string
    displayName: string
    description: string
    itemType: enum (Weapon, ArmorBody, ArmorLegs, ArmorArms, ArmorHead)
    rarity: enum (Common, Uncommon, Rare, Epic, Legendary)
    requiredLevel: int
    perTapBonus: float
}
```

### Upgrade
```
UpgradeSkillData {
    upgradeType: enum (12 типов)
    title: string
    description: string
    levels: [{
        level: int
        price: long
        parameter: float
    }]
}
```

### Referral
```
MyReferralResponse {
    referralCode: string
    inviteLink: string
    bonusPerFriend: int
    totalFriendsInvited: int
    totalBonusEarned: long
}

FriendInfo {
    playerId: UUID
    nickname: string
    level: int
    avatarUrl: string
    totalEarnings: long
    yourBonus: long
    invitedAt: datetime
}
```

### Mining Balance (конфигурация)
```
MiningBalanceDTO {
    coinsPerTap: float
    expPerTap: float
    energyCostPerTap: float
    startCapacityHits: int
    cooldownPerHitSec: float
    critMultiplier: float
    chanceCritPercent: float
    avgPlaytimeMinutes: float
    tapsPerSecond: float
    profitPerHour: float
    maxHoursOffline: int
    sessionsPerDay: int
    dailyProgression: DailyProgressionDTO[]
}
```

---

## Аутентификация

1. Telegram WebApp передаёт `initData` через JavaScript bridge
2. Клиент извлекает `referralCode` из `start_param` (формат: `REF_ABC123`)
3. `POST /auth/telegram` с `initData` + `referralCode`
4. Сервер **валидирует** `initData` (HMAC-SHA256 через Telegram Bot Token)
5. Возвращает JWT токен (`Bearer`)
6. Все последующие запросы: `Authorization: Bearer <jwt_token>`
7. Время жизни токена: ~24 часа

---

## Конфигурация окружений

| Окружение | URL |
|-----------|-----|
| Production | `https://api.wattstap.com/v1` |
| Staging | `https://api-staging.wattstap.com/v1` |
| Development | `https://api-dev.wattstap.com/v1` |
| Local | `http://localhost:8000` |

**Telegram Bot:** `wattstap_eu_bot`
**Mini App Short Name:** `app`
**Реферальная ссылка:** `https://t.me/{BotUsername}/{MiniAppShortName}?startapp=REF_{code}`

---

## Требования к бэкенд-разработчику

### Обязательный стек

**Язык:** Python (FastAPI) или Node.js (NestJS / Express)

**База данных:**
- PostgreSQL — основное хранилище (игроки, инвентарь, прогресс, апгрейды)
- Redis — кэширование, сессии, лидерборды, rate limiting, cooldown тапов

**Инфраструктура:**
- Docker + Docker Compose
- CI/CD (GitHub Actions / GitLab CI)
- Nginx / Caddy (reverse proxy)

### Обязательные навыки и опыт

1. **Telegram Bot API / Telegram Mini Apps**
   - Валидация `initData` (HMAC-SHA256)
   - Работа с Telegram WebApp SDK
   - Telegram Bot для уведомлений и команд
   - Telegram Stars / BTN оплата через Bot Payments API

2. **REST API разработка**
   - Проектирование и реализация REST API
   - JWT аутентификация и авторизация
   - Валидация входных данных
   - Обработка ошибок (FastAPI-совместимый формат: `{ detail: string }`)
   - Rate limiting и anti-abuse

3. **Игровая логика (server-authoritative)**
   - Серверная валидация тапов (anti-cheat)
   - Пакетная обработка тапов (batch processing)
   - Расчёт ресурсов, XP, уровней
   - Система апгрейдов с многоуровневой прогрессией
   - Инвентарь: экипировка, мерж, прокачка
   - Оффлайн-бонус с ограничением по времени
   - Ежедневные бонусы со стриком
   - Турнирная система

4. **Социальные функции**
   - Реферальная система с отслеживанием цепочки
   - Лидерборды (глобальный, друзья, турнирный)
   - Расчёт бонусов от друзей

5. **Платёжные системы**
   - Telegram Bot Payments (BTN / Telegram Stars)
   - Интеграция с TON blockchain (KiloWatt tokens)
   - WeChat Pay (планируется)

6. **Безопасность**
   - Anti-cheat: серверная валидация всех действий
   - Rate limiting на тапы и покупки
   - Защита от replay-атак (sessionId, clientTime)
   - Валидация Telegram initData

7. **Масштабируемость**
   - Оптимизация для высокой нагрузки (тапы = много запросов)
   - Кэширование с Redis
   - Batch processing
   - Горизонтальное масштабирование

### Желательные навыки

- **WeChat Mini Program** — аутентификация и оплата через WeChat
- **WebSocket** — реальное время для турниров и лидербордов (запланировано, но не реализовано)
- **Celery / Bull** — фоновые задачи (расчёт оффлайн-бонусов, турнирные награды)
- **Alembic / Prisma** — миграции базы данных
- **Prometheus + Grafana** — мониторинг
- **Sentry** — error tracking
- **S3 / MinIO** — хранение аватаров и ассетов
- **TON SDK** — работа с TON blockchain

---

## Объём работы (оценка)

### Фаза 1 — MVP (4-6 недель)
- [ ] Аутентификация через Telegram (initData validation + JWT)
- [ ] CRUD игрока (создание, загрузка, обновление)
- [ ] Система тапов с серверной валидацией (batch processing)
- [ ] Ресурсы: Watts, XP, Hits, уровни
- [ ] Оффлайн-бонус
- [ ] Ежедневный бонус
- [ ] Mining Balance API (конфигурация баланса)
- [ ] Базовая admin-панель

### Фаза 2 — Социальные функции (2-3 недели)
- [ ] Реферальная система
- [ ] Список друзей
- [ ] Лидерборды (глобальный, друзья)
- [ ] Реферальные бонусы

### Фаза 3 — Прогрессия (2-3 недели)
- [ ] Система апгрейдов (12 типов, многоуровневая)
- [ ] Инвентарь и экипировка
- [ ] Мерж предметов
- [ ] Прокачка предметов
- [ ] Аватары (покупка, разблокировка по уровню)

### Фаза 4 — Магазин и платежи (2-3 недели)
- [ ] Магазин (сундуки, бустеры)
- [ ] Telegram Bot Payments (BTN)
- [ ] Обработка платежей и выдача товаров

### Фаза 5 — Турниры и расширение (2-3 недели)
- [ ] Турнирная система
- [ ] Турнирные лидерборды
- [ ] Награды по рангу
- [ ] WebSocket для реального времени (опционально)

### Фаза 6 — WeChat (3-4 недели)
- [ ] Аутентификация через WeChat
- [ ] WeChat Pay интеграция
- [ ] Адаптация API для WeChat Mini Program

**Общая оценка: 15-22 недели** (1 разработчик, full-time)

---

## Архитектурные решения

### Рекомендуемая архитектура (Python / FastAPI)

```
wattstap-backend/
├── app/
│   ├── main.py                 # FastAPI application
│   ├── config.py               # Settings & environment
│   ├── database.py             # DB connection
│   ├── dependencies.py         # DI & auth dependencies
│   ├── models/                 # SQLAlchemy models
│   │   ├── player.py
│   │   ├── inventory.py
│   │   ├── upgrade.py
│   │   ├── referral.py
│   │   ├── tournament.py
│   │   └── transaction.py
│   ├── schemas/                # Pydantic schemas (DTOs)
│   │   ├── auth.py
│   │   ├── player.py
│   │   ├── gameplay.py
│   │   ├── inventory.py
│   │   ├── social.py
│   │   ├── shop.py
│   │   └── avatar.py
│   ├── routers/                # API routes
│   │   ├── auth.py
│   │   ├── player.py
│   │   ├── gameplay.py
│   │   ├── upgrades.py
│   │   ├── inventory.py
│   │   ├── social.py
│   │   ├── shop.py
│   │   ├── avatars.py
│   │   ├── tournaments.py
│   │   └── balance.py
│   ├── services/               # Business logic
│   │   ├── auth_service.py
│   │   ├── player_service.py
│   │   ├── tap_service.py
│   │   ├── upgrade_service.py
│   │   ├── inventory_service.py
│   │   ├── referral_service.py
│   │   ├── shop_service.py
│   │   ├── avatar_service.py
│   │   ├── tournament_service.py
│   │   └── payment_service.py
│   ├── core/                   # Core utilities
│   │   ├── security.py         # JWT, Telegram validation
│   │   ├── anti_cheat.py       # Anti-cheat logic
│   │   └── cache.py            # Redis cache
│   └── tasks/                  # Background tasks
│       ├── offline_bonus.py
│       └── tournament.py
├── migrations/                 # Alembic migrations
├── tests/
├── docker-compose.yml
├── Dockerfile
└── requirements.txt
```

### Ключевые технические решения

1. **Тапы — batch processing с серверной валидацией**
   - Клиент отправляет пакет из 5-10 тапов
   - Сервер проверяет: timing, rate, sessionId, энергию
   - Защита от спида и автокликеров

2. **Лидерборды — Redis Sorted Sets**
   - Быстрое обновление и выборка топов
   - Отдельные sorted sets для глобального, друзей, турниров

3. **Оффлайн-бонус — расчёт при логине**
   - Сохраняем `lastActiveAt`
   - При логине считаем разницу, ограничиваем 4 часами
   - Начисляем на основе `profitPerHour`

4. **Мультиплатформенность (Telegram + WeChat)**
   - Абстрактный слой аутентификации
   - `POST /auth/telegram` и `POST /auth/wechat` — разные провайдеры, единый JWT
   - Абстрактный слой платежей

---

## Формат ошибок API

Клиент ожидает ошибки в формате FastAPI:
```json
{
    "detail": "Error message here"
}
```

HTTP коды:
- `200` — успех
- `400` — ошибка валидации
- `401` — не авторизован
- `403` — доступ запрещён
- `404` — не найдено
- `429` — rate limit
- `500` — серверная ошибка

---

## Контакты и ресурсы

- **Клиент:** Unity 6, C#, WebGL build
- **Telegram Bot:** @wattstap_eu_bot
- **Репозиторий клиента:** WattsTapUnityClient
- **Формат данных:** JSON
- **Таймаут запросов:** 30 секунд
