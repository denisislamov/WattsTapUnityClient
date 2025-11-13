using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Scripts.Game.GlobalConfigs
{
    /// <summary>
    /// Конфигурация системы критических ударов
    /// Основана на параметрах из WattsBalanceMining.csv
    /// </summary>
    [CreateAssetMenu(fileName = "CriticalHitConfig", menuName = "WattsTap/Configs/Critical Hit Config")]
    public class CriticalHitConfig : BaseConfig
    {
        [Header("Critical Hit Settings")]
        [Tooltip("Базовый шанс критического удара (0-100%)")]
        [Range(0f, 100f)]
        public float baseCritChance = 1f;
        
        [Tooltip("Базовый множитель критического урона")]
        [Range(1f, 10f)]
        public float baseCritMultiplier = 1.2f;
        
        [Header("Visual & Audio")]
        [Tooltip("Цвет текста при критическом ударе")]
        public Color criticalHitColor = new Color(1f, 0.8f, 0f); // Золотой
        
        [Tooltip("Множитель размера текста при крите")]
        public float criticalTextSizeMultiplier = 1.5f;
        
        [Tooltip("Использовать спецэффекты при крите")]
        public bool useCriticalEffects = true;
        
        [Tooltip("Использовать звук при крите")]
        public bool useCriticalSound = true;

        /// <summary>
        /// Проверяет, произошел ли критический удар
        /// </summary>
        public bool RollCriticalHit(float bonusCritChance = 0f)
        {
            float totalChance = baseCritChance + bonusCritChance;
            totalChance = Mathf.Clamp(totalChance, 0f, 100f);
            
            float roll = UnityEngine.Random.Range(0f, 100f);
            return roll < totalChance;
        }
        
        /// <summary>
        /// Вычисляет урон с учетом критического множителя
        /// </summary>
        public long CalculateCriticalDamage(long baseDamage, float bonusMultiplier = 0f)
        {
            float totalMultiplier = baseCritMultiplier + bonusMultiplier;
            return (long)(baseDamage * totalMultiplier);
        }
        
        /// <summary>
        /// Получить эффективный шанс крита с учетом бонусов
        /// </summary>
        public float GetEffectiveCritChance(float bonusCritChance = 0f)
        {
            return Mathf.Clamp(baseCritChance + bonusCritChance, 0f, 100f);
        }
        
        /// <summary>
        /// Получить эффективный множитель крита с учетом бонусов
        /// </summary>
        public float GetEffectiveCritMultiplier(float bonusMultiplier = 0f)
        {
            return Mathf.Max(1f, baseCritMultiplier + bonusMultiplier);
        }

        private void OnValidate()
        {
            baseCritChance = Mathf.Clamp(baseCritChance, 0f, 100f);
            baseCritMultiplier = Mathf.Max(1f, baseCritMultiplier);
            criticalTextSizeMultiplier = Mathf.Max(1f, criticalTextSizeMultiplier);
        }
    }
}
