# Mock API - Руководство по использованию

## 📋 Обзор

Mock API Service позволяет тестировать и разрабатывать игру в Unity Editor без реального бэкенда. Все серверные ответы симулируются локально с настраиваемыми параметрами.

## 🚀 Быстрый старт

### 1. Регистрация Mock сервиса

В `ApplicationEntry.cs` замените реальный API сервис на Mock:

```csharp
using WattsTap.Core.API;

public class ApplicationEntry : MonoBehaviour
{
    [SerializeField] private MockAPIConfig mockConfig; // Опционально
    
    private void Awake()
    {
        #if UNITY_EDITOR
            // В редакторе используем Mock
            ServiceLocator.Register<IWattsTapAPIService>(
                new WattsTapAPIMockService(mockConfig)
            );
        #else
            // В билде используем реальный API
            ServiceLocator.Register<IWattsTapAPIService>(
                new WattsTapAPIService()
            );
        #endif
        
        ServiceLocator.Instance.InitializeAll();
    }
}
```

### 2. Создание конфигурации (опционально)

1. В Project окне: `Right Click → Create → WattsTap → API → Mock API Config`
2. Настройте параметры в Inspector
3. Присвойте конфиг в ApplicationEntry

## 🎮 Использование Editor Window

### Открытие панели управления

`Menu: WattsTap → Mock API Control Panel`

### Возможности панели

#### Status Section
- **Service Status**: Показывает, запущен ли Mock сервис
- **Mode**: MOCK (редактор) или PRODUCTION (билд)

#### Player Data Section
Отображает текущее состояние игрока:
- Player ID, Nickname, Level
- Resources (Watts, XP, Hits, Energy)
- Statistics (Total Taps, Income, etc.)

#### Modify Resources Section
Быстрое изменение ресурсов:
- **Add Watts**: Добавить определенное количество Watts
- **Add XP**: Добавить опыт
- **Set Level**: Установить уровень игрока
- **Restore Hits**: Восстановить удары до максимума

#### Simulation Settings
Контроль поведения мока:
- **Simulate Network Delay**: Включить/выключить задержку сети
  - Min/Max Delay: Диапазон задержки (0-5 сек)
- **Error Chance**: Вероятность случайных ошибок (0-100%)
  - Полезно для тестирования error handling

#### Quick Actions
Быстрые действия для тестирования:
- 💰 **Add 10K Watts**: +10,000 Watts
- ⚡ **Add 1K XP**: +1,000 XP
- 🔄 **Reset Player Data**: Сброс до начальных значений
- ❤️ **Restore Hits**: Восстановление ударов
- 📈 **Level Up**: Повысить уровень на 1
- 💎 **Add Million Watts**: +1,000,000 Watts

## ⚙️ MockAPIConfig параметры

### Network Simulation
- `simulateNetworkDelay` - Включить задержку сети
- `minDelay` / `maxDelay` - Диапазон задержки (секунды)

### Error Simulation
- `errorChance` - Вероятность ошибки (0.0 - 1.0)
- `possibleErrors` - Типы ошибок для симуляции
  - NetworkTimeout
  - ServerError
  - InvalidToken
  - RateLimitExceeded
  - InsufficientResources

### Mock Player Data
Начальные значения при инициализации:
- `startingWatts` - Стартовые Watts (default: 10,000)
- `startingLevel` - Начальный уровень (default: 5)
- `startingHits` / `maxHits` - Удары (default: 18/20)
- `incomePerTap` - Доход за тап (default: 50)
- `incomePerHour` - Пассивный доход (default: 1,000)

### Offline Simulation
- `offlineHours` - Симулировать время оффлайн (0-24 часа)

### Gameplay Tweaks
- `xpMultiplier` - Множитель опыта (1-10x)
  - Для быстрого тестирования level up
- `autoLevelUp` - Автоматический level up при достижении XP
- `infiniteHits` - Бесконечные удары (не тратятся)

### Logging
- `verboseLogging` - Подробные логи в консоль
- `logResponseTime` - Показывать время ответа в миллисекундах

## 📝 Примеры использования

