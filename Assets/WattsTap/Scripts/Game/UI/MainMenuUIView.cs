using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class MainMenuUIView : UIBaseView<MainMenuUIPresenter>
    {
        [System.Serializable]
        public class SkinElement
        {
            public Image[] backgroundImages;
            public Color foregroundColor;
            
            public void SetColor()
            {
                foreach (var backgroundImage in backgroundImages)
                {
                    backgroundImage.color = foregroundColor;
                }
            }
        }
        
        [System.Serializable]
        public class Skin
        {
            public SkinElement[] skinElements;
            
            public void SetColors()
            {
                foreach (var element in skinElements)
                {
                    element.SetColor();
                }
            }
        }
        
        [Header("User")]
        [SerializeField] private TMP_Text _userName;
        [SerializeField] private Image _userAvatar;
        
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

        [Header("Skins")]
        [SerializeField] private Skin[] skins;
        [SerializeField] public Button changeSkinButton;
        
        public Button ChangeSkinButton => changeSkinButton;
        
        private int _currentSkinIndex = 0;
        
        public void SetNextSkin()
        {
            _currentSkinIndex = (_currentSkinIndex + 1) % skins.Length;
            skins[_currentSkinIndex].SetColors();
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
    }
}