using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WattsTap.Core.API;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class FriendsReferralScreenUIView : UIBaseView<FriendsReferralScreenUIPresenter>
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button _referralButton;
        [SerializeField] private Button _friendsButton;

        [Header("Tab Content")]
        [SerializeField] private GameObject[] _referralElements;
        [SerializeField] private GameObject[] _friendsElements;

        [Header("Controls")]
        [SerializeField] private Button _miningButton;
        [SerializeField] private Button _questsButton;
        
        [Header("Referral Tab - Actions")]
        [SerializeField] private Button _shareButton;
        [SerializeField] private Button _copyLinkButton;
        
        [SerializeField] private Button _shareButton2;
        [SerializeField] private Button _copyLinkButton2;
        
        [Header("Referral Tab - Display")]
        [SerializeField] private TMP_Text _inviteLinkText;
        [SerializeField] private TMP_Text _inviteLinkText2;
        [SerializeField] private TMP_Text _friendsCountText;
        [SerializeField] private TMP_Text _totalBonusText;
        [SerializeField] private TMP_Text _bonusPerFriendText;
        [SerializeField] private GameObject _copiedNotification;
        
        [Header("Friends Tab")]
        [SerializeField] private Transform _friendsListContainer;
        [SerializeField] private GameObject _friendItemPrefab;
        [SerializeField] private GameObject _noFriendsPlaceholder;
        [SerializeField] private TMP_Text _friendsListTotalText;

        #region Properties
        
        public Button ReferralButton => _referralButton;
        public Button FriendsButton => _friendsButton;
        public Button MiningButton => _miningButton;
        public Button QuestsButton => _questsButton;
        public Button ShareButton => _shareButton;
        public Button ShareButton2 => _shareButton2;
        public Button CopyLinkButton => _copyLinkButton;
        public Button CopyLinkButton2 => _copyLinkButton2;
        
        #endregion

        #region Tab Switching

        /// <summary>
        /// Shows referral tab content and hides friends content
        /// </summary>
        public void ShowReferralTab()
        {
            SetElementsActive(_referralElements, true);
            SetElementsActive(_friendsElements, false);
        }

        /// <summary>
        /// Shows friends tab content and hides referral content
        /// </summary>
        public void ShowFriendsTab()
        {
            SetElementsActive(_referralElements, false);
            SetElementsActive(_friendsElements, true);
        }

        #endregion
        
        #region Referral Tab Updates
        
        /// <summary>
        /// Updates the referral information display
        /// </summary>
        public void UpdateReferralInfo(string inviteLink, int friendsCount, long totalBonus, int bonusPerFriend)
        {
            if (_inviteLinkText != null)
            {
                // Show shortened link for display
                _inviteLinkText.text = inviteLink ?? "Loading...";
            }
            
            if (_inviteLinkText2 != null)
            {
                // Show shortened link for display
                _inviteLinkText2.text = inviteLink ?? "Loading...";
            }
            
            if (_friendsCountText != null)
            {
                _friendsCountText.text = friendsCount.ToString();
            }
            
            if (_totalBonusText != null)
            {
                _totalBonusText.text = $"+{totalBonus:N0}";
            }
            
            if (_bonusPerFriendText != null)
            {
                _bonusPerFriendText.text = $"+{bonusPerFriend:N0} per friend";
            }
        }
        
        /// <summary>
        /// Shows a "Copied!" notification
        /// </summary>
        public void ShowCopiedNotification()
        {
            if (_copiedNotification != null)
            {
                _copiedNotification.SetActive(true);
                CancelInvoke(nameof(HideCopiedNotification));
                Invoke(nameof(HideCopiedNotification), 2f);
            }
        }
        
        private void HideCopiedNotification()
        {
            if (_copiedNotification != null)
            {
                _copiedNotification.SetActive(false);
            }
        }
        
        #endregion
        
        #region Friends Tab Updates
        
        /// <summary>
        /// Updates the friends list display
        /// </summary>
        public void UpdateFriendsList(List<FriendInfo> friends)
        {
            // Clear existing items
            if (_friendsListContainer != null)
            {
                foreach (Transform child in _friendsListContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            
            bool hasFriends = friends != null && friends.Count > 0;
            
            // Show/hide placeholder
            if (_noFriendsPlaceholder != null)
            {
                _noFriendsPlaceholder.SetActive(!hasFriends);
            }
            
            // Update total text
            if (_friendsListTotalText != null)
            {
                _friendsListTotalText.text = hasFriends ? $"{friends.Count} friends" : "No friends yet";
            }
            
            if (!hasFriends || _friendItemPrefab == null || _friendsListContainer == null)
            {
                return;
            }
            
            // Create friend items
            foreach (var friend in friends)
            {
                var item = Instantiate(_friendItemPrefab, _friendsListContainer);
                
                // Try to find and setup the friend item component
                var friendItem = item.GetComponent<FriendListItemView>();
                if (friendItem != null)
                {
                    friendItem.Setup(friend);
                }
                else
                {
                    // Fallback: try to find text components directly
                    SetupFriendItemFallback(item, friend);
                }
            }
        }
        
        private void SetupFriendItemFallback(GameObject item, FriendInfo friend)
        {
            // Try to find common text fields by name
            var nameText = item.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
            {
                nameText.text = $"{friend.nickname} (Lvl {friend.level})";
            }
        }
        
        #endregion

        #region Helpers
        
        private static void SetElementsActive(GameObject[] elements, bool isActive)
        {
            if (elements == null)
            {
                return;
            }

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] != null)
                {
                    elements[i].SetActive(isActive);
                }
            }
        }
        
        #endregion
    }
}








