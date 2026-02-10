using System;
using System.Collections.Generic;
using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Scripts.Game.GlobalConfigs
{
    /// <summary>
    /// Конфигурация улучшений (апгрейдов) из WattsBalanceUpgrades.csv
    /// Содержит данные о всех доступных улучшениях и их прогрессии
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradesConfig", menuName = "WattsTap/Configs/Upgrades Config")]
    public class UpgradesConfig : BaseConfig
    {
        [Header("Upgrade Skills")]
        [Tooltip("Список всех доступных апгрейдов")]
        public List<UpgradeSkillData> skills = new List<UpgradeSkillData>();
        
        /// <summary>
        /// Получить данные апгрейда по типу
        /// </summary>
        public UpgradeSkillData GetSkillData(UpgradeType upgradeType)
        {
            return skills.Find(s => s.upgradeType == upgradeType);
        }
        
        /// <summary>
        /// Получить данные конкретного уровня апгрейда
        /// </summary>
        public UpgradeLevelData GetLevelData(UpgradeType upgradeType, int level)
        {
            var skill = GetSkillData(upgradeType);
            if (skill == null || skill.levels == null)
            {
                Debug.LogWarning($"[UpgradesConfig] Skill {upgradeType} not found or has no levels");
                return null;
            }
            
            if (level < 1 || level > skill.levels.Count)
            {
                Debug.LogWarning($"[UpgradesConfig] Level {level} out of range for skill {upgradeType}");
                return null;
            }
            
            return skill.levels[level - 1];
        }
        
        /// <summary>
        /// Получить стоимость апгрейда на конкретном уровне
        /// </summary>
        public long GetUpgradeCost(UpgradeType upgradeType, int currentLevel)
        {
            var levelData = GetLevelData(upgradeType, currentLevel + 1);
            return levelData?.price ?? 0;
        }
        
        /// <summary>
        /// Получить значение параметра на конкретном уровне
        /// </summary>
        public float GetParameterValue(UpgradeType upgradeType, int level)
        {
            var levelData = GetLevelData(upgradeType, level);
            return levelData?.parameter ?? 0f;
        }
        
        /// <summary>
        /// Получить максимальный уровень апгрейда
        /// </summary>
        public int GetMaxLevel(UpgradeType upgradeType)
        {
            var skill = GetSkillData(upgradeType);
            return skill?.levels?.Count ?? 0;
        }
        
        /// <summary>
        /// Валидация конфигурации
        /// </summary>
        private void OnValidate()
        {
            if (skills == null || skills.Count == 0)
            {
                return;
            }
            
            // Проверяем что нет дубликатов типов апгрейдов
            var typeSet = new HashSet<UpgradeType>();
            foreach (var skill in skills)
            {
                if (skill == null) continue;
                
                if (typeSet.Contains(skill.upgradeType))
                {
                    Debug.LogWarning($"[UpgradesConfig] Duplicate upgrade type: {skill.upgradeType}");
                }
                typeSet.Add(skill.upgradeType);
                
                // Проверяем уровни
                if (skill.levels != null)
                {
                    for (int i = 0; i < skill.levels.Count; i++)
                    {
                        var level = skill.levels[i];
                        if (level != null && level.level != i + 1)
                        {
                            Debug.LogWarning($"[UpgradesConfig] {skill.upgradeType}: Level mismatch at index {i}, expected {i + 1}, got {level.level}");
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Типы доступных улучшений
    /// </summary>
    public enum UpgradeType
    {
        GoldHammer,         // Увеличивает профит от каждого удара
        FastTime,           // Уменьшает кулдаун каждого удара
        Endurance,          // Увеличивает вместимость ударов
        CriticalChance,     // Увеличивает шанс получения x2 профита с удара
        CriticalMultiplier, // Увеличивает множитель критического удара
        WorkExperience,     // Увеличивает опыт от каждого удара
        Economist,          // Уменьшает цену апгрейдов
        StrongFriendship,   // Увеличивает бонус от друзей
        Investor,           // Увеличивает доход в час
        ItemMaster,         // Уменьшает цену предметов для повышения уровня
        ShareProfit,        // Увеличивает награду монетами от % профита друзей
        GoldFriends         // Увеличивает награду монетами за приглашенного друга
    }
    
    /// <summary>
    /// Данные одного типа апгрейда (скилла)
    /// </summary>
    [Serializable]
    public class UpgradeSkillData
    {
        [Tooltip("Тип апгрейда")]
        public UpgradeType upgradeType;
        
        [Tooltip("Название навыка")]
        public string title;
        
        [Tooltip("Описание эффекта")]
        public string description;
        
        [Tooltip("Множитель стоимости на каждом уровне")]
        public float stepCostMultiplier = 1.3f;
        
        [Tooltip("Прирост параметра на каждом уровне")]
        public float stepParameter = 0.1f;
        
        [Tooltip("Стартовая стоимость")]
        public long startCost = 1000;
        
        [Tooltip("Стартовое значение параметра")]
        public float startParameter;
        
        [Tooltip("Данные всех уровней этого апгрейда")]
        public List<UpgradeLevelData> levels = new List<UpgradeLevelData>();
        
        /// <summary>
        /// Получить данные конкретного уровня
        /// </summary>
        public UpgradeLevelData GetLevel(int level)
        {
            if (level < 1 || level > levels.Count)
                return null;
            return levels[level - 1];
        }
    }
    
    /// <summary>
    /// Данные одного уровня апгрейда
    /// Соответствует строкам из WattsBalanceUpgrades.csv
    /// </summary>
    [Serializable]
    public class UpgradeLevelData
    {
        [Tooltip("Уровень апгрейда")]
        public int level;
        
        [Tooltip("Стоимость покупки этого уровня")]
        public long price;
        
        [Tooltip("Значение параметра на этом уровне")]
        public float parameter;
        
        /// <summary>
        /// Является ли это стартовым уровнем
        /// </summary>
        public bool IsStartLevel => level == 1;
    }
}
