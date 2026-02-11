using System;
using UnityEngine;

namespace WattsTap.Scripts.Game.GlobalConfigs
{
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