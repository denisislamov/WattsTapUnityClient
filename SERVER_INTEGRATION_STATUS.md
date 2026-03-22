# 🔌 Статус интеграции с Core Server API

> **Дата**: 20 марта 2026  
> **Сервер**: `https://api-dev.wattstap.energy`  
> **API клиент**: `CoreServerService` → `ICoreServerService`  
> **Аутентификация**: JWT (15 мин) + auto-refresh через cookie

---

## Обзор

Игра WattsTap мигрирует с legacy-сервера (`IReferralAPIService`, обёрнут в `#if OLD_SERVER`) на новый Core API (`ICoreServerService`). Данный документ описывает, какие системы игры **уже подключены** к новому серверу, а какие **работают только локально** (без серверной синхронизации).

---

## ✅ Подключено к новому серверу

### 1. Аутентификация (Auth)

| Эндпоинт | Метод | Описание |
|----------|-------|----------|
| `/auth/telegram` | POST | Аутентификация через Telegram initData |
| `/auth/refresh` | POST | Обновление JWT-токена (cookie-based) |
| `/auth/logout` | POST | Выход и инвалидация токенов |

**Файлы:**
- `CoreServerService.cs` — реализация всех auth-методов
- `ApplicationEntry.cs` → `AuthenticateWithCoreServer()` — вызов при получении initData от Telegram
- Автоматический refresh-токена запускается в `ApplicationEntry` после успешной аутентификации

**Статус:** ✅ Полностью работает

---

### 2. Профиль пользователя (User)

| Эндпоинт | Метод | Описание |
|----------|-------|----------|
| `/user/me` | GET | Получение профиля и событий пользователя |
| `/user/language` | PUT | Обновление языка |
| `/user/events/read` | POST | Пометить события прочитанными |

**Файлы:**
- `CoreServerService.cs` — `GetUserMe()`, `UpdateLanguage()`, `MarkEventsRead()`

**Статус:** ✅ Реализовано в API-клиенте, доступно для вызова

---

### 3. Прогресс (Progress — tap-based модель)

| Эндпоинт | Метод | Описание |
|----------|-------|----------|
| `/progress` | GET | Загрузка прогресса игрока |
| `/progress` | POST | Отправка тапов (батч `tapCount`) |
| `/progress/reset` | POST | Сброс прогресса |

**Файлы:**
- `CoreServerService.cs` — `LoadProgress()`, `SendTaps()`, `ResetProgress()`
- `ProgressSyncService.cs` — автоматическая синхронизация тапов (batch-отправка каждые N секунд)
- `PlayerService.cs` → `OnProgressLoadedFromServer()`, `OnProgressResetFromServer()` — обновление локальных данных игрока из серверного ответа
- `ApplicationEntry.cs` — после auth запускает `LoadProgress()` + `StartAutoSync()`

**Как работает:**
1. После аутентификации загружается прогресс (`GET /progress`) → PlayerService обновляет level, watts, xp
2. Каждый тап добавляется в `ProgressSyncService.AddTaps()` — локальный счётчик
3. Каждые `TapSyncInterval` секунд (по умолчанию 3 сек) накопленные тапы отправляются `POST /progress` с `tapCount`
4. Сервер возвращает обновлённое состояние (level, watts, xp, energy)

**Статус:** ✅ Полностью работает (tap-based модель)

---

### 4. Баланс майнинга (Mining Balance)

| Эндпоинт | Метод | Описание |
|----------|-------|----------|
| `/balance/mining` | GET | Публичный эндпоинт конфигурации майнинга |

**Файлы:**
- `CoreServerService.cs` — `LoadMiningBalance()`
- `MiningBalanceRemoteLoader.cs` — загружает конфиг с сервера при старте приложения
- `ApplicationEntry.cs` → `InitializeWithRemoteBalance()` — вызывается ДО инициализации всех сервисов

**Как работает:**
1. При запуске приложения загружается конфиг с сервера (`GET /balance/mining`)
2. Если сервер доступен — значения `MiningBalanceConfig` (coinsPerTap, expPerTap, energyCost и т.д.) перезаписываются серверными
3. Если сервер недоступен — используются локальные значения из CSV/ScriptableObject

