# Player System - Модели данных и сервис игрока

## Описание

Система для управления данными игрока в игре WattsTap, включающая модели данных, статистику, инвентарь и сервис для работы с ними.

## Структура

```
Assets/WattsTap/Scripts/Game/Player/
├── Models/
│   ├── PlayerData.cs           # Основные данные игрока
│   ├── PlayerStats.cs          # Статистика игрока
│   ├── PlayerResources.cs      # Игровые ресурсы (Watts, Energy, XP)
│   └── InventoryData.cs        # Инвентарь и предметы
└── Services/
    ├── IPlayerService.cs       # Интерфейс сервиса
    └── PlayerService.cs        # Реализация сервиса
```

## Модели данных

### PlayerData
Основная модель данных игрока, содержащая:
- `playerId` - уникальный ID игрока
- `nickname` - никнейм
- `level` - текущий уровень
- `avatarUrl` - URL аватара
- `telegramUserId` - Telegram User ID
- `tonWalletAddress` - адрес TON кошелька
- `resources` - игровые ресурсы (PlayerResources)
- `stats` - статистика (PlayerStats)
- `inventory` - инвентарь (InventoryData)
- `dailyLoginStreak` - серия ежедневных входов
- `lastDailyBonusDate` - дата последнего бонуса

### PlayerResources
Игровые ресурсы:
- `watts` - основная валюта
- `currentEnergy` - текущая энергия
- `maxEnergy` - максимальная энергия
- `currentXP` - текущий опыт
- `xpToNextLevel` - опыт до следующего уровня
- `kiloWattTokens` - криптовалюта kW

### PlayerStats
Статистика игрока:
- `totalTaps` - общее количество тапов
- `totalPlayTimeSeconds` - время в игре
- `incomePerHour` - пассивный доход в час
- `incomePerTap` - доход за тап
- `friendsCount` - количество друзей
- `upgradesPurchased` - количество апгрейдов
- `chestsOpened` - открытых сундуков
- `tournamentRank` - позиция в турнире
- `bestTournamentRank` - лучшая позиция
- `lastLoginTime` - время последнего входа
- `lastLogoutTime` - время выхода

### InventoryData
Инвентарь игрока:
- `equippedWeaponId` - ID экипированного оружия
- `equippedHelmetId` - ID шлема
- `equippedArmorId` - ID брони
- `equippedBootsId` - ID обуви
- `items` - список всех предметов

### InventoryItem
Предмет в инвентаре:
- `itemId` - уникальный ID
- `type` - тип (Weapon, Helmet, Armor, Boots)
- `rarity` - редкость (Common, Uncommon, Rare, Epic, Legendary)
- `level` - уровень предмета
- `tapIncomeBonus` - бонус к доходу за тап
- `passiveIncomeBonus` - бонус к пассивному доходу
- `energyBonus` - бонус к энергии

## PlayerService

### Инициализация

Сервис регистрируется через ServiceLocator в ApplicationEntry:

```csharp
ServiceLocator.Register<IPlayerService>(new PlayerService());
```

### Основные методы

#### Управление данными
- `GetPlayerData()` - получить данные игрока
- `LoadPlayerData()` - загрузить данные из PlayerPrefs
- `SavePlayerData()` - сохранить данные
- `CreateNewPlayer(nickname, telegramUserId)` - создать нового игрока

#### Ресурсы
- `AddWatts(amount)` - добавить Watts
- `SpendWatts(amount)` - потратить Watts
- `AddExperience(amount)` - добавить опыт
- `UseEnergy(amount)` - использовать энергию
- `RestoreEnergy(amount)` - восстановить энергию

#### Геймплей
- `PerformTap()` - выполнить тап (использует энергию, даёт Watts и XP)
- `CalculateOfflineIncome()` - рассчитать оффлайн-доход (макс. 4 часа)
- `ClaimDailyBonus(out streakDay)` - получить ежедневный бонус

#### Инвентарь
- `AddItem(item)` - добавить предмет
- `EquipItem(itemId)` - экипировать предмет
- `UnequipItem(itemType)` - снять предмет
- `UpgradeItem(itemId, cost)` - улучшить предмет
- `SellItem(itemId)` - продать предмет

#### Социальные функции
- `InviteFriend()` - пригласить друга (награда: 1000 Watts)
- `UpdateTournamentRank(rank)` - обновить позицию в турнире

#### Крипто
- `ConnectWallet(walletAddress)` - подключить TON кошелёк

### События

