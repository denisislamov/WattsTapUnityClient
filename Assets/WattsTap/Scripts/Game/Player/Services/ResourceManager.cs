using System;
using System.Collections.Generic;
using UnityEngine;

namespace WattsTap.Game.Player
{
    /// <summary>
    /// Менеджер управления игровыми ресурсами
    /// Работает с PlayerResources для хранения данных
    /// </summary>
    public class ResourceManager : IResourceManager
    {
        private readonly PlayerResources _resources;
        private readonly Dictionary<ResourceType, long> _maxValues;

        public event Action<ResourceType, long, long> OnResourceChanged;
        public event Action<ResourceTransaction> OnResourceTransaction;

        public ResourceManager(PlayerResources resources)
        {
            _resources = resources ?? throw new ArgumentNullException(nameof(resources));
            _maxValues = new Dictionary<ResourceType, long>
            {
                { ResourceType.Energy, resources.maxEnergy }
            };
        }

        public long GetResource(ResourceType type)
        {
            return type switch
            {
                ResourceType.Watts => _resources.watts,
                ResourceType.Energy => _resources.currentEnergy,
                ResourceType.Experience => _resources.currentXP,
                ResourceType.KiloWatt => (long)(_resources.kiloWattTokens * 1000000), // Convert to micro-units
                ResourceType.Premium => 0, // Not implemented yet
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
            
            // Check max value constraints
            if (_maxValues.TryGetValue(type, out var maxValue) && maxValue > 0)
            {
                newValue = Math.Min(newValue, maxValue);
            }

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
            // First check if all resources are available
            foreach (var (type, amount) in costs)
            {
                if (!HasEnough(type, amount))
                {
                    Debug.LogWarning($"[ResourceManager] Cannot spend multiple: not enough {type} (need {amount}, have {GetResource(type)})");
                    return false;
                }
            }

            // If all checks pass, spend all resources
            foreach (var (type, amount) in costs)
            {
                SpendResource(type, amount);
            }

            return true;
        }

        public long GetMaxResource(ResourceType type)
        {
            if (_maxValues.TryGetValue(type, out var maxValue))
            {
                return maxValue;
            }
            return long.MaxValue; // No limit
        }

        public void SetMaxResource(ResourceType type, long maxValue)
        {
            _maxValues[type] = maxValue;
            
            // Update the underlying data for Energy
            if (type == ResourceType.Energy)
            {
                _resources.maxEnergy = (int)maxValue;
                
                // Cap current energy if it exceeds new max
                if (_resources.currentEnergy > maxValue)
                {
                    var previousValue = _resources.currentEnergy;
                    _resources.currentEnergy = (int)maxValue;
                    OnResourceChanged?.Invoke(ResourceType.Energy, previousValue, maxValue);
                }
            }
        }

        private void SetResourceInternal(ResourceType type, long value)
        {
            switch (type)
            {
                case ResourceType.Watts:
                    _resources.watts = Math.Max(0, value);
                    break;
                case ResourceType.Energy:
                    _resources.currentEnergy = (int)Math.Max(0, value);
                    break;
                case ResourceType.Experience:
                    _resources.currentXP = Math.Max(0, value);
                    break;
                case ResourceType.KiloWatt:
                    _resources.kiloWattTokens = Math.Max(0, value / 1000000m);
                    break;
                case ResourceType.Premium:
                    // Not implemented yet
                    Debug.LogWarning("[ResourceManager] Premium currency not implemented");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown resource type");
            }
        }
    }
}

