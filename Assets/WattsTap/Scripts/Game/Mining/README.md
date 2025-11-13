# Mining Balance Service - Документация

## 📋 Обзор

Полная система управления балансом майнинга с лимитами по дням, сохранением прогресса и интеграцией с текущей архитектурой.

## 🏗️ Созданные компоненты

### 1. **Data Persistence Layer**

#### `IDataPersistenceService` 
Абстрактный интерфейс для сохранения данных:
```csharp
void SaveData<T>(string key, T data)
T LoadData<T>(string key)
bool HasData(string key)
void DeleteData(string key)
void ClearAll()
```

#### `PlayerPrefsDataPersistenceService`
Реализация через PlayerPrefs (по умолчанию):
- Сохранение в JSON формате
- Автоматическое сохранение
- Логирование операций

#### `RestApiDataPersistenceService`
Заглушка для REST API (для будущей реализации):
- Готов к интеграции с бэкендом
- Пока выводит предупреждения

### 2. **Mining Balance System**

#### `MiningProgressData`
Модель данных прогресса игрока:
```csharp
public class MiningProgressData
{
    public int currentDay;              // Текущий день (1-30)
    public long totalTaps;              // Всего тапов
    public long totalCoinsEarned;       // Всего монет
    public long totalExpEarned;         // Всего опыта
    public int tapsToday;               // Тапов сегодня
    public float coinsEarnedToday;      // Монет сегодня
    public long expEarnedToday;         // Опыта сегодня
    public DateTime lastTapTime;        // Время последнего тапа
    public DateTime lastDayResetTime;   // Время сброса дня
}
```

#### `IMiningBalanceService`
Интерфейс сервиса управления майнингом:
- `CurrentDay` - текущий день прогрессии
- `ProgressData` - данные прогресса
- `SetDay(int day)` - установить день (для тестирования)
- `ProcessTap(out coins, out exp)` - обработать тап с лимитами
- `CanTap()` - проверить возможность тапа
- `GetRemainingTapsToday()` - оставшиеся тапы
- `ResetProgress()` - сброс прогресса
- `SaveProgress()` / `LoadProgress()` - сохранение/загрузка

#### `MiningBalanceService`
Реализация сервиса:
- ✅ Лимитирует тапы по дням из конфига
- ✅ Автоматический сброс дневного прогресса (24 часа)
- ✅ Поддержка критических ударов
- ✅ Автосохранение каждые 10 тапов
- ✅ События для UI (OnDayChanged, OnProgressChanged, OnDayLimitReached)

### 3. **Application Entry Integration**

Обновлен `ApplicationEntry.cs`:
```csharp
public enum DataPersistenceType
{
    PlayerPrefs,  // По умолчанию
    RestAPI       // Для будущей интеграции
}

[SerializeField] private DataPersistenceType _dataPersistenceType;
```

Регистрация сервисов:
1. `IDataPersistenceService` (выбор в Inspector)
2. `IMiningBalanceService` (после PlayerService)

## 🚀 Использование

### В ApplicationEntry

1. Откройте `ApplicationEntry` объект в сцене
2. В Inspector найдите секцию **"Data Persistence"**
3. Выберите тип: `PlayerPrefs` или `RestAPI`

### В коде игрока

```csharp
// Получение сервиса
var miningService = ServiceLocator.Get<IMiningBalanceService>();

// Подписка на события
miningService.OnDayChanged += OnDayChanged;
miningService.OnProgressChanged += OnProgressChanged;
miningService.OnDayLimitReached += OnDayLimitReached;

// Обработка тапа
if (miningService.CanTap())
{
    if (miningService.ProcessTap(out long coins, out long exp))
    {
        // Начислить монеты и опыт игроку
        playerService.AddWatts(coins);
        playerService.AddExperience(exp);
    }
}

// Проверка лимитов
int remaining = miningService.GetRemainingTapsToday();
Debug.Log($"Remaining taps: {remaining}");

// Получение прогрессии текущего дня
var dayData = miningService.GetCurrentDayProgression();
Debug.Log($"Max taps today: {dayData.tapsPerDay}");
```

### Установка дня (для тестирования)

```csharp
// Установить день 5
miningService.SetDay(5);

// Сбросить прогресс
miningService.ResetProgress();

// Принудительное сохранение
miningService.SaveProgress();
```

## 🎯 Интеграция с TapController

