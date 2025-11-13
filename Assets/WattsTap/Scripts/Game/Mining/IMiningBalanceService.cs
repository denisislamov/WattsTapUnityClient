using System;
using WattsTap.Game.Player;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Scripts.Game.Mining
{
    /// <summary>
    /// Интерфейс сервиса управления балансом майнинга
    /// </summary>
    public interface IMiningBalanceService : Core.IService
    {
        /// <summary>
        /// Текущий день прогрессии
        /// </summary>
        int CurrentDay { get; }
        
        /// <summary>
        /// Данные прогресса майнинга
        /// </summary>
        MiningProgressData ProgressData { get; }
        
        /// <summary>
        /// Событие изменения дня
        /// </summary>
        event Action<int> OnDayChanged;
        
        /// <summary>
        /// Событие изменения прогресса
        /// </summary>
        event Action<MiningProgressData> OnProgressChanged;
        
        /// <summary>
        /// Событие лимитов дня (например, достигнут лимит тапов)
        /// </summary>
        event Action<string> OnDayLimitReached;
        
        /// <summary>
        /// Установить текущий день (для тестирования)
        /// </summary>
        void SetDay(int day);
        
        /// <summary>
        /// Обработать тап с учетом лимитов дня
        /// </summary>
        bool ProcessTap(out long coins, out long exp);
        
        /// <summary>
        /// Проверить, можно ли тапать (не достигнут ли лимит)
        /// </summary>
        bool CanTap();
        
        /// <summary>
        /// Получить оставшиеся тапы на сегодня
        /// </summary>
        int GetRemainingTapsToday();
        
        /// <summary>
        /// Получить ожидаемую прогрессию для текущего дня
        /// </summary>
        DailyProgressionData GetCurrentDayProgression();
        
        /// <summary>
        /// Сбросить все данные прогресса
        /// </summary>
        void ResetProgress();
        
        /// <summary>
        /// Сохранить данные
        /// </summary>
        void SaveProgress();
        
        /// <summary>
        /// Загрузить данные
        /// </summary>
        void LoadProgress();
    }
}
