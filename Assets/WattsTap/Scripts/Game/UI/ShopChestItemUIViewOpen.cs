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
        [SerializeField] private RectTransform _chestTransform;
        [SerializeField] private Image _chestClosedImage;
        [SerializeField] private Image _chestOpenImage1;
        [SerializeField] private Image _chestOpenImage2;
        [SerializeField] private Image _glowImage;
        
        [Header("Back Glow (Behind Chest)")]
        [SerializeField] private Image _backGlowImage1;
        [SerializeField] private Image _backGlowImage2;

        [Header("Dust Particles (Base Impact)")]
        [SerializeField] private RectTransform _dustLeftTransform;
        [SerializeField] private CanvasGroup _dustLeftCanvasGroup;
        [SerializeField] private RectTransform _dustRightTransform;
        [SerializeField] private CanvasGroup _dustRightCanvasGroup;

        [Header("Sparkle Particles")]
        [SerializeField] private RectTransform[] _sparkleTransforms;
        [SerializeField] private CanvasGroup[] _sparkleCanvasGroups;

        [Header("Animation Settings")]
        [SerializeField] private float _jumpHeight = 50f;
        [SerializeField] private float _jumpDuration = 0.3f;
        [SerializeField] private int _jumpCount = 3;
        [SerializeField] private float _scaleAmount = 1.15f;
        [SerializeField] private float _crossfadeDuration = 0.4f;
        
        [Header("Idle Animation Settings")]
        [SerializeField] private float _idleJumpHeight = 50f;
        [SerializeField] private float _idleJumpDuration = 0.3f;
        [SerializeField] private int _idleJumpCount = 3;
        [SerializeField] private float _idleScaleAmount = 1.15f;
        [SerializeField] private float _idleInitialDelay = 1f;
        [SerializeField] private float _idleJumpInterval = 2f;
        [SerializeField] private float _glowFadeInDuration = 0.3f;
        [SerializeField] private float _glowPulseMin = 0.4f;
        [SerializeField] private float _glowPulseMax = 1f;
        [SerializeField] private float _glowPulseDuration = 0.8f;
        
        [Header("Back Glow Animation Settings")]
        [SerializeField] private AnimationCurve _backGlow1Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _backGlow1Duration = 2.5f;
        [SerializeField] private AnimationCurve _backGlow2Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _backGlow2Duration = 3f;

        [Header("Dust Animation Settings")]
        [SerializeField] private AnimationCurve _dustAlphaCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private float _dustDuration = 0.7f;
        [SerializeField] private float _dustSpreadDistance = 80f;
        [SerializeField] private float _dustVerticalDrift = 25f;

        [Header("Sparkle Animation Settings")]
        [SerializeField] private AnimationCurve _sparkleAlphaCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _sparkleScaleCurve = AnimationCurve.Linear(0f, 0.3f, 1f, 1.5f);
        [SerializeField] private float _sparkleDuration = 0.35f;
        [SerializeField] private float _sparkleInterval = 0.1f;
        [SerializeField] private float _sparkleRadius = 45f;

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
        [SerializeField] private float _itemBounceScaleMinX = 0.85f;
        [SerializeField] private float _itemBounceScaleMaxX = 1.15f;
        [SerializeField] private float _itemBounceScaleMinY = 0.8f;
        [SerializeField] private float _itemBounceScaleMaxY = 1.2f;
        [SerializeField] private int _itemBounceCount = 3;

        [Header("Item Crossfade Animation")]
        [SerializeField] private RectTransform[] _finalItemTransforms;
        [SerializeField] private CanvasGroup[] _finalItemCanvasGroups;
        [SerializeField] private float _itemCrossfadeDuration = 0.35f;
        [SerializeField] private float _itemCrossfadeScaleMin = 0.8f;
        [SerializeField] private float _itemCrossfadeScaleMax = 1.05f;

        [Header("Labels Animation")]
        [SerializeField] private CanvasGroup _label1CanvasGroup;
        [SerializeField] private CanvasGroup _label2CanvasGroup;
        [SerializeField] private float _labelFadeDuration = 0.3f;

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
        private Coroutine _idleAnimationCoroutine;
        private Coroutine _backGlow1Coroutine;
        private Coroutine _backGlow2Coroutine;
        private Coroutine _dustCoroutine;
        private Coroutine _sparkleCoroutine;
        private Vector2 _dustLeftOrigin;
        private Vector2 _dustRightOrigin;
        private Vector3[] _sparkleOriginalScales;
        private Vector3 _originalPosition;
        private Vector3 _originalScale;
        private Vector3 _itemOriginalScale;
        private Vector3 _finalItemOriginalScale;
        
        // Состояние анимации для массива элементов
        private int _currentFinalItemIndex = -1; // -1 означает, что анимация еще не запускалась
        private bool _isFirstAnimation = true;
        
        // Кэшированные значения для оптимизации WebGL
        private WaitForEndOfFrame _waitForEndOfFrame;
        private float _cachedDeltaTime;
        private bool _isWebGL;

        public Button BackButton => _backButton;
        public Button OpenButton => _openButton;

        public event Action OnOpenAnimationComplete;
        public event Action OnAnimationReset;

        private void Awake()
        {
            _isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
            _waitForEndOfFrame = new WaitForEndOfFrame();
            SaveAnimationOrigins();
            InitializeAnimationState();
        }

        private void SaveAnimationOrigins()
        {
            if (_dustLeftTransform != null)
                _dustLeftOrigin = _dustLeftTransform.anchoredPosition;
            if (_dustRightTransform != null)
                _dustRightOrigin = _dustRightTransform.anchoredPosition;

            if (_sparkleTransforms != null)
            {
                _sparkleOriginalScales = new Vector3[_sparkleTransforms.Length];
                for (int i = 0; i < _sparkleTransforms.Length; i++)
                {
                    _sparkleOriginalScales[i] = _sparkleTransforms[i] != null
                        ? _sparkleTransforms[i].localScale
                        : Vector3.one;
                }
            }
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

            if (_finalItemTransforms != null && _finalItemTransforms.Length > 0 && _finalItemTransforms[0] != null)
            {
                _finalItemOriginalScale = _finalItemTransforms[0].localScale;
            }

            // Закрытый сундук - видимый
            SetImageAlpha(_chestClosedImage, 1f);
            
            // Открытые картинки сундука - скрыты
            SetImageAlpha(_chestOpenImage1, 0f);
            SetImageAlpha(_chestOpenImage2, 0f);
            
            // Свечение - скрыто
            SetImageAlpha(_glowImage, 0f);
            
            // Свечения сзади - скрыты
            SetImageAlpha(_backGlowImage1, 0f);
            SetImageAlpha(_backGlowImage2, 0f);

            // Пыль - скрыта и в начальных позициях
            ResetDustState();

            // Искры - скрыты
            ResetSparklesState();
            
            // Предмет - скрыт и в начальной позиции
            InitializeItemState();

            // Финальный предмет - скрыт и синхронизирован
            InitializeFinalItemState();
            
            // Надписи - скрыты изначально (появятся после анимации предмета)
            InitializeLabelsState();
        }
        
        /// <summary>
        /// Инициализирует начальное состояние надписей (скрыты)
        /// </summary>
        private void InitializeLabelsState()
        {
            if (_label1CanvasGroup != null)
            {
                _label1CanvasGroup.alpha = 0f;
            }

            if (_label2CanvasGroup != null)
            {
                _label2CanvasGroup.alpha = 0f;
            }
        }

        private void InitializeFinalItemState()
        {
            if (_finalItemTransforms != null)
            {
                for (int i = 0; i < _finalItemTransforms.Length; i++)
                {
                    if (_finalItemTransforms[i] != null)
                    {
                        // Совмещаем позицию с конечной позицией основного предмета
                        if (_itemEndPosition != null)
                        {
                            _finalItemTransforms[i].anchoredPosition = _itemEndPosition.anchoredPosition;
                        }
                        _finalItemTransforms[i].localRotation = Quaternion.identity;
                        _finalItemTransforms[i].localScale = _itemOriginalScale * _itemCrossfadeScaleMin;
                    }
                }
            }

            if (_finalItemCanvasGroups != null)
            {
                for (int i = 0; i < _finalItemCanvasGroups.Length; i++)
                {
                    if (_finalItemCanvasGroups[i] != null)
                    {
                        _finalItemCanvasGroups[i].alpha = 0f;
                    }
                }
            }
        }


        private void Start()
        {
            if (_openButton != null)
            {
                _openButton.onClick.AddListener(PlayOpenAnimation);
            }
            
            StartIdleAnimation();
        }
        
        /// <summary>
        /// Запускает idle анимацию подпрыгивания сундука
        /// </summary>
        public void StartIdleAnimation()
        {
            StopIdleAnimation();
            _idleAnimationCoroutine = StartCoroutine(IdleAnimationLoop());
        }
        
        /// <summary>
        /// Останавливает idle анимацию
        /// </summary>
        public void StopIdleAnimation()
        {
            if (_idleAnimationCoroutine != null)
            {
                StopCoroutine(_idleAnimationCoroutine);
                _idleAnimationCoroutine = null;
            }
        }
        
        /// <summary>
        /// Цикл idle анимации с интервалами между прыжками
        /// </summary>
        private IEnumerator IdleAnimationLoop()
        {
            // Начальная задержка перед первым прыжком
            if (_useUnscaledTime)
            {
                yield return new WaitForSecondsRealtime(_idleInitialDelay);
            }
            else
            {
                yield return new WaitForSeconds(_idleInitialDelay);
            }
            
            while (true)
            {
                // Выполняем серию прыжков
                for (int i = 0; i < _idleJumpCount; i++)
                {
                    yield return StartCoroutine(IdleJumpAndSquash());
                }
                
                // Ждём интервал перед следующей серией прыжков
                if (_useUnscaledTime)
                {
                    yield return new WaitForSecondsRealtime(_idleJumpInterval);
                }
                else
                {
                    yield return new WaitForSeconds(_idleJumpInterval);
                }
            }
        }
        
        /// <summary>
        /// Один прыжок для idle анимации (использует idle параметры)
        /// </summary>
        private IEnumerator IdleJumpAndSquash()
        {
            float elapsed = 0f;
            Vector3 startPos = _originalPosition;
            
            // Предрасчитанные значения для оптимизации
            float piValue = Mathf.PI;

            while (elapsed < _idleJumpDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _idleJumpDuration);

                // Parabolic jump curve
                float jumpProgress = 1f - (2f * t - 1f) * (2f * t - 1f);
                float yOffset = jumpProgress * _idleJumpHeight;

                // Squash and stretch - упрощенный расчет
                float scaleT = Mathf.Sin(t * piValue);
                float scaleX = 1f - (1f - 1f / _idleScaleAmount) * scaleT * 0.3f;
                float scaleY = 1f + (_idleScaleAmount - 1f) * scaleT * 0.5f;

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
            yield return StartCoroutine(IdleLandingSquash());
        }
        
        /// <summary>
        /// Landing squash для idle анимации (использует idle параметры)
        /// </summary>
        private IEnumerator IdleLandingSquash()
        {
            float elapsed = 0f;
            float duration = 0.1f;
            float startSquash = _idleScaleAmount * 0.9f;
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

        public void PlayOpenAnimation()
        {
            // Отключаем кнопку на время анимации
            SetOpenButtonInteractable(false);
            
            // Останавливаем idle анимацию
            StopIdleAnimation();
            
            // Проверяем, нужно ли перезапустить с начала
            if (_finalItemTransforms == null || _finalItemTransforms.Length == 0 || 
                _currentFinalItemIndex >= _finalItemTransforms.Length - 1)
            {
                // Массив закончился или не инициализирован - перезапускаем с начала
                _currentFinalItemIndex = -1;
                _isFirstAnimation = true;
                ResetAnimation();
                
                // Запускаем свечения сзади сразу при тапе
                StartBackGlowAnimations();
                
                _animationCoroutine = StartCoroutine(OpenAnimationSequence());
            }
            else if (_isFirstAnimation)
            {
                // Первый запуск - полная анимация до первого элемента
                // Запускаем свечения сзади сразу при тапе
                StartBackGlowAnimations();
                
                _animationCoroutine = StartCoroutine(OpenAnimationSequence());
            }
            else
            {
                // Последующие нажатия - показываем следующий элемент
                _animationCoroutine = StartCoroutine(ShowNextItemSequence());
            }
        }
        
        /// <summary>
        /// Запускает анимации свечений сзади сундука (вызывается сразу при тапе)
        /// </summary>
        private void StartBackGlowAnimations()
        {
            StopBackGlowAnimations();
            _backGlow1Coroutine = StartCoroutine(BackGlow1AnimationSequence());
            _backGlow2Coroutine = StartCoroutine(BackGlow2AnimationSequence());
        }
        
        /// <summary>
        /// Останавливает анимации свечений сзади
        /// </summary>
        private void StopBackGlowAnimations()
        {
            if (_backGlow1Coroutine != null)
            {
                StopCoroutine(_backGlow1Coroutine);
                _backGlow1Coroutine = null;
            }
            if (_backGlow2Coroutine != null)
            {
                StopCoroutine(_backGlow2Coroutine);
                _backGlow2Coroutine = null;
            }
        }
        
        /// <summary>
        /// Анимация первого свечения сзади по кривой
        /// </summary>
        private IEnumerator BackGlow1AnimationSequence()
        {
            if (_backGlowImage1 == null || _backGlow1Curve == null) yield break;
            
            float elapsed = 0f;

            while (elapsed < _backGlow1Duration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _backGlow1Duration);
                float alpha = _backGlow1Curve.Evaluate(t);
                SetImageAlpha(_backGlowImage1, alpha);
                yield return GetAnimationYield();
            }
            
            SetImageAlpha(_backGlowImage1, _backGlow1Curve.Evaluate(1f));
        }
        
        /// <summary>
        /// Анимация второго свечения сзади по кривой
        /// </summary>
        private IEnumerator BackGlow2AnimationSequence()
        {
            if (_backGlowImage2 == null || _backGlow2Curve == null) yield break;
            
            float elapsed = 0f;

            while (elapsed < _backGlow2Duration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _backGlow2Duration);
                float alpha = _backGlow2Curve.Evaluate(t);
                SetImageAlpha(_backGlowImage2, alpha);
                yield return GetAnimationYield();
            }
            
            SetImageAlpha(_backGlowImage2, _backGlow2Curve.Evaluate(1f));
        }

        private IEnumerator OpenAnimationSequence()
        {
            // Phase 1: Bouncy jumps with scale
            for (int i = 0; i < _jumpCount; i++)
            {
                yield return StartCoroutine(JumpAndSquash());
            }

            // Сундук приземлился — запускаем пыль
            _dustCoroutine = StartCoroutine(DustImpactAnimation());

            // Phase 2: Crossfade chest sprites
            yield return StartCoroutine(CrossfadeChestSprites());

            // Phase 3: Glow appears and pulses
            yield return StartCoroutine(FadeInGlow());
            _glowPulseCoroutine = StartCoroutine(PulseGlow());

            // Phase 4: Item flies out
            yield return StartCoroutine(ItemFlyOutSequence());

            // Устанавливаем индекс на первый элемент и отмечаем, что первая анимация завершена
            _currentFinalItemIndex = 0;
            _isFirstAnimation = false;

            // Включаем кнопку обратно после завершения анимации
            SetOpenButtonInteractable(true);

            OnOpenAnimationComplete?.Invoke();
        }

        /// <summary>
        /// Последовательность для показа следующего элемента (без полной анимации сундука)
        /// </summary>
        private IEnumerator ShowNextItemSequence()
        {
            // 1. Прячем предыдущий элемент и текст
            yield return StartCoroutine(HidePreviousItemAndLabels());

            // 2. Сбрасываем состояние летящего предмета
            InitializeItemState();

            // 3. Проигрываем анимацию вылета предмета
            yield return StartCoroutine(ItemFlyOutSequence());

            // Переходим к следующему элементу
            _currentFinalItemIndex++;

            // Включаем кнопку обратно после завершения анимации
            SetOpenButtonInteractable(true);

            OnOpenAnimationComplete?.Invoke();
        }

        /// <summary>
        /// Прячем предыдущий finalItem и надписи
        /// </summary>
        private IEnumerator HidePreviousItemAndLabels()
        {
            if (_currentFinalItemIndex < 0 || _finalItemTransforms == null || 
                _currentFinalItemIndex >= _finalItemTransforms.Length)
            {
                yield break;
            }

            float elapsed = 0f;
            float fadeDuration = 0.2f;

            CanvasGroup previousCanvasGroup = (_finalItemCanvasGroups != null && _currentFinalItemIndex < _finalItemCanvasGroups.Length) 
                ? _finalItemCanvasGroups[_currentFinalItemIndex] 
                : null;

            float startAlpha = previousCanvasGroup != null ? previousCanvasGroup.alpha : 1f;

            while (elapsed < fadeDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                float alpha = Mathf.Lerp(startAlpha, 0f, t);

                // Прячем предыдущий элемент
                if (previousCanvasGroup != null)
                {
                    previousCanvasGroup.alpha = alpha;
                }

                // Прячем надписи
                if (_label1CanvasGroup != null)
                {
                    _label1CanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                }
                if (_label2CanvasGroup != null)
                {
                    _label2CanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                }

                yield return GetAnimationYield();
            }

            // Финальные значения
            if (previousCanvasGroup != null)
            {
                previousCanvasGroup.alpha = 0f;
            }
            if (_label1CanvasGroup != null)
            {
                _label1CanvasGroup.alpha = 0f;
            }
            if (_label2CanvasGroup != null)
            {
                _label2CanvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Плавное появление надписей после анимации предмета (оптимизировано для WebGL)
        /// </summary>
        private IEnumerator FadeInLabels()
        {
            float elapsed = 0f;

            while (elapsed < _labelFadeDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _labelFadeDuration);
                float alpha = t * (2f - t); // Простой ease out для появления

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
                _label1CanvasGroup.alpha = 1f;
            }

            if (_label2CanvasGroup != null)
            {
                _label2CanvasGroup.alpha = 1f;
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
            
            // Количество тактов пульсации перед исчезновением
            int pulseCount = 2;

            for (int i = 0; i < pulseCount; i++)
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
            
            // Плавное исчезновение свечения после двух тактов
            yield return StartCoroutine(FadeOutGlow());
        }
        
        /// <summary>
        /// Плавное исчезновение свечения
        /// </summary>
        private IEnumerator FadeOutGlow()
        {
            float elapsed = 0f;
            float startAlpha = _glowImage != null ? _glowImage.color.a : _glowPulseMin;
            float fadeOutDuration = _glowFadeInDuration; // Используем ту же длительность что и для появления

            while (elapsed < fadeOutDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / fadeOutDuration);

                // Плавный ease out для исчезновения
                float easeT = t * t;
                float alpha = Mathf.Lerp(startAlpha, 0f, easeT);
                SetImageAlpha(_glowImage, alpha);

                yield return GetAnimationYield();
            }
            
            SetImageAlpha(_glowImage, 0f);
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

            // Мгновенное появление предмета с начальным масштабом 0.5
            if (_itemCanvasGroup != null)
            {
                _itemCanvasGroup.alpha = 1f;
            }
            if (_itemTransform != null)
            {
                _itemTransform.localScale = _itemOriginalScale * 0.5f;
            }

            // Искры только при первом открытии сундука
            if (_isFirstAnimation)
            {
                _sparkleCoroutine = StartCoroutine(SparklesDuringFlight());
            }

            // Вылет предмета по дуге с вращением, bounce и увеличением масштаба
            yield return StartCoroutine(ItemFlyWithRotationAndScale());
            
            // Замена на финальный предмет с появлением текста
            yield return StartCoroutine(CrossfadeToFinalItem());
        }
        
        /// <summary>
        /// Анимация замены предмета на финальный: FinalItem появляется мгновенно за _item, затем _item исчезает
        /// </summary>
        private IEnumerator CrossfadeToFinalItem()
        {
            // Получаем следующий индекс для отображения
            int targetIndex = _isFirstAnimation ? 0 : _currentFinalItemIndex + 1;
            
            if (_finalItemTransforms == null || targetIndex >= _finalItemTransforms.Length ||
                _finalItemCanvasGroups == null || targetIndex >= _finalItemCanvasGroups.Length)
            {
                // Если финальный предмет не назначен, просто показываем надписи
                yield return StartCoroutine(FadeInLabels());
                yield break;
            }

            RectTransform currentFinalTransform = _finalItemTransforms[targetIndex];
            CanvasGroup currentFinalCanvasGroup = _finalItemCanvasGroups[targetIndex];

            if (currentFinalTransform == null || currentFinalCanvasGroup == null)
            {
                // Если элемент не настроен, показываем надписи
                yield return StartCoroutine(FadeInLabels());
                yield break;
            }

            // Синхронизируем позицию и масштаб финального предмета с текущим _item
            if (currentFinalTransform != null && _itemTransform != null)
            {
                currentFinalTransform.anchoredPosition = _itemTransform.anchoredPosition;
                currentFinalTransform.localScale = _itemOriginalScale;
            }
            
            // Мгновенно показываем финальный предмет (он будет за _item)
            if (currentFinalCanvasGroup != null)
            {
                currentFinalCanvasGroup.alpha = 1f;
            }

            // Теперь анимируем исчезновение _item и появление надписей
            float elapsed = 0f;
            float itemStartAlpha = 1f;

            while (elapsed < _itemCrossfadeDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _itemCrossfadeDuration);
                float smoothT = t * t * (3f - 2f * t); // Smoothstep

                // Исходный предмет: только исчезает
                if (_itemCanvasGroup != null)
                {
                    _itemCanvasGroup.alpha = Mathf.Lerp(itemStartAlpha, 0f, smoothT);
                }

                // Надписи появляются синхронно с исчезновением _item
                float labelAlpha = smoothT;
                if (_label1CanvasGroup != null)
                {
                    _label1CanvasGroup.alpha = labelAlpha;
                }
                if (_label2CanvasGroup != null)
                {
                    _label2CanvasGroup.alpha = labelAlpha;
                }

                yield return GetAnimationYield();
            }

            // Финальные значения
            if (_itemCanvasGroup != null)
            {
                _itemCanvasGroup.alpha = 0f;
            }

            if (currentFinalCanvasGroup != null)
            {
                currentFinalCanvasGroup.alpha = 1f;
            }

            if (_label1CanvasGroup != null)
            {
                _label1CanvasGroup.alpha = 1f;
            }
            if (_label2CanvasGroup != null)
            {
                _label2CanvasGroup.alpha = 1f;
            }
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
        /// Вылет предмета от стартовой до конечной позиции с дугой, вращением и увеличением масштаба (оптимизировано для WebGL)
        /// </summary>
        private IEnumerator ItemFlyWithRotationAndScale()
        {
            if (_itemTransform == null || _itemStartPosition == null || _itemEndPosition == null)
            {
                yield break;
            }

            float elapsed = 0f;
            Vector2 startPos = _itemStartPosition.anchoredPosition;
            Vector2 endPos = _itemEndPosition.anchoredPosition;
            
            // Масштаб от 0.5 до 1.0
            float startScaleMultiplier = 0.5f;
            float endScaleMultiplier = 1f;
            
            // Количество полных оборотов для вращения
            int fullSpins = Mathf.FloorToInt(_itemRotationSpeed * _itemFlyDuration / 360f);
            float targetRotation = fullSpins * 360f;
            
            // Bounce параметры - количество bounce циклов за время полёта
            float bounceCycleDuration = _itemFlyDuration / _itemBounceCount;

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
                
                // Базовый масштаб от 0.5 до 1.0
                float baseScale = Mathf.Lerp(startScaleMultiplier, endScaleMultiplier, smoothT);
                
                // Bounce эффект поверх базового масштаба
                float bounceProgress = (elapsed % bounceCycleDuration) / bounceCycleDuration;
                float bounceIntensity = 1f - smoothT; // Bounce затухает к концу полёта
                float sinValue = Mathf.Sin(bounceProgress * Mathf.PI);
                
                // X axis bounce
                float maxScaleX = _itemBounceScaleMaxX - (_itemBounceScaleMaxX - 1f) * (1f - bounceIntensity);
                float minScaleX = _itemBounceScaleMinX + (1f - _itemBounceScaleMinX) * (1f - bounceIntensity);
                float bounceX = Mathf.Lerp(maxScaleX, minScaleX, sinValue);
                bounceX = Mathf.Lerp(bounceX, 1f, smoothT);
                
                // Y axis bounce
                float maxScaleY = _itemBounceScaleMaxY - (_itemBounceScaleMaxY - 1f) * (1f - bounceIntensity);
                float minScaleY = _itemBounceScaleMinY + (1f - _itemBounceScaleMinY) * (1f - bounceIntensity);
                float bounceY = Mathf.Lerp(maxScaleY, minScaleY, sinValue);
                bounceY = Mathf.Lerp(bounceY, 1f, smoothT);
                
                _itemTransform.localScale = new Vector3(
                    _itemOriginalScale.x * baseScale * bounceX,
                    _itemOriginalScale.y * baseScale * bounceY,
                    _itemOriginalScale.z * baseScale
                );
                
                // Вращение
                float easedRotationT = 1f - (1f - t) * (1f - t) * (1f - t); // EaseOutCubic
                float currentAngle = easedRotationT * targetRotation;
                _itemTransform.localRotation = Quaternion.Euler(0f, currentAngle, 0f);

                yield return GetAnimationYield();
            }

            // Финальные значения
            _itemTransform.anchoredPosition = endPos;
            _itemTransform.localScale = _itemOriginalScale;
            _itemTransform.localRotation = Quaternion.identity;
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
        private float EaseInQuad(float t)
        {
            return t * t;
        }

        private void ResetDustState()
        {
            if (_dustLeftCanvasGroup != null) _dustLeftCanvasGroup.alpha = 0f;
            if (_dustRightCanvasGroup != null) _dustRightCanvasGroup.alpha = 0f;
            if (_dustLeftTransform != null)
                _dustLeftTransform.anchoredPosition = _dustLeftOrigin;
            if (_dustRightTransform != null)
                _dustRightTransform.anchoredPosition = _dustRightOrigin;
        }

        private void ResetSparklesState()
        {
            if (_sparkleCanvasGroups == null) return;
            for (int i = 0; i < _sparkleCanvasGroups.Length; i++)
            {
                if (_sparkleCanvasGroups[i] != null)
                    _sparkleCanvasGroups[i].alpha = 0f;
            }
        }

        /// <summary>
        /// Пыль разлетается в стороны от основания сундука при вылете предмета
        /// </summary>
        private IEnumerator DustImpactAnimation()
        {
            if (_dustLeftTransform == null && _dustRightTransform == null) yield break;

            float elapsed = 0f;

            while (elapsed < _dustDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _dustDuration);
                float alpha = _dustAlphaCurve.Evaluate(t);
                float verticalOffset = _dustVerticalDrift * Mathf.Sin(t * Mathf.PI);

                if (_dustLeftTransform != null)
                {
                    _dustLeftTransform.anchoredPosition = _dustLeftOrigin +
                        new Vector2(-_dustSpreadDistance * t, verticalOffset);
                    if (_dustLeftCanvasGroup != null) _dustLeftCanvasGroup.alpha = alpha;
                }
                if (_dustRightTransform != null)
                {
                    _dustRightTransform.anchoredPosition = _dustRightOrigin +
                        new Vector2(_dustSpreadDistance * t, verticalOffset);
                    if (_dustRightCanvasGroup != null) _dustRightCanvasGroup.alpha = alpha;
                }

                yield return GetAnimationYield();
            }

            ResetDustState();
            _dustCoroutine = null;
        }

        /// <summary>
        /// Запускает искры поочерёдно вокруг предмета на протяжении его полёта
        /// </summary>
        private IEnumerator SparklesDuringFlight()
        {
            if (_sparkleCanvasGroups == null || _sparkleCanvasGroups.Length == 0 ||
                _sparkleTransforms == null || _sparkleTransforms.Length == 0) yield break;

            int count = Mathf.Min(_sparkleCanvasGroups.Length, _sparkleTransforms.Length);
            ResetSparklesState();

            float totalDuration = _itemFlyDuration;
            float elapsed = 0f;
            int nextIndex = 0;

            while (elapsed < totalDuration)
            {
                StartCoroutine(AnimateSingleSparkle(nextIndex % count));
                nextIndex++;

                float intervalElapsed = 0f;
                while (intervalElapsed < _sparkleInterval)
                {
                    float dt = GetDeltaTime();
                    intervalElapsed += dt;
                    elapsed += dt;
                    yield return GetAnimationYield();
                    if (elapsed >= totalDuration) break;
                }
            }

            // Ждём завершения последней искры
            if (_useUnscaledTime)
                yield return new WaitForSecondsRealtime(_sparkleDuration);
            else
                yield return new WaitForSeconds(_sparkleDuration);

            ResetSparklesState();
            _sparkleCoroutine = null;
        }

        /// <summary>
        /// Анимирует одну искру: появляется рядом с предметом, масштабируется и гаснет
        /// </summary>
        private IEnumerator AnimateSingleSparkle(int index)
        {
            if (_sparkleCanvasGroups == null || index >= _sparkleCanvasGroups.Length ||
                _sparkleTransforms == null || index >= _sparkleTransforms.Length) yield break;

            CanvasGroup sparkleGroup = _sparkleCanvasGroups[index];
            RectTransform sparkleTransform = _sparkleTransforms[index];

            if (sparkleGroup == null || sparkleTransform == null || _itemTransform == null) yield break;

            // Случайный угол и дистанция вокруг предмета
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(_sparkleRadius * 0.3f, _sparkleRadius);
            Vector2 offset = new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
            Vector2 basePos = _itemTransform.anchoredPosition + offset;

            Vector3 originalScale = (_sparkleOriginalScales != null && index < _sparkleOriginalScales.Length)
                ? _sparkleOriginalScales[index]
                : Vector3.one;

            float elapsed = 0f;

            while (elapsed < _sparkleDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _sparkleDuration);

                sparkleGroup.alpha = _sparkleAlphaCurve.Evaluate(t);
                sparkleTransform.localScale = originalScale * _sparkleScaleCurve.Evaluate(t);
                // Лёгкий дрейф вверх вместе с предметом
                sparkleTransform.anchoredPosition = basePos + new Vector2(0f, t * 12f);

                yield return GetAnimationYield();
            }

            sparkleGroup.alpha = 0f;
            sparkleTransform.localScale = originalScale;
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
            
            StopIdleAnimation();
            StopBackGlowAnimations();

            if (_dustCoroutine != null)
            {
                StopCoroutine(_dustCoroutine);
                _dustCoroutine = null;
            }
            if (_sparkleCoroutine != null)
            {
                StopCoroutine(_sparkleCoroutine);
                _sparkleCoroutine = null;
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
            SetImageAlpha(_backGlowImage1, 0f);
            SetImageAlpha(_backGlowImage2, 0f);
            ResetDustState();
            ResetSparklesState();

            // Сброс предмета к начальному состоянию
            InitializeItemState();

            // Сброс финального предмета
            InitializeFinalItemState();
            
            // Сброс надписей к начальному состоянию (скрыты)
            InitializeLabelsState();
            
            // Сброс состояния индексов
            _currentFinalItemIndex = -1;
            _isFirstAnimation = true;
            
            // Включаем кнопку обратно при сбросе
            SetOpenButtonInteractable(true);
            
            // Вызов события сброса анимации
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
            UpdateChestSprites(config.ChestClosedSprite, config.ChestOpenSprite1, config.ChestOpenSprite2);
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

        public void UpdateChestSprites(Sprite closedSprite, Sprite openSprite1, Sprite openSprite2)
        {
            if (_chestClosedImage != null && closedSprite != null)
            {
                _chestClosedImage.sprite = closedSprite;
            }

            if (_chestOpenImage1 != null && openSprite1 != null)
            {
                _chestOpenImage1.sprite = openSprite1;
            }

            if (_chestOpenImage2 != null && openSprite2 != null)
            {
                _chestOpenImage2.sprite = openSprite2;
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
