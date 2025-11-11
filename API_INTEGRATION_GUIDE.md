# WattsTap API - Примеры интеграции для Unity

## 📦 Установка и настройка

### 1. Создание API Service в Unity

```csharp
// Assets/WattsTap/Scripts/Core/API/WattsTapAPIService.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace WattsTap.Core.API
{
    public interface IWattsTapAPIService : IService
    {
        IEnumerator Authenticate(string initData, Action<AuthResponse> onSuccess, Action<string> onError);
        IEnumerator SendTapBatch(List<TapData> taps, Action<TapBatchResponse> onSuccess, Action<string> onError);
        IEnumerator GetPlayerData(Action<PlayerData> onSuccess, Action<string> onError);
        IEnumerator SyncGameState(Action<SyncResponse> onSuccess, Action<string> onError);
        IEnumerator ClaimOfflineBonus(Action<OfflineRewards> onSuccess, Action<string> onError);
    }

    public class WattsTapAPIService : IWattsTapAPIService
    {
        private const string BASE_URL = "https://api.wattstap.com/v1";
        private string _authToken;
        private string _sessionId;

        public int InitializationOrder => 5;

        public void Initialize()
        {
            _sessionId = Guid.NewGuid().ToString();
            Debug.Log($"[WattsTapAPI] Service initialized with session: {_sessionId}");
        }

        public void Shutdown()
        {
            _authToken = null;
        }

        // Аутентификация
        public IEnumerator Authenticate(string initData, Action<AuthResponse> onSuccess, Action<string> onError)
        {
            var request = new AuthRequest
            {
                initData = initData
            };

            yield return PostRequest("/auth/telegram", request, 
                (ApiResponse<AuthResponse> response) =>
                {
                    _authToken = response.data.token;
                    PlayerPrefs.SetString("api_token", _authToken);
                    PlayerPrefs.Save();
                    onSuccess?.Invoke(response.data);
                },
                onError,
                useAuth: false
            );
        }

        // Отправка пакета тапов
        public IEnumerator SendTapBatch(List<TapData> taps, Action<TapBatchResponse> onSuccess, Action<string> onError)
        {
            var request = new TapBatchRequest
            {
                taps = taps,
                clientTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                sessionId = _sessionId
            };

            yield return PostRequest("/gameplay/tap-batch", request, 
                (ApiResponse<TapBatchResponse> response) =>
                {
                    onSuccess?.Invoke(response.data);
                },
                onError
            );
        }

        // Получение данных игрока
        public IEnumerator GetPlayerData(Action<PlayerData> onSuccess, Action<string> onError)
        {
            yield return GetRequest("/player/me",
                (ApiResponse<PlayerData> response) =>
                {
                    onSuccess?.Invoke(response.data);
                },
                onError
            );
        }

        // Синхронизация состояния
        public IEnumerator SyncGameState(Action<SyncResponse> onSuccess, Action<string> onError)
        {
            var request = new SyncRequest
            {
                lastKnownServerTime = PlayerPrefs.GetString("last_server_time", DateTime.UtcNow.ToString("o")),
                clientTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            yield return PostRequest("/gameplay/sync", request,
                (ApiResponse<SyncResponse> response) =>
                {
                    onSuccess?.Invoke(response.data);
                },
                onError
            );
        }

        // Получение оффлайн бонуса
        public IEnumerator ClaimOfflineBonus(Action<OfflineRewards> onSuccess, Action<string> onError)
        {
            yield return PostRequest("/player/claim-offline-bonus", new {},
                (ApiResponse<OfflineBonusResponse> response) =>
                {
                    onSuccess?.Invoke(response.data);
                },
                onError
            );
        }

        // Базовые HTTP методы
        private IEnumerator GetRequest<T>(string endpoint, Action<ApiResponse<T>> onSuccess, Action<string> onError, bool useAuth = true)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(BASE_URL + endpoint))
            {
                if (useAuth && !string.IsNullOrEmpty(_authToken))
                {
                    www.SetRequestHeader("Authorization", $"Bearer {_authToken}");
                }

                yield return www.SendWebRequest();

                yield return HandleResponse(www, onSuccess, onError);
            }
        }

        private IEnumerator PostRequest<TRequest, TResponse>(string endpoint, TRequest data, Action<ApiResponse<TResponse>> onSuccess, Action<string> onError, bool useAuth = true)
        {
            string json = JsonConvert.SerializeObject(data);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest www = new UnityWebRequest(BASE_URL + endpoint, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                if (useAuth && !string.IsNullOrEmpty(_authToken))
                {
                    www.SetRequestHeader("Authorization", $"Bearer {_authToken}");
                }

                yield return www.SendWebRequest();

                yield return HandleResponse(www, onSuccess, onError);
            }
        }

        private IEnumerator HandleResponse<T>(UnityWebRequest www, Action<ApiResponse<T>> onSuccess, Action<string> onError)
        {
            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonConvert.DeserializeObject<ApiResponse<T>>(www.downloadHandler.text);
                    
                    if (response.success)
                    {
                        // Сохранить серверное время
                        if (!string.IsNullOrEmpty(response.serverTime))
                        {
                            PlayerPrefs.SetString("last_server_time", response.serverTime);
                        }

                        onSuccess?.Invoke(response);
                    }
                    else
                    {
                        onError?.Invoke(response.error?.message ?? "Unknown error");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[WattsTapAPI] Parse error: {e.Message}");
                    onError?.Invoke($"Failed to parse response: {e.Message}");
                }
            }
            else
            {
                // Обработка ошибок HTTP
                Debug.LogError($"[WattsTapAPI] Request failed: {www.error}");
                
                // Попытка получить детали ошибки
                if (!string.IsNullOrEmpty(www.downloadHandler.text))
                {
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(www.downloadHandler.text);
                        onError?.Invoke(errorResponse.error?.message ?? www.error);
                    }
                    catch
                    {
                        onError?.Invoke(www.error);
                    }
                }
                else
                {
                    onError?.Invoke(www.error);
                }
            }

            yield return null;
        }
    }

    // DTO классы
    [Serializable]
    public class ApiResponse<T>
    {
        public bool success;
        public T data;
        public string serverTime;
        public ErrorInfo error;
    }

    [Serializable]
    public class ErrorResponse
    {
        public bool success;
        public ErrorInfo error;
        public string serverTime;
    }

    [Serializable]
    public class ErrorInfo
    {
        public string code;
        public string message;
        public Dictionary<string, object> details;
    }

    [Serializable]
    public class AuthRequest
    {
        public string initData;
    }

    [Serializable]
    public class AuthResponse
    {
        public string token;
        public int expiresIn;
        public PlayerInfo player;
    }

    [Serializable]
    public class PlayerInfo
    {
        public string playerId;
        public string nickname;
        public int level;
        public bool isNewPlayer;
    }

    [Serializable]
    public class TapBatchRequest
    {
        public List<TapData> taps;
        public long clientTime;
        public string sessionId;
    }

    [Serializable]
    public class TapData
    {
        public long clientTimestamp;
        public Vector2Data screenPosition;
    }

    [Serializable]
    public class Vector2Data
    {
        public float x;
        public float y;

        public Vector2Data(Vector2 vec)
        {
            x = vec.x;
            y = vec.y;
        }
    }

    [Serializable]
    public class TapBatchResponse
    {
        public int validTapsCount;
        public int invalidTapsCount;
        public long wattsEarned;
        public long xpEarned;
        public PlayerResources resources;
        public List<TapEffect> effects;
        public LevelUpInfo levelUp;
    }

    [Serializable]
    public class TapEffect
    {
        public string type;
        public Vector2Data position;
        public float value;
        public float multiplier;
    }

    [Serializable]
    public class LevelUpInfo
    {
        public int newLevel;
        public Rewards rewards;
    }

    [Serializable]
    public class Rewards
    {
        public long watts;
        public long xp;
        public string kiloWattTokens;
        public List<InventoryItem> items;
    }

    [Serializable]
    public class SyncRequest
    {
        public string lastKnownServerTime;
        public long clientTime;
    }

    [Serializable]
    public class SyncResponse
    {
        public PlayerResources resources;
        public OfflineRewards offlineRewards;
        public int hitsRecovered;
    }

    [Serializable]
    public class OfflineRewards
    {
        public long wattsEarned;
        public int offlineDuration;
        public float multiplier;
        public int maxDuration;
    }

    [Serializable]
    public class OfflineBonusResponse : OfflineRewards
    {
        public PlayerResources resources;
    }
}
```