Сервис предоставляет события для реакции на изменения:

```csharp
playerService.OnPlayerDataChanged += (playerData) => {
    // Обновить UI
};

playerService.OnResourcesChanged += (resources) => {
    // Обновить отображение ресурсов
};

playerService.OnLevelUp += (newLevel) => {
    // Показать Level Up Popup
};

playerService.OnEnergyChanged += (current, max) => {
    // Обновить энергию
};
```

## Примеры использования

### Получение данных игрока

```csharp
var playerService = ServiceLocator.Get<IPlayerService>();
var playerData = playerService.GetPlayerData();

Debug.Log($"Player: {playerData.nickname}, Level: {playerData.level}");
Debug.Log($"Watts: {playerData.resources.watts}");
```

### Выполнение тапа

```csharp
var playerService = ServiceLocator.Get<IPlayerService>();

if (playerService.PerformTap())
{
    // Тап успешен - показать эффект
}
else
{
    // Недостаточно энергии
}
```

### Покупка апгрейда

```csharp
var playerService = ServiceLocator.Get<IPlayerService>();
long upgradeCost = 1000;

if (playerService.SpendWatts(upgradeCost))
{
    // Применить апгрейд
    var playerData = playerService.GetPlayerData();
    playerData.stats.incomePerTap += 10;
    playerData.stats.upgradesPurchased++;
}
else
{
    // Недостаточно Watts
}
```

### Открытие сундука и добавление предмета

```csharp
var playerService = ServiceLocator.Get<IPlayerService>();

// Создаём новый предмет
var newItem = new InventoryItem
{
    type = ItemType.Weapon,
    rarity = ItemRarity.Rare,
    level = 1,
    tapIncomeBonus = 50,
    passiveIncomeBonus = 100,
    energyBonus = 10
};

playerService.AddItem(newItem);

// Экипируем предмет
playerService.EquipItem(newItem.itemId);
```

### Получение Daily Bonus

```csharp
var playerService = ServiceLocator.Get<IPlayerService>();

if (playerService.ClaimDailyBonus(out int streakDay))
{
    Debug.Log($"Daily bonus claimed! Day {streakDay}");
    // Показать Daily Bonus Popup
}
else
{
    Debug.Log("Already claimed today");
}
```

### Восстановление энергии (из Update)

```csharp
private IPlayerService _playerService;

void Start()
{
    _playerService = ServiceLocator.Get<IPlayerService>();
}

void Update()
{
    _playerService.UpdateEnergyRestore(Time.deltaTime);
}
```

## Сохранение и загрузка

Данные игрока автоматически:
- **Загружаются** при инициализации сервиса
- **Сохраняются** при вызове `SavePlayerData()` или `Shutdown()`

Данные хранятся в PlayerPrefs в формате JSON с ключом `"PlayerData"`.

## Оффлайн-доход

При возвращении в игру автоматически рассчитывается оффлайн-доход:
- Максимум 4 часа
- Основывается на `incomePerHour` из статистики
- Начисляется автоматически при загрузке

## Система прогрессии

### Повышение уровня
- При накоплении достаточного опыта (`currentXP >= xpToNextLevel`)
- Награда: `level * 100` Watts
- Восстановление энергии до максимума
- Формула XP для следующего уровня: `100 * level^1.5`

### Daily Login Streak
- Максимум 7 дней подряд
- Сброс при пропуске дня
- Награда растёт: `день * 500` Watts

## Интеграция с UI

События сервиса упрощают интеграцию с UI:

```csharp
public class PlayerUI : MonoBehaviour
{
    private IPlayerService _playerService;
    
    void Start()
    {
        _playerService = ServiceLocator.Get<IPlayerService>();
        
        _playerService.OnResourcesChanged += UpdateResourcesUI;
        _playerService.OnLevelUp += ShowLevelUpPopup;
        _playerService.OnEnergyChanged += UpdateEnergyBar;
    }
    
    void OnDestroy()
    {
        _playerService.OnResourcesChanged -= UpdateResourcesUI;
        _playerService.OnLevelUp -= ShowLevelUpPopup;
        _playerService.OnEnergyChanged -= UpdateEnergyBar;
    }
    
    void UpdateResourcesUI(PlayerResources resources)
    {
        // Обновить отображение Watts, XP
    }
    
    void ShowLevelUpPopup(int newLevel)
    {
        // Показать Level Up Popup
    }
    
    void UpdateEnergyBar(int current, int max)
    {
        // Обновить энергию
    }
}
```

