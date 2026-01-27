using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class UpdatesPopupUIView : UIBaseView<UpdatesPopupUIPresenter>
    {
        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;
        
        [Header("Navigation")]
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _friendsReferralButton;
        [SerializeField] private Button _inventoryButton;
        [SerializeField] private Button _questsButton;
        
        public Button MainMenuButton => _mainMenuButton;
        public Button FriendsReferralButton => _friendsReferralButton;
        public Button InventoryButton => _inventoryButton;
        public Button QuestsButton => _questsButton;
        
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
    }
}

