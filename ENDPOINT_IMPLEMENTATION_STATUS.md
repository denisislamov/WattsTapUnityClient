# 📊 Статус реализации API-эндпоинтов в клиенте

> **Дата**: 27 марта 2026  
> **Сервер**: `https://api-dev.wattstap.energy`  
> **Swagger**: `https://api-dev.wattstap.energy/docs/`  
> **Всего эндпоинтов в API**: 65 (включая admin)  
> **Релевантных для клиента**: ~35 (без admin)

---

## Условные обозначения

| Символ | Значение |
|--------|----------|
| ✅ | **Реализовано** — метод есть в `ICoreServerService` / `CoreServerService`, DTO готовы, используется в игре |
| 🔌 | **API-метод готов** — метод есть в `CoreServerService`, DTO готовы, но **не интегрировано в геймплей** (нет UI/сервиса) |
| 🧪 | **Только в API Tester** — можно тестировать из Editor-окна, но нет метода в `CoreServerService` |
| ❌ | **Не реализовано** — нет ни метода, ни DTO |
| 🔧 | **Admin** — не нужно в клиенте (серверная админка) |

---

## 1. Health

| Метод | Path | Статус | Примечание |
|-------|------|--------|------------|
| `GET` | `/health` | 🧪 | Только в API Tester |
| `GET` | `/openapi.json` | ❌ | Не нужно в клиенте |
| `GET` | `/docs` | ❌ | Не нужно в клиенте |

---

## 2. Auth — Аутентификация

| Метод | Path | Статус | Файлы |
|-------|------|--------|-------|
| `POST` | `/auth/telegram` | ✅ | `CoreServerService.Authenticate()`, `ApplicationEntry.cs` |
| `POST` | `/auth/refresh` | ✅ | `CoreServerService.RefreshToken()`, auto-refresh coroutine |
| `POST` | `/auth/logout` | ✅ | `CoreServerService.Logout()` |

**Интеграция**: Полная. Auth вызывается при старте приложения, auto-refresh работает.

---

## 3. Progress — Игровой прогресс

| Метод | Path | Статус | Файлы |
|-------|------|--------|-------|
| `GET` | `/progress` | ✅ | `CoreServerService.LoadProgress()`, `PlayerService`, `ApplicationEntry` |
| `POST` | `/progress` | ✅ | `CoreServerService.SendTaps()`, `ProgressSyncService` |
| `POST` | `/progress/reset` | ✅ | `CoreServerService.ResetProgress()` |
| `GET` | `/balance/mining` | ✅ | `CoreServerService.LoadMiningBalance()`, `MiningBalanceRemoteLoader` |

**Интеграция**: Полная. Тапы отправляются батчами через `ProgressSyncService`.

---

## 4. Social — Рефералы и друзья

| Метод | Path | Статус | Файлы |
|-------|------|--------|-------|
| `GET` | `/social/my-referral` | ✅ | `CoreServerService.GetMyReferral()`, `ReferralService` |
| `GET` | `/social/friends` | ✅ | `CoreServerService.GetFriends()`, `ReferralService` |
| `POST` | `/social/bonus/claim` | ✅ | `CoreServerService.ClaimReferralBonus()` |
| `POST` | `/referral/apply` | ✅ | `CoreServerService.ApplyReferralCode()` |

**Интеграция**: Полная. Рефералы загружаются при старте, бонусы доступны в UI.

---

## 5. Avatars — Аватары

| Метод | Path | Статус | Файлы |
|-------|------|--------|-------|
| `GET` | `/avatars` | ✅ | `CoreServerService.GetAvatars()`, `AvatarsService` |
| `POST` | `/avatars/purchase` | ✅ | `CoreServerService.PurchaseAvatar()`, `AvatarsService` |
| `POST` | `/avatars/{id}/claim` | ✅ | `CoreServerService.ClaimAvatar()`, `AvatarsService` |

**Интеграция**: Частичная. Покупка/claim через сервер, но хранение — всё ещё PlayerPrefs.

---

## 6. Boosters — Бустеры 🆕

| Метод | Path | Статус | Примечание |
|-------|------|--------|------------|
| `GET` | `/game/boosters` | 🧪 | DTO: `BoosterListResponse`. Только в API Tester. Нет метода в `CoreServerService` |
| `POST` | `/game/boosters/use` | 🧪 | DTO: `BoosterUseResponse`. Только в API Tester |
| `POST` | `/game/boosters/purchase` | 🧪 | DTO: `BoosterPurchaseResponse`. Только в API Tester |

**DTO**: ✅ Готовы (`BoostersChestsDTOs.cs`)  
**CoreServerService**: ❌ Методы не реализованы  
**Геймплей**: ❌ Нет `BoosterService`, нет UI интеграции с новым API  
**Что нужно**: Создать методы в `CoreServerService` → `BoosterService` → подключить к `ShopBoosterItemUIPresenter`

