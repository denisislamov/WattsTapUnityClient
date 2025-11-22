using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class ProfileScreenUIView : UIBaseView<ProfileScreenUIPresenter>
    {
        [Header("User Info")]
        [SerializeField] private TMP_Text _userName;
        [SerializeField] private Image _userAvatar;
        [SerializeField] private TMP_Text _currentLevelText;
        [SerializeField] private Slider _levelProgressBar;

        [Header("Controls")]
        [SerializeField] private Button _closeButton;

        public Button CloseButton => _closeButton;

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
                _levelProgressBar.value = Mathf.Clamp01(progress);
            }
        }
    }
}

