# Инструкция по использованию GlobalConfigs

## ✅ Что было создано

В папке `Assets/WattsTap/Scripts/Game/GlobalConfigs/` созданы следующие файлы:

### 1. **Конфигурационные модели:**

#### `MiningBalanceConfig.cs`
- Основной конфиг баланса майнинга
- Содержит все параметры из CSV файла
- Включает стартовые параметры и прогрессию по 30 дням

#### `CriticalHitConfig.cs`
- Конфигурация системы критических ударов
- Настройки визуальных и звуковых эффектов

### 2. **Утилиты:**

#### `MiningBalanceCsvConverter.cs`
- Автоматическая конвертация данных из CSV
- Поддержка различных форматов чисел
- Валидация данных

### 3. **Editor Tools:**

#### `MiningBalanceConfigEditor.cs` (в папке Editor/)
- Кастомный инспектор с GUI для импорта
- Кнопка импорта из CSV
- Валидация конфигурации
- Отображение статистики

### 4. **Документация:**

#### `README.md`
- Полная документация по использованию
- Примеры кода

#### `MiningBalanceUsageExample.cs`
- Практические примеры использования в коде

---

## 🚀 Быстрый старт

### Шаг 1: Создание конфигурации

1. В Unity Project окне: `Right Click → Create → WattsTap → Configs → Mining Balance Config`
2. Назовите файл: `MiningBalanceConfig`
3. Сохраните в папке `Assets/Resources/Configs/` (или в вашей папке с конфигами)

### Шаг 2: Импорт данных из CSV

1. Выберите созданный `MiningBalanceConfig` в Project
2. В Inspector найдите секцию **"CSV Import Tools"**
3. Путь по умолчанию: `Assets/CvsConfigs/WattsBalanceMining.csv`
4. Нажмите кнопку **"Import from CSV"**
5. Подтвердите импорт

### Шаг 3: Использование в коде

```csharp
// В PlayerService или другом сервисе
var configService = ServiceLocator.Get<IConfigService>();
var miningConfig = configService.GetConfig<MiningBalanceConfig>("MiningBalanceConfig");

// Применение стартовых параметров
_playerData.stats.incomePerTap = miningConfig.coinsPerTap;
_playerData.resources.maxHits = miningConfig.startCapacityHits;
```

---

## 📊 Структура данных

### Start Parameters (из CSV)
- `coinsPerTap` - монет за тап (1)
- `expPerTap` - опыт за тап (1)
- `startCapacityHits` - начальные удары (1500)
- `cooldownPerHitSec` - восстановление удара (2 сек)
- `critMultiplier` - множитель крита (1.2)
- `chanceCritPercent` - шанс крита (1%)
- `avgPlaytimeMinutes` - среднее время игры (15 мин)
- `tapsPerSecond` - тапов/сек (10)
- `profitPerHour` - пассивный доход/час (500)
- `maxHoursOffline` - макс часов оффлайн (3)
- `sessionsPerDay` - сессий в день (2)

### Daily Progression Data (30 дней)
Каждый день содержит:
- `playtimeSec` - время игры за сессию
- `tapsPerSession` - тапов за сессию
- `tapsPerDay` - тапов за день
- `expPerDay` - опыт за день
- `coinsFromTaps` - монеты с тапов
- `coinsFromOfflineBonus` - монеты с оффлайн бонуса
- `profitCoins` - общий профит за день
- `cumulativeProfitCoins` - накопленный профит
- `cumulativeExp` - накопленный опыт

---

## 💡 Примеры использования

### Получение данных конкретного дня
```csharp
var day5Data = miningConfig.GetDayProgression(5);
Debug.Log($"Day 5 - Taps: {day5Data.tapsPerDay}, Profit: {day5Data.profitCoins}");
```

### Расчет накопленного прогресса
```csharp
float totalProfit = miningConfig.CalculateCumulativeProfit(30);
long totalExp = miningConfig.CalculateCumulativeExp(30);
```

### Использование критических ударов
```csharp
var critConfig = configService.GetConfig<CriticalHitConfig>("CriticalHitConfig");

if (critConfig.RollCriticalHit())
{
    long critDamage = critConfig.CalculateCriticalDamage(baseDamage);
    // Показать эффект крита
}
```

---

## 🔧 Интеграция с существующей архитектурой

### PlayerService
Конфиги совместимы с текущей архитектурой:
- Используют `BaseConfig` из `WattsTap.Core.Configs`
- Работают через `ConfigService`
- Интегрируются с `PlayerService` и `MainMenuUIModel`

### Пример интеграции в PlayerService:
```csharp
public class PlayerService : IPlayerService
{
    private MiningBalanceConfig _miningConfig;
    
    public void Initialize()
    {
        var configService = ServiceLocator.Get<IConfigService>();
        _miningConfig = configService.GetConfig<MiningBalanceConfig>("MiningBalanceConfig");
        
        // Применяем стартовые параметры
        if (_miningConfig != null)
        {
            _playerData.stats.incomePerTap = _miningConfig.coinsPerTap;
            // ... другие параметры
        }
    }
}
```

---

## ⚠️ Важные замечания

1. **Namespace**: Используется `WattsTap.Scripts.Game.GlobalConfigs` (соответствует структуре папок)

2. **CSV формат**: Поддерживаются числа с запятой (европейский формат), например: `3946,8`

3. **Валидация**: После импорта используйте кнопку "Validate Configuration" для проверки

4. **Расширение**: Для добавления новых параметров:
   - Добавьте поле в `MiningBalanceConfig`
   - Обновите `ParseStartParameters()` в конвертере
   - Добавьте строку в CSV

---

## 📝 Следующие шаги

1. ✅ Создайте ScriptableObject конфиг в Unity
2. ✅ Импортируйте данные из CSV
3. ✅ Интегрируйте в `PlayerService`
4. ✅ Используйте в игровой логике
5. 🔄 При изменении CSV - переимпортируйте данные

---

## 🎯 Результат

Теперь у вас есть:
- ✅ Типобезопасные модели данных из CSV
- ✅ Автоматическая конвертация CSV → Config
- ✅ Удобный GUI для импорта в Unity Editor
- ✅ Полная интеграция с текущей архитектурой
- ✅ Примеры использования и документация

