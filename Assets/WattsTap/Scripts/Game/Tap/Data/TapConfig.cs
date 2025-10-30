using UnityEngine;
using WattsTap.Core.Configs;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Scripts.Game.Tap.Data
{
    /// <summary>
    /// Конфиг для TapControllerService. Создайте экземпляр в Resources/Configs/TapConfig (если нужен кастом).
    /// </summary>
    [CreateAssetMenu(menuName = "WattsTap/Configs/TapConfig", fileName = "TapConfig")]
    public class TapConfig : BaseConfig
    {
        [Header("Hits")]
        public int baseMaxHits = 10;
        public float baseHitRecoverySeconds = 5f; // время восстановления одного удара

        [Header("Offline")]
        public int maxOfflineIncomeHours = 4;
        public float offlineIncomeBaseMultiplier = 1f;

        [Header("Misc")]
        public float offlineBonusInitial = 1f; // начальный множитель оффлайн
    }
}
