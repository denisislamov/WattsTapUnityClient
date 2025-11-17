using System;
using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.Configs;
using WattsTap.Scripts.Game.Tap.Data;
using WattsTap.Game.Player;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Game.Tap.Services
{
    /// <summary>
    /// Реализация контроллера тапов. Не зависит от UI.
    /// Логика:
    /// - Хранит текущие и максимальные удары (hits) — каждый тап потребляет 1 hit.
    /// - Восстановление хитсов по таймеру (HitRecoverySeconds).
    /// - Увеличивает подсчёт TotalTaps и делегирует фактическое начисление ресурсов в IPlayerService.PerformTap().
    /// - Применяет апгрейды, которые меняют параметры (доход/MaxHits/recovery/offline).
    /// - Рассчитывает оффлайн-бонус с учётом внутр. множителей.
    /// </summary>
    public class TapControllerService : ITapControllerService
    {
        private IPlayerService _playerService;
        private TapConfig _config;
        private MiningBalanceConfig _miningBalanceConfig;
        
        public int InitializationOrder => 20;
        public bool IsInitialized { get; private set; }

        public long TotalTaps { get; private set; }
        public int CurrentHits { get; private set; }
        public int MaxHits => _miningBalanceConfig.startCapacityHits;

        private float _recoveryTimer;

        public float CooldownPerHitSec => _miningBalanceConfig.cooldownPerHitSec;
        // Upgrade multipliers / modifiers
       
        public event Action<bool> OnTapPerformed;
        public event Action<int, int> OnHitsChanged;
        public event Action<long> OnOfflineBonusChanged;
        public event Action<float> OnIncomeMultiplierChanged;
        
        public void Initialize()
        {
            if (IsInitialized) return;

            // Resolve player service
            _playerService = ServiceLocator.Get<IPlayerService>();

            // Load config scriptable object if present
            var configService = ServiceLocator.Get<IConfigService>();
            _config = configService.GetConfig<TapConfig>( "TapConfig");
            _miningBalanceConfig = configService.GetConfig<MiningBalanceConfig>("MiningBalanceConfig");
          
            if (_config == null)
            {
                Debug.LogWarning("[TapControllerService] TapConfig not found, using defaults");
                _config = ScriptableObject.CreateInstance<TapConfig>();
            }
            
            // Initialize Hits via ResourceManager to persist across sessions
            var rm = _playerService.ResourceManager;
            CurrentHits = _miningBalanceConfig.startCapacityHits;
            rm.AddResource(ResourceType.Hits, _miningBalanceConfig.startCapacityHits);
            _recoveryTimer = 0f;
            
            // TotalTaps = _playerService.GetPlayerData().stats.totalTaps;

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
            
            var rm = _playerService.ResourceManager;
            if (!rm.HasEnough(ResourceType.Hits, 1))
            {
                OnTapPerformed?.Invoke(false);
                return false;
            }

            // Attempt to perform tap in player service (handles energy and resource awarding)
            var success = _playerService.PerformTap();
            if (success)
            {
                // _idleTimer = 0f; // reset idle on tap
                rm.SpendResource(ResourceType.Hits, _miningBalanceConfig.energyCostPerTap);
                CurrentHits = (int)rm.GetResource(ResourceType.Hits);
                // TotalTaps++;
                //
                // // Apply income multiplier by adding extra watts equal to (multiplier-1)*income
                // if (Math.Abs(_incomePerTapMultiplier - 1f) > 0.0001f)
                // {
                //     var baseIncome = _playerService.IncomePerTap();
                //     var extra = (long)((_incomePerTapMultiplier - 1f) * baseIncome);
                //     
                //     if (extra > 0)
                //     {
                //         _playerService.AddWatts(extra);
                //
                //         var expPerTap = _miningBalanceConfig.expPerTap;
                //         _playerService.AddExperience(expPerTap);
                //     }
                // }

                // Grow multipliers per tap within caps
                // if (_config.perTapIncomeMultiplierIncrement > 0f)
                // {
                //     var prev = _incomePerTapMultiplier;
                //     _incomePerTapMultiplier = Mathf.Min(_incomePerTapMultiplier + _config.perTapIncomeMultiplierIncrement,
                //         Mathf.Max(_config.incomePerTapMultiplierMax, 1f));
                //     if (Math.Abs(_incomePerTapMultiplier - prev) > 0.0001f)
                //         OnIncomeMultiplierChanged?.Invoke(_incomePerTapMultiplier);
                // }
                // if (_config.perTapOfflineMultiplierIncrement > 0f)
                // {
                //     _offlineBonusMultiplier = Mathf.Min(_offlineBonusMultiplier + _config.perTapOfflineMultiplierIncrement,
                //         Mathf.Max(_config.offlineMultiplierMax, 1f));
                //     OnOfflineBonusChanged?.Invoke(CalculateOfflineBonus(_playerService.GetPlayerData().stats.lastLogoutTime));
                // }

                OnHitsChanged?.Invoke(CurrentHits, MaxHits);
                OnTapPerformed?.Invoke(true);
                return true;
            }

            OnTapPerformed?.Invoke(false);
            return false;
        }

        // public long CalculateOfflineBonus(DateTime lastLogoutUtc)
        // {
        //     var now = DateTime.UtcNow;
        //     var diff = now - lastLogoutUtc;
        //     var maxHours = _resourceConfig.maxOfflineIncomeHours;
        //     var hours = Math.Min(diff.TotalHours, maxHours);
        //
        //     var baseIncome = (long)(hours * _playerService.GetPlayerData().stats.incomePerHour * _resourceConfig.offlineIncomeMultiplier);
        //     var total = (long)(baseIncome * _offlineBonusMultiplier);
        //
        //     return total;
        // }

        // public void ApplyUpgrade(TapUpgradeType type, float value)
        // {
        //     switch (type)
        //     {
        //         case TapUpgradeType.IncomePerTapPercent:
        //             _incomePerTapMultiplier += value;
        //             _baseIncomeMultiplier += value; // persist upgrade into baseline so reset keeps upgrades
        //             OnIncomeMultiplierChanged?.Invoke(_incomePerTapMultiplier);
        //             break;
        //         case TapUpgradeType.MaxHitsFlat:
        //             var rm = _playerService.ResourceManager;
        //             var newMax = (int)(rm.GetMaxResource(ResourceType.Hits) + value);
        //             rm.SetMaxResource(ResourceType.Hits, newMax);
        //             MaxHits = newMax;
        //             CurrentHits = (int)rm.GetResource(ResourceType.Hits);
        //             OnHitsChanged?.Invoke(CurrentHits, MaxHits);
        //             break;
        //         case TapUpgradeType.HitRecoveryPercent:
        //             // value = -0.2f for -20%
        //             HitRecoverySeconds *= (1f + value);
        //             HitRecoverySeconds = Mathf.Max(0.1f, HitRecoverySeconds);
        //             break;
        //         case TapUpgradeType.OfflineBonusPercent:
        //             _offlineBonusMultiplier += value;
        //             _baseOfflineMultiplier += value; // persist into baseline
        //             OnOfflineBonusChanged?.Invoke(CalculateOfflineBonus(_playerService.GetPlayerData().stats.lastLogoutTime));
        //             break;
        //     }
        // }
    }
}
