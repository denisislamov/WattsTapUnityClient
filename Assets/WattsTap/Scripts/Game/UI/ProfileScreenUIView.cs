using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
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
        [SerializeField] private Button _avatarButton;

        [Header("Wallet")]
        [SerializeField] private Button _connectWalletButton;
        [SerializeField] private Button _changeWalletButton;
        [SerializeField] private GameObject[] _walletConnectedObjects;
        [SerializeField] private GameObject[] _walletDisconnectedObjects;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private float _scrollAnchoredYConnected = -306.21185f;
        [SerializeField] private float _scrollAnchoredYDisconnected = -278f;
        [SerializeField] private bool _startWithWalletConnected = true;

        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;
        
        private Coroutine _scrollToTopRoutine;

        public Button CloseButton => _closeButton;
        public Button AvatarButton => _avatarButton;
        public Button ConnectWalletButton => _connectWalletButton;
        public Button ChangeWalletButton => _changeWalletButton;
        public bool StartWithWalletConnected => _startWithWalletConnected;
        
        private void OnEnable()
        {
            _themeManager = ServiceLocator.Get<MainMenuThemeManager>();
            
            if (_themeManager != null)
            {
                _themeManager.SkinChanged += OnSkinChanged;

                if (_themeManager.CurrentSkin != null)
                {
                    _themeManager.ApplySkin(_themeManager.CurrentSkin, _skinBindings, this);
                    return;
                }
                
                _themeManager.ApplySkin(null, _skinBindings, this);
            }
        }

        private void OnDisable()
        {
            if (_themeManager != null)
            {
                _themeManager.SkinChanged -= OnSkinChanged;
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
            ResetScrollToTop();
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
            if (_scrollRect == null)
            {
                return;
            }

            var scrollViewRect = _scrollRect.GetComponent<RectTransform>();
            if (scrollViewRect == null)
            {
                return;
            }

            var anchoredPosition = scrollViewRect.anchoredPosition;
            anchoredPosition.y = isConnected ? _scrollAnchoredYConnected : _scrollAnchoredYDisconnected;
            scrollViewRect.anchoredPosition = anchoredPosition;
        }

        private void ResetScrollToTop()
        {
            if (!isActiveAndEnabled || _scrollRect == null)
            {
                return;
            }

            if (_scrollToTopRoutine != null)
            {
                StopCoroutine(_scrollToTopRoutine);
            }

            _scrollToTopRoutine = StartCoroutine(ScrollToTopNextFrame());
        }

        private IEnumerator ScrollToTopNextFrame()
        {
            yield return null;

            if (_scrollRect == null)
            {
                yield break;
            }

            var content = _scrollRect.content;
            if (content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            }

            Canvas.ForceUpdateCanvases();
            _scrollRect.StopMovement();
            _scrollRect.verticalNormalizedPosition = 1f;
            _scrollToTopRoutine = null;
        }
        
        private void OnSkinChanged(MainMenuSkinDefinition skin)
        {
            if (_themeManager != null)
            {
                _themeManager.ApplySkin(skin, _skinBindings, this);
            }
        }
    }
}

