using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.API;
using WattsTap.Core.Services;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class FriendsReferralScreenUIPresenter : UIBasePresenter<FriendsReferralScreenUIView, FriendsReferralScreenUIModel>
    {
        private IUIService _uiService;
        private IReferralService _referralService;
        private IHapticFeedbackService _hapticService;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _referralService);
            ServiceLocator.TryGet(out _hapticService);

            // Tab buttons
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

            if (View.QuestsButton != null)
            {
                View.QuestsButton.onClick.AddListener(OnQuestsButtonClicked);
            }

            if (View.InventoryButton != null)
            {
                View.InventoryButton.onClick.AddListener(OnInventoryButtonClicked);
            }

            if (View.UpdatesButton != null)
            {
                View.UpdatesButton.onClick.AddListener(OnUpdatesButtonClicked);
            }
            
            // Referral action buttons
            if (View.ShareButton != null)
            {
                View.ShareButton.onClick.AddListener(OnShareButtonClicked);
            }
            
            if (View.ShareButton2 != null)
            {
                View.ShareButton2.onClick.AddListener(OnShareButtonClicked);
            }
            
            if (View.CopyLinkButton != null)
            {
                View.CopyLinkButton.onClick.AddListener(OnCopyLinkClicked);
            }
            
            if (View.CopyLinkButton2 != null)
            {
                View.CopyLinkButton2.onClick.AddListener(OnCopyLinkClicked);
            }
            
            // Subscribe to referral service events
            if (_referralService != null)
            {
                _referralService.OnReferralDataLoaded += OnReferralDataLoaded;
                _referralService.OnFriendsListLoaded += OnFriendsListLoaded;
                _referralService.OnLinkCopied += OnLinkCopied;
            }

            // Show referral tab by default
            View.ShowReferralTab();
            
            // Load data from server
            LoadData();
        }
        
        private void LoadData()
        {
            if (_referralService == null)
            {
                return;
            }
            
            // Load referral info
            _referralService.LoadReferralData();
            
            // Load friends list
            _referralService.LoadFriendsList();
        }
        
        #region Event Handlers - Referral Service
        
        private void OnReferralDataLoaded(MyReferralResponse data)
        {
            View.UpdateReferralInfo(
                data.inviteLink,
                data.totalFriendsInvited,
                data.totalBonusEarned,
                data.bonusPerFriend
            );
        }
        
        private void OnFriendsListLoaded(FriendsListResponse data)
        {
            View.UpdateFriendsList(data.friends);
        }
        
        private void OnLinkCopied()
        {
            View.ShowCopiedNotification();
        }
        
        #endregion
        
        #region Event Handlers - Buttons

        private void OnReferralButtonClicked()
        {
            _hapticService?.ButtonPressed();
            View.ShowReferralTab();
        }

        private void OnFriendsButtonClicked()
        {
            _hapticService?.ButtonPressed();
            View.ShowFriendsTab();
        }

        private void OnMiningButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.FriendsReferralScreen);
        }

        private void OnQuestsButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.FriendsReferralScreen);
            _uiService?.Open(UIConstants.QuestsScreen);
        }

        private void OnInventoryButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.FriendsReferralScreen);
            _uiService?.Open(UIConstants.InventoryScreen);
        }

        private void OnUpdatesButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close FriendsReferralScreen and open UpdatesPopup
            _uiService?.Close(UIConstants.FriendsReferralScreen);
            _uiService?.Open(UIConstants.UpdatesPopup);
        }
        
        private void OnShareButtonClicked()
        {
            _hapticService?.ButtonPressed();
            _referralService?.ShareInviteLink();
        }
        
        private void OnCopyLinkClicked()
        {
            _hapticService?.ButtonPressed();
            _referralService?.CopyInviteLink();
            // Note: ShowCopiedNotification is now called via OnLinkCopied event
        }
        
        #endregion

        protected override void OnDispose()
        {
            // Unsubscribe from referral service
            if (_referralService != null)
            {
                _referralService.OnReferralDataLoaded -= OnReferralDataLoaded;
                _referralService.OnFriendsListLoaded -= OnFriendsListLoaded;
                _referralService.OnLinkCopied -= OnLinkCopied;
            }
            
            // Remove button listeners
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

            if (View?.QuestsButton != null)
            {
                View.QuestsButton.onClick.RemoveListener(OnQuestsButtonClicked);
            }

            if (View?.InventoryButton != null)
            {
                View.InventoryButton.onClick.RemoveListener(OnInventoryButtonClicked);
            }

            if (View?.UpdatesButton != null)
            {
                View.UpdatesButton.onClick.RemoveListener(OnUpdatesButtonClicked);
            }
            
            if (View?.ShareButton != null)
            {
                View.ShareButton.onClick.RemoveListener(OnShareButtonClicked);
            }
            
            if (View?.CopyLinkButton != null)
            {
                View.CopyLinkButton.onClick.RemoveListener(OnCopyLinkClicked);
            }
            
            if (View?.ShareButton2 != null)
            {
                View.ShareButton2.onClick.RemoveListener(OnShareButtonClicked);
            }
            
            if (View?.CopyLinkButton2 != null)
            {
                View.CopyLinkButton2.onClick.RemoveListener(OnCopyLinkClicked);
            }
        }
    }
}
