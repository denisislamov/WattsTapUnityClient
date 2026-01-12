using UnityEngine;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Scriptable Object representing a single item definition in the catalog.
    /// Contains all static data about an item.
    /// </summary>
    [CreateAssetMenu(menuName = "WattsTap/Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this item. Must be unique across all items.")]
        [SerializeField] private string _id;
        
        [Tooltip("Display name of the item")]
        [SerializeField] private string _displayName;
        
        [Tooltip("Item description")]
        [TextArea(2, 5)]
        [SerializeField] private string _description;
        
        [Header("Classification")]
        [SerializeField] private ItemType _itemType = ItemType.None;
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;
        
        [Header("Requirements")]
        [Tooltip("Minimum level required to use this item")]
        [SerializeField] private int _requiredLevel = 1;
        
        [Header("Visual")]
        [SerializeField] private Sprite _icon;
        
        [Header("Stats")]
        [Tooltip("Bonus added per tap when this item is equipped")]
        [SerializeField] private float _perTapBonus;
        
        /// <summary>Unique identifier for this item</summary>
        public string Id => _id;
        
        /// <summary>Display name of the item</summary>
        public string DisplayName => _displayName;
        
        /// <summary>Item description</summary>
        public string Description => _description;
        
        /// <summary>Type of the item</summary>
        public ItemType ItemType => _itemType;
        
        /// <summary>Rarity of the item</summary>
        public ItemRarity Rarity => _rarity;
        
        /// <summary>Minimum level required to use this item</summary>
        public int RequiredLevel => _requiredLevel;
        
        /// <summary>Item icon sprite</summary>
        public Sprite Icon => _icon;
        
        /// <summary>Bonus added per tap when this item is equipped</summary>
        public float PerTapBonus => _perTapBonus;
        
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
        }
    }
}
