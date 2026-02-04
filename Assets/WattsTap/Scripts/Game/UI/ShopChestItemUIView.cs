using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.UI;
using WattsTap.Game.Shop;

namespace WattsTap.Game.UI
{
    public class ShopChestItemUIView : UIBaseView<ShopChestItemUIPresenter>
    {
        [Header("Display Elements")]
        [SerializeField] private TMP_Text _itemNameText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _primaryColorImage;
        [SerializeField] private Image _secondaryColorImage;

        [Header("Controls")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _openButton;

        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;

        public Button BackButton => _backButton;
        public Button OpenButton => _openButton;

        public void UpdateFromConfig(ShopChestItemConfig config)
        {
            if (config == null)
            {
                return;
            }

            UpdateItemName(config.ItemName);
            UpdateIcon(config.Icon);
            UpdateColors(config.PrimaryColor, config.SecondaryColor);
        }

        public void UpdateItemName(string itemName)
        {
            if (_itemNameText != null)
            {
                _itemNameText.text = itemName;
            }
        }

        public void UpdateIcon(Sprite icon)
        {
            if (_iconImage != null && icon != null)
            {
                _iconImage.sprite = icon;
            }
        }

        public void UpdateColors(Color primaryColor, Color secondaryColor)
        {
            if (_primaryColorImage != null)
            {
                _primaryColorImage.color = primaryColor;
            }

            if (_secondaryColorImage != null)
            {
                _secondaryColorImage.color = secondaryColor;
            }
        }
        
        private void OnEnable()
        {
            _themeManager = ServiceLocator.Get<MainMenuThemeManager>();
            
            if (_themeManager != null)
            {
                _themeManager.SkinChanged += OnSkinChanged;

                if (_themeManager.CurrentSkin != null)
                {
                    _themeManager.ApplySkin(_themeManager.CurrentSkin, _skinBindings, this);
                    return;
                }
                
                _themeManager.ApplySkin(null, _skinBindings, this);
            }
        }

        private void OnDisable()
        {
            if (_themeManager != null)
            {
                _themeManager.SkinChanged -= OnSkinChanged;
            }
        }
        
        private void OnSkinChanged(MainMenuSkinDefinition skin)
        {
            if (_themeManager != null)
            {
                _themeManager.ApplySkin(skin, _skinBindings, this);
            }
        }
    }
}
