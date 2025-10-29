using System;
using UnityEngine;
using WattsTap.Core;
using WattsTap.Scripts.Game.Tap.Data;
using WattsTap.Game.Player;

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

        public int InitializationOrder => 20;
        public bool IsInitialized { get; private set; }

        public long TotalTaps { get; private set; }
        public int CurrentHits { get; private set; }
        public int MaxHits { get; private set; }
        public float HitRecoverySeconds { get; private set; }

        private float _recoveryTimer;

        // Upgrade multipliers / modifiers
        private float _incomePerTapMultiplier = 1f;
        private float _offlineBonusMultiplier = 1f;

        public event Action<bool> OnTapPerformed;
        public event Action<int, int> OnHitsChanged;
        public event Action<long> OnOfflineBonusChanged;

        public void Initialize()
        {
            if (IsInitialized) return;

            // Resolve player service
            _playerService = ServiceLocator.Get<IPlayerService>();

            // Load config scriptable object if present
            _config = Resources.Load<TapConfig>("Configs/TapConfig");
            if (_config == null)
            {
                Debug.LogWarning("[TapControllerService] TapConfig not found, using defaults");
                _config = ScriptableObject.CreateInstance<TapConfig>();
            }

            MaxHits = _config.baseMaxHits;
            CurrentHits = MaxHits;
            HitRecoverySeconds = _config.baseHitRecoverySeconds;
            _recoveryTimer = 0f;

            TotalTaps = _playerService.GetPlayerData().stats.totalTaps;

            IsInitialized = true;
            Debug.Log("[TapControllerService] Initialized");

            OnHitsChanged?.Invoke(CurrentHits, MaxHits);
            OnOfflineBonusChanged?.Invoke(CalculateOfflineBonus(_playerService.GetPlayerData().stats.lastLogoutTime));
        }

        public void Shutdown()
        {
            IsInitialized = false;
        }

        public void Update(float deltaTime)
        {
            if (!IsInitialized) return;

            if (CurrentHits < MaxHits)
            {
                _recoveryTimer += deltaTime;
                if (_recoveryTimer >= HitRecoverySeconds)
                {
                    var recovered = (int)(_recoveryTimer / HitRecoverySeconds);
                    _recoveryTimer -= recovered * HitRecoverySeconds;
                    CurrentHits = Math.Min(MaxHits, CurrentHits + recovered);
                    OnHitsChanged?.Invoke(CurrentHits, MaxHits);
                }
            }
        }

        public bool HandleTap()
        {
            if (!IsInitialized) return false;

            if (CurrentHits <= 0)
            {
                OnTapPerformed?.Invoke(false);
                return false;
            }

            // Attempt to perform tap in player service (handles energy and resource awarding)
            var success = _playerService.PerformTap();
            if (success)
            {
                CurrentHits--;
                TotalTaps++;

                // Apply income multiplier by temporarily adjusting player stats: we add additional watts equal to (multiplier-1)*income
                if (Math.Abs(_incomePerTapMultiplier - 1f) > 0.0001f)
                {
                    var extra = (long)((_incomePerTapMultiplier - 1f) * (_playerService.GetPlayerData().stats.incomePerTap));
                    if (extra > 0) _playerService.AddWatts(extra);
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
            var lastLogout = lastLogoutUtc;
            var now = DateTime.UtcNow;
            var diff = now - lastLogout;
            var maxHours = _config.maxOfflineIncomeHours;
            var hours = Math.Min(diff.TotalHours, maxHours);

            var baseIncome = (long)(hours * _playerService.GetPlayerData().stats.incomePerHour * _config.offlineIncomeBaseMultiplier);
            var total = (long)(baseIncome * _offlineBonusMultiplier);

            return total;
        }

        public void ApplyUpgrade(TapUpgradeType type, float value)
        {
            switch (type)
            {
                case TapUpgradeType.IncomePerTapPercent:
                    _incomePerTapMultiplier += value;
                    break;
                case TapUpgradeType.MaxHitsFlat:
                    MaxHits += (int)value;
                    CurrentHits = Math.Min(CurrentHits, MaxHits);
                    OnHitsChanged?.Invoke(CurrentHits, MaxHits);
                    break;
                case TapUpgradeType.HitRecoveryPercent:
                    // value = -0.2f for -20%
                    HitRecoverySeconds *= (1f + value);
                    break;
                case TapUpgradeType.OfflineBonusPercent:
                    _offlineBonusMultiplier += value;
                    OnOfflineBonusChanged?.Invoke(CalculateOfflineBonus(_playerService.GetPlayerData().stats.lastLogoutTime));
                    break;
            }
        }
    }
}