## 🎮 Интеграция в Tap System

### 2. Модификация TapControllerService для работы с API

```csharp
// Assets/WattsTap/Scripts/Game/Tap/Services/TapControllerService.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.API;

namespace WattsTap.Scripts.Game.Tap.Services
{
    public class TapControllerService : ITapControllerService
    {
        private List<TapData> _pendingTaps = new List<TapData>();
        private const int BATCH_SIZE = 10; // Отправляем по 10 тапов
        private const float BATCH_TIMEOUT = 2f; // Или каждые 2 секунды
        private float _timeSinceLastBatch = 0f;
        
        private IWattsTapAPIService _apiService;
        private IPlayerService _playerService;

        public void Initialize()
        {
            _apiService = ServiceLocator.Get<IWattsTapAPIService>();
            _playerService = ServiceLocator.Get<IPlayerService>();
        }

        public void HandleTap(Vector2 screenPosition)
        {
            var playerData = _playerService.GetPlayerData();
            
            // Локальная проверка возможности тапа
            if (playerData.resources.currentHits <= 0)
            {
                Debug.Log("[TapController] No hits available");
                return;
            }

            // Добавляем тап в очередь
            _pendingTaps.Add(new TapData
            {
                clientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                screenPosition = new Vector2Data(screenPosition)
            });

            // Оптимистичное обновление UI (будет скорректировано сервером)
            _playerService.UpdateResourcesOptimistic(-1, 0); // -1 hit

            // Отправляем пакет если накопилось достаточно
            if (_pendingTaps.Count >= BATCH_SIZE)
            {
                SendTapBatch();
            }
        }

        public void Update(float deltaTime)
        {
            _timeSinceLastBatch += deltaTime;

            // Отправляем оставшиеся тапы по таймауту
            if (_pendingTaps.Count > 0 && _timeSinceLastBatch >= BATCH_TIMEOUT)
            {
                SendTapBatch();
            }
        }

        private void SendTapBatch()
        {
            if (_pendingTaps.Count == 0) return;

            var tapsToSend = new List<TapData>(_pendingTaps);
            _pendingTaps.Clear();
            _timeSinceLastBatch = 0f;

            // Корутина для отправки
            CoroutineRunner.Instance.StartCoroutine(
                _apiService.SendTapBatch(
                    tapsToSend,
                    onSuccess: (response) => OnTapBatchSuccess(response),
                    onError: (error) => OnTapBatchError(error, tapsToSend)
                )
            );
        }

        private void OnTapBatchSuccess(TapBatchResponse response)
        {
            Debug.Log($"[TapController] Batch processed: {response.validTapsCount} valid, {response.wattsEarned} watts earned");

            // Обновляем ресурсы из серверного ответа
            _playerService.UpdateResourcesFromServer(response.resources);

            // Показываем эффекты
            foreach (var effect in response.effects)
            {
                ShowTapEffect(effect);
            }

            // Проверяем level up
            if (response.levelUp != null)
            {
                OnLevelUp(response.levelUp);
            }
        }

        private void OnTapBatchError(string error, List<TapData> failedTaps)
        {
            Debug.LogError($"[TapController] Batch failed: {error}");

            // В случае ошибки возвращаем тапы обратно или синхронизируем состояние
            CoroutineRunner.Instance.StartCoroutine(
                _apiService.SyncGameState(
                    onSuccess: (syncResponse) =>
                    {
                        _playerService.UpdateResourcesFromServer(syncResponse.resources);
                    },
                    onError: (syncError) =>
                    {
                        Debug.LogError($"[TapController] Sync failed: {syncError}");
                    }
                )
            );
        }

        private void ShowTapEffect(TapEffect effect)
        {
            // Показать визуальный эффект на позиции тапа
            var effectService = ServiceLocator.Get<IVFXService>();
            
            switch (effect.type)
            {
                case "tap_success":
                    effectService?.PlayTapEffect(
                        new Vector2(effect.position.x, effect.position.y),
                        effect.value
                    );
                    break;
                case "combo":
                    effectService?.PlayComboEffect(effect.multiplier);
                    break;
                case "critical":
                    effectService?.PlayCriticalEffect(
                        new Vector2(effect.position.x, effect.position.y),
                        effect.value
                    );
                    break;
            }
        }

        private void OnLevelUp(LevelUpInfo levelUp)
        {
            Debug.Log($"[TapController] LEVEL UP! New level: {levelUp.newLevel}");
            
            // Показать Level Up UI
            var uiService = ServiceLocator.Get<IUIService>();
            uiService?.ShowLevelUpPopup(levelUp);
        }
    }
}
```

