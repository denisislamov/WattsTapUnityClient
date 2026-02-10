# 🎮 Upgrades Config - Быстрый старт

## ✅ Что было создано

Создана полная система конфигурации для загрузки данных из `WattsBalanceUpgrades.csv`:

### 📁 Файлы

1. **UpgradesConfig.cs** - ScriptableObject конфигурация
2. **UpgradesCsvConverter.cs** - Парсер CSV файла
3. **Editor/UpgradesConfigEditor.cs** - Редактор с кнопкой импорта
4. **UPGRADES_CONFIG_README.md** - Подробная документация

### 🎯 Возможности

- ✅ Импорт данных из CSV одной кнопкой
- ✅ 12 типов улучшений (GoldHammer, FastTime, Endurance и т.д.)
- ✅ До 53 уровней для каждого улучшения
- ✅ Валидация данных
- ✅ Статистика по всем апгрейдам
- ✅ API для получения данных в коде

## 🚀 Быстрый старт (5 шагов)

### 1️⃣ Создайте конфигурацию

В Unity:
```
ПКМ в Project → Create → WattsTap → Configs → Upgrades Config
```

Сохраните как: `UpgradesConfig`

### 2️⃣ Импортируйте данные

1. Выберите созданный `UpgradesConfig`
2. В Inspector нажмите **"Import from CSV"**
3. Подтвердите импорт

✅ Готово! Данные загружены.

### 3️⃣ Проверьте данные

Нажмите **"Validate Configuration"** для проверки.

В Inspector вы увидите статистику:
- Количество скиллов: 12
- Количество уровней каждого
- Общую стоимость всех апгрейдов

### 4️⃣ Зарегистрируйте в ConfigService

Найдите объект с `ConfigService` в сцене и добавьте `UpgradesConfig` в массив `configs`.

### 5️⃣ Используйте в коде

```csharp
// Получить конфиг
var config = ServiceLocator.Get<IConfigService>()
    .GetConfig<UpgradesConfig>();

// Получить стоимость апгрейда
long cost = config.GetUpgradeCost(UpgradeType.GoldHammer, currentLevel);

// Получить значение параметра
float value = config.GetParameterValue(UpgradeType.FastTime, level);
```

## 📊 Структура данных

### Типы апгрейдов (UpgradeType)

```csharp
public enum UpgradeType
{
    GoldHammer,         // Профит от удара
    FastTime,           // Кулдаун удара
    Endurance,          // Вместимость ударов
    CriticalChance,     // Шанс крита
    CriticalMultiplier, // Множитель крита
    WorkExperience,     // Опыт от удара
    Economist,          // Снижение цены апгрейдов
    StrongFriendship,   // Бонус от друзей
    Investor,           // Доход в час
    ItemMaster,         // Снижение цены предметов
    ShareProfit,        // Награда от профита друзей
    GoldFriends         // Награда за приглашенного друга
}
```

### API методы

```csharp
// Получить данные скилла
UpgradeSkillData GetSkillData(UpgradeType upgradeType)

// Получить данные уровня
UpgradeLevelData GetLevelData(UpgradeType upgradeType, int level)

// Получить стоимость
long GetUpgradeCost(UpgradeType upgradeType, int currentLevel)

// Получить значение параметра
float GetParameterValue(UpgradeType upgradeType, int level)

// Получить макс уровень
int GetMaxLevel(UpgradeType upgradeType)
```

## 🔍 Пример использования

```csharp
public class PlayerUpgradeSystem : MonoBehaviour
{
    private UpgradesConfig _config;
    private Dictionary<UpgradeType, int> _playerUpgrades;
    
    void Start()
    {
        // Получаем конфиг
        _config = ServiceLocator.Get<IConfigService>()
            .GetConfig<UpgradesConfig>();
        
        // Инициализируем уровни игрока
        _playerUpgrades = new Dictionary<UpgradeType, int>();
    }
    
    public bool TryPurchaseUpgrade(UpgradeType type, long playerCoins)
    {
        int currentLevel = GetUpgradeLevel(type);
        long cost = _config.GetUpgradeCost(type, currentLevel);
        
        if (playerCoins < cost)
        {
            Debug.Log($"Not enough coins. Need: {cost}, Have: {playerCoins}");
            return false;
        }
        
        // Покупаем апгрейд
        _playerUpgrades[type] = currentLevel + 1;
        
        // Получаем новое значение параметра
        float newValue = _config.GetParameterValue(type, currentLevel + 1);
        Debug.Log($"Upgraded {type} to level {currentLevel + 1}. New value: {newValue}");
        
        return true;
    }
    
    public int GetUpgradeLevel(UpgradeType type)
    {
        return _playerUpgrades.ContainsKey(type) ? _playerUpgrades[type] : 0;
    }
    
    public float GetCurrentUpgradeValue(UpgradeType type)
    {
        int level = GetUpgradeLevel(type);
        return level > 0 ? _config.GetParameterValue(type, level) : 0f;
    }
}
```

## 📈 Статистика из CSV

После импорта вы получите:

- **12 скиллов** с полными данными
- **До 53 уровней** на каждый скилл (некоторые меньше)
- **Общая стоимость всех апгрейдов**: ~27 миллиардов монет

### Примеры данных:

**Gold Hammer:**
- Стартовая стоимость: 3,000
- Стартовый параметр: 1.0
- Макс уровень: 53
- Множитель стоимости: 1.3x

**Fast Time:**
- Стартовая стоимость: 1,750
- Стартовый параметр: 0
- Макс уровень: 53

**Gold Friends:**
- Стартовая стоимость: 3,000
- Стартовый параметр: 15,000
- Макс уровень: 36 (меньше чем у других)

## 🔧 Дополнительно

### Обновление данных из CSV

Просто нажмите **"Import from CSV"** снова - данные будут перезаписаны.

### Отладка

Все сообщения логируются в Console с префиксом `[UpgradesCsvConverter]` или `[UpgradesConfig]`.

### Форматы чисел

Парсер поддерживает:
- Целые числа: `1000`, `1,000,000`
- Дробные: `1.5`, `1,5` (оба формата)
- Строка "Start" (для первого уровня)

## 📚 Дополнительная информация

Смотрите **UPGRADES_CONFIG_README.md** для полной документации.

## 🎉 Готово!

Теперь у вас есть полная система работы с апгрейдами:
- ✅ Конфигурация создана
- ✅ Данные импортированы
- ✅ API готов к использованию
- ✅ Примеры кода готовы

Можно начинать разработку системы апгрейдов! 🚀
