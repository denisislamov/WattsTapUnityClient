# Mining Balance System - Итоговая документация

## ✅ Что было создано

### 1. **Data Persistence Layer** (`Assets/WattsTap/Scripts/Core/DataPersistence/`)

#### Интерфейс сохранения данных
- **IDataPersistenceService.cs** - абстрактный интерфейс для сохранения/загрузки данных
- **PlayerPrefsDataPersistenceService.cs** - реализация через PlayerPrefs (готова к использованию)
- **RestApiDataPersistenceService.cs** - заглушка для REST API (для будущей интеграции)

### 2. **Mining Balance System** (`Assets/WattsTap/Scripts/Game/Mining/`)

#### Модели данных
- **MiningProgressData.cs** - модель прогресса игрока (тапы, монеты, опыт, текущий день)
- **IMiningBalanceService.cs** - интерфейс сервиса управления балансом
- **MiningBalanceService.cs** - реализация сервиса с:
  - ✅ Лимитами тапов по дням из конфига
  - ✅ Автоматическим сбросом дневного прогресса (24 часа)
  - ✅ Поддержкой критических ударов
  - ✅ Автосохранением каждые 10 тапов
  - ✅ События для UI (OnDayChanged, OnProgressChanged, OnDayLimitReached)
- **README.md** - полная документация системы

### 3. **Application Entry Integration**

Обновлен **ApplicationEntry.cs**:
- Добавлен enum `DataPersistenceType` для выбора типа сохранения
- Добавлен параметр `[SerializeField] DataPersistenceType _dataPersistenceType` в Inspector
- Автоматическая регистрация выбранного сервиса сохранения
- Регистрация `IMiningBalanceService` с правильным порядком инициализации

### 4. **Примеры и документация**

- **MiningBalanceUsageExample.cs** - обновленные примеры использования с Context Menu
- **README.md** (в Mining/) - полная документация по использованию системы

## 🎯 Ключевые возможности

### Лимитирование данных по дням
```csharp
var miningService = ServiceLocator.Get<IMiningBalanceService>();

// Текущий день и лимиты
int currentDay = miningService.CurrentDay; // 1-30
var dayData = miningService.GetCurrentDayProgression();
int maxTapsToday = dayData.tapsPerDay; // Из конфига
int remaining = miningService.GetRemainingTapsToday();
```

### Установка дня (для тестирования)
```csharp
miningService.SetDay(5); // Установить день 5
```

### Сохранение и сброс данных
```csharp
// Автоматическое сохранение каждые 10 тапов
miningService.ProcessTap(out long coins, out long exp);

// Ручное сохранение
miningService.SaveProgress();

// Полный сброс прогресса
miningService.ResetProgress();
```

### Выбор типа сохранения
В Unity Inspector на объекте ApplicationEntry:
- **Data Persistence Type**: PlayerPrefs или RestAPI
- Переключается без изменения кода
- Готово к интеграции с backend

## 🔄 Архитектура

```
ApplicationEntry (Inspector: выбор типа сохранения)
    ↓
IDataPersistenceService (Order: 5)
    ├── PlayerPrefsDataPersistenceService ✅ Работает
    └── RestApiDataPersistenceService ⏳ Заглушка
    ↓
MiningBalanceService (Order: 15)
    ├── Использует MiningBalanceConfig (CSV → Config)
    ├── Использует CriticalHitConfig
    ├── Использует IDataPersistenceService
    └── Лимитирует игрока по текущему дню
```

## 📝 Порядок инициализации сервисов

1. **ConfigService** (0) - загружает конфиги
2. **DataPersistenceService** (5) - готов к сохранению
3. **PlayerService** (10) - данные игрока
4. **MiningBalanceService** (15) - использует все предыдущие

## 🚀 Быстрый старт

### Шаг 1: Выбор типа сохранения
1. Откройте сцену с `ApplicationEntry`
2. В Inspector найдите секцию **"Data Persistence"**
3. Выберите `PlayerPrefs` (по умолчанию) или `RestAPI`