### 3. CoroutineRunner Helper

```csharp
// Assets/WattsTap/Scripts/Core/Utilities/CoroutineRunner.cs
using UnityEngine;

namespace WattsTap.Core
{
    /// <summary>
    /// Singleton для запуска корутин из не-MonoBehaviour классов
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        
        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("CoroutineRunner");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
    }
}
```

## 🔄 Примеры использования

### Пример 1: Аутентификация при запуске

```csharp
// Assets/WattsTap/Scripts/Game/ApplicationEntry.cs
public class ApplicationEntry : MonoBehaviour
{
    private IWattsTapAPIService _apiService;

    private IEnumerator Start()
    {
        // Регистрация сервисов
        ServiceLocator.Register<IWattsTapAPIService>(new WattsTapAPIService());
        ServiceLocator.Register<IPlayerService>(new PlayerService());
        ServiceLocator.Register<ITapControllerService>(new TapControllerService());
        
        ServiceLocator.Instance.InitializeAll();

        _apiService = ServiceLocator.Get<IWattsTapAPIService>();

        // Получаем initData от Telegram
        string initData = GetTelegramInitData();

        bool authSuccess = false;

        yield return _apiService.Authenticate(
            initData,
            onSuccess: (authResponse) =>
            {
                Debug.Log($"Auth successful! Player: {authResponse.player.nickname}");
                authSuccess = true;
                
                if (authResponse.player.isNewPlayer)
                {
                    ShowWelcomeScreen();
                }
            },
            onError: (error) =>
            {
                Debug.LogError($"Auth failed: {error}");
                ShowErrorScreen(error);
            }
        );

        if (authSuccess)
        {
            // Загружаем данные игрока
            yield return LoadPlayerData();
            
            // Проверяем оффлайн награды
            yield return ClaimOfflineRewards();
            
            // Запускаем игру
            StartGame();
        }
    }

    private IEnumerator LoadPlayerData()
    {
        var playerService = ServiceLocator.Get<IPlayerService>();

        yield return _apiService.GetPlayerData(
            onSuccess: (playerData) =>
            {
                playerService.SetPlayerData(playerData);
                Debug.Log($"Player data loaded: Level {playerData.level}, {playerData.resources.watts} watts");
            },
            onError: (error) =>
            {
                Debug.LogError($"Failed to load player data: {error}");
            }
        );
    }

    private IEnumerator ClaimOfflineRewards()
    {
        yield return _apiService.ClaimOfflineBonus(
            onSuccess: (rewards) =>
            {
                if (rewards.wattsEarned > 0)
                {
                    Debug.Log($"Offline rewards: +{rewards.wattsEarned} watts for {rewards.offlineDuration}s");
                    ShowOfflineRewardsPopup(rewards);
                }
            },
            onError: (error) =>
            {
                // Не критично если не удалось получить оффлайн награды
                Debug.LogWarning($"Could not claim offline bonus: {error}");
            }
        );
    }

    private string GetTelegramInitData()
    {
        // В production получаем от Telegram WebApp
        #if UNITY_WEBGL && !UNITY_EDITOR
            return GetTelegramInitDataFromJS();
        #else
            // В редакторе используем моковые данные для тестирования
            return "query_id=test&user=%7B%22id%22%3A123456789%7D";
        #endif
    }

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern string GetTelegramInitDataFromJS();
}
```

