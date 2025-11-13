using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.Configs;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Game.Examples
{
    /// <summary>
    /// Пример использования MiningBalanceConfig в игровой логике
    /// </summary>
    public class MiningBalanceUsageExample : MonoBehaviour
    {
        private MiningBalanceConfig _miningConfig;
        private CriticalHitConfig _critConfig;
        
        private void Start()
        {
            // Получаем конфигурации через ConfigService
            var configService = ServiceLocator.Get<IConfigService>();
            
            _miningConfig = configService.GetConfig<MiningBalanceConfig>("MiningBalanceConfig");
            _critConfig = configService.GetConfig<CriticalHitConfig>("CriticalHitConfig");
            
            if (_miningConfig != null)
            {
                ExampleUsage();
            }
        }
        
        private void ExampleUsage()
        {
            // Пример 1: Использование стартовых параметров
            Debug.Log($"<color=cyan>--- Стартовые параметры ---</color>");
            Debug.Log($"Монет за тап: {_miningConfig.coinsPerTap}");
            Debug.Log($"Опыт за тап: {_miningConfig.expPerTap}");
            Debug.Log($"Начальные удары: {_miningConfig.startCapacityHits}");
            Debug.Log($"Шанс крита: {_miningConfig.chanceCritPercent}%");
            Debug.Log($"Множитель крита: {_miningConfig.critMultiplier}x");
            
            // Пример 2: Получение данных конкретного дня
            Debug.Log($"\n<color=cyan>--- Прогрессия День 1 ---</color>");
            var day1 = _miningConfig.GetDayProgression(1);
            Debug.Log($"Тапов за день: {day1.tapsPerDay}");
            Debug.Log($"Опыт за день: {day1.expPerDay}");
            Debug.Log($"Профит за день: {day1.profitCoins}");
            
            // Пример 3: Расчет прогрессии
            Debug.Log($"\n<color=cyan>--- Расчеты ---</color>");
            float profit30Days = _miningConfig.CalculateCumulativeProfit(30);
            long exp30Days = _miningConfig.CalculateCumulativeExp(30);
            Debug.Log($"Накопленный профит за 30 дней: {profit30Days}");
            Debug.Log($"Накопленный опыт за 30 дней: {exp30Days}");
            
            // Пример 4: Симуляция тапа с критом
            Debug.Log($"\n<color=cyan>--- Симуляция тапа ---</color>");
            SimulateTap();
        }
        
        private void SimulateTap()
        {
            long baseDamage = _miningConfig.coinsPerTap;
            
            // Проверяем критический удар
            if (_critConfig != null && _critConfig.RollCriticalHit())
            {
                long critDamage = _critConfig.CalculateCriticalDamage(baseDamage);
                Debug.Log($"<color=yellow>CRITICAL HIT!</color> Урон: {critDamage} (базовый: {baseDamage})");
            }
            else
            {
                Debug.Log($"Обычный удар: {baseDamage}");
            }
        }
        
        /// <summary>
        /// Пример интеграции с PlayerService
        /// </summary>
        private void IntegrateWithPlayerService()
        {
            var playerService = ServiceLocator.Get<Player.IPlayerService>();
            if (playerService == null) return;
            
            var playerData = playerService.GetPlayerData();
            
            // Определяем день игрока (например, на основе createdAt)
            int playerDay = CalculatePlayerDay(playerData.createdAt);
            
            // Получаем ожидаемую прогрессию для этого дня
            var expectedProgress = _miningConfig.GetDayProgression(playerDay);
            
            Debug.Log($"День игрока: {playerDay}");
            Debug.Log($"Ожидаемый профит: {expectedProgress.profitCoins}");
            Debug.Log($"Ожидаемый опыт: {expectedProgress.expPerDay}");
        }
        
        private int CalculatePlayerDay(System.DateTime createdAt)
        {
            var timeSinceCreation = System.DateTime.UtcNow - createdAt;
            int day = (int)timeSinceCreation.TotalDays + 1;
            return Mathf.Clamp(day, 1, 30);
        }
    }
}

