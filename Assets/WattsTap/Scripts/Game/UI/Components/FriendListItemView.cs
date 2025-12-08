using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WattsTap.Core.API;

namespace WattsTap.Game.UI
{
    /// <summary>
    /// View component for displaying a single friend in the friends list
    /// </summary>
    public class FriendListItemView : MonoBehaviour
    {
        [Header("Display Elements")]
        [SerializeField] private TMP_Text _nicknameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _bonusText;
        [SerializeField] private TMP_Text _invitedAtText;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private GameObject _bonusIndicator;
        
        private FriendInfo _friendInfo;
        
        /// <summary>
        /// Setup the view with friend information
        /// </summary>
        public void Setup(FriendInfo friendInfo)
        {
            _friendInfo = friendInfo;
            
            // Set nickname
            if (_nicknameText != null)
            {
                _nicknameText.text = friendInfo.nickname ?? "Unknown";
            }
            
            // Set level
            if (_levelText != null)
            {
                _levelText.text = $"Lvl {friendInfo.level}";
            }
            
            // Set bonus (only show if you earned bonus from this friend)
            if (_bonusText != null)
            {
                if (friendInfo.yourBonus > 0)
                {
                    _bonusText.text = $"+{friendInfo.yourBonus:N0}";
                    _bonusText.gameObject.SetActive(true);
                }
                else
                {
                    _bonusText.gameObject.SetActive(false);
                }
            }
            
            // Show bonus indicator
            if (_bonusIndicator != null)
            {
                _bonusIndicator.SetActive(friendInfo.yourBonus > 0);
            }
            
            // Set invited at date
            if (_invitedAtText != null)
            {
                if (!string.IsNullOrEmpty(friendInfo.invitedAt))
                {
                    // Try to parse and format the date
                    if (System.DateTime.TryParse(friendInfo.invitedAt, out var date))
                    {
                        _invitedAtText.text = date.ToString("MMM dd, yyyy");
                    }
                    else
                    {
                        _invitedAtText.text = friendInfo.invitedAt;
                    }
                }
                else
                {
                    _invitedAtText.text = "";
                }
            }
            
            // Load avatar (async loading would be implemented here)
            // For now, just log if there's an avatar URL
            if (_avatarImage != null && !string.IsNullOrEmpty(friendInfo.avatarUrl))
            {
                // TODO: Implement async avatar loading
                // StartCoroutine(LoadAvatarAsync(friendInfo.avatarUrl));
            }
        }
        
        /// <summary>
        /// Get the friend info for this item
        /// </summary>
        public FriendInfo GetFriendInfo() => _friendInfo;
    }
}





