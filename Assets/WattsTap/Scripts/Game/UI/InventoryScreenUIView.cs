using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class InventoryScreenUIView : UIBaseView<InventoryScreenUIPresenter>
    {
        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;
        
        [Header("Navigation")]
        [SerializeField] private Button _miningButton;
        
        [Header("Hits Display")]
        [SerializeField] private TMP_Text _currentHitsText;
        [SerializeField] private TMP_Text _maxHitsText;
        
        [Header("Currency Display")]
        [SerializeField] private TMP_Text _coinsPerTapText;

        #region Properties
        
        public Button MiningButton => _miningButton;
        
        #endregion

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
        
        public void UpdateHits(int current, int max)
        {
            if (_currentHitsText != null)
            {
                _currentHitsText.text = $"{current}";
            }
            
            if (_maxHitsText != null)
            {
                _maxHitsText.text = $"{max}";
            }
        }
        
        public void UpdateCoinsPerTap(int coinsPerTap)
        {
            if (_coinsPerTapText != null)
            {
                _coinsPerTapText.text = $"+{coinsPerTap}";
            }
        }
    }
}

