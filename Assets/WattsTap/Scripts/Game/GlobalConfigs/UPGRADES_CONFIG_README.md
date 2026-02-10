# Upgrades Config - Конфигурация улучшений

## Описание

Конфигурация для загрузки и управления данными об улучшениях (апгрейдах) из файла `WattsBalanceUpgrades.csv`.

## Файлы

- **UpgradesConfig.cs** - ScriptableObject конфигурация с данными всех улучшений
- **UpgradesCsvConverter.cs** - Утилита для парсинга CSV файла
- **Editor/UpgradesConfigEditor.cs** - Custom Editor с кнопкой импорта CSV

## Как использовать

### 1. Создание конфигурации

1. В Unity, в окне Project, нажмите правой кнопкой мыши
2. Выберите: `Create > WattsTap > Configs > Upgrades Config`
3. Назовите файл, например `UpgradesConfig`

### 2. Импорт данных из CSV

1. Выберите созданный ScriptableObject в Project
2. В Inspector вы увидите кнопку **"Import from CSV"**
3. При необходимости измените путь к CSV файлу (по умолчанию: `Assets/CvsConfigs/WattsBalanceUpgrades.csv`)
4. Нажмите **"Import from CSV"**
5. Подтвердите импорт

После импорта конфигурация будет содержать все данные из CSV файла.

### 3. Валидация конфигурации

После импорта можно нажать кнопку **"Validate Configuration"** для проверки корректности данных.

## Структура данных

### UpgradeType (Enum)

Типы доступных улучшений:

- **GoldHammer** - Увеличивает профит от каждого удара
- **FastTime** - Уменьшает кулдаун каждого удара
- **Endurance** - Увеличивает вместимость ударов
- **CriticalChance** - Увеличивает шанс критического удара
- **CriticalMultiplier** - Увеличивает множитель критического урона
- **WorkExperience** - Увеличивает опыт от каждого удара
- **Economist** - Уменьшает цену апгрейдов
- **StrongFriendship** - Увеличивает бонус от друзей
- **Investor** - Увеличивает доход в час
- **ItemMaster** - Уменьшает цену предметов для повышения уровня
- **ShareProfit** - Увеличивает награду от профита друзей
- **GoldFriends** - Увеличивает награду за приглашенного друга

### UpgradeSkillData

Данные одного типа улучшения:

```csharp
public class UpgradeSkillData
{
    public UpgradeType upgradeType;       // Тип апгрейда
    public string title;                   // Название
    public string description;             // Описание эффекта
    public float stepCostMultiplier;       // Множитель стоимости
    public float stepParameter;            // Прирост параметра
    public long startCost;                 // Стартовая стоимость
    public float startParameter;           // Стартовое значение
    public List<UpgradeLevelData> levels;  // Уровни
}
```

### UpgradeLevelData

Данные одного уровня улучшения:

```csharp
public class UpgradeLevelData
{
    public int level;         // Номер уровня
    public long price;        // Стоимость покупки
    public float parameter;   // Значение параметра
}
```

## Использование в коде

### Получение данных улучшения

```csharp
// Получить конфигурацию через ConfigService
var config = ServiceLocator.Get<IConfigService>()
    .GetConfig<UpgradesConfig>();

// Получить данные конкретного скилла
var goldHammerData = config.GetSkillData(UpgradeType.GoldHammer);

// Получить данные уровня
var levelData = config.GetLevelData(UpgradeType.GoldHammer, 5);

// Получить стоимость улучшения
long cost = config.GetUpgradeCost(UpgradeType.FastTime, currentLevel);

// Получить значение параметра на уровне
float paramValue = config.GetParameterValue(UpgradeType.Endurance, level);

// Получить максимальный уровень
int maxLevel = config.GetMaxLevel(UpgradeType.CriticalChance);
```

### Пример использования

```csharp
public class UpgradeService : IUpgradeService
{
    private UpgradesConfig _config;
    
    public void Initialize()
    {
        _config = ServiceLocator.Get<IConfigService>()
            .GetConfig<UpgradesConfig>();
    }
    
    public bool CanAffordUpgrade(UpgradeType type, int currentLevel, long playerCoins)
    {
        long cost = _config.GetUpgradeCost(type, currentLevel);
        return playerCoins >= cost;
    }
    
    public void PurchaseUpgrade(UpgradeType type, int currentLevel)
    {
        var levelData = _config.GetLevelData(type, currentLevel + 1);
        if (levelData != null)
        {
            // Применяем улучшение
            ApplyUpgradeEffect(type, levelData.parameter);
        }
    }
}
```

## Формат CSV

CSV файл должен иметь следующую структуру:

- **Строка 1**: TAB, BATTERY (категория)
- **Строка 2**: SKILL TITTLE - названия скиллов
- **Строка 3**: DESCRIPTION - описания
- **Строка 4**: ICON (не используется)
- **Строка 5**: PARAMETRS AND PRICE - заголовок
- **Строка 6**: step_cost, step_parameter - множители прогрессии
- **Строка 7**: start_cost, start_parameter - стартовые значения
- **Строка 8**: LEVEL, PRICE, PARAMETR... - заголовок данных
- **Строки 9+**: данные уровней (LEVEL, PRICE1, PARAM1, PRICE2, PARAM2, ...)

## Регистрация в ConfigService

Не забудьте зарегистрировать созданный ScriptableObject в ConfigService:

1. Найдите объект со скриптом `ConfigService` в сцене
2. В Inspector добавьте созданный `UpgradesConfig` в массив `configs`
3. Убедитесь что имя конфига установлено корректно (например "UpgradesConfig")

Теперь конфиг будет доступен через:

```csharp
var config = ServiceLocator.Get<IConfigService>()
    .GetConfig<UpgradesConfig>("UpgradesConfig");
```

## Особенности

- Парсер автоматически обрабатывает европейский формат чисел (запятая как разделитель дробной части)
- Поддерживается пропуск пустых строк и данных
- Некоторые скиллы могут иметь меньше уровней чем другие (например, Gold Friends)
- Все данные валидируются при импорте

## Связанные файлы

Аналогичные конфигурации:
- `MiningBalanceConfig.cs` - для WattsBalanceMining.csv
- `PlayerLevelConfig.cs` - для WattsBalanceLevelupProfile.csv
