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

        [Header("Item Fly Out Animation")]
        [SerializeField] private RectTransform _itemTransform;
        [SerializeField] private CanvasGroup _itemCanvasGroup;
        [SerializeField] private RectTransform _itemStartPosition;
        [SerializeField] private RectTransform _itemEndPosition;
        [SerializeField] private float _itemFlyDuration = 0.6f;
        [SerializeField] private float _itemFlyHeight = 100f;
        [SerializeField] private float _itemFadeInDuration = 0.2f;
        [SerializeField] private float _itemRotationSpeed = 720f;
        [SerializeField] private float _itemRotationDuration = 0.8f;
        [SerializeField] private float _itemBounceScaleMin = 0.8f;
        [SerializeField] private float _itemBounceScaleMax = 1.2f;
        [SerializeField] private float _itemFinalScaleDuration = 0.4f;

        [Header("Labels to Hide on Open")]
        [SerializeField] private CanvasGroup _label1CanvasGroup;
        [SerializeField] private CanvasGroup _label2CanvasGroup;
        [SerializeField] private float _labelFadeOutDuration = 0.3f;

        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;
        private Coroutine _animationCoroutine;
        private Coroutine _glowPulseCoroutine;
        private Coroutine _itemAnimationCoroutine;
        private Coroutine _itemRotationCoroutine;
        private Vector3 _originalPosition;
        private Vector3 _originalScale;
        private Vector3 _itemOriginalScale;

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

            // Сохраняем начальный масштаб предмета
            if (_itemTransform != null)
            {
                _itemOriginalScale = _itemTransform.localScale;
            }

            // Закрытый сундук - видимый
            SetImageAlpha(_chestClosedImage, 1f);
            
            // Открытые картинки сундука - скрыты
            SetImageAlpha(_chestOpenImage1, 0f);
            SetImageAlpha(_chestOpenImage2, 0f);
            
            // Свечение - скрыто
            SetImageAlpha(_glowImage, 0f);
            
            // Предмет - скрыт и в начальной позиции
            InitializeItemState();
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
            // Полный сброс анимации перед запуском
            ResetAnimation();

            _animationCoroutine = StartCoroutine(OpenAnimationSequence());
        }

        private IEnumerator OpenAnimationSequence()
        {
            // Скрываем надписи параллельно с прыжками
            StartCoroutine(FadeOutLabels());
            
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

            // Phase 4: Item flies out
            yield return StartCoroutine(ItemFlyOutSequence());

            OnOpenAnimationComplete?.Invoke();
        }

        /// <summary>
        /// Плавное скрытие надписей
        /// </summary>
        private IEnumerator FadeOutLabels()
        {
            float elapsed = 0f;

            while (elapsed < _labelFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _labelFadeOutDuration;
                float alpha = 1f - EaseOutQuad(t);

                if (_label1CanvasGroup != null)
                {
                    _label1CanvasGroup.alpha = alpha;
                }

                if (_label2CanvasGroup != null)
                {
                    _label2CanvasGroup.alpha = alpha;
                }

                yield return null;
            }

            // Финальные значения
            if (_label1CanvasGroup != null)
            {
                _label1CanvasGroup.alpha = 0f;
            }

            if (_label2CanvasGroup != null)
            {
                _label2CanvasGroup.alpha = 0f;
            }
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

        /// <summary>
        /// Инициализирует начальное состояние предмета (скрыт, в начальной позиции)
        /// </summary>
        private void InitializeItemState()
        {
            if (_itemTransform != null && _itemStartPosition != null)
            {
                _itemTransform.anchoredPosition = _itemStartPosition.anchoredPosition;
                _itemTransform.localRotation = Quaternion.identity;
                _itemTransform.localScale = _itemOriginalScale;
            }

            if (_itemCanvasGroup != null)
            {
                _itemCanvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Последовательность анимации вылета предмета
        /// </summary>
        private IEnumerator ItemFlyOutSequence()
        {
            if (_itemTransform == null || _itemCanvasGroup == null || 
                _itemStartPosition == null || _itemEndPosition == null)
            {
                yield break;
            }

            // Появление предмета (fade in)
            yield return StartCoroutine(ItemFadeIn());

            // Вылет предмета по дуге
            _itemRotationCoroutine = StartCoroutine(ItemRotation());
            yield return StartCoroutine(ItemFlyToTarget());

            // Остановка вращения и веселый скейл bounce
            if (_itemRotationCoroutine != null)
            {
                StopCoroutine(_itemRotationCoroutine);
                _itemRotationCoroutine = null;
            }

            yield return StartCoroutine(ItemBounceScale());
        }

        /// <summary>
        /// Fade in предмета
        /// </summary>
        private IEnumerator ItemFadeIn()
        {
            float elapsed = 0f;

            while (elapsed < _itemFadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _itemFadeInDuration;

                if (_itemCanvasGroup != null)
                {
                    _itemCanvasGroup.alpha = EaseOutQuad(t);
                }

                yield return null;
            }

            if (_itemCanvasGroup != null)
            {
                _itemCanvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Вылет предмета от стартовой до конечной позиции с дугой
        /// </summary>
        private IEnumerator ItemFlyToTarget()
        {
            if (_itemTransform == null || _itemStartPosition == null || _itemEndPosition == null)
            {
                yield break;
            }

            float elapsed = 0f;
            Vector2 startPos = _itemStartPosition.anchoredPosition;
            Vector2 endPos = _itemEndPosition.anchoredPosition;

            while (elapsed < _itemFlyDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _itemFlyDuration;
                float smoothT = EaseOutQuad(t);

                // Линейная интерполяция позиции
                Vector2 currentPos = Vector2.Lerp(startPos, endPos, smoothT);

                // Дуга (параболическое смещение по Y)
                float arcProgress = 1f - (2f * t - 1f) * (2f * t - 1f);
                currentPos.y += arcProgress * _itemFlyHeight;

                _itemTransform.anchoredPosition = currentPos;

                yield return null;
            }

            _itemTransform.anchoredPosition = endPos;
        }

        /// <summary>
        /// Веселое вращение предмета вокруг оси Y (как монетка)
        /// </summary>
        private IEnumerator ItemRotation()
        {
            if (_itemTransform == null)
            {
                yield break;
            }

            float elapsed = 0f;
            float totalRotation = 0f;
            
            // Количество полных оборотов (360 * n)
            int fullSpins = Mathf.FloorToInt(_itemRotationSpeed * _itemRotationDuration / 360f);
            float targetRotation = fullSpins * 360f; // Гарантируем кратность 360

            while (elapsed < _itemRotationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _itemRotationDuration;
                
                // Ease out для замедления к концу
                float easedT = EaseOutCubic(t);
                totalRotation = easedT * targetRotation;

                _itemTransform.localRotation = Quaternion.Euler(0f, totalRotation, 0f);

                yield return null;
            }

            // Финальный snap к нулевому углу (0 градусов = начальная позиция)
            _itemTransform.localRotation = Quaternion.identity;
        }

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        /// <summary>
        /// Веселый bounce скейл в конце
        /// </summary>
        private IEnumerator ItemBounceScale()
        {
            if (_itemTransform == null)
            {
                yield break;
            }

            float elapsed = 0f;
            int bounceCount = 3;
            float bounceDuration = _itemFinalScaleDuration / bounceCount;

            for (int i = 0; i < bounceCount; i++)
            {
                elapsed = 0f;
                float bounceIntensity = 1f - (float)i / bounceCount; // Уменьшаем интенсивность

                while (elapsed < bounceDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / bounceDuration;

                    // Синусоидальный bounce
                    float scaleMultiplier = Mathf.Lerp(
                        _itemBounceScaleMax - (_itemBounceScaleMax - 1f) * (1f - bounceIntensity),
                        _itemBounceScaleMin + (1f - _itemBounceScaleMin) * (1f - bounceIntensity),
                        (Mathf.Sin(t * Mathf.PI) + 1f) / 2f
                    );

                    // К концу стремимся к 1
                    scaleMultiplier = Mathf.Lerp(scaleMultiplier, 1f, t * (1f - bounceIntensity));

                    _itemTransform.localScale = _itemOriginalScale * scaleMultiplier;

                    yield return null;
                }
            }

            // Финальный snap к оригинальному масштабу
            _itemTransform.localScale = _itemOriginalScale;
        }

        private float EaseInQuad(float t)
        {
            return t * t;
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
            // Останавливаем все корутины
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

            if (_itemAnimationCoroutine != null)
            {
                StopCoroutine(_itemAnimationCoroutine);
                _itemAnimationCoroutine = null;
            }

            if (_itemRotationCoroutine != null)
            {
                StopCoroutine(_itemRotationCoroutine);
                _itemRotationCoroutine = null;
            }

            // Сброс сундука к начальному состоянию
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

            // Сброс предмета к начальному состоянию
            InitializeItemState();
            
            // Восстанавливаем видимость надписей
            if (_label1CanvasGroup != null)
            {
                _label1CanvasGroup.alpha = 1f;
            }

            if (_label2CanvasGroup != null)
            {
                _label2CanvasGroup.alpha = 1f;
            }
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

