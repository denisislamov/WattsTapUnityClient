using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.Configs;
using WattsTap.Scripts.Game.Mining;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Game.Examples
{
    /// <summary>
    /// Пример использования MiningBalanceService в игровой логике
    /// </summary>
    public class MiningBalanceUsageExample : MonoBehaviour
    {
        private IMiningBalanceService _miningService;
        private MiningBalanceConfig _miningConfig;
        private CriticalHitConfig _critConfig;
        
        private void Start()
        {
            // Получаем сервисы
            _miningService = ServiceLocator.Get<IMiningBalanceService>();
            
            var configService = ServiceLocator.Get<IConfigService>();
            _miningConfig = configService.GetConfig<MiningBalanceConfig>("MiningBalanceConfig");
            _critConfig = configService.GetConfig<CriticalHitConfig>("CriticalHitConfig");
            
            if (_miningService != null)
            {
                // Подписываемся на события
                _miningService.OnDayChanged += OnDayChanged;
                _miningService.OnProgressChanged += OnProgressChanged;
                _miningService.OnDayLimitReached += OnDayLimitReached;
                
                ExampleUsage();
            }
        }

        private void OnDestroy()
        {
            if (_miningService != null)
            {
                _miningService.OnDayChanged -= OnDayChanged;
                _miningService.OnProgressChanged -= OnProgressChanged;
                _miningService.OnDayLimitReached -= OnDayLimitReached;
            }
        }

        private void ExampleUsage()
        {
            Debug.Log($"<color=cyan>--- Mining Balance Service Example ---</color>");
            
            // Пример 1: Получение текущего состояния
            Debug.Log($"\n<color=yellow>Current State:</color>");
            Debug.Log($"Current Day: {_miningService.CurrentDay}");
            Debug.Log($"Total Taps: {_miningService.ProgressData.totalTaps}");
            Debug.Log($"Taps Today: {_miningService.ProgressData.tapsToday}");
            Debug.Log($"Remaining Taps: {_miningService.GetRemainingTapsToday()}");
            
            // Пример 2: Получение лимитов текущего дня
            var dayProgression = _miningService.GetCurrentDayProgression();
            if (dayProgression != null)
            {
                Debug.Log($"\n<color=yellow>Day {_miningService.CurrentDay} Limits:</color>");
                Debug.Log($"Max Taps Per Day: {dayProgression.tapsPerDay}");
                Debug.Log($"Expected Coins: {dayProgression.coinsFromTaps}");
                Debug.Log($"Expected Exp: {dayProgression.expPerDay}");
            }
            
            // Пример 3: Симуляция тапа
            Debug.Log($"\n<color=yellow>Tap Simulation:</color>");
            SimulateTap();
        }
        
        private void SimulateTap()
        {
            if (_miningService.CanTap())
            {
                if (_miningService.ProcessTap(out long coins, out long exp))
                {
                    Debug.Log($"<color=green>Tap successful!</color> Coins: +{coins}, Exp: +{exp}");
                    Debug.Log($"Remaining taps today: {_miningService.GetRemainingTapsToday()}");
                }
            }
            else
            {
                Debug.Log("<color=red>Cannot tap - daily limit reached!</color>");
            }
        }

        // Обработчики событий
        private void OnDayChanged(int newDay)
        {
            Debug.Log($"<color=cyan>Day changed to: {newDay}</color>");
        }

        private void OnProgressChanged(MiningProgressData progress)
        {
            Debug.Log($"Progress updated - Total taps: {progress.totalTaps}, Today: {progress.tapsToday}");
        }

        private void OnDayLimitReached(string message)
        {
            Debug.Log($"<color=orange>Limit reached: {message}</color>");
        }

        // Пример методов для UI
        [ContextMenu("Set Day 1")]
        public void SetDay1() => _miningService?.SetDay(1);

        [ContextMenu("Set Day 5")]
        public void SetDay5() => _miningService?.SetDay(5);

        [ContextMenu("Set Day 30")]
        public void SetDay30() => _miningService?.SetDay(30);

        [ContextMenu("Reset Progress")]
        public void ResetProgress() => _miningService?.ResetProgress();

        [ContextMenu("Save Progress")]
        public void SaveProgress() => _miningService?.SaveProgress();

        [ContextMenu("Simulate 10 Taps")]
        public void Simulate10Taps()
        {
            for (int i = 0; i < 10; i++)
            {
                if (!_miningService.CanTap()) break;
                _miningService.ProcessTap(out _, out _);
            }
        }
    }
}
