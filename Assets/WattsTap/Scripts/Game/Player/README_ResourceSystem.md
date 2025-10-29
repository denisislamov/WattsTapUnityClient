# Система управления ресурсами (Resource System)

## Обзор

Система управления ресурсами предоставляет инфраструктуру для работы с игровыми ресурсами (энергия, монеты, опыт и т.д.). Система интегрирована в существующий `PlayerService` и дополняет его функциональность.

## Архитектура

### Основные компоненты

1. **ResourceType** (enum) - типы ресурсов
   - Watts - основная валюта
   - Energy - энергия для тапов
   - Experience - опыт игрока
   - KiloWatt - криптовалюта
   - Premium - премиум валюта (для будущего)

2. **IResourceManager** - интерфейс менеджера ресурсов
   - Добавление/трата ресурсов
   - Проверка достаточности
   - Атомарные транзакции
   - События изменения ресурсов

3. **ResourceManager** - реализация менеджера
   - Работает с существующим `PlayerResources`
   - Управляет максимальными значениями (для энергии)
   - Логирует все транзакции
   - Валидация операций

4. **ResourceTransaction** - результат транзакции
   - Успех/неудача операции
   - Предыдущее/новое значение
   - Причина неудачи
   - Временная метка

5. **ResourceConfig** - ScriptableObject конфигурация
   - Параметры энергии
   - Параметры опыта и левелапа
   - Настройки тапов
   - Оффлайн доход

## Использование

### Доступ к ResourceManager

```csharp
// Через PlayerService
var playerService = ServiceLocator.Get<IPlayerService>();
var resourceManager = playerService.ResourceManager;
```

### Базовые операции

```csharp
// Добавить ресурс
resourceManager.AddResource(ResourceType.Watts, 100);

// Потратить ресурс
var transaction = resourceManager.SpendResource(ResourceType.Watts, 50);
if (transaction.Success)
{
    Debug.Log("Успешно потрачено!");
}

// Проверить достаточность
if (resourceManager.HasEnough(ResourceType.Energy, 10))
{
    // Выполнить действие
}

// Получить текущее значение
long currentWatts = resourceManager.GetResource(ResourceType.Watts);
```

### Атомарные транзакции

```csharp
// Потратить несколько ресурсов одновременно (все или ничего)
bool success = resourceManager.TrySpendMultiple(
    (ResourceType.Watts, 100),
    (ResourceType.Energy, 10)
);
```

### События

```csharp
// Подписка на изменения ресурсов
resourceManager.OnResourceChanged += (type, oldValue, newValue) =>
{
    Debug.Log($"{type}: {oldValue} -> {newValue}");
};

// Подписка на транзакции
resourceManager.OnResourceTransaction += (transaction) =>
{
    if (!transaction.Success)
    {
        Debug.LogWarning($"Транзакция не удалась: {transaction.Reason}");
    }
};
```

### Работа через PlayerService

```csharp
var playerService = ServiceLocator.Get<IPlayerService>();

// Методы PlayerService используют ResourceManager внутри
playerService.AddWatts(100);
playerService.SpendWatts(50);
playerService.AddExperience(10);
playerService.UseEnergy(1);
playerService.PerformTap(); // Комплексная операция с несколькими ресурсами
```

## Интеграция с существующей системой

Система **НЕ заменяет** старую архитектуру, а **дополняет** её:

1. `PlayerResources` остаётся источником данных
2. `PlayerService` использует `ResourceManager` внутри
3. Все существующие методы `PlayerService` работают как раньше
4. `ResourceManager` добавляет:
   - Централизованную валидацию
   - Транзакционный подход
   - Единообразное логирование
   - Гибкую систему событий

## Конфигурация

Создайте `ResourceConfig` ScriptableObject:

```
Assets/Resources/Configs/ResourceConfig.asset
```

Параметры:
- `baseMaxEnergy` - базовая максимальная энергия (100)
- `energyRestoreInterval` - время восстановления 1 энергии (1 сек)
- `baseIncomePerTap` - базовый доход за тап (1)
- `xpPerTap` - опыт за тап (1)
- `energyCostPerTap` - стоимость тапа в энергии (1)
- `maxOfflineIncomeHours` - макс часов оффлайн дохода (4)
- `offlineIncomeMultiplier` - процент от дохода (0.5)
- `levelUpWattsMultiplier` - множитель награды за уровень (100)

## Преимущества

1. **Централизация** - вся логика ресурсов в одном месте
2. **Валидация** - проверки перед каждой операцией
3. **Транзакции** - информация о результате каждой операции
4. **События** - реактивная модель для UI
5. **Гибкость** - легко добавить новые типы ресурсов
6. **Отладка** - логирование всех операций
7. **Атомарность** - поддержка сложных транзакций

## Расширение

Для добавления нового типа ресурса:

1. Добавить значение в `ResourceType` enum
2. Добавить поле в `PlayerResources` (если нужно сохранение)
3. Обновить `GetResource()` и `SetResourceInternal()` в `ResourceManager`
4. (Опционально) Добавить методы-обёртки в `IPlayerService`

## Файлы системы

- `ResourceType.cs` - enum типов ресурсов
- `ResourceTransaction.cs` - результат транзакции
- `IResourceManager.cs` - интерфейс менеджера
- `ResourceManager.cs` - реализация менеджера
- `ResourceConfig.cs` - ScriptableObject конфигурация
- `PlayerService.cs` - интеграция в существующий сервис
- `ResourceSystemExample.cs` - примеры использования

## Примеры использования

Полные примеры кода можно найти в файле `ResourceSystemExample.cs`.

