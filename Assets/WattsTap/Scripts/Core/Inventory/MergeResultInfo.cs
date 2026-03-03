using UnityEngine;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Lightweight data describing the expected result of a merge operation.
    /// Used for preview display before the merge is confirmed.
    /// </summary>
    public class MergeResultInfo
    {
        /// <summary>The item type of the result (same as merged items).</summary>
        public ItemType ItemType { get; }

        /// <summary>The next rarity tier (one step above the merged items' rarity).</summary>
        public ItemRarity ResultRarity { get; }

        /// <summary>The maximum level among the merged items.</summary>
        public int Level { get; }

        /// <summary>Display name preview (e.g. "Epic Weapon").</summary>
        public string DisplayName { get; }

        /// <summary>Icon from one of the source items (best available preview).</summary>
        public Sprite Icon { get; }

        public MergeResultInfo(ItemType itemType, ItemRarity resultRarity, int level, string displayName, Sprite icon)
        {
            ItemType = itemType;
            ResultRarity = resultRarity;
            Level = level;
            DisplayName = displayName;
            Icon = icon;
        }

        /// <summary>
        /// Computes the next rarity tier. Returns null if already at max rarity.
        /// </summary>
        public static ItemRarity? GetNextRarity(ItemRarity current)
        {
            switch (current)
            {
                case ItemRarity.Common:   return ItemRarity.Uncommon;
                case ItemRarity.Uncommon: return ItemRarity.Rare;
                case ItemRarity.Rare:     return ItemRarity.Epic;
                case ItemRarity.Epic:     return null; // Already max
                default:                  return null;
            }
        }
    }
}

