using System;
using System.Collections.Generic;
using UnityEngine;

namespace WattsTap.Scripts.Game.GlobalConfigs
{
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
        
        [Tooltip("Иконка апгрейда для отображения в UI")]
        public Sprite icon;
        
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
}