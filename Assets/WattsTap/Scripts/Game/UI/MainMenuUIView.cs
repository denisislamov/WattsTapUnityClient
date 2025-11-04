using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class MainMenuUIView : UIBaseView<MainMenuUIPresenter>
    {
        [Header("User")]
        [SerializeField] private Text _userName;
        [SerializeField] private Image _userAvatar;
        
        [Header("Currency Display")]
        [SerializeField] private Text totalCoinsText;
        [SerializeField] private Text coinsPerTapText;

        [Header("Hits Display")]
        [SerializeField] private Text hitsText;

        public void UpdateTotalCoins(long totalCoins)
        {
            if (totalCoinsText != null)
            {
                totalCoinsText.text = $"{totalCoins:N0}";
            }
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
            if (hitsText != null)
            {
                hitsText.text = $"{current}/{max}";
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