**Статус:** ✅ Полностью работает, fallback на локальные значения

---

### 5. Аватары (Avatars)

| Эндпоинт | Метод | Описание |
|----------|-------|----------|
| `/avatars` | GET | Получение каталога аватаров и статуса игрока |
| `/avatars/purchase` | POST | Покупка аватара за валюту |
| `/avatars/{id}/claim` | POST | Получение бесплатного аватара (по уровню) |

**Файлы:**
- `CoreServerService.cs` — `GetAvatars()`, `PurchaseAvatar()`, `ClaimAvatar()`
- `AvatarsService.cs` — 
  - `PurchaseAvatarWithCoinsAsync()` — сначала пробует CoreServerService, fallback на локальную покупку
  - `UnlockAvatarByLevelAsync()` — сначала пробует `ClaimAvatar()`, fallback на локальный unlock
  - `PurchaseAvatarCoroutine()` — корутина покупки через новый сервер
  - `ClaimAvatarCoroutine()` — корутина claim через новый сервер

**⚠️ Примечание:** Хранение разблокированных аватаров и текущего аватара всё ещё использует `PlayerPrefs` (`UnlockedAvatarsKey`, `CurrentAvatarKey`). Серверный каталог (`GetAvatars`) загружается, но синхронизация состояния аватаров между клиентом и сервером **не полная** — нет загрузки списка купленных аватаров с сервера при старте.

**Статус:** ⚠️ Частично (покупка/claim через сервер, но хранение — PlayerPrefs)

---

### 6. Социальные функции / Рефералы (Social)

| Эндпоинт | Метод | Описание |
|----------|-------|----------|
| `/social/my-referral` | GET | Реферальный код и статистика |
| `/social/friends` | GET | Список приглашённых друзей |
| `/social/bonus/claim` | POST | Получение реферального бонуса |
| `/referral/apply` | POST | Применение реферального кода |

**Файлы:**
- `CoreServerService.cs` — `GetMyReferral()`, `GetFriends()`, `ClaimReferralBonus()`, `ApplyReferralCode()`
- `ReferralService.cs` —
  - `LoadReferralData()` — сначала CoreServerService → fallback legacy → fallback mock (Editor)
  - `LoadFriendsList()` — сначала CoreServerService → fallback legacy → fallback mock (Editor)
- `ApplicationEntry.cs` — реферальный код передаётся при аутентификации, сохраняется в `ReferralService`

**Статус:** ✅ Полностью работает

---

## ❌ НЕ подключено к серверу (работает только локально)

### 7. Каталог предметов (Game Catalog)

**Определены эндпоинты в `CoreServerConfig`, но НЕ реализованы в `CoreServerService`:**

| Эндпоинт | Метод | Описание |
|----------|-------|----------|
| `/game/items/catalog` | GET | Каталог игровых предметов |

**Текущее состояние:**
- `CatalogService.cs` — загружает предметы только из локального `CatalogConfig` (ScriptableObject)
- Комментарий в коде: _"Future extension: Can be modified to load data from a server"_
- Данные определяются в Unity Editor через конфиг

**Файлы:** `CatalogService.cs`, `CatalogConfig.cs`

**Что нужно:** Реализовать `GetItemsCatalog()` в `CoreServerService` и загрузку каталога с сервера в `CatalogService.Initialize()`

---

### 8. Инвентарь игрока (Game Inventory)

**Определены эндпоинты в `CoreServerConfig`, но НЕ реализованы в `CoreServerService`:**

| Эндпоинт | Метод | Описание |
|----------|-------|----------|
| `/game/inventory` | GET | Получение инвентаря игрока |
| `/game/inventory/equip` | POST | Экипировать предмет |
| `/game/inventory/unequip` | POST | Снять предмет |
| `/game/inventory/upgrade` | POST | Улучшить предмет |

**Текущее состояние:**
- `InventoryService.cs` — работает только локально, загружает начальные предметы из `InventoryStartConfig`
- Комментарий в коде: _"Future extension: Can sync with server by implementing LoadFromServer/SaveToServer methods"_
- Нет сериализации/персистентности — при перезапуске инвентарь сбрасывается к начальному состоянию

