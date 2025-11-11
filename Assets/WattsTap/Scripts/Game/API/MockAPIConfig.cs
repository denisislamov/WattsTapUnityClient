using UnityEngine;

namespace WattsTap.Game.API
{
    /// <summary>
    /// ScriptableObject конфигурация для Mock API
    /// Позволяет настраивать поведение мока через Inspector
    /// </summary>
    [CreateAssetMenu(fileName = "MockAPIConfig", menuName = "WattsTap/API/Mock API Config")]
    public class MockAPIConfig : ScriptableObject
    {
        [Header("Network Simulation")]
        [Tooltip("Симулировать задержку сети")]
        public bool simulateNetworkDelay = true;
        
        [Tooltip("Минимальная задержка в секундах")]
        [Range(0f, 2f)]
        public float minDelay = 0.1f;
        
        [Tooltip("Максимальная задержка в секундах")]
        [Range(0f, 5f)]
        public float maxDelay = 0.5f;

        [Header("Error Simulation")]
        [Tooltip("Вероятность ошибки (0 = никогда, 1 = всегда)")]
        [Range(0f, 1f)]
        public float errorChance;
        
        [Tooltip("Типы ошибок для симуляции")]
        public ErrorType[] possibleErrors = new ErrorType[]
        {
            ErrorType.NetworkTimeout,
            ErrorType.ServerError,
            ErrorType.RateLimitExceeded
        };

        [Header("Mock Player Data")]
        [Tooltip("Начальное количество Watts")]
        public long startingWatts = 10000;
        
        [Tooltip("Начальный уровень игрока")]
        public int startingLevel = 5;
        
        [Tooltip("Начальное количество ударов")]
        public int startingHits = 18;
        
        [Tooltip("Максимальное количество ударов")]
        public int maxHits = 20;
        
        [Tooltip("Доход за тап")]
        public long incomePerTap = 50;
        
        [Tooltip("Пассивный доход в час")]
        public long incomePerHour = 1000;

        [Header("Offline Simulation")]
        [Tooltip("Симулировать время оффлайн (в часах)")]
        [Range(0f, 24f)]
        public float offlineHours = 1f;

        [Header("Gameplay Tweaks")]
        [Tooltip("Множитель опыта (для быстрого тестирования level up)")]
        [Range(1f, 10f)]
        public float xpMultiplier = 1f;
        
        [Tooltip("Автоматически давать level up при достижении XP")]
        public bool autoLevelUp = true;
        
        [Tooltip("Бесконечные удары (не тратятся при тапах)")]
        public bool infiniteHits;

        [Header("Logging")]
        [Tooltip("Выводить подробные логи в консоль")]
        public bool verboseLogging = true;
        
        [Tooltip("Показывать время ответа в логах")]
        public bool logResponseTime = true;

        public enum ErrorType
        {
            NetworkTimeout,
            ServerError,
            InvalidToken,
            RateLimitExceeded,
            InsufficientResources
        }

        public string GetRandomErrorMessage()
        {
            if (possibleErrors.Length == 0)
                return "Unknown error";

            var errorType = possibleErrors[Random.Range(0, possibleErrors.Length)];
            
            return errorType switch
            {
                ErrorType.NetworkTimeout => "Network timeout. Please check your connection.",
                ErrorType.ServerError => "Internal server error. Please try again later.",
                ErrorType.InvalidToken => "Invalid or expired authentication token.",
                ErrorType.RateLimitExceeded => "Rate limit exceeded. Please slow down.",
                ErrorType.InsufficientResources => "Insufficient resources to complete this action.",
                _ => "Unknown error occurred."
            };
        }
    }
}

