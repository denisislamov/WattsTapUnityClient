using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class AvatarScreenUIView : UIBaseView<AvatarScreenUIPresenter>
    {
        [Header("Controls")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _equipButton;
        [SerializeField] private TMP_Text _equipButtonText;
        
        [Header("Equip Button Currency Icons")]
        [SerializeField] private GameObject _coinsIcon;
        [SerializeField] private GameObject _btnIcon;
        
        [Header("Avatar Display")]
        [SerializeField] private Transform _avatarsContainer;
        [SerializeField] private AvatarItemUIView _avatarItemPrefab;

        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;

        public Button BackButton => _backButton;
        public Button EquipButton => _equipButton;
        public Transform AvatarsContainer => _avatarsContainer;
        public AvatarItemUIView AvatarItemPrefab => _avatarItemPrefab;
        
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
        
        /// <summary>
        /// Shows or hides the equip button based on selection state.
        /// </summary>
        public void SetEquipButtonVisible(bool visible)
        {
            if (_equipButton != null)
            {
                _equipButton.gameObject.SetActive(visible);
            }
        }
        
        /// <summary>
        /// Sets the equip button interactable state.
        /// </summary>
        public void SetEquipButtonInteractable(bool interactable)
        {
            if (_equipButton != null)
            {
                _equipButton.interactable = interactable;
            }
        }
        
        /// <summary>
        /// Sets the equip button text (e.g. "Equip" or price).
        /// </summary>
        public void SetEquipButtonText(string text)
        {
            if (_equipButtonText != null)
            {
                _equipButtonText.text = text;
            }
        }
        
        /// <summary>
        /// Shows the equip button with "Equip" text and hides currency icons.
        /// Used when the avatar is already owned.
        /// </summary>
        public void SetEquipButtonAsEquip()
        {
            SetEquipButtonText("Equip");
            SetCurrencyIconsVisible(false, false);
        }
        
        /// <summary>
        /// Shows the equip button with price and the appropriate currency icon.
        /// Used when the avatar needs to be purchased.
        /// </summary>
        public void SetEquipButtonAsPrice(string formattedPrice, bool isCoins)
        {
            SetEquipButtonText(formattedPrice);
            SetCurrencyIconsVisible(isCoins, !isCoins);
        }
        
        /// <summary>
        /// Shows the equip button with "Level N" text and hides currency icons.
        /// Used when the avatar requires a certain player level.
        /// </summary>
        public void SetEquipButtonAsLevel(int requiredLevel)
        {
            SetEquipButtonText($"Level {requiredLevel}");
            SetCurrencyIconsVisible(false, false);
        }
        
        /// <summary>
        /// Sets visibility of currency icons on the equip button.
        /// </summary>
        private void SetCurrencyIconsVisible(bool showCoins, bool showBtn)
        {
            if (_coinsIcon != null)
            {
                _coinsIcon.SetActive(showCoins);
            }
            
            if (_btnIcon != null)
            {
                _btnIcon.SetActive(showBtn);
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

