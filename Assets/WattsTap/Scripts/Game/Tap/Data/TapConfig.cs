using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Scripts.Game.Tap.Data
{
    /// <summary>
    /// Конфиг для TapControllerService. Создайте экземпляр в Resources/Configs/TapConfig (если нужен кастом).
    /// </summary>
    [CreateAssetMenu(menuName = "WattsTap/Configs/TapConfig", fileName = "TapConfig")]
    public class TapConfig : BaseConfig
    {
        [Header("Hits")]
        public int baseMaxHits = 20;
        public float baseHitRecoverySeconds = 3f; // время восстановления одного удара (меньше = быстрее)

        [Header("Income")]
        [Tooltip("Базовый множитель дохода за тап поверх дохода игрока (стартовый)")]
        public float incomePerTapMultiplier = 2f;
        
        [Tooltip("Прирост множителя дохода за каждый успешный тап (например 0.05 = +5%)")]
        public float perTapIncomeMultiplierIncrement = 0.05f;
        
        [Tooltip("Максимальный множитель дохода за тап (ограничение сверху)")]
        public float incomePerTapMultiplierMax = 3f;

        [Header("Misc")]
        [Tooltip("Дополнительный множитель к оффлайн-доходу, специфичный для механики тапов (стартовый)")] 
        public float offlineBonusInitial = 1.5f; // начальный множитель оффлайн
        
        [Tooltip("Прирост оффлайн-множителя за каждый успешный тап (например 0.02 = +2%)")] 
        public float perTapOfflineMultiplierIncrement = 0.02f;
        
        [Tooltip("Максимальный оффлайн-множитель (ограничение сверху)")] 
        public float offlineMultiplierMax = 3f;

        [Header("Decay")]
        [Tooltip("Сбрасывать множители, если не было тапов N секунд (0 или меньше — отключено)")]
        public float multiplierResetSeconds = 5f;
    }
}
