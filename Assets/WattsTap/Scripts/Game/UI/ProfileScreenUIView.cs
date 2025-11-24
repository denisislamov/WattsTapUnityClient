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

        [Header("Wallet")]
        [SerializeField] private Button _connectWalletButton;
        [SerializeField] private Button _changeWalletButton;
        [SerializeField] private GameObject[] _walletConnectedObjects;
        [SerializeField] private GameObject[] _walletDisconnectedObjects;
        [SerializeField] private RectTransform _scrollViewRect;
        [SerializeField] private float _scrollAnchoredYConnected = -306.21185f;
        [SerializeField] private float _scrollAnchoredYDisconnected = -278f;
        [SerializeField] private bool _startWithWalletConnected = true;

        public Button CloseButton => _closeButton;
        public Button ConnectWalletButton => _connectWalletButton;
        public Button ChangeWalletButton => _changeWalletButton;
        public bool StartWithWalletConnected => _startWithWalletConnected;

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

        public void SetWalletConnectionState(bool isConnected)
        {
            if (_connectWalletButton != null)
            {
                _connectWalletButton.gameObject.SetActive(!isConnected);
            }

            if (_changeWalletButton != null)
            {
                _changeWalletButton.gameObject.SetActive(isConnected);
            }

            ToggleObjects(_walletConnectedObjects, isConnected);
            ToggleObjects(_walletDisconnectedObjects, !isConnected);
            UpdateScrollViewTop(isConnected);
        }

        private static void ToggleObjects(GameObject[] targets, bool isActive)
        {
            if (targets == null)
            {
                return;
            }

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                {
                    targets[i].SetActive(isActive);
                }
            }
        }

        private void UpdateScrollViewTop(bool isConnected)
        {
            if (_scrollViewRect == null)
            {
                return;
            }

            var anchoredPosition = _scrollViewRect.anchoredPosition;
            anchoredPosition.y = isConnected ? _scrollAnchoredYConnected : _scrollAnchoredYDisconnected;
            _scrollViewRect.anchoredPosition = anchoredPosition;
        }
    }
}

