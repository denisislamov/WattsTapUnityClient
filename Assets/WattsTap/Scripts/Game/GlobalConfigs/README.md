# Global Configs - Mining Balance Configuration

## Описание

Эта папка содержит конфигурации для балансировки системы майнинга в игре WattsTap, основанные на данных из `WattsBalanceMining.csv`.

## Структура

### Конфигурационные файлы

#### 1. **MiningBalanceConfig.cs**
Основной конфиг баланса майнинга, содержащий:
- **Стартовые параметры:**
  - `coinsPerTap` - монет за тап
  - `expPerTap` - опыт за тап
  - `startCapacityHits` - начальное количество ударов
  - `cooldownPerHitSec` - время восстановления удара
  - `critMultiplier` - множитель критического урона
  - `chanceCritPercent` - шанс крита
  - `avgPlaytimeMinutes` - среднее время игры
  - `tapsPerSecond` - тапов в секунду
  - `profitPerHour` - пассивный доход в час
  - `maxHoursOffline` - макс. часов оффлайн дохода
  - `sessionsPerDay` - сессий в день

- **Прогрессия по дням (30 дней):**
  - Время игры за сессию
  - Тапы за сессию/день
  - Опыт за день
  - Монеты с тапов и оффлайн бонуса
  - Накопленный профит и опыт

**Методы:**
- `GetDayProgression(int day)` - получить данные для конкретного дня
- `CalculateDailyProfit(int day)` - рассчитать доход за день
- `CalculateCumulativeProfit(int upToDay)` - накопленный профит
- `CalculateCumulativeExp(int upToDay)` - накопленный опыт

#### 2. **CriticalHitConfig.cs**
Конфиг системы критических ударов:
- Шанс и множитель критического урона
- Визуальные настройки (цвет, размер текста)
- Звуковые и визуальные эффекты

**Методы:**
- `RollCriticalHit(float bonusCritChance)` - проверка крита
- `CalculateCriticalDamage(long baseDamage, float bonusMultiplier)` - расчет критического урона
- `GetEffectiveCritChance(float bonusCritChance)` - эффективный шанс с бонусами
- `GetEffectiveCritMultiplier(float bonusMultiplier)` - эффективный множитель с бонусами

### Утилиты

#### **MiningBalanceCSVConverter.cs**
Конвертер данных из CSV в конфигурацию.

**Основные методы:**
- `ParseCSVToConfig(string csvText, MiningBalanceConfig config)` - парсинг CSV текста
- `LoadFromCSVFile(string csvFilePath, MiningBalanceConfig config)` - загрузка из файла

**Особенности:**
- Поддержка различных форматов чисел (с запятой и точкой)
- Обработка значений в кавычках
- Валидация и логирование ошибок

### Editor Tools

#### **MiningBalanceConfigEditor.cs**
Кастомный инспектор для удобного импорта данных из CSV.

**Функции:**
- **Import from CSV** - импорт данных из CSV файла
- **Validate Configuration** - валидация конфигурации
- Отображение статистики (дни, накопленный профит/опыт)

## Использование

### Создание конфигурации

1. В Unity: `Right Click → Create → WattsTap → Configs → Mining Balance Config`
2. Назовите файл, например: `MiningBalanceConfig`

### Импорт данных из CSV

1. Выберите созданный `MiningBalanceConfig` в Project
2. В Inspector найдите секцию **CSV Import Tools**
3. Укажите путь к CSV файлу (по умолчанию: `Assets/CvsConfigs/WattsBalanceMining.csv`)
4. Нажмите **Import from CSV**
5. Подтвердите импорт

### Использование в коде

```csharp
// Получение конфига через ConfigService
var configService = ServiceLocator.Get<IConfigService>();
var miningConfig = configService.GetConfig<MiningBalanceConfig>("MiningBalanceConfig");

// Доступ к стартовым параметрам
int coinsPerTap = miningConfig.coinsPerTap;
float critChance = miningConfig.chanceCritPercent;

// Получение данных прогрессии
var day5Data = miningConfig.GetDayProgression(5);
Debug.Log($"Day 5 profit: {day5Data.profitCoins}");

// Расчеты
float totalProfit = miningConfig.CalculateCumulativeProfit(30);
long totalExp = miningConfig.CalculateCumulativeExp(30);
```

### Интеграция с PlayerService

```csharp
public class PlayerService : IPlayerService
{
    private MiningBalanceConfig _miningConfig;
    
    public void Initialize()
    {
        var configService = ServiceLocator.Get<IConfigService>();
        _miningConfig = configService.GetConfig<MiningBalanceConfig>("MiningBalanceConfig");
        
        // Применение стартовых параметров
        _playerData.stats.incomePerTap = _miningConfig.coinsPerTap;
        _playerData.resources.maxHits = _miningConfig.startCapacityHits;
    }
    
    // Использование в игровой логике
    public void ProcessDailyRewards(int playerDay)
    {
        var dayData = _miningConfig.GetDayProgression(playerDay);
        // Начисление наград на основе прогрессии
    }
}
```

### Использование CriticalHitConfig

```csharp
var critConfig = configService.GetConfig<CriticalHitConfig>("CriticalHitConfig");

// Проверка крита
if (critConfig.RollCriticalHit())
{
    long critDamage = critConfig.CalculateCriticalDamage(baseDamage);
    // Применить критический урон
    
    if (critConfig.useCriticalEffects)
    {
        // Показать эффекты
    }
}
```

## Формат CSV

CSV файл должен иметь следующую структуру:

```csv
Start Parametrs,,,,
Coins Per tap,1,,,
Exp per Tap,1,,,
...

,Day 1,Day 2,Day 3,...
Playtime (sec),900,900,900,...
Taps per session,1950,1950,1950,...
...
```

**Важно:**
- Первая колонка - название параметра
- Вторая колонка - значение (для стартовых параметров)
- Для прогрессии: первая колонка - название метрики, остальные - значения по дням
- Поддерживаются числа с запятой (европейский формат)

## Валидация

При импорте данных автоматически выполняется валидация:
- Проверка положительных значений для критических параметров
- Проверка диапазонов (например, шанс крита 0-100%)
- Логирование предупреждений в консоль

## Расширение

Для добавления новых параметров:

1. Добавьте поле в `MiningBalanceConfig` или `DailyProgressionData`
2. Обновите `MiningBalanceCSVConverter.ParseStartParameters()` или `ParseDailyProgression()`
3. Добавьте соответствующую строку в CSV файл
4. Переимпортируйте данные

## См. также

- `PlayerService.cs` - сервис управления игроком
- `MainMenuUIModel.cs` - UI модель главного меню
- `ResourceConfig.cs` - конфигурация ресурсов
- `WattsBalanceMining.csv` - исходные данные баланса

