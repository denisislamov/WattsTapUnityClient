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
        [SerializeField] private MainMenuThemeManager _themeManager;
        [SerializeField] private MainMenuSkinDefinition _fallbackSkin;
        [SerializeField] private SkinTokenBinding[] _skinBindings;
        
        [SerializeField] public Button changeSkinButton;
        
        [Header("Version Info")]
        [SerializeField] private TMP_Text _versionText;
        
        public Button ChangeSkinButton => changeSkinButton;
        public Button ProfileButton => _profileButton;

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
                    ApplySkin(_themeManager.CurrentSkin);
                    return;
                }
            }

            if (_fallbackSkin != null)
            {
                ApplySkin(_fallbackSkin);
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
                if (skin == null && _fallbackSkin != null)
                {
                    ApplySkin(_fallbackSkin);
                }
                
                return;
            }

            if (_fallbackSkin != null)
            {
                ApplySkin(_fallbackSkin);
            }
        }
        
        public void SetNextSkin()
        {
            if (_themeManager != null)
            {
                var skin = _themeManager.SetNextSkin();
                if (skin == null && _fallbackSkin != null)
                {
                    ApplySkin(_fallbackSkin);
                }
                
                return;
            }

            if (_fallbackSkin != null)
            {
                ApplySkin(_fallbackSkin);
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
            if (skin == null)
            {
                if (_fallbackSkin != null)
                {
                    ApplySkin(_fallbackSkin);
                }
                
                return;
            }

            ApplySkin(skin);
        }

        public void UpdateVersionText(string version)
        {
            if (_versionText != null)
            {
                _versionText.text = $"v{version}";
            }
        }
        
        private void ApplySkin(MainMenuSkinDefinition skin)
        {
            if (skin == null)
            {
                Debug.LogWarning("MainMenuUIView: No skin provided to apply.");
                return;
            }

            if (_skinBindings == null || _skinBindings.Length == 0)
            {
                Debug.LogWarning("MainMenuUIView: No skin bindings configured.");
                return;
            }

            foreach (var binding in _skinBindings)
            {
                binding?.Apply(skin, this);
            }
        }
        
        [System.Serializable]
        private class SkinTokenBinding
        {
            [SerializeField] private string tokenId;
            [SerializeField] private Image[] imageTargets;
            [SerializeField] private TMP_Text[] textTargets;
            [SerializeField] private bool suppressMissingTokenWarning;

            public void Apply(MainMenuSkinDefinition skin, MonoBehaviour context)
            {
                if (skin == null || string.IsNullOrEmpty(tokenId))
                {
                    return;
                }

                if (!skin.TryGetToken(tokenId, out var token))
                {
                    if (!suppressMissingTokenWarning)
                    {
                        Debug.LogWarning($"MainMenuUIView: Token '{tokenId}' was not found in skin '{skin.name}'.", context);
                    }
                    
                    return;
                }

                ApplyToImages(token);
                ApplyToTexts(token);
            }

            private void ApplyToImages(MainMenuSkinDefinition.SkinToken token)
            {
                if (imageTargets == null)
                {
                    return;
                }
                
                foreach (var image in imageTargets)
                {
                    if (image == null)
                    {
                        continue;
                    }

                    image.color = token.Color;
                    
                    if (token.Sprite != null)
                    {
                        image.sprite = token.Sprite;
                    }
                    
                    if (token.Material != null)
                    {
                        image.material = token.Material;
                    }
                }
            }

            private void ApplyToTexts(MainMenuSkinDefinition.SkinToken token)
            {
                if (textTargets == null)
                {
                    return;
                }
                
                foreach (var text in textTargets)
                {
                    if (text == null)
                    {
                        continue;
                    }

                    text.color = token.Color;
                    if (token.Material != null)
                    {
                        text.material = token.Material;
                    }
                }
            }
        }
    }
}