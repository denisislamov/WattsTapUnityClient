using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class FriendsReferralScreenUIPresenter : UIBasePresenter<FriendsReferralScreenUIView, FriendsReferralScreenUIModel>
    {
        private IUIService _uiService;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);

            if (View.ReferralButton != null)
            {
                View.ReferralButton.onClick.AddListener(OnReferralButtonClicked);
            }

            if (View.FriendsButton != null)
            {
                View.FriendsButton.onClick.AddListener(OnFriendsButtonClicked);
            }

            if (View.MiningButton != null)
            {
                View.MiningButton.onClick.AddListener(OnMiningButtonClicked);
            }

            // Show referral tab by default
            View.ShowReferralTab();
        }

        private void OnReferralButtonClicked()
        {
            View.ShowReferralTab();
        }

        private void OnFriendsButtonClicked()
        {
            View.ShowFriendsTab();
        }

        private void OnMiningButtonClicked()
        {
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.FriendsReferralScreen);
        }

        protected override void OnDispose()
        {
            if (View?.ReferralButton != null)
            {
                View.ReferralButton.onClick.RemoveListener(OnReferralButtonClicked);
            }

            if (View?.FriendsButton != null)
            {
                View.FriendsButton.onClick.RemoveListener(OnFriendsButtonClicked);
            }

            if (View?.MiningButton != null)
            {
                View.MiningButton.onClick.RemoveListener(OnMiningButtonClicked);
            }
        }
    }
}