---

## 7. Chests — Сундуки 🆕

| Метод | Path | Статус | Примечание |
|-------|------|--------|------------|
| `GET` | `/game/chests/catalog` | 🧪 | DTO: `ChestCatalogResponse`. Только в API Tester |
| `GET` | `/game/chests/state` | 🧪 | DTO: `ChestStateResponse`. Только в API Tester |
| `POST` | `/game/chests/open` | 🧪 | DTO: `ChestOpenResponse`. Только в API Tester |
| `POST` | `/game/chests/purchase` | 🧪 | DTO: `ChestPurchaseResponse`. Только в API Tester |

**DTO**: ✅ Готовы (`BoostersChestsDTOs.cs`)  
**CoreServerService**: ❌ Методы не реализованы  
**Геймплей**: ❌ Нет `ChestService`, нет UI  
**Что нужно**: Создать методы в `CoreServerService` → `ChestService` → UI для открытия/покупки сундуков

---

## 8. Items — Каталог предметов

| Метод | Path | Статус | Файлы |
|-------|------|--------|-------|
| `GET` | `/game/items/catalog` | 🔌 | `CoreServerService.GetCatalog()`, `CatalogService.TryLoadFromServer()` |

**DTO**: ✅ Готовы (`GameItemsDTOs.cs`)  
**CoreServerService**: ✅ Метод реализован  
**Геймплей**: ⚠️ Частично — каталог загружается с сервера при старте, но fallback на локальный `CatalogConfig`

---

## 9. Inventory — Инвентарь

| Метод | Path | Статус | Файлы |
|-------|------|--------|-------|
| `GET` | `/game/inventory` | 🔌 | `CoreServerService.GetInventory()`, `InventoryService.TryLoadFromServer()` |
| `POST` | `/game/inventory/equip` | 🔌 | `CoreServerService.EquipItem()` |
| `POST` | `/game/inventory/unequip` | 🔌 | `CoreServerService.UnequipItem()` |
| `POST` | `/game/inventory/upgrade` | 🔌 | `CoreServerService.UpgradeItem()` |

**DTO**: ✅ Готовы (`GameItemsDTOs.cs`)  
**CoreServerService**: ✅ Все методы реализованы  
**Геймплей**: ⚠️ Частично — инвентарь загружается с сервера, но equip/unequip/upgrade пока локальные

---

## 10. Wallet — TON-кошелёк

| Метод | Path | Статус | Примечание |
|-------|------|--------|------------|
| `GET` | `/wallet/my-wallet` | 🧪 | Только в API Tester. Нет метода в `CoreServerService` |
| `GET` | `/wallet/payload` | 🧪 | Только в API Tester |
| `POST` | `/wallet/validate` | 🧪 | Только в API Tester |

**DTO**: ❌ Не созданы  
**CoreServerService**: ❌ Методы не реализованы (URL в конфиге есть)  
**Геймплей**: ❌ Нет `WalletService`  
**Что нужно**: Создать `WalletDTOs.cs` → методы в `CoreServerService` → `WalletService` → TON Connect UI

---

## 11. User — Профиль пользователя

| Метод | Path | Статус | Файлы |
|-------|------|--------|-------|
| `GET` | `/user/me` | ✅ | `CoreServerService.GetUserMe()` |
| `POST` | `/user/language` | ✅ | `CoreServerService.UpdateLanguage()` |
| `POST` | `/user/events/read` | ✅ | `CoreServerService.MarkEventsRead()` |

**DTO**: ✅ Готовы (`CoreServerDTOs.cs`)  
**CoreServerService**: ✅ Все методы реализованы  
**Геймплей**: ⚠️ Методы доступны для вызова, но полная интеграция событий в UI не завершена

---

## 12. Orders — Заказы

| Метод | Path | Статус | Примечание |
|-------|------|--------|------------|
| `GET` | `/orders/{id}` | 🧪 | URL в конфиге (`OrderByIdFmt`). Только в API Tester |

**DTO**: ❌ Не созданы (`OrderDetailsResponse`)  
**CoreServerService**: ❌ Метод не реализован  
**Что нужно**: Нужен после реализации покупок бустеров/сундуков за TON

---

## 13. Dev — Отладочные (non-production)

| Метод | Path | Статус | Файлы |
|-------|------|--------|-------|
| `POST` | `/dev/add-resources` | ✅ | `CoreServerService.DevAddResources()`, `SROptions.DebugResources` |
| `POST` | `/game/dev/inventory/grant` | 🔌 | `CoreServerService.DevGrantInventoryItem()` |
| `POST` | `/game/dev/boosters/grant` | 🔌 | `CoreServerService.DevGrantBooster()` |

