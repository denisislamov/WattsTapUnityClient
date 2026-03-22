namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Rarity levels for items.
    /// Values are spaced to allow future insertions.
    /// </summary>
    public enum ItemRarity
    {
        Common = 0,
        Uncommon = 10,
        Rare = 20,
        Epic = 30,
        Legendary = 40,
    }

    public static class ItemRarityExtensions
    {
        /// <summary>
        /// Convert server rarity string (COMMON, UNCOMMON, RARE, LEGENDARY) to enum.
        /// </summary>
        public static ItemRarity FromServerString(string serverRarity)
        {
            if (string.IsNullOrEmpty(serverRarity)) return ItemRarity.Common;
            switch (serverRarity.ToUpperInvariant())
            {
                case "COMMON": return ItemRarity.Common;
                case "UNCOMMON": return ItemRarity.Uncommon;
                case "RARE": return ItemRarity.Rare;
                case "EPIC": return ItemRarity.Epic;
                case "LEGENDARY": return ItemRarity.Legendary;
                default: return ItemRarity.Common;
            }
        }

        /// <summary>
        /// Convert enum to server string.
        /// </summary>
        public static string ToServerString(this ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return "COMMON";
                case ItemRarity.Uncommon: return "UNCOMMON";
                case ItemRarity.Rare: return "RARE";
                case ItemRarity.Epic: return "EPIC";
                case ItemRarity.Legendary: return "LEGENDARY";
                default: return "COMMON";
            }
        }
    }
}
