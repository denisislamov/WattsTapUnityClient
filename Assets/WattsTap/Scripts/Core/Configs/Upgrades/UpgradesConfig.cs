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
}
