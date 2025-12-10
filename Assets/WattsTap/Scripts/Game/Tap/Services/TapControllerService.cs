using System;
using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.Configs;
using WattsTap.Core.Telegram;
using WattsTap.Scripts.Game.Tap.Data;
using WattsTap.Game.Player;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Game.Tap.Services
{
    public class TapControllerService : ITapControllerService
    {
        private IPlayerService _playerService;
        private IHapticFeedbackService _hapticService;
        private TapConfig _config;
        private MiningBalanceConfig _miningBalanceConfig;
        private float _recoveryTimer;

        private float CooldownPerHitSec => _miningBalanceConfig.cooldownPerHitSec;

        public int InitializationOrder => 20;
        public bool IsInitialized { get; private set; }

        public long TotalTaps { get; private set; }
        public int CurrentHits { get; private set; }
        public int MaxHits => _miningBalanceConfig.startCapacityHits;
        
        public event Action<bool> OnTapPerformed;
        public event Action<int, int> OnHitsChanged;
        public event Action<long> OnOfflineBonusChanged;
        public event Action<float> OnIncomeMultiplierChanged;
        
        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            _playerService = ServiceLocator.Get<IPlayerService>();
            ServiceLocator.TryGet(out _hapticService);

            var configService = ServiceLocator.Get<IConfigService>();
            _config = configService.GetConfig<TapConfig>( "TapConfig");
            _miningBalanceConfig = configService.GetConfig<MiningBalanceConfig>("MiningBalanceConfig");
          
            if (_config == null)
            {
                Debug.LogWarning("[TapControllerService] TapConfig not found, using defaults");
                _config = ScriptableObject.CreateInstance<TapConfig>();
            }
            
            var resourceManager = _playerService.ResourceManager;
            CurrentHits = _miningBalanceConfig.startCapacityHits;
            resourceManager.AddResource(ResourceType.Hits, _miningBalanceConfig.startCapacityHits);
            _recoveryTimer = 0f;
            
            IsInitialized = true;
            Debug.Log("[TapControllerService] Initialized");

            OnHitsChanged?.Invoke(CurrentHits, MaxHits);
        }

        public void Shutdown()
        {
            IsInitialized = false;
        }

        public void Update(float deltaTime)
        {
            if (!IsInitialized)
            {
                return;
            }
            
            var rm = _playerService.ResourceManager;
            CurrentHits = (int)rm.GetResource(ResourceType.Hits);
            
            if (CurrentHits < MaxHits)
            {
                _recoveryTimer += deltaTime;
                if (_recoveryTimer >= CooldownPerHitSec)
                {
                    _recoveryTimer = 0.0f;

                    rm.AddResource(ResourceType.Hits, 1);
                    CurrentHits = (int)rm.GetResource(ResourceType.Hits);
                    OnHitsChanged?.Invoke(CurrentHits, MaxHits);

                }
            }
        }

        public bool HandleTap()
        {
            if (!IsInitialized) return false;
            
            var resourceManager = _playerService.ResourceManager;
            if (!resourceManager.HasEnough(ResourceType.Hits, 1))
            {
                OnTapPerformed?.Invoke(false);
                return false;
            }

            var success = _playerService.PerformTap();
            if (success)
            {
                // Trigger haptic feedback for successful tap
                _hapticService?.TapPerformed();
                
                // _idleTimer = 0f; // reset idle on tap
                resourceManager.SpendResource(ResourceType.Hits, _miningBalanceConfig.energyCostPerTap);
                CurrentHits = (int)resourceManager.GetResource(ResourceType.Hits);
                OnHitsChanged?.Invoke(CurrentHits, MaxHits);
                OnTapPerformed?.Invoke(true);
                return true;
            }

            OnTapPerformed?.Invoke(false);
            return false;
        }
    }
}
