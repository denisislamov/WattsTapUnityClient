namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Types of stats that items can provide.
    /// Matches the server stat types from game/items/catalog.
    /// </summary>
    public enum StatType
    {
        None = 0,
        CoinsPerTap = 1,
        XpPerTap = 2,
        CapacityHits = 3,
        RecoverHitsPerSecond = 4,
        CritChance = 5,
        CritMultiplier = 6,
        OfflineBonusPercent = 7,
    }

    public static class StatTypeExtensions
    {
        /// <summary>
        /// Convert server stat type string to enum.
        /// </summary>
        public static StatType FromServerString(string serverStatType)
        {
            if (string.IsNullOrEmpty(serverStatType)) return StatType.None;
            switch (serverStatType.ToUpperInvariant())
            {
                case "COINS_PER_TAP":           return StatType.CoinsPerTap;
                case "XP_PER_TAP":              return StatType.XpPerTap;
                case "CAPACITY_HITS":           return StatType.CapacityHits;
                case "RECOVER_HITS_PER_SECOND": return StatType.RecoverHitsPerSecond;
                case "CRIT_CHANCE":             return StatType.CritChance;
                case "CRIT_MULTIPLIER":         return StatType.CritMultiplier;
                case "OFFLINE_BONUS_PERCENT":   return StatType.OfflineBonusPercent;
                default:                        return StatType.None;
            }
        }

        /// <summary>
        /// Convert enum to server string.
        /// </summary>
        public static string ToServerString(this StatType statType)
        {
            switch (statType)
            {
                case StatType.CoinsPerTap:          return "COINS_PER_TAP";
                case StatType.XpPerTap:             return "XP_PER_TAP";
                case StatType.CapacityHits:         return "CAPACITY_HITS";
                case StatType.RecoverHitsPerSecond: return "RECOVER_HITS_PER_SECOND";
                case StatType.CritChance:           return "CRIT_CHANCE";
                case StatType.CritMultiplier:       return "CRIT_MULTIPLIER";
                case StatType.OfflineBonusPercent:  return "OFFLINE_BONUS_PERCENT";
                default:                            return "";
            }
        }

        /// <summary>
        /// Short display name for UI.
        /// </summary>
        public static string ToDisplayName(this StatType statType)
        {
            switch (statType)
            {
                case StatType.CoinsPerTap:          return "Coins/Tap";
                case StatType.XpPerTap:             return "XP/Tap";
                case StatType.CapacityHits:         return "Capacity";
                case StatType.RecoverHitsPerSecond: return "Recovery";
                case StatType.CritChance:           return "Crit Chance";
                case StatType.CritMultiplier:       return "Crit Mult";
                case StatType.OfflineBonusPercent:  return "Offline Bonus";
                default:                            return "Unknown";
            }
        }
    }
}

