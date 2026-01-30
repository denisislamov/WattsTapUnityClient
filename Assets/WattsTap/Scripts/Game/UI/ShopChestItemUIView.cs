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
    public class ShopChestItemUIView : UIBaseView<ShopChestItemUIPresenter>
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
        [SerializeField] private RectTransform _chestTransform;
        [SerializeField] private Image _chestClosedImage;
        [SerializeField] private Image _chestOpenImage1;
        [SerializeField] private Image _chestOpenImage2;
        [SerializeField] private Image _glowImage;

        [Header("Animation Settings")]
        [SerializeField] private float _jumpHeight = 50f;
        [SerializeField] private float _jumpDuration = 0.3f;
        [SerializeField] private int _jumpCount = 3;
        [SerializeField] private float _scaleAmount = 1.15f;
        [SerializeField] private float _crossfadeDuration = 0.4f;
        [SerializeField] private float _glowFadeInDuration = 0.3f;
        [SerializeField] private float _glowPulseMin = 0.4f;
        [SerializeField] private float _glowPulseMax = 1f;
        [SerializeField] private float _glowPulseDuration = 0.8f;

        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;
        private Coroutine _animationCoroutine;
        private Coroutine _glowPulseCoroutine;
        private Vector3 _originalPosition;
        private Vector3 _originalScale;

        public Button BackButton => _backButton;
        public Button OpenButton => _openButton;

        public event Action OnOpenAnimationComplete;

        private void Awake()
        {
            InitializeAnimationState();
        }

        /// <summary>
        /// Инициализирует начальное состояние всех элементов анимации (до открытия сундука)
        /// </summary>
        private void InitializeAnimationState()
        {
            // Сохраняем начальную позицию и масштаб
            if (_chestTransform != null)
            {
                _originalPosition = _chestTransform.anchoredPosition;
                _originalScale = _chestTransform.localScale;
            }

            // Закрытый сундук - видимый
            SetImageAlpha(_chestClosedImage, 1f);
            
            // Открытые картинки сундука - скрыты
            SetImageAlpha(_chestOpenImage1, 0f);
            SetImageAlpha(_chestOpenImage2, 0f);
            
            // Свечение - скрыто
            SetImageAlpha(_glowImage, 0f);
        }


        private void Start()
        {
            if (_openButton != null)
            {
                _openButton.onClick.AddListener(PlayOpenAnimation);
            }
        }

        public void PlayOpenAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            _animationCoroutine = StartCoroutine(OpenAnimationSequence());
        }

        private IEnumerator OpenAnimationSequence()
        {
            // Phase 1: Bouncy jumps with scale
            for (int i = 0; i < _jumpCount; i++)
            {
                yield return StartCoroutine(JumpAndSquash());
            }

            // Phase 2: Crossfade chest sprites
            yield return StartCoroutine(CrossfadeChestSprites());

            // Phase 3: Glow appears and pulses
            yield return StartCoroutine(FadeInGlow());
            _glowPulseCoroutine = StartCoroutine(PulseGlow());

            OnOpenAnimationComplete?.Invoke();
        }

        private IEnumerator JumpAndSquash()
        {
            float elapsed = 0f;
            Vector3 startPos = _originalPosition;

            while (elapsed < _jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _jumpDuration;

                // Parabolic jump curve
                float jumpProgress = 1f - (2f * t - 1f) * (2f * t - 1f);
                float yOffset = jumpProgress * _jumpHeight;

                // Squash and stretch
                float scaleT = Mathf.Sin(t * Mathf.PI);
                float scaleX = Mathf.Lerp(1f, 1f / _scaleAmount, scaleT * 0.3f);
                float scaleY = Mathf.Lerp(1f, _scaleAmount, scaleT * 0.5f);

                if (_chestTransform != null)
                {
                    _chestTransform.anchoredPosition = new Vector2(startPos.x, startPos.y + yOffset);
                    _chestTransform.localScale = new Vector3(
                        _originalScale.x * scaleX,
                        _originalScale.y * scaleY,
                        _originalScale.z
                    );
                }

                yield return null;
            }

            // Landing squash
            yield return StartCoroutine(LandingSquash());
        }

        private IEnumerator LandingSquash()
        {
            float elapsed = 0f;
            float duration = 0.1f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                float squash = Mathf.Lerp(_scaleAmount * 0.9f, 1f, EaseOutElastic(t));
                float stretch = Mathf.Lerp(1f / (_scaleAmount * 0.9f), 1f, EaseOutElastic(t));

                if (_chestTransform != null)
                {
                    _chestTransform.anchoredPosition = _originalPosition;
                    _chestTransform.localScale = new Vector3(
                        _originalScale.x * squash,
                        _originalScale.y * stretch,
                        _originalScale.z
                    );
                }

                yield return null;
            }

            if (_chestTransform != null)
            {
                _chestTransform.localScale = _originalScale;
            }
        }

        private IEnumerator CrossfadeChestSprites()
        {
            float elapsed = 0f;

            while (elapsed < _crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _crossfadeDuration;
                float smoothT = EaseInOutQuad(t);

                // Закрытый сундук исчезает
                SetImageAlpha(_chestClosedImage, 1f - smoothT);
                
                // Открытые картинки появляются
                SetImageAlpha(_chestOpenImage1, smoothT);
                SetImageAlpha(_chestOpenImage2, smoothT);

                yield return null;
            }

            // Финальные значения
            SetImageAlpha(_chestClosedImage, 0f);
            SetImageAlpha(_chestOpenImage1, 1f);
            SetImageAlpha(_chestOpenImage2, 1f);
        }

        private IEnumerator FadeInGlow()
        {
            float elapsed = 0f;

            while (elapsed < _glowFadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _glowFadeInDuration;

                SetImageAlpha(_glowImage, EaseOutQuad(t) * _glowPulseMax);

                yield return null;
            }
        }

        private IEnumerator PulseGlow()
        {
            while (true)
            {
                float elapsed = 0f;

                while (elapsed < _glowPulseDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / _glowPulseDuration;

                    float alpha = Mathf.Lerp(_glowPulseMin, _glowPulseMax, 
                        (Mathf.Sin(t * Mathf.PI * 2f - Mathf.PI / 2f) + 1f) / 2f);
                    SetImageAlpha(_glowImage, alpha);

                    yield return null;
                }
            }
        }

        private void SetImageAlpha(Image image, float alpha)
        {
            if (image != null)
            {
                Color color = image.color;
                color.a = alpha;
                image.color = color;
            }
        }

        // Easing functions
        private float EaseOutElastic(float t)
        {
            const float c4 = (2f * Mathf.PI) / 3f;
            return t <= 0 ? 0 : t >= 1 ? 1 : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }

        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        public void ResetAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            if (_glowPulseCoroutine != null)
            {
                StopCoroutine(_glowPulseCoroutine);
                _glowPulseCoroutine = null;
            }

            if (_chestTransform != null)
            {
                _chestTransform.anchoredPosition = _originalPosition;
                _chestTransform.localScale = _originalScale;
            }

            // Сброс к начальному состоянию (до открытия)
            SetImageAlpha(_chestClosedImage, 1f);
            SetImageAlpha(_chestOpenImage1, 0f);
            SetImageAlpha(_chestOpenImage2, 0f);
            SetImageAlpha(_glowImage, 0f);
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

