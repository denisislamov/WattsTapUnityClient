using System.Collections.Generic;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Utility that aggregates all stat bonuses from currently equipped items.
    /// Considers both main stat (level-based) and additional bonuses.
    /// </summary>
    public static class EquippedStatsCalculator
    {
        /// <summary>
        /// Result of calculating bonuses for one stat type.
        /// </summary>
        public struct StatBonus
        {
            /// <summary>Flat bonus (absolute value).</summary>
            public float Flat;
            /// <summary>Percent bonus (0–100 style).</summary>
            public float Percent;

            public bool HasAny => Flat != 0f || Percent != 0f;
        }

        /// <summary>
        /// Calculate total bonuses per StatType from all equipped items.
        /// </summary>
        public static Dictionary<StatType, StatBonus> Calculate(IInventoryService inventoryService)
        {
            var result = new Dictionary<StatType, StatBonus>();

            if (inventoryService == null) return result;

            var equippedItems = inventoryService.GetEquippedItems();
            foreach (var item in equippedItems)
            {
                if (item?.Data == null) continue;

                // 1. Main stat contribution (level-based)
                var mainStat = item.Data.MainStatType;
                if (mainStat != StatType.None)
                {
                    var bonus = GetOrDefault(result, mainStat);
                    bonus.Flat += item.GetCurrentValue();
                    bonus.Percent += item.GetCurrentValuePercent();
                    result[mainStat] = bonus;
                }

                // 2. Additional bonuses
                if (item.Data.Bonuses != null)
                {
                    foreach (var b in item.Data.Bonuses)
                    {
                        if (b == null || b.StatType == StatType.None) continue;

                        var bonus = GetOrDefault(result, b.StatType);
                        bonus.Flat += b.Value;
                        bonus.Percent += b.ValuePercent;
                        result[b.StatType] = bonus;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get bonus for a specific stat type. Returns zero bonus if not present.
        /// </summary>
        public static StatBonus GetBonus(Dictionary<StatType, StatBonus> bonuses, StatType statType)
        {
            if (bonuses != null && bonuses.TryGetValue(statType, out var b))
                return b;
            return default;
        }

        private static StatBonus GetOrDefault(Dictionary<StatType, StatBonus> dict, StatType key)
        {
            if (dict.TryGetValue(key, out var existing))
                return existing;
            return default;
        }
    }
}

