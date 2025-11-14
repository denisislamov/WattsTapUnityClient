using System;
using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Scripts.Game.GlobalConfigs
{
    /// <summary>
    /// Конфигурация уровней игрока из WattsBalanceLevelupProfile.csv
    /// Содержит данные о прогрессии уровней и наградах
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerLevelConfig", menuName = "WattsTap/Configs/Player Level Config")]
    public class PlayerLevelConfig : BaseConfig
    {
        [Header("Level Progression")]
        [Tooltip("Данные о всех уровнях игрока")]
        public PlayerLevelData[] levels = new PlayerLevelData[100];

        /// <summary>
        /// Получить данные для конкретного уровня
        /// </summary>
        public PlayerLevelData GetLevelData(int level)
        {
            if (level < 1 || level > levels.Length)
            {
                Debug.LogWarning($"[PlayerLevelConfig] Level {level} out of range. Returning default values.");
                return new PlayerLevelData();
            }
            
            return levels[level - 1];
        }
        
        /// <summary>
        /// Получить необходимый опыт для достижения уровня
        /// </summary>
        public long GetExpRequiredForLevel(int level)
        {
            var levelData = GetLevelData(level);
            return levelData.needExp;
        }
        
        /// <summary>
        /// Получить суммарный опыт для достижения уровня
        /// </summary>
        public long GetTotalExpForLevel(int level)
        {
            var levelData = GetLevelData(level);
            return levelData.sumExp;
        }
        
        /// <summary>
        /// Получить награды за достижение уровня
        /// </summary>
        public LevelRewards GetRewardsForLevel(int level)
        {
            var levelData = GetLevelData(level);
            return levelData.rewards;
        }
        
        /// <summary>
        /// Определить текущий уровень по опыту
        /// </summary>
        public int GetLevelByExp(long currentExp)
        {
            for (int i = levels.Length - 1; i >= 0; i--)
            {
                if (currentExp >= levels[i].sumExp)
                {
                    return levels[i].level;
                }
            }
            
            return 1; // Минимальный уровень
        }
        
        /// <summary>
        /// Получить прогресс до следующего уровня (0-1)
        /// </summary>
        public float GetProgressToNextLevel(long currentExp, int currentLevel)
        {
            if (currentLevel >= levels.Length)
            {
                return 1f; // Максимальный уровень достигнут
            }
            
            var currentLevelData = GetLevelData(currentLevel);
            var nextLevelData = GetLevelData(currentLevel + 1);
            
            long expInCurrentLevel = currentExp - currentLevelData.sumExp;
            long expNeededForNext = nextLevelData.needExp;
            
            if (expNeededForNext <= 0)
            {
                return 1f;
            }
            
            return Mathf.Clamp01((float)expInCurrentLevel / expNeededForNext);
        }

        /// <summary>
        /// Валидация конфигурации
        /// </summary>
        private void OnValidate()
        {
            if (levels == null || levels.Length == 0)
            {
                return;
            }
            
            // Проверка что уровни идут последовательно
            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i] != null && levels[i].level != i + 1)
                {
                    Debug.LogWarning($"[PlayerLevelConfig] Level mismatch at index {i}: expected {i + 1}, got {levels[i].level}");
                }
            }
        }
    }

    /// <summary>
    /// Данные одного уровня игрока
    /// Соответствует строкам из WattsBalanceLevelupProfile.csv
    /// </summary>
    [Serializable]
    public class PlayerLevelData
    {
        [Tooltip("Номер уровня")]
        public int level;
        
        [Tooltip("Коэффициент опыта для следующего уровня")]
        public float expCoefficient = 1.0f;
        
        [Tooltip("Опыт необходимый для достижения этого уровня")]
        public long needExp;
        
        [Tooltip("Суммарный опыт на этом уровне")]
        public long sumExp;
        
        [Tooltip("Награды за достижение уровня")]
        public LevelRewards rewards = new LevelRewards();
    }

    /// <summary>
    /// Награды за достижение уровня
    /// </summary>
    [Serializable]
    public class LevelRewards
    {
        [Tooltip("Монеты")]
        public long coins;
        
        [Tooltip("Розыгрыши")]
        public int draws;
        
        [Tooltip("Обычные сундуки (Common)")]
        public int commonChests;
        
        [Tooltip("Необычные сундуки (Uncommon)")]
        public int uncommonChests;
        
        [Tooltip("Редкие сундуки (Rare)")]
        public int rareChests;
        
        [Tooltip("Легендарные сундуки (Legendary)")]
        public int legendaryChests;

        /// <summary>
        /// Проверка есть ли какие-то награды
        /// </summary>
        public bool HasAnyRewards()
        {
            return coins > 0 || draws > 0 || commonChests > 0 || 
                   uncommonChests > 0 || rareChests > 0 || legendaryChests > 0;
        }
        
        /// <summary>
        /// Получить общее количество сундуков
        /// </summary>
        public int GetTotalChests()
        {
            return commonChests + uncommonChests + rareChests + legendaryChests;
        }
    }
}

