using System.Collections.Generic;
using UnityEngine;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Scriptable Object representing a single item variant in the catalog.
    /// On the server, one "item template" has multiple variants (one per rarity).
    /// Each variant is represented by one ItemData instance.
    /// Contains all static data about an item variant.
    /// </summary>
    [CreateAssetMenu(menuName = "WattsTap/Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Server Identification")]
        [Tooltip("Variant UUID from server (unique per rarity variant)")]
        [SerializeField] private string _id;

        [Tooltip("Template UUID from server (shared by all rarity variants of the same item)")]
        [SerializeField] private string _templateId;

        [Tooltip("Item code from server (e.g. weapon_amp_thumper)")]
        [SerializeField] private string _code;

        [Header("Display")]
        [Tooltip("Display name of the item")]
        [SerializeField] private string _displayName;

        [Tooltip("Item description")]
        [TextArea(2, 5)]
        [SerializeField] private string _description;

        [Header("Classification")]
        [SerializeField] private ItemType _itemType = ItemType.None;
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;

        [Header("Stats")]
        [Tooltip("Primary stat type for this item variant")]
        [SerializeField] private StatType _mainStatType = StatType.None;

        [Tooltip("Maximum level for this variant")]
        [SerializeField] private int _maxLevel = 5;

        [Tooltip("Level progression data")]
        [SerializeField] private List<ItemLevelData> _levels = new List<ItemLevelData>();

        [Tooltip("Additional bonuses (typically for Uncommon+ rarities)")]
        [SerializeField] private List<ItemBonusData> _bonuses = new List<ItemBonusData>();

        [Header("Legacy / Visual")]
        [Tooltip("Minimum level required to use this item")]
        [SerializeField] private int _requiredLevel = 1;

        [SerializeField] private Sprite _icon;

        [Tooltip("Legacy: bonus added per tap. Now derived from levels[0].Value for mainStatType")]
        [SerializeField] private float _perTapBonus;

        // === Public Properties ===

        /// <summary>Variant UUID (unique identifier for this specific rarity variant)</summary>
        public string Id => _id;

        /// <summary>Template UUID (shared by all rarity variants of the same item)</summary>
        public string TemplateId => _templateId;

        /// <summary>Item code from server (e.g. weapon_amp_thumper)</summary>
        public string Code => _code;

        /// <summary>Display name of the item</summary>
        public string DisplayName => _displayName;

        /// <summary>Item description</summary>
        public string Description => _description;

        /// <summary>Type of the item (slot)</summary>
        public ItemType ItemType => _itemType;

        /// <summary>Rarity of the item variant</summary>
        public ItemRarity Rarity => _rarity;

        /// <summary>Primary stat type</summary>
        public StatType MainStatType => _mainStatType;

        /// <summary>Maximum level this variant can reach</summary>
        public int MaxLevel => _maxLevel;

        /// <summary>Level progression data</summary>
        public IReadOnlyList<ItemLevelData> Levels => _levels;

        /// <summary>Additional bonuses</summary>
        public IReadOnlyList<ItemBonusData> Bonuses => _bonuses;

        /// <summary>Minimum level required to use this item</summary>
        public int RequiredLevel => _requiredLevel;

        /// <summary>Item icon sprite</summary>
        public Sprite Icon => _icon;

        /// <summary>Legacy: bonus per tap. Returns level 1 value if available.</summary>
        public float PerTapBonus => _levels != null && _levels.Count > 0 ? _levels[0].Value : _perTapBonus;

        /// <summary>
        /// Get stat value at a specific level.
        /// </summary>
        public ItemLevelData GetLevelData(int level)
        {
            if (_levels == null) return null;
            foreach (var ld in _levels)
            {
                if (ld.Level == level) return ld;
            }
            return null;
        }

        /// <summary>
        /// Get stat value (flat) at a specific level.
        /// </summary>
        public float GetValueAtLevel(int level)
        {
            var ld = GetLevelData(level);
            return ld?.Value ?? 0f;
        }

        /// <summary>
        /// Get stat value (percent) at a specific level.
        /// </summary>
        public float GetValuePercentAtLevel(int level)
        {
            var ld = GetLevelData(level);
            return ld?.ValuePercent ?? 0f;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = name;
            }

            if (_requiredLevel < 1)
            {
                _requiredLevel = 1;
            }

            if (_maxLevel < 1)
            {
                _maxLevel = 1;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Populate this ItemData from server DTOs. Editor only.
        /// </summary>
        public void PopulateFromServer(
            string templateId, string code, string displayName, string description,
            ItemType itemType, string variantId, ItemRarity rarity,
            StatType mainStatType, int maxLevel,
            List<ItemLevelData> levels, List<ItemBonusData> bonuses)
        {
            _templateId = templateId;
            _code = code;
            _displayName = displayName;
            _description = description;
            _itemType = itemType;
            _id = variantId;
            _rarity = rarity;
            _mainStatType = mainStatType;
            _maxLevel = maxLevel;
            _levels = levels ?? new List<ItemLevelData>();
            _bonuses = bonuses ?? new List<ItemBonusData>();
            _perTapBonus = _levels.Count > 0 ? _levels[0].Value : 0f;
            _requiredLevel = 1;
        }

        /// <summary>
        /// Set the icon sprite. Editor only.
        /// </summary>
        public void SetIcon(Sprite icon)
        {
            _icon = icon;
        }
#endif
    }
}
