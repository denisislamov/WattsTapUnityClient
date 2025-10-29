using UnityEngine;
using WattsTap.Core;
using WattsTap.Game.Player;

namespace WattsTap.Examples
{
    /// <summary>
    /// Пример использования системы ресурсов
    /// </summary>
    public class ResourceSystemExample : MonoBehaviour
    {
        private IPlayerService _playerService;
        private IResourceManager _resourceManager;

        private void Start()
        {
            // Получить PlayerService из ServiceLocator
            _playerService = ServiceLocator.Get<IPlayerService>();
            _resourceManager = _playerService.ResourceManager;
            
            // Подписаться на события
            SubscribeToEvents();
            
            // Примеры использования
            ExampleBasicOperations();
            ExampleAtomicTransactions();
        }

        private void SubscribeToEvents()
        {
            // Событие изменения ресурса
            _resourceManager.OnResourceChanged += OnResourceChanged;
            
            // Событие транзакции
            _resourceManager.OnResourceTransaction += OnResourceTransaction;
        }

        private void OnResourceChanged(ResourceType type, long oldValue, long newValue)
        {
            Debug.Log($"[Example] Resource changed: {type} {oldValue} -> {newValue}");
        }

        private void OnResourceTransaction(ResourceTransaction transaction)
        {
            if (transaction.Success)
            {
                Debug.Log($"[Example] Transaction success: {transaction.ResourceType} {transaction.Amount:+#;-#;0}");
            }
            else
            {
                Debug.LogWarning($"[Example] Transaction failed: {transaction.Reason}");
            }
        }

        private void ExampleBasicOperations()
        {
            Debug.Log("=== Basic Operations Example ===");
            
            // Добавить Watts
            _resourceManager.AddResource(ResourceType.Watts, 1000);
            
            // Проверить достаточность
            bool hasEnough = _resourceManager.HasEnough(ResourceType.Watts, 500);
            Debug.Log($"Has 500 Watts? {hasEnough}");
            
            // Потратить Watts
            var transaction = _resourceManager.SpendResource(ResourceType.Watts, 300);
            if (transaction.Success)
            {
                Debug.Log($"Spent 300 Watts. Remaining: {transaction.NewValue}");
            }
            
            // Получить текущее значение
            long currentWatts = _resourceManager.GetResource(ResourceType.Watts);
            Debug.Log($"Current Watts: {currentWatts}");
        }

        private void ExampleAtomicTransactions()
        {
            Debug.Log("=== Atomic Transactions Example ===");
            
            // Добавить ресурсы для теста
            _resourceManager.AddResource(ResourceType.Watts, 1000);
            _resourceManager.AddResource(ResourceType.Energy, 50);
            
            // Попытка атомарной траты (все или ничего)
            bool success = _resourceManager.TrySpendMultiple(
                (ResourceType.Watts, 100),
                (ResourceType.Energy, 10)
            );
            
            Debug.Log($"Atomic transaction result: {success}");
        }

        private void ExamplePlayerServiceMethods()
        {
            Debug.Log("=== PlayerService Methods Example ===");
            
            // Использовать методы PlayerService (они используют ResourceManager внутри)
            _playerService.AddWatts(500);
            
            bool spentSuccess = _playerService.SpendWatts(200);
            Debug.Log($"Spent 200 Watts via PlayerService: {spentSuccess}");
            
            // Выполнить тап (комплексная операция)
            bool tapSuccess = _playerService.PerformTap();
            Debug.Log($"Tap performed: {tapSuccess}");
        }

        private void ExampleEnergyManagement()
        {
            Debug.Log("=== Energy Management Example ===");
            
            // Получить максимальную энергию
            long maxEnergy = _resourceManager.GetMaxResource(ResourceType.Energy);
            Debug.Log($"Max Energy: {maxEnergy}");
            
            // Использовать энергию
            _playerService.UseEnergy(10);
            
            // Восстановить энергию
            _playerService.RestoreEnergy(5);
            
            long currentEnergy = _resourceManager.GetResource(ResourceType.Energy);
            Debug.Log($"Current Energy: {currentEnergy}/{maxEnergy}");
        }

        private void OnDestroy()
        {
            // Отписаться от событий
            if (_resourceManager != null)
            {
                _resourceManager.OnResourceChanged -= OnResourceChanged;
                _resourceManager.OnResourceTransaction -= OnResourceTransaction;
            }
        }
    }
}

