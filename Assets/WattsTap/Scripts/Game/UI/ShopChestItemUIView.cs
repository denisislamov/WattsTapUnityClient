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

        [Header("WebGL/Mobile Optimization")]
        [SerializeField] private bool _useOptimizedMode = true;
        [SerializeField] private float _targetFrameInterval = 0.016f; // ~60fps cap for smooth animation
        [SerializeField] private bool _useUnscaledTime = true;
        [SerializeField] private int _rotationSteps = 12; // Количество дискретных шагов вращения для WebGL

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
        
        // Кэшированные значения для оптимизации WebGL
        private WaitForEndOfFrame _waitForEndOfFrame;
        private float _cachedDeltaTime;
        private bool _isWebGL;

        public Button BackButton => _backButton;
        public Button OpenButton => _openButton;

        public event Action OnOpenAnimationComplete;

        private void Awake()
        {
            _isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
            _waitForEndOfFrame = new WaitForEndOfFrame();
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
        /// Плавное скрытие надписей (оптимизировано для WebGL)
        /// </summary>
        private IEnumerator FadeOutLabels()
        {
            float elapsed = 0f;

            while (elapsed < _labelFadeOutDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _labelFadeOutDuration);
                float alpha = 1f - t * t; // Простой ease out

                if (_label1CanvasGroup != null)
                {
                    _label1CanvasGroup.alpha = alpha;
                }

                if (_label2CanvasGroup != null)
                {
                    _label2CanvasGroup.alpha = alpha;
                }

                yield return GetAnimationYield();
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
            
            // Предрасчитанные значения для оптимизации
            float piValue = Mathf.PI;

            while (elapsed < _jumpDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _jumpDuration);

                // Parabolic jump curve
                float jumpProgress = 1f - (2f * t - 1f) * (2f * t - 1f);
                float yOffset = jumpProgress * _jumpHeight;

                // Squash and stretch - упрощенный расчет
                float scaleT = Mathf.Sin(t * piValue);
                float scaleX = 1f - (1f - 1f / _scaleAmount) * scaleT * 0.3f;
                float scaleY = 1f + (_scaleAmount - 1f) * scaleT * 0.5f;

                if (_chestTransform != null)
                {
                    _chestTransform.anchoredPosition = new Vector2(startPos.x, startPos.y + yOffset);
                    _chestTransform.localScale = new Vector3(
                        _originalScale.x * scaleX,
                        _originalScale.y * scaleY,
                        _originalScale.z
                    );
                }

                yield return GetAnimationYield();
            }

            // Landing squash
            yield return StartCoroutine(LandingSquash());
        }

        private IEnumerator LandingSquash()
        {
            float elapsed = 0f;
            float duration = 0.1f;
            float startSquash = _scaleAmount * 0.9f;
            float startStretch = 1f / startSquash;
            
            // Используем упрощенный elastic на WebGL
            bool useSimpleEasing = _useOptimizedMode && _isWebGL;

            while (elapsed < duration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / duration);

                float easeValue = useSimpleEasing ? EaseOutElasticSimple(t) : EaseOutElastic(t);
                float squash = Mathf.Lerp(startSquash, 1f, easeValue);
                float stretch = Mathf.Lerp(startStretch, 1f, easeValue);

                if (_chestTransform != null)
                {
                    _chestTransform.anchoredPosition = _originalPosition;
                    _chestTransform.localScale = new Vector3(
                        _originalScale.x * squash,
                        _originalScale.y * stretch,
                        _originalScale.z
                    );
                }

                yield return GetAnimationYield();
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
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _crossfadeDuration);
                
                // Открытый сундук появляется в 2 раза быстрее и с опережением
                // При t=0.5 открытый уже полностью виден (alpha=1)
                float openAlpha = Mathf.Clamp01(t * 2f);
                
                // Закрытый сундук исчезает медленнее и с задержкой
                // Начинает исчезать только когда открытый уже наполовину виден
                float closeAlpha = 1f - Mathf.Clamp01((t - 0.25f) * 1.33f);
                
                // Применяем smooth step для плавности
                openAlpha = openAlpha * openAlpha * (3f - 2f * openAlpha);
                closeAlpha = closeAlpha * closeAlpha * (3f - 2f * closeAlpha);

                SetImageAlpha(_chestClosedImage, closeAlpha);
                SetImageAlpha(_chestOpenImage1, openAlpha);
                SetImageAlpha(_chestOpenImage2, openAlpha);

                yield return GetAnimationYield();
            }

            // Финальные значения
            SetImageAlpha(_chestClosedImage, 0f);
            SetImageAlpha(_chestOpenImage1, 1f);
            SetImageAlpha(_chestOpenImage2, 1f);
        }

        private IEnumerator FadeInGlow()
        {
            float elapsed = 0f;
            
            // Целевое значение - минимум пульсации, чтобы плавно перейти в PulseGlow
            // PulseGlow начинается с _glowPulseMin (синус начинается с -1 при сдвиге на -PI/2)
            float targetAlpha = _glowPulseMin;

            while (elapsed < _glowFadeInDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _glowFadeInDuration);

                // Плавный ease out к минимальному значению пульсации
                float easeT = t * (2f - t);
                SetImageAlpha(_glowImage, easeT * targetAlpha);

                yield return GetAnimationYield();
            }
            
            SetImageAlpha(_glowImage, targetAlpha);
        }

        private IEnumerator PulseGlow()
        {
            // Предрасчитанные константы
            float piTimesTwo = Mathf.PI * 2f;
            float range = _glowPulseMax - _glowPulseMin;
            float center = (_glowPulseMax + _glowPulseMin) * 0.5f;
            float halfRange = range * 0.5f;

            while (true)
            {
                float elapsed = 0f;

                while (elapsed < _glowPulseDuration)
                {
                    elapsed += GetDeltaTime();
                    float t = elapsed / _glowPulseDuration;

                    // Синус начинается с 0 и идёт вверх (от min к max и обратно)
                    // sin(0) = 0, поэтому начальное значение = center = (min+max)/2
                    // Но нам нужно начать с min, поэтому используем косинус со сдвигом
                    // cos(PI) = -1, значит начинаем с min
                    float alpha = center + Mathf.Cos(Mathf.PI + t * piTimesTwo) * halfRange;
                    SetImageAlpha(_glowImage, alpha);

                    yield return GetAnimationYield();
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
        /// Fade in предмета (оптимизировано для WebGL)
        /// </summary>
        private IEnumerator ItemFadeIn()
        {
            float elapsed = 0f;

            while (elapsed < _itemFadeInDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _itemFadeInDuration);

                if (_itemCanvasGroup != null)
                {
                    // Простой квадратичный ease out
                    _itemCanvasGroup.alpha = t * (2f - t);
                }

                yield return GetAnimationYield();
            }

            if (_itemCanvasGroup != null)
            {
                _itemCanvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Вылет предмета от стартовой до конечной позиции с дугой (оптимизировано для WebGL)
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
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _itemFlyDuration);
                
                // Упрощенный ease out
                float smoothT = t * (2f - t);

                // Линейная интерполяция позиции
                Vector2 currentPos = Vector2.Lerp(startPos, endPos, smoothT);

                // Дуга (параболическое смещение по Y)
                float arcProgress = 1f - (2f * t - 1f) * (2f * t - 1f);
                currentPos.y += arcProgress * _itemFlyHeight;

                _itemTransform.anchoredPosition = currentPos;

                yield return GetAnimationYield();
            }

            _itemTransform.anchoredPosition = endPos;
        }

        /// <summary>
        /// Веселое вращение предмета вокруг оси Y (оптимизировано для WebGL)
        /// </summary>
        private IEnumerator ItemRotation()
        {
            if (_itemTransform == null)
            {
                yield break;
            }

            float elapsed = 0f;
            
            // Количество полных оборотов (360 * n)
            int fullSpins = Mathf.FloorToInt(_itemRotationSpeed * _itemRotationDuration / 360f);
            float targetRotation = fullSpins * 360f; // Гарантируем кратность 360

            // Для WebGL используем меньше обновлений для плавности
            if (_useOptimizedMode && _isWebGL)
            {
                // Дискретное вращение с фиксированными шагами
                int totalSteps = _rotationSteps * fullSpins;
                float stepDuration = _itemRotationDuration / totalSteps;
                
                for (int step = 0; step < totalSteps; step++)
                {
                    float t = (float)(step + 1) / totalSteps;
                    float easedT = 1f - (1f - t) * (1f - t) * (1f - t); // EaseOutCubic inline
                    float currentAngle = easedT * targetRotation;
                    
                    _itemTransform.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
                    
                    yield return new WaitForSecondsRealtime(stepDuration);
                }
            }
            else
            {
                // Стандартное плавное вращение
                while (elapsed < _itemRotationDuration)
                {
                    elapsed += GetDeltaTime();
                    float t = Mathf.Clamp01(elapsed / _itemRotationDuration);
                    
                    // Ease out для замедления к концу
                    float easedT = 1f - (1f - t) * (1f - t) * (1f - t);
                    float totalRotation = easedT * targetRotation;

                    _itemTransform.localRotation = Quaternion.Euler(0f, totalRotation, 0f);

                    yield return GetAnimationYield();
                }
            }

            // Финальный snap к нулевому углу (0 градусов = начальная позиция)
            _itemTransform.localRotation = Quaternion.identity;
        }

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        /// <summary>
        /// Веселый bounce скейл в конце (оптимизировано для WebGL)
        /// </summary>
        private IEnumerator ItemBounceScale()
        {
            if (_itemTransform == null)
            {
                yield break;
            }

            int bounceCount = 3;
            float bounceDuration = _itemFinalScaleDuration / bounceCount;

            for (int i = 0; i < bounceCount; i++)
            {
                float elapsed = 0f;
                float bounceIntensity = 1f - (float)i / bounceCount; // Уменьшаем интенсивность

                while (elapsed < bounceDuration)
                {
                    elapsed += GetDeltaTime();
                    float t = Mathf.Clamp01(elapsed / bounceDuration);

                    // Упрощенный bounce - используем простой синус
                    float sinValue = Mathf.Sin(t * Mathf.PI);
                    float maxScale = _itemBounceScaleMax - (_itemBounceScaleMax - 1f) * (1f - bounceIntensity);
                    float minScale = _itemBounceScaleMin + (1f - _itemBounceScaleMin) * (1f - bounceIntensity);
                    
                    float scaleMultiplier = Mathf.Lerp(maxScale, minScale, sinValue);
                    
                    // К концу стремимся к 1
                    scaleMultiplier = Mathf.Lerp(scaleMultiplier, 1f, t * (1f - bounceIntensity));

                    _itemTransform.localScale = _itemOriginalScale * scaleMultiplier;

                    yield return GetAnimationYield();
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

        /// <summary>
        /// Упрощенный elastic для WebGL - меньше вычислений
        /// </summary>
        private float EaseOutElasticSimple(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            
            // Упрощенная версия без тяжелых Pow и Sin
            float bounce = 1f - t;
            return 1f - bounce * bounce * Mathf.Cos(t * 3f * Mathf.PI) * 0.3f;
        }

        /// <summary>
        /// Получает delta time в зависимости от настроек оптимизации
        /// </summary>
        private float GetDeltaTime()
        {
            if (_useOptimizedMode && _useUnscaledTime)
            {
                return Time.unscaledDeltaTime;
            }
            return Time.deltaTime;
        }

        /// <summary>
        /// Возвращает объект ожидания для анимации в зависимости от настроек
        /// </summary>
        private object GetAnimationYield()
        {
            if (_useOptimizedMode && _isWebGL)
            {
                // На WebGL используем WaitForEndOfFrame для более стабильной анимации
                return _waitForEndOfFrame;
            }
            return null;
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