Обновите `TapControllerService` для использования лимитов:

```csharp
public void ProcessTap()
{
    var miningService = ServiceLocator.Get<IMiningBalanceService>();
    
    if (!miningService.CanTap())
    {
        // Показать сообщение о лимите
        return;
    }
    
    if (miningService.ProcessTap(out long coins, out long exp))
    {
        // Добавить монеты и опыт
        var playerService = ServiceLocator.Get<IPlayerService>();
        playerService.ResourceManager.AddResource(ResourceType.Watts, coins);
        playerService.AddExperience(exp);
        
        // Визуальные эффекты
        ShowCoinEffect(coins);
    }
}
```

## 📊 События

### OnDayChanged
Вызывается при изменении дня:
```csharp
miningService.OnDayChanged += (newDay) => 
{
    Debug.Log($"New day: {newDay}");
    UpdateUI();
};
```

### OnProgressChanged
Вызывается при изменении прогресса:
```csharp
miningService.OnProgressChanged += (progress) => 
{
    tapsCountText.text = $"{progress.tapsToday}/{maxTaps}";
};
```

### OnDayLimitReached
Вызывается при достижении лимита:
```csharp
miningService.OnDayLimitReached += (message) => 
{
    ShowPopup(message);
};
```

## 💾 Сохранение данных

### Автоматическое сохранение
- Каждые 10 тапов
- При изменении дня
- При завершении приложения (Shutdown)

### Ручное сохранение
```csharp
miningService.SaveProgress();
```

### Загрузка
Автоматически при инициализации сервиса

### Сброс
```csharp
miningService.ResetProgress(); // Удаляет все данные прогресса
```

## 🔧 Архитектура

```
ApplicationEntry
    ↓
IDataPersistenceService (выбор типа в Inspector)
    ├── PlayerPrefsDataPersistenceService
    └── RestApiDataPersistenceService
    ↓
IMiningBalanceService
    ├── Использует MiningBalanceConfig
    ├── Использует CriticalHitConfig
    └── Использует IDataPersistenceService для сохранения
```

## 📝 Порядок инициализации

1. **ConfigService** (Order: 0) - загружает конфиги
2. **DataPersistenceService** (Order: 5) - готов к сохранению/загрузке
3. **PlayerService** (Order: 10) - данные игрока
4. **MiningBalanceService** (Order: 15) - использует все предыдущие

## 🎮 Пример для UI

```csharp
public class MiningUI : MonoBehaviour
{
    private IMiningBalanceService _miningService;
    
    [SerializeField] private Text tapsRemainingText;
    [SerializeField] private Button tapButton;
    
    void Start()
    {
        _miningService = ServiceLocator.Get<IMiningBalanceService>();
        _miningService.OnProgressChanged += UpdateUI;
        
        UpdateUI(_miningService.ProgressData);
    }
    
    void UpdateUI(MiningProgressData progress)
    {
        var dayData = _miningService.GetCurrentDayProgression();
        int remaining = _miningService.GetRemainingTapsToday();
        
        tapsRemainingText.text = $"{remaining} / {dayData.tapsPerDay}";
        tapButton.interactable = remaining > 0;
    }
    
    public void OnTapButtonClick()
    {
        if (_miningService.ProcessTap(out long coins, out long exp))
        {
            // Показать эффект получения монет
            ShowCoinAnimation(coins);
        }
    }
}
```

## ⚠️ Важно

1. **MiningBalanceConfig должен быть создан**: Используйте Editor для импорта из CSV
2. **Выбор типа сохранения**: В ApplicationEntry Inspector
3. **Лимиты дня**: Автоматически применяются из конфига
4. **24-часовой сброс**: Дневные счетчики обнуляются через 24 часа

## 🔄 Миграция с REST API

Когда будет готов backend:

1. Реализуйте методы в `RestApiDataPersistenceService`
2. Добавьте async/await (UniTask)
3. Измените тип в ApplicationEntry на `RestAPI`
4. Данные автоматически начнут сохраняться на сервер

## 🐛 Отладка

Используйте Context Menu в примере:
```csharp
[ContextMenu("Set Day 5")]
[ContextMenu("Reset Progress")]
[ContextMenu("Simulate 10 Taps")]
```

Или через консоль:
```csharp
var service = ServiceLocator.Get<IMiningBalanceService>();
service.SetDay(10);
service.ResetProgress();
```

