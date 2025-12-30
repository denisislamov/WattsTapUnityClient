using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class MainMenuUIView : UIBaseView<MainMenuUIPresenter>
    {
        [Header("User")]
        [SerializeField] private TMP_Text _userName;
        [SerializeField] private Image _userAvatar;
        [SerializeField] private TMP_Text _currentLevelText;
        [SerializeField] private Slider _levelProgressBar;
        [SerializeField] private Button _profileButton;
        
        [Header("Currency Display")]
        [SerializeField] private RectTransform _currencyPanel;
        [SerializeField] private TMP_Text totalCoinsText;
        [SerializeField] private TMP_Text coinsPerTapText;
        
        [Header("Currency Panel Width Settings")]
        [SerializeField] private float _baseCurrencyPanelWidth = 200f;
        [SerializeField] private float _widthIncreasePerDigit = 15f;
        
        [Header("Hits Display")]
        [SerializeField] private TMP_Text currentHitsText;
        [SerializeField] private TMP_Text maxMitsText;

        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        [SerializeField] public Button changeSkinButton;
        
        private MainMenuThemeManager _themeManager;
        
        [Header("Version Info")]
        [SerializeField] private TMP_Text _versionText;

        [Header("Navigation")]
        [SerializeField] private Button _friendsReferralButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _questsButton;
        [SerializeField] private Button _bugReportButton;
        
        [Header("Tap Hint")]
        [SerializeField] private GameObject _tapHintObject;
        
        public Button ChangeSkinButton => changeSkinButton;
        public Button ProfileButton => _profileButton;
        public Button FriendsReferralButton => _friendsReferralButton;
        public Button ShopButton => _shopButton;
        public Button QuestsButton => _questsButton;
        public Button BugReportButton => _bugReportButton;

        private void Awake()
        {
            if (_profileButton == null && _userAvatar != null)
            {
                _profileButton = _userAvatar.GetComponent<Button>();

                if (_profileButton == null)
                {
                    _profileButton = _userAvatar.gameObject.AddComponent<Button>();
                    _profileButton.transition = Selectable.Transition.None;
                    _profileButton.targetGraphic = _userAvatar;
                }
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
        
        public void SetDefaultSkin()
        {
            if (_themeManager != null)
            {
                var skin = _themeManager.SetDefaultSkin();
                if (skin == null)
                {
                    _themeManager.ApplySkin(null, _skinBindings, this);
                }
            }
        }
        
        public void SetNextSkin()
        {
            if (_themeManager != null)
            {
                var skin = _themeManager.SetNextSkin();
                if (skin == null)
                {
                    _themeManager.ApplySkin(null, _skinBindings, this);
                }
            }
        }
        
        public void UpdateTotalCoins(long totalCoins)
        {
            if (totalCoinsText == null)
            {
                return;
            }
            
            totalCoinsText.text = $"{totalCoins:N0}";
            UpdateCurrencyPanelWidth(totalCoins);
        }

        private void UpdateCurrencyPanelWidth(long totalCoins)
        {
            if (_currencyPanel == null)
            {
                return;
            }

            int digitCount = totalCoins == 0 ? 1 : Mathf.FloorToInt(Mathf.Log10(totalCoins) + 1);
            float newWidth = _baseCurrencyPanelWidth + (digitCount - 1)* _widthIncreasePerDigit;
            _currencyPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
        }

        public void UpdateCoinsPerTap(int coinsPerTap)
        {
            if (coinsPerTapText != null)
            {
                coinsPerTapText.text = $"+{coinsPerTap}";
            }
        }

        public void UpdateHits(int current, int max)
        {
            if (currentHitsText != null)
            {
                currentHitsText.text = $"{current}";
            }
            
            if (maxMitsText != null)
            {
                maxMitsText.text = $"{max}";
            }
        }
        
        public void UpdateUserName(string userName)
        {
            if (_userName != null)
            {
                _userName.text = userName;
            }
        }
        
        public void UpdateAvatar(Sprite avatarSprite)
        {
            if (_userAvatar != null && avatarSprite != null)
            {
                _userAvatar.sprite = avatarSprite;
            }
        }

        public void UpdateCurrentLevel(int level)
        {
            if (_currentLevelText != null)
            {
                _currentLevelText.text = $"lvl {level}";
            }
        }

        public void UpdateLevelProgressBar(float progress)
        {
            if (_levelProgressBar != null)
            {
                _levelProgressBar.value = progress;
            }
        }
        
        private void OnSkinChanged(MainMenuSkinDefinition skin)
        {
            if (_themeManager != null)
            {
                _themeManager.ApplySkin(skin, _skinBindings, this);
            }
        }

        public void UpdateVersionText(string version)
        {
            if (_versionText != null)
            {
                _versionText.text = $"v{version}";
            }
        }

        public void HideTapHint()
        {
            if (_tapHintObject != null)
            {
                _tapHintObject.SetActive(false);
            }
        }
    }
}