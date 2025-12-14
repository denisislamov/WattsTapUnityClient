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
        
        [Header("Colors")]
        [Tooltip("Primary color for the chest item UI")]
        [SerializeField] private Color _primaryColor = Color.white;
        
        [Tooltip("Secondary color for the chest item UI")]
        [SerializeField] private Color _secondaryColor = Color.gray;

        public string ItemName => _itemName;
        public Sprite Icon => _icon;
        public Color PrimaryColor => _primaryColor;
        public Color SecondaryColor => _secondaryColor;
    }
}

