using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WattsTap.Core;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Game.UI
{
    /// <summary>
    /// View component for displaying a single upgrade item in the updates list
    /// </summary>
    public class UpdatesPopupListElementView : MonoBehaviour
    {
        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;
        
        [Header("Display Elements")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _parameterText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _iconImage;
        
        [Header("Controls")]
        [SerializeField] private Button _upgradeButton;
        
        private UpgradeDisplayData _upgradeData;
        
        #region Skinning
        
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
        
        #endregion
        
        /// <summary>
        /// Setup the view with upgrade information
        /// </summary>
        public void Setup(UpgradeDisplayData upgradeData)
        {
            _upgradeData = upgradeData;
            
            // Set title
            if (_titleText != null)
            {
                _titleText.text = upgradeData.Title ?? "Unknown";
            }
            
            // Set description
            if (_descriptionText != null)
            {
                _descriptionText.text = upgradeData.Description ?? "";
            }
            
            // Set level
            if (_levelText != null)
            {
                _levelText.text = $"Lv. {upgradeData.CurrentLevel}";
            }
            
            // Set parameter value (formatted based on upgrade type)
            if (_parameterText != null)
            {
                _parameterText.text = FormatParameterValue(upgradeData);
            }
            
            // Set price
            if (_priceText != null)
            {
                if (upgradeData.IsMaxLevel)
                {
                    _priceText.text = "MAX";
                }
                else
                {
                    _priceText.text = FormatPrice(upgradeData.NextLevelPrice);
                }
            }
            
            // Set icon
            if (_iconImage != null && upgradeData.Icon != null)
            {
                _iconImage.sprite = upgradeData.Icon;
                _iconImage.enabled = true;
            }
            else if (_iconImage != null)
            {
                _iconImage.enabled = false;
            }
            
            // Setup button state
            if (_upgradeButton != null)
            {
                _upgradeButton.interactable = !upgradeData.IsMaxLevel;
            }
        }
        
        /// <summary>
        /// Format parameter value based on upgrade type
        /// </summary>
        private string FormatParameterValue(UpgradeDisplayData data)
        {
            // Different formatting based on upgrade type
            switch (data.UpgradeType)
            {
                case UpgradeType.CriticalChance:
                case UpgradeType.ShareProfit:
                    // Percentage values
                    return $"{data.CurrentParameterValue:P0}";
                    
                case UpgradeType.CriticalMultiplier:
                    // Multiplier values
                    return $"x{data.CurrentParameterValue:F1}";
                    
                case UpgradeType.FastTime:
                    // Time in seconds
                    return $"{data.CurrentParameterValue:F2}s";
                    
                case UpgradeType.Economist:
                case UpgradeType.ItemMaster:
                    // Cost reduction percentage
                    return $"-{(1f - data.CurrentParameterValue):P0}";
                    
                default:
                    // Default numeric format
                    if (data.CurrentParameterValue >= 1000)
                    {
                        return $"+{data.CurrentParameterValue:N0}";
                    }
                    return $"+{data.CurrentParameterValue:F1}";
            }
        }
        
        /// <summary>
        /// Format price with proper number formatting
        /// </summary>
        private string FormatPrice(long price)
        {
            if (price >= 1000000000)
            {
                return $"{price / 1000000000f:F1}B";
            }
            if (price >= 1000000)
            {
                return $"{price / 1000000f:F1}M";
            }
            if (price >= 1000)
            {
                return $"{price / 1000f:F1}K";
            }
            return price.ToString("N0");
        }
        
        /// <summary>
        /// Get the upgrade data for this item
        /// </summary>
        public UpgradeDisplayData GetUpgradeData() => _upgradeData;
        
        /// <summary>
        /// Get the upgrade button for external subscription
        /// </summary>
        public Button UpgradeButton => _upgradeButton;
    }
}