### Пример 2: Обработка тапов в UI

```csharp
// Assets/WattsTap/Scripts/Game/UI/TapArea.cs
using UnityEngine;
using UnityEngine.EventSystems;
using WattsTap.Core;
using WattsTap.Scripts.Game.Tap.Services;

public class TapArea : MonoBehaviour, IPointerDownHandler
{
    private ITapControllerService _tapController;
    
    private void Start()
    {
        _tapController = ServiceLocator.Get<ITapControllerService>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Передаём тап в контроллер с позицией на экране
        _tapController.HandleTap(eventData.position);
        
        // Локальная визуализация (мгновенная)
        PlayLocalTapAnimation(eventData.position);
    }

    private void PlayLocalTapAnimation(Vector2 position)
    {
        // Показываем локальную анимацию сразу для отзывчивости UI
        // Реальные награды придут от сервера
    }
}
```

### Пример 3: Покупка улучшения

```csharp
// Assets/WattsTap/Scripts/Game/UI/UpgradeButton.cs
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.API;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private string upgradeId;
    [SerializeField] private Button button;
    [SerializeField] private Text costText;

    private IWattsTapAPIService _apiService;

    private void Start()
    {
        _apiService = ServiceLocator.Get<IWattsTapAPIService>();
        button.onClick.AddListener(OnPurchaseClick);
    }

    private void OnPurchaseClick()
    {
        button.interactable = false;
        
        StartCoroutine(_apiService.PurchaseUpgrade(
            upgradeId,
            onSuccess: (response) =>
            {
                Debug.Log($"Upgrade purchased! New level: {response.upgrade.newLevel}");
                
                // Обновляем ресурсы
                var playerService = ServiceLocator.Get<IPlayerService>();
                playerService.UpdateResourcesFromServer(response.resources);
                
                // Обновляем UI
                RefreshUI();
                
                button.interactable = true;
            },
            onError: (error) =>
            {
                Debug.LogError($"Purchase failed: {error}");
                ShowErrorMessage(error);
                button.interactable = true;
            }
        ));
    }
}
```

