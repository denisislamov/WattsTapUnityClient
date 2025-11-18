using System;
using System.Collections.Generic;
using UnityEngine;

namespace WattsTap.Game.Player
{
    public class ResourceManager : IResourceManager
    {
        private readonly PlayerResources _resources;
        private readonly Dictionary<ResourceType, long> _maxValues;

        public event Action<ResourceType, long, long> OnResourceChanged;
        public event Action<ResourceTransaction> OnResourceTransaction;

        public ResourceManager(PlayerResources resources)
        {
            _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        }

        public long GetResource(ResourceType type)
        {
            return type switch
            {
                ResourceType.Watts => _resources.watts,
                ResourceType.Experience => _resources.currentXP,
                ResourceType.XpToNextLevel => _resources.xpToNextLevel,
                ResourceType.SummXp => _resources.sumExp,
                ResourceType.KiloWatt => (long)(_resources.kiloWattTokens * 1000000),
                ResourceType.Hits => _resources.currentHits,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown resource type")
            };
        }

        public bool HasEnough(ResourceType type, long amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[ResourceManager] Checking negative amount for {type}: {amount}");
                return false;
            }
            return GetResource(type) >= amount;
        }

        public ResourceTransaction AddResource(ResourceType type, long amount, bool notifyChange = true)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[ResourceManager] Attempted to add negative amount for {type}: {amount}");
                return ResourceTransaction.CreateFailure(type, amount, GetResource(type), "Negative amount");
            }

            var previousValue = GetResource(type);
            var newValue = previousValue + amount;
            
            SetResourceInternal(type, newValue);
            
            var transaction = ResourceTransaction.CreateSuccess(type, amount, previousValue, newValue);
            
            if (notifyChange)
            {
                OnResourceChanged?.Invoke(type, previousValue, newValue);
                OnResourceTransaction?.Invoke(transaction);
            }

            return transaction;
        }

        public ResourceTransaction SpendResource(ResourceType type, long amount, bool notifyChange = true)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[ResourceManager] Attempted to spend negative amount for {type}: {amount}");
                return ResourceTransaction.CreateFailure(type, amount, GetResource(type), "Negative amount");
            }

            var previousValue = GetResource(type);
            
            if (!HasEnough(type, amount))
            {
                var transaction = ResourceTransaction.CreateFailure(type, amount, previousValue, $"Not enough {type}");
                if (notifyChange)
                {
                    OnResourceTransaction?.Invoke(transaction);
                }
                return transaction;
            }

            var newValue = previousValue - amount;
            SetResourceInternal(type, newValue);
            
            var successTransaction = ResourceTransaction.CreateSuccess(type, -amount, previousValue, newValue);
            
            if (notifyChange)
            {
                OnResourceChanged?.Invoke(type, previousValue, newValue);
                OnResourceTransaction?.Invoke(successTransaction);
            }

            return successTransaction;
        }

        public void SetResource(ResourceType type, long value, bool notifyChange = false)
        {
            var previousValue = GetResource(type);
            SetResourceInternal(type, value);
            
            if (notifyChange && previousValue != value)
            {
                OnResourceChanged?.Invoke(type, previousValue, value);
            }
        }

        public bool TrySpendMultiple(params (ResourceType type, long amount)[] costs)
        {
            foreach (var (type, amount) in costs)
            {
                if (!HasEnough(type, amount))
                {
                    Debug.LogWarning($"[ResourceManager] Cannot spend multiple: not enough {type}" +
                                     $" (need {amount}, have {GetResource(type)})");
                    return false;
                }
            }

            foreach (var (type, amount) in costs)
            {
                SpendResource(type, amount);
            }

            return true;
        }
        
        private void SetResourceInternal(ResourceType type, long value)
        {
            switch (type)
            {
                case ResourceType.Watts:
                    _resources.watts = Math.Max(0, value);
                    break;
                case ResourceType.Experience:
                    _resources.currentXP = Math.Max(0, value);
                    break;
                case ResourceType.KiloWatt:
                    _resources.kiloWattTokens = Math.Max(0, value / 1000000m);
                    break;
                case ResourceType.Hits:
                    _resources.currentHits = (int)Math.Max(0, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown resource type");
            }
        }
    }
}