**Файлы:** `InventoryService.cs`, `InventoryStartConfig.cs`

**Что нужно:** Реализовать методы `GetInventory()`, `EquipItem()`, `UnequipItem()`, `UpgradeItem()` в `CoreServerService` и серверную синхронизацию в `InventoryService`

---

### 9. Улучшения (Upgrades)

**Нет серверных эндпоинтов.**

**Текущее состояние:**
- `UpgradesService.cs` — загружает данные из `UpgradesConfig` (ScriptableObject, CSV)
- Только отображение первого уровня улучшений — нет прогрессии
- Нет покупки, нет сохранения прогресса улучшений

**Файлы:** `UpgradesService.cs`, `UpgradesConfig.cs`

**Что нужно:** Эндпоинт для покупки/прогрессии улучшений, серверное хранение уровней улучшений игрока

---

### 10. Кошелёк (Wallet / TON)

**Определены эндпоинты в `CoreServerConfig`, но НЕ реализованы в `CoreServerService`:**

| Эндпоинт | Метод | Описание |
|----------|-------|----------|
| `/wallet/my-wallet` | GET | Получение данных кошелька |
| `/wallet/payload` | POST | Получение payload для подключения |
| `/wallet/validate` | POST | Валидация подключённого кошелька |

**Текущее состояние:**
- Нет `WalletService` в клиенте
- Эндпоинты определены только в конфиге

**Файлы:** Только `CoreServerConfig.cs`

**Что нужно:** Создать `IWalletService`/`WalletService`, реализовать интеграцию TON Connect, добавить методы в `CoreServerService`

---

### 11. Заказы (Orders)

**Определён эндпоинт в `CoreServerConfig`, но НЕ реализован в `CoreServerService`:**

| Эндпоинт | Метод | Описание |
|----------|-------|----------|
| `/orders/{id}` | GET | Получение заказа по ID |

**Текущее состояние:**
- Нет `OrderService` в клиенте
- Эндпоинт определён только в конфиге

**Файлы:** Только `CoreServerConfig.cs`

**Что нужно:** Создать `IOrderService`/`OrderService`, реализовать интеграцию заказов

---

### 12. Данные игрока (PlayerService — локальное хранение)

**Текущее состояние:**
- `PlayerService.cs` — создаёт `PlayerData` при каждом запуске заново
- `LoadPlayerData()` — TODO, всегда создаёт нового игрока
- `SavePlayerData()` — TODO, ничего не сохраняет
- Серверные данные (level, watts, xp) приходят через `ProgressSyncService` → `OnProgressLoadedFromServer`, но локальное сохранение отсутствует

**Файлы:** `PlayerService.cs`, `PlayerData.cs`

**Что нужно:** Реализовать `LoadPlayerData` (из серверного ответа или локального кэша) и `SavePlayerData` (локальный кэш между сессиями)

---

### 13. Бустеры магазина (Shop Boosters)

**Текущее состояние:**
- `ShopBoosterItemUIPresenterAnimation.cs` — при получении награды бустера **добавляет монеты локально** (`AddRewardLocally`)
- Без `OLD_SERVER` нет серверного вызова для начисления ресурсов
- Отсутствует эндпоинт `AddResources` в новом Core API

**Файлы:** `ShopBoosterItemUIPresenterAnimation.cs`

**Что нужно:** Определить, как бустеры должны работать с tap-based моделью. Вероятно, награда за бустер должна начисляться через серверный эндпоинт (аналог `AddResources`)

---

### 14. Дебаг ресурсов (SROptions)

**Текущее состояние:**
- `SROptions.DebugResources.cs` — без `OLD_SERVER` метод `SendAddResources` ничего не делает
- Нет аналога `AddResources` в новом CoreServerService

**Файлы:** `SROptions.DebugResources.cs`

**Что нужно:** Реализовать дебаг-эндпоинт для тестового начисления ресурсов через Core API

---

### 15. Отсутствующие системы (не реализованы ни клиент, ни сервер)