### Пример 1: Быстрое тестирование Level Up

```csharp
// Создайте MockAPIConfig asset
// Установите:
mockConfig.xpMultiplier = 10f;
mockConfig.autoLevelUp = true;

// Теперь при тапах XP будет начисляться в 10 раз быстрее
// Level up будет происходить автоматически
```

### Пример 2: Тестирование ошибок сети

```csharp
mockConfig.errorChance = 0.3f; // 30% ошибок
mockConfig.possibleErrors = new[] {
    MockAPIConfig.ErrorType.NetworkTimeout,
    MockAPIConfig.ErrorType.ServerError
};

// Теперь 30% запросов будут возвращать случайные ошибки
// Проверьте, что ваш UI правильно обрабатывает ошибки
```

### Пример 3: Тестирование медленной сети

```csharp
mockConfig.simulateNetworkDelay = true;
mockConfig.minDelay = 1f;
mockConfig.maxDelay = 3f;

// Все запросы будут иметь задержку 1-3 секунды
// Проверьте loading индикаторы и UX
```

### Пример 4: Тестирование оффлайн наград

```csharp
mockConfig.offlineHours = 4f; // Симулируем 4 часа оффлайн

// При старте игры и вызове ClaimOfflineBonus()
// Игрок получит награды за 4 часа
```

### Пример 5: Бесконечные ресурсы для UI тестирования

```csharp
mockConfig.infiniteHits = true;
mockConfig.startingWatts = 1000000;

// Удары не будут тратиться
// Много стартовых Watts для тестирования покупок
```

## 🔧 Программное управление

### Из кода можно управлять Mock сервисом:

```csharp
var mockService = ServiceLocator.Get<IWattsTapAPIService>() as WattsTapAPIMockService;

if (mockService != null)
{
    // Добавить ресурсы
    mockService.AddWatts(5000);
    mockService.AddXP(1000);
    
    // Изменить уровень
    mockService.SetLevel(10);
    
    // Восстановить удары
    mockService.RestoreHits();
    
    // Настроить симуляцию
    mockService.SetErrorChance(0.2f); // 20% ошибок
    mockService.simulateNetworkDelay = true;
    mockService.minDelay = 0.5f;
    mockService.maxDelay = 1.5f;
    
    // Сброс данных
    mockService.ResetPlayerData();
    
    // Получить текущие данные
    var playerData = mockService.GetCurrentMockData();
    Debug.Log($"Current level: {playerData.level}");
}
```

## 🎯 Best Practices

### 1. Используйте разные конфиги для разных сценариев

Создайте несколько конфигов:
- `MockAPI_Normal.asset` - Обычное тестирование
- `MockAPI_FastLevelUp.asset` - Быстрый XP для тестирования прогрессии
- `MockAPI_SlowNetwork.asset` - Медленная сеть
- `MockAPI_ErrorProne.asset` - Много ошибок для стресс-теста

### 2. Тестируйте разные состояния

```csharp
// Тест: новый игрок
mockService.ResetPlayerData();
mockService.SetLevel(1);
mockService.AddWatts(0);

// Тест: опытный игрок
mockService.SetLevel(50);
mockService.AddWatts(10000000);

// Тест: нет ресурсов
mockService.AddWatts(-mockService.GetCurrentMockData().resources.watts);
```

### 3. Комбинируйте с Unity Test Framework

```csharp
[Test]
public void TestTapBatch_WithInsufficientHits()
{
    var mockService = new WattsTapAPIMockService();
    mockService.Initialize();
    
    // Установить 0 ударов
    var playerData = mockService.GetCurrentMockData();
    playerData.resources.currentHits = 0;
    
    // Попробовать тапнуть
    bool errorOccurred = false;
    StartCoroutine(mockService.SendTapBatch(
        new List<TapData> { new TapData() },
        onSuccess: (response) => {
            Assert.AreEqual(0, response.validTapsCount);
        },
        onError: (error) => {
            errorOccurred = true;
        }
    ));
}
```

### 4. Логирование для дебага

