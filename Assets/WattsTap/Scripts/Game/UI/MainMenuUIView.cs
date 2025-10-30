using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class MainMenuUIView : UIBaseView<MainMenuUIPresenter>
    {
        [Header("Currency Display")]
        [SerializeField] private Text totalCoinsText;
        [SerializeField] private Text coinsPerTapText;

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
    }
}