## 🔧 Настройка для разных окружений

### Development / Staging / Production

```csharp
// Assets/WattsTap/Scripts/Core/Config/APIConfig.cs
using UnityEngine;

namespace WattsTap.Core.Config
{
    [CreateAssetMenu(menuName = "WattsTap/API Config")]
    public class APIConfig : ScriptableObject
    {
        public Environment environment = Environment.Production;
        
        public string GetBaseURL()
        {
            return environment switch
            {
                Environment.Development => "https://api-dev.wattstap.com/v1",
                Environment.Staging => "https://api-staging.wattstap.com/v1",
                Environment.Production => "https://api.wattstap.com/v1",
                _ => "https://api.wattstap.com/v1"
            };
        }
    }

    public enum Environment
    {
        Development,
        Staging,
        Production
    }
}
```

## 📊 Мониторинг и отладка

### Debug Logger для API запросов

```csharp
// Assets/WattsTap/Scripts/Core/API/APILogger.cs
using UnityEngine;

namespace WattsTap.Core.API
{
    public static class APILogger
    {
        private static bool _enableLogging = true;

        public static void LogRequest(string endpoint, object requestData)
        {
            if (!_enableLogging) return;
            
            Debug.Log($"[API] → {endpoint}\n{JsonUtility.ToJson(requestData, true)}");
        }

        public static void LogResponse(string endpoint, object responseData)
        {
            if (!_enableLogging) return;
            
            Debug.Log($"[API] ← {endpoint}\n{JsonUtility.ToJson(responseData, true)}");
        }

        public static void LogError(string endpoint, string error)
        {
            Debug.LogError($"[API] ✗ {endpoint}: {error}");
        }
    }
}
```

## 🎯 Best Practices

1. **Batching тапов**: Не отправляйте каждый тап отдельно - используйте пакеты по 5-10 тапов
2. **Optimistic Updates**: Обновляйте UI локально сразу, корректируйте по ответу сервера
3. **Error Handling**: Всегда обрабатывайте ошибки и имейте fallback логику
4. **Синхронизация**: При возобновлении приложения вызывайте `/gameplay/sync`
5. **Кэширование токена**: Сохраняйте JWT токен в PlayerPrefs для повторного использования
6. **Rate Limiting**: Учитывайте лимиты запросов в UI (показывайте cooldowns)
7. **Server Time**: Используйте серверное время для критичных проверок

## 🚀 Deployment Checklist

- [ ] Переключить API на production URL
- [ ] Отключить debug логирование
- [ ] Настроить обработку ошибок сети
- [ ] Добавить аналитику для API ошибок
- [ ] Настроить retry логику для критичных запросов
- [ ] Проверить CORS настройки для WebGL домена
- [ ] Тестирование на медленном интернете
- [ ] Тестирование offline режима