```csharp
mockConfig.verboseLogging = true;
mockConfig.logResponseTime = true;

// Теперь в консоли будут подробные логи:
// [MockAPI] ✓ SendTapBatch: 5 taps, +250 watts, +25 XP (127ms)
```

## 🔄 Переключение между Mock и Real API

### Автоматическое (рекомендуется)

```csharp
#if UNITY_EDITOR
    var apiService = new WattsTapAPIMockService(mockConfig);
#else
    var apiService = new WattsTapAPIService();
#endif

ServiceLocator.Register<IWattsTapAPIService>(apiService);
```

### Через ScriptableObject флаг

```csharp
[CreateAssetMenu(menuName = "WattsTap/Build Config")]
public class BuildConfig : ScriptableObject
{
    public bool useMockAPI;
}

// В ApplicationEntry:
[SerializeField] private BuildConfig buildConfig;

if (buildConfig.useMockAPI)
{
    ServiceLocator.Register<IWattsTapAPIService>(new WattsTapAPIMockService());
}
else
{
    ServiceLocator.Register<IWattsTapAPIService>(new WattsTapAPIService());
}
```

## 📊 Отладка

### Console логи

Mock API выводит цветные логи:
- ✓ (зеленый) - Успешный запрос
- ✗ (красный) - Ошибка
- 🎉 - Level Up
- ℹ️ - Информация

### Формат логов

```
[MockAPI] ✓ SendTapBatch: 10 taps, +500 watts, +50 XP (245ms)
[MockAPI] 🎉 LEVEL UP! New level: 6
[MockAPI] ✗ ClaimOfflineBonus: Network timeout. Please check your connection.
```

### Включение/выключение логов

```csharp
mockConfig.verboseLogging = false; // Отключить большинство логов
// Ошибки все равно будут выводиться
```

## 🚀 Продакшен

### Убедитесь перед билдом:

```csharp
// ❌ Плохо - Mock будет в билде
ServiceLocator.Register<IWattsTapAPIService>(new WattsTapAPIMockService());

// ✅ Хорошо - Conditional compilation
#if UNITY_EDITOR
    ServiceLocator.Register<IWattsTapAPIService>(new WattsTapAPIMockService());
#else
    ServiceLocator.Register<IWattsTapAPIService>(new WattsTapAPIService());
#endif
```

### Build Validation

Добавьте проверку в Build Pipeline:

```csharp
public class BuildValidator : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;
    
    public void OnPreprocessBuild(BuildReport report)
    {
        var entry = FindObjectOfType<ApplicationEntry>();
        if (entry.IsUsingMockAPI())
        {
            throw new BuildFailedException("Mock API is enabled in production build!");
        }
    }
}
```

## 📚 Дополнительные ресурсы

- [API_README.md](./API_README.md) - Полная документация REST API
- [API_SWAGGER.yaml](./API_SWAGGER.yaml) - OpenAPI спецификация
- [API_INTEGRATION_GUIDE.md](./API_INTEGRATION_GUIDE.md) - Интеграция в Unity

## 🐛 Известные ограничения

1. **Persistence**: Mock данные не сохраняются между сессиями
   - Решение: Используйте `PlayerPrefs` или создайте Mock Storage
   
2. **Multi-player**: Mock не поддерживает мультиплеер тестирование
   - Решение: Для этого используйте реальный dev сервер

3. **Timing**: Задержка сети симулируется упрощенно
   - Решение: Для точного тестирования используйте Network Link Conditioner

## 💡 Советы

1. **Ctrl+P** в Editor Window для быстрого доступа
2. Создайте хоткеи для частых действий (Add Watts, Level Up)
3. Используйте конфиг для каждого тестового сценария
4. Комбинируйте Mock с Unity Recorder для создания демо-видео
5. Тестируйте error handling с `errorChance > 0`

---

**Поддержка**: Если Mock API не работает, проверьте:
- ✅ Сервис зарегистрирован в ServiceLocator
- ✅ Приложение в Play Mode
- ✅ Используется `WattsTapAPIMockService`, а не `WattsTapAPIService`
- ✅ Editor Window открыто через меню `WattsTap → Mock API Control Panel`

