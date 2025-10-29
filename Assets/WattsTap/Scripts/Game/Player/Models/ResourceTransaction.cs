using System;

namespace WattsTap.Game.Player
{
    /// <summary>
    /// Результат транзакции с ресурсами
    /// </summary>
    public class ResourceTransaction
    {
        public ResourceType ResourceType { get; }
        public long Amount { get; }
        public long PreviousValue { get; }
        public long NewValue { get; }
        public bool Success { get; }
        public string Reason { get; }
        public DateTime Timestamp { get; }

        public ResourceTransaction(ResourceType resourceType, long amount, long previousValue, long newValue, bool success, string reason = "")
        {
            ResourceType = resourceType;
            Amount = amount;
            PreviousValue = previousValue;
            NewValue = newValue;
            Success = success;
            Reason = reason;
            Timestamp = DateTime.UtcNow;
        }

        public static ResourceTransaction CreateSuccess(ResourceType type, long amount, long previousValue, long newValue)
        {
            return new ResourceTransaction(type, amount, previousValue, newValue, true);
        }

        public static ResourceTransaction CreateFailure(ResourceType type, long amount, long currentValue, string reason)
        {
            return new ResourceTransaction(type, amount, currentValue, currentValue, false, reason);
        }
    }
}