**DTO**: ✅ Готовы (`BoostersChestsDTOs.cs`)  
**CoreServerService**: ✅ Все методы реализованы  
**SROptions**: ✅ Debug tweak для watts/xp работает через CoreServer

---

## 14. Admin — Административные

| Метод | Path | Статус |
|-------|------|--------|
| `POST` | `/admin/test/{userId}` | 🔧 |
| `POST` | `/admin/game/config` | 🔧 |
| `GET` | `/admin/game/config/{id}` | 🔧 |
| `PUT` | `/admin/game/config/{id}` | 🔧 |
| `GET` | `/admin/game/configs` | 🔧 |
| `POST` | `/admin/user` | 🔧 |
| `POST` | `/admin/avatars` | 🔧 |
| `PATCH` | `/admin/avatars/{id}` | 🔧 |
| `POST` | `/admin/orders/{id}/fulfill` | 🔧 |
| `POST` | `/admin/orders/{id}/cancel` | 🔧 |
| `GET` | `/admin/boosters` | 🔧 |
| `POST` | `/admin/boosters` | 🔧 |
| `PATCH` | `/admin/boosters/{id}` | 🔧 |
| `DELETE` | `/admin/boosters/{id}` | 🔧 |
| `POST` | `/admin/boosters/{id}/activate` | 🔧 |
| `POST` | `/admin/boosters/{id}/deactivate` | 🔧 |
| `GET` | `/admin/chests/configs` | 🔧 |
| `POST` | `/admin/chests/configs` | 🔧 |
| `GET` | `/admin/chests/configs/{id}` | 🔧 |
| `PATCH` | `/admin/chests/configs/{id}` | 🔧 |
| ... (+16 endpoints) | `/admin/chests/*` | 🔧 |

**Примечание**: Admin-эндпоинты не нужны в игровом клиенте. Используются через отдельную админ-панель. `AdminToken` добавлен в `CoreServerConfig` на случай необходимости.

---

## 📈 Сводная таблица

| Раздел | Всего | ✅ Готово | 🔌 API only | 🧪 Tester | ❌ Нет | 🔧 Admin |
|--------|-------|----------|-------------|-----------|--------|----------|
| Health | 3 | — | — | 1 | 2 | — |
| Auth | 3 | 3 | — | — | — | — |
| Progress | 4 | 4 | — | — | — | — |
| Social | 4 | 4 | — | — | — | — |
| Avatars | 3 | 3 | — | — | — | — |
| **Boosters** | 3 | — | — | **3** | — | — |
| **Chests** | 4 | — | — | **4** | — | — |
| Items | 1 | — | 1 | — | — | — |
| Inventory | 4 | — | 4 | — | — | — |
| Wallet | 3 | — | — | **3** | — | — |
| User | 3 | 3 | — | — | — | — |
| Orders | 1 | — | — | 1 | — | — |
| Dev | 3 | 1 | 2 | — | — | — |
| Admin | ~30 | — | — | — | — | ~30 |
| **ИТОГО** | **~66** | **18** | **7** | **12** | **2** | **~30** |

### Процент покрытия (без admin):
- **Клиентских эндпоинтов**: ~35
- **Реализовано (✅ + 🔌)**: 25 (~71%)
- **Только в тестере (🧪)**: 12 (~34%)
- **Полностью в геймплее (✅)**: 18 (~51%)

---

## 🔄 Приоритеты реализации

### 🔴 Высокий приоритет
1. **Boosters** → Создать методы в `CoreServerService`, подключить к `ShopBoosterItemUIPresenter`
2. **Orders GET** → Нужен для отслеживания статуса TON-покупок

### 🟡 Средний приоритет
3. **Chests** → Полная новая система: `ChestService` + UI
4. **Inventory equip/unequip/upgrade** → Подключить серверные вызовы к локальной логике
5. **Wallet DTO + CoreServerService** → Подготовка к TON Connect

### 🟢 Низкий приоритет
6. **Wallet UI** → TON Connect интеграция
7. **User events UI** → Отображение уведомлений
8. **Orders полный flow** → Отслеживание оплаты

---

## 📁 Затронутые файлы

| Файл | Что изменено |
|------|-------------|
| `CoreServerConfig.cs` | Добавлены: `AdminToken`, Boosters/Chests/Dev endpoint paths, `HasAdminToken` |
| `ICoreServerService.cs` | Добавлены: `DevAddResources()`, `DevGrantInventoryItem()`, `DevGrantBooster()` |
| `CoreServerService.cs` | Добавлены: реализации Dev-методов |
| `BoostersChestsDTOs.cs` | **Новый**: DTO для Boosters, Chests, Dev, ShopPayment |
| `SROptions.DebugResources.cs` | Переделан: теперь использует `CoreServerService.DevAddResources()` |
| `WattsTapAPITesterWindow.cs` | Добавлены вкладки: Boosters, Chests, Dev |

---

*Документ сгенерирован 27 марта 2026*