| Система | Описание |
|---------|----------|
| 🏆 Турниры (Tournaments) | Нет сервиса, нет эндпоинтов |
| 📊 Лидерборд (Leaderboard) | Нет сервиса, нет эндпоинтов |
| 🎁 Ежедневные бонусы (Daily Rewards) | Нет сервиса, нет эндпоинтов |
| 💤 Оффлайн-бонус (Offline Bonus) | Интерфейс определён в `ITapControllerService` (закомментирован), реализация отсутствует |
| 🎯 Достижения (Achievements) | Нет сервиса, нет эндпоинтов |
| 📋 Задания (Quests/Tasks) | Нет сервиса, нет эндпоинтов |
| 🎫 Battle Pass / Season Pass | Нет сервиса, нет эндпоинтов |
| 📈 Аналитика (Analytics) | Нет сервиса, нет эндпоинтов |

---

## Сводная таблица

| Система           | Сервер             | Локально             | Статус               |
|-------------------|--------------------|----------------------|----------------------|
| Auth (Telegram)   | [V]                | —                    | [V] Готово           |
| User Profile      | [V]                | —                    | [V] Готово           |
| Progress (Taps)   | [V]                | [V]                  | [V] Готово           |
| Mining Balance    | [V]                | [V] fallback         | [V] Готово           |
| Avatars           | [~] purchase/claim | [V] PlayerPrefs      | [~] Частично         |
| Social / Referral | [V]                | [V] mock (Editor)    | [V] Готово           |
| Game Catalog      | [X]                | [V] ScriptableObject | [X] Только локально  |
| Inventory         | [X]                | [V] стартовый конфиг | [X] Только локально  |
| Upgrades          | [X]                | [V] ScriptableObject | [X] Только локально  |
| Wallet (TON)      | [X]                | [X]                  | [X] Не реализовано   |
| Orders            | [X]                | [X]                  | [X] Не реализовано   |
| Player Data       | —                  | [~] нет сохранения   | [~] TODO             |
| Shop Boosters     | [X]                | [V] локально         | [X] Только локально  |
| Tournaments       | [X]                | [X]                  | [X] Не реализовано   |
| Leaderboard       | [X]                | [X]                  | [X] Не реализовано   |
| Daily Rewards     | [X]                | [X]                  | [X] Не реализовано   |
| Offline Bonus     | [X]                | [X]                  | [X] Не реализовано   |

---

## 🏗️ Эндпоинты в CoreServerConfig, но НЕ в CoreServerService

Эти URL определены в `CoreServerConfig.cs`, но метод для их вызова **не реализован** в `CoreServerService.cs`:

```
/wallet/my-wallet          — GET  — Кошелёк пользователя
/wallet/payload            — POST — Payload для подключения
/wallet/validate           — POST — Валидация кошелька
/game/items/catalog        — GET  — Каталог предметов
/game/inventory            — GET  — Инвентарь игрока
/game/inventory/equip      — POST — Экипировать предмет
/game/inventory/unequip    — POST — Снять предмет
/game/inventory/upgrade    — POST — Улучшить предмет
/orders/{id}               — GET  — Получить заказ по ID
```

---

## 🔄 Рекомендуемый порядок реализации

1. **PlayerService persistence** — кэширование данных игрока локально (PlayerPrefs/JSON) для работы между сессиями
2. **Avatars full sync** — загрузка купленных аватаров с сервера при старте, замена PlayerPrefs
3. **Game Inventory** — реализация серверных эндпоинтов в CoreServerService + синхронизация InventoryService
4. **Game Catalog** — серверная загрузка каталога вместо (или в дополнение к) локального ScriptableObject
5. **Upgrades** — серверный эндпоинт для покупки/прогрессии улучшений
6. **Wallet** — создание WalletService, интеграция TON Connect
7. **Shop Boosters** — серверный эндпоинт для начисления награды бустера
8. **Orders** — реализация системы заказов
9. **Leaderboard / Tournaments** — после стабилизации основного геймплея
10. **Daily Rewards / Offline Bonus / Achievements** — дополнительные engagement-системы


