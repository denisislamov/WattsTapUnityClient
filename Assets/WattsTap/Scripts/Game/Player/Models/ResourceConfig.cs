using System;
using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Game.Player
{
    /// <summary>
    /// Конфигурация параметров ресурсов
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceConfig", menuName = "WattsTap/Configs/Resource Config")]
    public class ResourceConfig : BaseConfig
    {
        [Header("Energy Settings")]
        [Tooltip("Базовая максимальная энергия")]
        public int baseMaxEnergy = 100;
        
        [Tooltip("Время восстановления 1 единицы энергии (в секундах)")]
        public float energyRestoreInterval = 1f;
        
        [Tooltip("Энергия восстанавливается постоянно")]
        public bool energyAutoRestore = true;

        [Header("Experience Settings")]
        [Tooltip("Базовый опыт для перехода на уровень 2")]
        public long baseXpForNextLevel = 100;
        
        [Tooltip("Множитель роста опыта для следующих уровней")]
        public float xpGrowthMultiplier = 1.5f;

        [Header("Tap Settings")]
        [Tooltip("Базовый доход за один тап")]
        public long baseIncomePerTap = 1;
        
        [Tooltip("Опыт получаемый за один тап")]
        public long xpPerTap = 1;
        
        [Tooltip("Стоимость энергии за один тап")]
        public int energyCostPerTap = 1;

        [Header("Offline Income Settings")]
        [Tooltip("Максимальное количество часов оффлайн дохода")]
        public int maxOfflineIncomeHours = 4;
        
        [Tooltip("Процент от активного дохода в час для оффлайн дохода (0-1)")]
        [Range(0f, 1f)]
        public float offlineIncomeMultiplier = 0.5f;

        [Header("Level Up Rewards")]
        [Tooltip("Множитель награды в Watts за повышение уровня (уровень * множитель)")]
        public int levelUpWattsMultiplier = 100;
        
        [Tooltip("Восстанавливать ли энергию до максимума при повышении уровня")]
        public bool restoreEnergyOnLevelUp = true;

        /// <summary>
        /// Рассчитать опыт необходимый для следующего уровня
        /// </summary>
        public long CalculateXpForLevel(int level)
        {
            return (long)(baseXpForNextLevel * Math.Pow(level, xpGrowthMultiplier));
        }

        /// <summary>
        /// Рассчитать награду в Watts за достижение уровня
        /// </summary>
        public long CalculateLevelUpReward(int level)
        {
            return level * levelUpWattsMultiplier;
        }
    }
}

