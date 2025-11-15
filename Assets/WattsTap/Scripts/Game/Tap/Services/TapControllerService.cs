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
        private ResourceConfig _resourceConfig;
        private MiningBalanceConfig _miningBalanceConfig;
        
        public int InitializationOrder => 20;
        public bool IsInitialized { get; private set; }

        public long TotalTaps { get; private set; }
        public int CurrentHits { get; private set; }
        public int MaxHits { get; private set; }
        public float HitRecoverySeconds { get; private set; }

        private float _recoveryTimer;
        private float _idleTimer;
        private float _baseIncomeMultiplier;
        private float _baseOfflineMultiplier;

        // Upgrade multipliers / modifiers
        private float _incomePerTapMultiplier = 1f;
        private float _offlineBonusMultiplier = 1f;

        public event Action<bool> OnTapPerformed;
        public event Action<int, int> OnHitsChanged;
        public event Action<long> OnOfflineBonusChanged;
        public event Action<float> OnIncomeMultiplierChanged;

        public float IncomeMultiplier => _incomePerTapMultiplier;

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

            _resourceConfig = configService.GetConfig<ResourceConfig>("ResourceConfig");
            if (_resourceConfig == null)
            {
                Debug.LogWarning("[TapControllerService] ResourceConfig not found, using defaults");
                _resourceConfig = ScriptableObject.CreateInstance<ResourceConfig>();
            }
            // Apply config
            HitRecoverySeconds = Mathf.Max(0.1f, _config.baseHitRecoverySeconds);
            _baseIncomeMultiplier = Mathf.Max(1f, _config.incomePerTapMultiplier);
            _incomePerTapMultiplier = _baseIncomeMultiplier;
            _baseOfflineMultiplier = Mathf.Max(1f, _config.offlineBonusInitial);
            _offlineBonusMultiplier = _baseOfflineMultiplier;

            // Initialize Hits via ResourceManager to persist across sessions
            var rm = _playerService.ResourceManager;
            var savedMax = rm.GetMaxResource(ResourceType.Hits);
            var configMaxHits = _config.baseMaxHits;
            
            var desiredMax = Math.Max(configMaxHits, (int)savedMax);
            if (desiredMax != savedMax)
            {
                rm.SetMaxResource(ResourceType.Hits, desiredMax);
            }
            // Always fill current hits to max on startup
            rm.SetResource(ResourceType.Hits, desiredMax, true);
            var currentHits = desiredMax;

            MaxHits = (int)rm.GetMaxResource(ResourceType.Hits);
            CurrentHits = currentHits;
            _recoveryTimer = 0f;
            _idleTimer = 0f;
            
            TotalTaps = _playerService.GetPlayerData().stats.totalTaps;

            IsInitialized = true;
            Debug.Log("[TapControllerService] Initialized");

            OnHitsChanged?.Invoke(CurrentHits, MaxHits);
            OnOfflineBonusChanged?.Invoke(CalculateOfflineBonus(_playerService.GetPlayerData().stats.lastLogoutTime));
            OnIncomeMultiplierChanged?.Invoke(_incomePerTapMultiplier);
        }

        public void Shutdown()
        {
            IsInitialized = false;
        }

        public void Update(float deltaTime)
        {
            if (!IsInitialized) return;

            // Idle timer for multiplier reset
            if (_config.multiplierResetSeconds > 0f)
            {
                _idleTimer += deltaTime;
                if (_idleTimer >= _config.multiplierResetSeconds)
                {
                    bool incomeChanged = Math.Abs(_incomePerTapMultiplier - _baseIncomeMultiplier) > 0.0001f;
                    bool offlineChanged = Math.Abs(_offlineBonusMultiplier - _baseOfflineMultiplier) > 0.0001f;
                    _incomePerTapMultiplier = _baseIncomeMultiplier;
                    _offlineBonusMultiplier = _baseOfflineMultiplier;
                    if (incomeChanged) OnIncomeMultiplierChanged?.Invoke(_incomePerTapMultiplier);
                    if (offlineChanged) OnOfflineBonusChanged?.Invoke(CalculateOfflineBonus(_playerService.GetPlayerData().stats.lastLogoutTime));
                    _idleTimer = 0f; // reset timer after applying decay
                }
            }

            var rm = _playerService.ResourceManager;
            CurrentHits = (int)rm.GetResource(ResourceType.Hits);
            MaxHits = (int)rm.GetMaxResource(ResourceType.Hits);

            if (CurrentHits < MaxHits)
            {
                _recoveryTimer += deltaTime;
                if (_recoveryTimer >= HitRecoverySeconds)
                {
                    var recovered = (int)(_recoveryTimer / HitRecoverySeconds);
                    _recoveryTimer -= recovered * HitRecoverySeconds;
                    if (recovered > 0)
                    {
                        rm.AddResource(ResourceType.Hits, recovered);
                        CurrentHits = (int)rm.GetResource(ResourceType.Hits);
                        MaxHits = (int)rm.GetMaxResource(ResourceType.Hits);
                        OnHitsChanged?.Invoke(CurrentHits, MaxHits);
                    }
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
                _idleTimer = 0f; // reset idle on tap
                rm.SpendResource(ResourceType.Hits, 1);
                CurrentHits = (int)rm.GetResource(ResourceType.Hits);
                MaxHits = (int)rm.GetMaxResource(ResourceType.Hits);
                TotalTaps++;

                // Apply income multiplier by adding extra watts equal to (multiplier-1)*income
                if (Math.Abs(_incomePerTapMultiplier - 1f) > 0.0001f)
                {
                    var baseIncome = _playerService.GetPlayerData().stats.incomePerTap;
                    var extra = (long)((_incomePerTapMultiplier - 1f) * baseIncome);
                    
                    if (extra > 0)
                    {
                        _playerService.AddWatts(extra);

                        var expPerTap = _miningBalanceConfig.expPerTap;
                        _playerService.AddExperience(expPerTap);
                    }
                }

                // Grow multipliers per tap within caps
                if (_config.perTapIncomeMultiplierIncrement > 0f)
                {
                    var prev = _incomePerTapMultiplier;
                    _incomePerTapMultiplier = Mathf.Min(_incomePerTapMultiplier + _config.perTapIncomeMultiplierIncrement,
                        Mathf.Max(_config.incomePerTapMultiplierMax, 1f));
                    if (Math.Abs(_incomePerTapMultiplier - prev) > 0.0001f)
                        OnIncomeMultiplierChanged?.Invoke(_incomePerTapMultiplier);
                }
                if (_config.perTapOfflineMultiplierIncrement > 0f)
                {
                    _offlineBonusMultiplier = Mathf.Min(_offlineBonusMultiplier + _config.perTapOfflineMultiplierIncrement,
                        Mathf.Max(_config.offlineMultiplierMax, 1f));
                    OnOfflineBonusChanged?.Invoke(CalculateOfflineBonus(_playerService.GetPlayerData().stats.lastLogoutTime));
                }

                OnHitsChanged?.Invoke(CurrentHits, MaxHits);
                OnTapPerformed?.Invoke(true);
                return true;
            }

            OnTapPerformed?.Invoke(false);
            return false;
        }

        public long CalculateOfflineBonus(DateTime lastLogoutUtc)
        {
            var now = DateTime.UtcNow;
            var diff = now - lastLogoutUtc;
            var maxHours = _resourceConfig.maxOfflineIncomeHours;
            var hours = Math.Min(diff.TotalHours, maxHours);

            var baseIncome = (long)(hours * _playerService.GetPlayerData().stats.incomePerHour * _resourceConfig.offlineIncomeMultiplier);
            var total = (long)(baseIncome * _offlineBonusMultiplier);

            return total;
        }

        public void ApplyUpgrade(TapUpgradeType type, float value)
        {
            switch (type)
            {
                case TapUpgradeType.IncomePerTapPercent:
                    _incomePerTapMultiplier += value;
                    _baseIncomeMultiplier += value; // persist upgrade into baseline so reset keeps upgrades
                    OnIncomeMultiplierChanged?.Invoke(_incomePerTapMultiplier);
                    break;
                case TapUpgradeType.MaxHitsFlat:
                    var rm = _playerService.ResourceManager;
                    var newMax = (int)(rm.GetMaxResource(ResourceType.Hits) + value);
                    rm.SetMaxResource(ResourceType.Hits, newMax);
                    MaxHits = newMax;
                    CurrentHits = (int)rm.GetResource(ResourceType.Hits);
                    OnHitsChanged?.Invoke(CurrentHits, MaxHits);
                    break;
                case TapUpgradeType.HitRecoveryPercent:
                    // value = -0.2f for -20%
                    HitRecoverySeconds *= (1f + value);
                    HitRecoverySeconds = Mathf.Max(0.1f, HitRecoverySeconds);
                    break;
                case TapUpgradeType.OfflineBonusPercent:
                    _offlineBonusMultiplier += value;
                    _baseOfflineMultiplier += value; // persist into baseline
                    OnOfflineBonusChanged?.Invoke(CalculateOfflineBonus(_playerService.GetPlayerData().stats.lastLogoutTime));
                    break;
            }
        }
    }
}
