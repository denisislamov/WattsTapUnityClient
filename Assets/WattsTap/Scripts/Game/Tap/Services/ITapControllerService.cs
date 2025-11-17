using System;

namespace WattsTap.Game.Tap.Services
{
    /// <summary>
    /// Сервис контроллера тапов — чистая игровая логика без UI.
    /// Отвечает за лимит «ударов», восстановление ударов, оффлайн-бонусы и применение апгрейдов, влияющих на поведение тапов.
    /// Делегирует фактическое начисление ресурсов в IPlayerService (PlayerService.PerformTap()).
    /// </summary>
    public interface ITapControllerService : Core.IService
    {
        /// <summary>
        /// Общее количество выполненных тапов
        /// </summary>
        long TotalTaps { get; }

        /// <summary>
        /// Текущее число доступных ударов (consume on tap)
        /// </summary>
        int CurrentHits { get; }

        /// <summary>
        /// Максимальное число ударов
        /// </summary>
        int MaxHits { get; }

        /// <summary>
        /// Выполнить тап. Возвращает true, если тап применён (энергия была потрачена и ресурсы начислены).
        /// </summary>
        bool HandleTap();

        /// <summary>
        /// Обновление сервиса (для восстановления ударов и учета таймеров) — должен вызываться извне (например из MonoBehaviour.Update)
        /// </summary>
        void Update(float deltaTime);

        // /// <summary>
        // /// Рассчитать оффлайн-бонус в ваттах, на основании времени оффлайн и внутренних бонусов тапового контроллера.
        // /// </summary>
        // long CalculateOfflineBonus(DateTime lastLogoutUtc);

        // /// <summary>
        // /// Применить апгрейд, влияющий на поведение тапов (увеличение дохода за тап, увеличение MaxHits или снижение HitRecoverySeconds и т.п.)
        // /// value — размер увеличения/множителя в зависимости от типа апгрейда
        // /// </summary>
        // void ApplyUpgrade(TapUpgradeType type, float value);

        
        /// <summary>
        /// Событие изменения множителя дохода за тап
        /// </summary>
         event Action<float> OnIncomeMultiplierChanged;
        
        // event Action<bool> OnTapPerformed; // success
         event Action<int, int> OnHitsChanged; // current, max
        // event Action<long> OnOfflineBonusChanged; // when internal offline-bonus metric changed
    }
}
