TapControllerService / InputService

Кратко
-----
Добавлены два сервиса без привязки к UI:
- `ITapControllerService` / `TapControllerService` — логика тапов: хиты (hits), восстановление, апгрейды влияющие на доход/MaxHits/рековери, расчет оффлайн-бонуса. Делегирует фактическое начисление ресурсов в `IPlayerService` (существующий `PlayerService`).
- `IInputService` / `InputService` — слабая обёртка ввода: генерирует событие `OnTap(Vector2 screenPos)`; внешняя логика должна вызывать `Update(deltaTime)`.

Куда смотреть
--------------
Файлы:
- `Assets/WattsTap/Scripts/Game/Tap/Services/ITapControllerService.cs`
- `Assets/WattsTap/Scripts/Game/Tap/Services/TapControllerService.cs`
- `Assets/WattsTap/Scripts/Game/Tap/Services/IInputService.cs`
- `Assets/WattsTap/Scripts/Game/Tap/Services/InputService.cs`
- `Assets/WattsTap/Scripts/Game/Tap/Data/TapConfig.cs` (ScriptableObject конфиг)

Состояние сборки
----------------
Я прогнал статическую проверку ошибок для новых файлов — компиляционных ошибок нет. Есть одно предупреждение: ресурс `Resources/Configs/TapConfig` не найден в проекте (это нормально, если вы хотите использовать дефолтные параметры `TapConfig`). Рекомендуется создать ассет конфигурации в `Resources/Configs/TapConfig` если нужно менять значения по умолчанию.

Как зарегистрировать и использовать
----------------------------------
Пример простой привязки в `ApplicationEntry` или любом другом bootstrap-коде (на C#):

```csharp
// Registration (например в Awake() вашего ApplicationEntry или Bootstrap)
ServiceLocator.Register<IInputService>(new WattsTap.Scripts.Game.Tap.Services.InputService());
ServiceLocator.Register<ITapControllerService>(new WattsTap.Scripts.Game.Tap.Services.TapControllerService());

// После регистрации: инициализация через ServiceLocator.Instance.InitializeAll() уже вызовет Initialize у сервисов.
```

Пример «runtime bridge» — MonoBehaviour, который вызывает Update у сервисов и связывает OnTap -> HandleTap:

```csharp
using UnityEngine;
using WattsTap.Core;
using WattsTap.Scripts.Game.Tap.Services;

public class TapRuntimeBridge : MonoBehaviour
{
    private IInputService _input;
    private ITapControllerService _tapController;

    void Start()
    {
        _input = ServiceLocator.Get<IInputService>();
        _tapController = ServiceLocator.Get<ITapControllerService>();

        // Подписаться на событие тапа — мы не используем screenPos здесь, но можно
        _input.OnTap += OnTap;
    }

    void Update()
    {
        var dt = Time.deltaTime;
        _input.Update(dt);
        _tapController.Update(dt);
    }

    private void OnTap(Vector2 screenPos)
    {
        // Попытаться выполнить тап (логика внутри TapControllerService вызовет PlayerService.PerformTap())
        _tapController.HandleTap();
    }

    void OnDestroy()
    {
        if (_input != null) _input.OnTap -= OnTap;
    }
}
```

Важные замечания
----------------
- `TapControllerService` использует `IPlayerService` (через `ServiceLocator.Get<IPlayerService>()`) для начисления ресурсов и подсчёта статистики. Поэтому `PlayerService` должен быть зарегистрирован и инициализирован раньше (в `ServiceLocator` это обеспечивается полем `InitializationOrder` у сервисов). По умолчанию `PlayerService.InitializationOrder == 10`, у `TapControllerService == 20`, значит порядок корректный.
- Если вы хотите использовать кастомные значения `TapConfig`, создайте ScriptableObject:
  - Правый клик в проекте -> Create -> WattsTap -> TapConfig
  - Сохраните в `Assets/Resources/Configs/TapConfig.asset`
  - Тогда `TapControllerService` загрузит его автоматически.
- `InputService` реализован минималистично: слушает `Input.GetMouseButtonDown(0)` и `Touch` и генерирует `OnTap`. Можно заменить на более сложный, основанный на новой Unity InputSystem, если нужно.

Тестирование
-----------
- Запустите сцену с `ApplicationEntry` (или вручную зарегистрируйте сервисы как в примере) и прикрепите `TapRuntimeBridge` на GameObject.
- В Play Mode тапайте/кликните по экрану — `TapControllerService.HandleTap()` попытается выполнить тап через `PlayerService.PerformTap()` и уменьшит `CurrentHits`.
- Проверьте логи: `TapControllerService` выводит инициализацию и предупреждение про отсутствие конфигурации (если нет Asset).

Дальше (рекомендации)
--------------------
- Добавить unit-тесты для логики восстановления хитов и расчёта оффлайн-бонуса (формулы и граничные случаи).
- Экспортировать события `OnHitsChanged` и `OnTapPerformed` на UI-представление для отображения эффектов и звуков.
- Добавить обработку массовых тапов (например мульти-тапы, автоклик и т.п.) если это нужно для механики.

Если хотите, могу:
- автоматически зарегистрировать `InputService` и `TapControllerService` в `ApplicationEntry` (с правкой файла),
- добавить `TapRuntimeBridge` как файл в репозиторий, или
- реализовать unit-тесты для `TapControllerService`.

Скажите, что делаем дальше.
