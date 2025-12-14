using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Game.Shop
{
    /// <summary>
    /// Configuration for a shop booster item.
    /// Contains display data: name, icon, and description.
    /// </summary>
    [CreateAssetMenu(fileName = "ShopBoosterItemConfig", menuName = "WattsTap/Configs/Shop Booster Item Config")]
    public class ShopBoosterItemConfig : BaseConfig
    {
        [Header("Display Info")]
        [Tooltip("Name of the booster item")]
        [SerializeField] private string _itemName;
        
        [Tooltip("Icon sprite for the booster item")]
        [SerializeField] private Sprite _icon;
        
        [Tooltip("Description of the booster item")]
        [TextArea(2, 5)]
        [SerializeField] private string _description;

        public string ItemName => _itemName;
        public Sprite Icon => _icon;
        public string Description => _description;
    }
}