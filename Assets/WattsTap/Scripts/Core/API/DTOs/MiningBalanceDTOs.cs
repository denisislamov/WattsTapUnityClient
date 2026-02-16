using System;
using System.Collections.Generic;

namespace WattsTap.Core.API
{
    #region Mining Balance DTOs

    /// <summary>
    /// Daily progression data from server.
    /// Field names match server JSON (camelCase) for JsonUtility deserialization.
    /// </summary>
    [Serializable]
    public class DailyProgressionDTO
    {
        public int day;
        public int playtimeSec;
        public int tapsPerSession;
        public int tapsPerDay;
        public long expPerDay;
        public float coinsFromTaps;
        public float coinsFromOfflineBonus;
        public float profitCoins;
        public float cumulativeProfitCoins;
        public long cumulativeExp;
    }

    /// <summary>
    /// Full mining balance configuration from server.
    /// Mirrors the server's MiningBalanceResponse schema.
    /// </summary>
    [Serializable]
    public class MiningBalanceDTO
    {
        // Start Parameters
        public int coinsPerTap;
        public int expPerTap;
        public int energyCostPerTap;
        public int startCapacityHits;
        public float cooldownPerHitSec;
        public float critMultiplier;
        public float chanceCritPercent;
        public int avgPlaytimeMinutes;
        public int tapsPerSecond;
        public int profitPerHour;
        public int maxHoursOffline;
        public int sessionsPerDay;

        // Daily Progression
        public List<DailyProgressionDTO> dailyProgression;
    }

    /// <summary>
    /// Server response wrapping the mining balance.
    /// Matches server's MiningBalancePublicResponse.
    /// </summary>
    [Serializable]
    public class MiningBalancePublicResponse
    {
        public bool success;
        public MiningBalanceDTO balance;
    }

    #endregion
}