### Шаг 2: Использование в игре
```csharp
// Получение сервиса
var miningService = ServiceLocator.Get<IMiningBalanceService>();

// Подписка на события
miningService.OnProgressChanged += UpdateUI;
miningService.OnDayLimitReached += ShowLimitMessage;

// Обработка тапа
if (miningService.CanTap())
{
    if (miningService.ProcessTap(out long coins, out long exp))
    {
        playerService.ResourceManager.AddResource(ResourceType.Watts, coins);
        playerService.AddExperience(exp);
    }
}
```

### Шаг 3: Интеграция в TapController
```csharp
public void OnTap()
{
    var miningService = ServiceLocator.Get<IMiningBalanceService>();
    
    if (!miningService.CanTap())
    {
        // Показать: "Дневной лимит достигнут!"
        return;
    }
    
    if (miningService.ProcessTap(out long coins, out long exp))
    {
        // Добавить награды
        // Показать эффекты
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
    ResetDailyUI();
};
```

### OnProgressChanged
Вызывается при каждом тапе:
```csharp
miningService.OnProgressChanged += (progress) => 
{
    tapsText.text = $"{progress.tapsToday}/{maxTaps}";
    coinsText.text = $"{progress.totalCoinsEarned}";
};
```

### OnDayLimitReached
Вызывается при достижении лимита:
```csharp
miningService.OnDayLimitReached += (message) => 
{
    ShowPopup("Daily tap limit reached!");
};
```

## 🎮 Context Menu для отладки

В `MiningBalanceUsageExample` доступны команды:
- **Set Day 1/5/30** - установить день
- **Reset Progress** - сбросить весь прогресс
- **Save Progress** - сохранить вручную
- **Simulate 10 Taps** - симулировать 10 тапов

## 🔧 Миграция на REST API

Когда backend будет готов:

1. Реализуйте методы в `RestApiDataPersistenceService`
2. Добавьте async/await (UniTask)
3. В ApplicationEntry выберите `RestAPI`
4. Данные автоматически начнут сохраняться на сервер

Пример будущей реализации:
```csharp
public async void SaveData<T>(string key, T data)
{
    var json = JsonUtility.ToJson(data);
    await apiClient.PostAsync($"/player/save/{key}", json);
}
```

## ✨ Особенности реализации

1. **Автоматический сброс через 24 часа** - дневные счетчики обнуляются
2. **Лимиты из конфига** - все значения берутся из MiningBalanceConfig
3. **Критические удары** - автоматически применяются из CriticalHitConfig
4. **Автосохранение** - каждые 10 тапов + при изменении дня + при Shutdown
5. **События для UI** - реактивное обновление интерфейса
6. **Типобезопасность** - все через IService и конфиги

## 📦 Созданные файлы

```
Core/
└── DataPersistence/
    ├── IDataPersistenceService.cs
    ├── PlayerPrefsDataPersistenceService.cs
    └── RestApiDataPersistenceService.cs

Game/
├── ApplicationEntry.cs (обновлен)
├── Mining/
│   ├── IMiningBalanceService.cs
│   ├── MiningBalanceService.cs
│   ├── MiningProgressData.cs
│   └── README.md
└── GlobalConfigs/
    ├── MiningBalanceConfig.cs
    ├── CriticalHitConfig.cs
    ├── MiningBalanceCsvConverter.cs
    ├── MiningBalanceUsageExample.cs (обновлен)
    ├── README.md
    ├── USAGE_GUIDE.md
    └── Editor/
        └── MiningBalanceConfigEditor.cs
```

## 🎯 Итог

Создана полная система управления балансом майнинга:
- ✅ Лимиты по дням из CSV конфига
- ✅ Установка дня для тестирования
- ✅ Абстрактная система сохранения данных
- ✅ Реализация через PlayerPrefs (работае��)
- ✅ Заглушка для REST API (готова к расширению)
- ✅ Выбор типа сохранения в ApplicationEntry Inspector
- ✅ Полная интеграция с текущей архитектурой (IService, BaseConfig)
- ✅ События для реактивного UI
- ✅ Автосохранение и 24-часовой сброс
- ✅ Примеры использования и документация

Все готово к использованию! 🚀

