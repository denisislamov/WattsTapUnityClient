using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Game.Shop
{
    /// <summary>
    /// Configuration for a shop chest item.
    /// Contains display data: name, icon, and two colors for styling.
    /// </summary>
    [CreateAssetMenu(fileName = "ShopChestItemConfig", menuName = "WattsTap/Configs/Shop Chest Item Config")]
    public class ShopChestItemConfig : BaseConfig
    {
        [Header("Display Info")]
        [Tooltip("Name of the chest item")]
        [SerializeField] private string _itemName;
        
        [Tooltip("Icon sprite for the chest item")]
        [SerializeField] private Sprite _icon;
        
        [Header("Chest Sprites")]
        [Tooltip("Sprite for closed chest state")]
        [SerializeField] private Sprite _chestClosedSprite;
        
        [Tooltip("Sprite for open chest state 1")]
        [SerializeField] private Sprite _chestOpenSprite1;
        
        [Tooltip("Sprite for open chest state 2")]
        [SerializeField] private Sprite _chestOpenSprite2;
        
        [Header("Colors")]
        [Tooltip("Primary color for the chest item UI")]
        [SerializeField] private Color _primaryColor = Color.white;
        
        [Tooltip("Secondary color for the chest item UI")]
        [SerializeField] private Color _secondaryColor = Color.gray;

        public string ItemName => _itemName;
        public Sprite Icon => _icon;
        public Sprite ChestClosedSprite => _chestClosedSprite;
        public Sprite ChestOpenSprite1 => _chestOpenSprite1;
        public Sprite ChestOpenSprite2 => _chestOpenSprite2;
        public Color PrimaryColor => _primaryColor;
        public Color SecondaryColor => _secondaryColor;
    }
}

