using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.UI;
using WattsTap.Game.Shop;

namespace WattsTap.Game.UI
{
    public class ShopChestItemUIViewOpen : UIBaseView<ShopChestItemUIPresenterOpen>
    {
        [Header("Display Elements")]
        [SerializeField] private TMP_Text _itemNameText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _primaryColorImage;
        [SerializeField] private Image _secondaryColorImage;

        [Header("Controls")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _openButton;

        [Header("Chest Animation")]
        [SerializeField] private Animator _chestAnimator;
        
        private static readonly int OpenTrigger = Animator.StringToHash("Open");

        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;
        private bool _isAnimating;
        private Coroutine _waitForAnimationCoroutine;

        public Button BackButton => _backButton;
        public Button OpenButton => _openButton;

        public event Action OnOpenAnimationComplete;
        public event Action OnAnimationReset;

        private void Start()
        {
            if (_openButton != null)
            {
                _openButton.onClick.AddListener(PlayOpenAnimation);
            }
        }

        public void PlayOpenAnimation()
        {
            if (_isAnimating)
            {
                return;
            }
            
            if (_chestAnimator == null)
            {
                return;
            }

            _isAnimating = true;
            SetOpenButtonInteractable(false);
            
            _chestAnimator.SetTrigger(OpenTrigger);
            
            if (_waitForAnimationCoroutine != null)
            {
                StopCoroutine(_waitForAnimationCoroutine);
            }
            _waitForAnimationCoroutine = StartCoroutine(WaitForAnimationComplete());
        }

        private IEnumerator WaitForAnimationComplete()
        {
            // Ждём один кадр, чтобы аниматор обновился
            yield return null;
            
            // Ждём пока аниматор находится в состоянии анимации открытия
            AnimatorStateInfo stateInfo = _chestAnimator.GetCurrentAnimatorStateInfo(0);
            while (stateInfo.normalizedTime < 1f || _chestAnimator.IsInTransition(0))
            {
                yield return null;
                stateInfo = _chestAnimator.GetCurrentAnimatorStateInfo(0);
            }

            _isAnimating = false;
            SetOpenButtonInteractable(true);
            
            OnOpenAnimationComplete?.Invoke();
        }

        /// <summary>
        /// Устанавливает состояние интерактивности кнопки открытия
        /// </summary>
        private void SetOpenButtonInteractable(bool interactable)
        {
            if (_openButton != null)
            {
                _openButton.interactable = interactable;
            }
        }

        public void ResetAnimation()
        {
            if (_waitForAnimationCoroutine != null)
            {
                StopCoroutine(_waitForAnimationCoroutine);
                _waitForAnimationCoroutine = null;
            }

            _isAnimating = false;
            SetOpenButtonInteractable(true);
            
            // Сброс аниматора к начальному состоянию
            if (_chestAnimator != null)
            {
                _chestAnimator.Rebind();
                _chestAnimator.Update(0f);
            }
            
            OnAnimationReset?.Invoke();
        }

        public void UpdateFromConfig(ShopChestItemConfig config)
        {
            if (config == null)
            {
                return;
            }

            UpdateItemName(config.ItemName);
            UpdateIcon(config.Icon);
            UpdateColors(config.PrimaryColor, config.SecondaryColor);
        }

        public void UpdateItemName(string itemName)
        {
            if (_itemNameText != null)
            {
                _itemNameText.text = itemName;
            }
        }

        public void UpdateIcon(Sprite icon)
        {
            if (_iconImage != null && icon != null)
            {
                _iconImage.sprite = icon;
            }
        }

        public void UpdateColors(Color primaryColor, Color secondaryColor)
        {
            if (_primaryColorImage != null)
            {
                _primaryColorImage.color = primaryColor;
            }

            if (_secondaryColorImage != null)
            {
                _secondaryColorImage.color = secondaryColor;
            }
        }
        
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
        
        private void OnSkinChanged(MainMenuSkinDefinition skin)
        {
            if (_themeManager != null)
            {
                _themeManager.ApplySkin(skin, _skinBindings, this);
            }
        }

        private new void OnDestroy()
        {
            if (_openButton != null)
            {
                _openButton.onClick.RemoveListener(PlayOpenAnimation);
            }
            
            base.OnDestroy();
        }
    }
}
