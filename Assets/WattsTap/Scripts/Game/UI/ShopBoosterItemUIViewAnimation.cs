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
    /// <summary>
    /// Animated view shown after purchasing a booster item.
    /// Displays the booster icon, a reward counter that counts up from 0 to the target value,
    /// a rotating decorative RectTransform, and a Claim button to dismiss.
    /// Pattern follows ShopChestItemUIViewOpen / MergeScreenUIViewAnimation.
    /// </summary>
    public class ShopBoosterItemUIViewAnimation : UIBaseView<ShopBoosterItemUIPresenterAnimation>
    {
        [Header("Display Elements")]
        [SerializeField] private TMP_Text _itemNameText;
        [SerializeField] private Image _boosterIconImage;
        [SerializeField] private TMP_Text _rewardCounterText;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Claim")]
        [SerializeField] private Button _claimButton;
        [SerializeField] private CanvasGroup _claimButtonCanvasGroup;

        [Header("Rotating Decoration")]
        [SerializeField] private RectTransform _rotatingTransform;
        [SerializeField] private float _rotationSpeed = 90f;

        [Header("Booster Icon Animation")]
        [SerializeField] private RectTransform _boosterIconTransform;
        [SerializeField] private CanvasGroup _boosterIconCanvasGroup;
        [SerializeField] private float _iconScaleFrom = 0f;
        [SerializeField] private float _iconScaleTo = 1f;
        [SerializeField] private float _iconRevealDuration = 0.4f;
        [SerializeField] private float _iconOvershootScale = 1.2f;

        [Header("Glow / VFX")]
        [SerializeField] private CanvasGroup _glowCanvasGroup;
        [SerializeField] private float _glowFadeInDuration = 0.3f;
        [SerializeField] private float _glowPulseMin = 0.4f;
        [SerializeField] private float _glowPulseMax = 1f;
        [SerializeField] private float _glowPulseDuration = 0.8f;

        [Header("Counter Animation")]
        [SerializeField] private float _counterDuration = 4f;
        [SerializeField] private AnimationCurve _counterCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private CanvasGroup _counterCanvasGroup;

        [Header("WebGL/Mobile Optimization")]
        [SerializeField] private bool _useUnscaledTime = true;

        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;

        private MainMenuThemeManager _themeManager;

        // ── State ──────────────────────────────────────────────────────
        private Coroutine _animationCoroutine;
        private Coroutine _rotationCoroutine;
        private Coroutine _glowPulseCoroutine;
        private bool _animationPlaying;
        private bool _animationFinished;

        private long _targetRewardAmount;
        private Vector3 _iconOriginalScale;
        private bool _isWebGL;
        private WaitForEndOfFrame _waitForEndOfFrame;

        // ── Events ─────────────────────────────────────────────────────
        public event Action OnBackClicked;
        public event Action OnClaimClicked;

        public Button BackButton => _backButton;
        public Button ClaimButton => _claimButton;
        public bool IsAnimationFinished => _animationFinished;

        // ── Unity Lifecycle ────────────────────────────────────────────

        private void Awake()
        {
            _isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
            _waitForEndOfFrame = new WaitForEndOfFrame();

            if (_boosterIconTransform != null)
                _iconOriginalScale = _boosterIconTransform.localScale;
        }

        private void OnEnable()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(HandleBackClick);

            if (_claimButton != null)
                _claimButton.onClick.AddListener(HandleClaimClick);

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
            if (_backButton != null)
                _backButton.onClick.RemoveListener(HandleBackClick);

            if (_claimButton != null)
                _claimButton.onClick.RemoveListener(HandleClaimClick);

            if (_themeManager != null)
                _themeManager.SkinChanged -= OnSkinChanged;
        }

        // ── Public API ─────────────────────────────────────────────────

        /// <summary>
        /// Populate the view with data from the booster config.
        /// </summary>
        public void UpdateFromConfig(ShopBoosterItemConfig config)
        {
            if (config == null) return;

            if (_itemNameText != null)
                _itemNameText.text = config.ItemName;

            if (_boosterIconImage != null && config.AnimationIcon != null)
                _boosterIconImage.sprite = config.AnimationIcon;

            _targetRewardAmount = config.RewardAmount;
        }

        /// <summary>
        /// Set the reward amount directly (overrides config value).
        /// </summary>
        public void SetRewardAmount(long amount)
        {
            _targetRewardAmount = amount;
        }

        /// <summary>
        /// Enable / disable claim button interactability.
        /// </summary>
        public void SetClaimButtonInteractable(bool interactable)
        {
            if (_claimButton != null)
                _claimButton.interactable = interactable;
        }

        // ── Main Animation ─────────────────────────────────────────────

        /// <summary>
        /// Starts the full booster purchase animation sequence.
        /// </summary>
        public void PlayAnimation()
        {
            if (_animationPlaying) return;

            StopAnimation();
            InitializeAnimationState();
            _animationPlaying = true;
            _animationFinished = false;
            _animationCoroutine = StartCoroutine(AnimationSequence());
        }

        public void StopAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            StopRotation();
            StopGlowPulse();
            _animationPlaying = false;
        }

        // ── Private: Animation Orchestration ───────────────────────────

        private void InitializeAnimationState()
        {
            // Icon — hidden and scaled to zero
            SetCanvasGroupAlpha(_boosterIconCanvasGroup, 0f);
            if (_boosterIconTransform != null)
                _boosterIconTransform.localScale = _iconOriginalScale * _iconScaleFrom;

            // Glow — hidden
            SetCanvasGroupAlpha(_glowCanvasGroup, 0f);

            // Counter — hidden, text zero
            SetCanvasGroupAlpha(_counterCanvasGroup, 0f);
            SetCounterText(0);

            // Claim button — hidden & non-interactable
            SetCanvasGroupAlpha(_claimButtonCanvasGroup, 0f);
            SetClaimButtonInteractable(false);

            // Rotating decoration — reset rotation
            if (_rotatingTransform != null)
                _rotatingTransform.localRotation = Quaternion.identity;
        }

        private IEnumerator AnimationSequence()
        {
            // Short idle hold
            yield return WaitSeconds(0.15f);

            // Start continuous rotation
            StartRotation();

            // Phase 1: Glow fades in
            yield return StartCoroutine(Phase_GlowFadeIn());
            StartGlowPulse();

            // Phase 2: Icon reveals (scale-up with overshoot)
            yield return StartCoroutine(Phase_IconReveal());

            // Phase 3: Counter counts up
            yield return StartCoroutine(Phase_CounterCountUp());

            // Phase 4: Claim button fades in
            yield return StartCoroutine(Phase_ClaimButtonReveal());

            _animationPlaying = false;
            _animationFinished = true;
        }

        // ── Phase 1: Glow ──────────────────────────────────────────────

        private IEnumerator Phase_GlowFadeIn()
        {
            if (_glowCanvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < _glowFadeInDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _glowFadeInDuration);
                SetCanvasGroupAlpha(_glowCanvasGroup, EaseOutQuad(t));
                yield return GetAnimationYield();
            }

            SetCanvasGroupAlpha(_glowCanvasGroup, 1f);
        }

        // ── Phase 2: Icon Reveal ───────────────────────────────────────

        private IEnumerator Phase_IconReveal()
        {
            SetCanvasGroupAlpha(_boosterIconCanvasGroup, 1f);

            if (_boosterIconTransform == null) yield break;

            float elapsed = 0f;
            while (elapsed < _iconRevealDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _iconRevealDuration);
                float eased = EaseOutBack(t);

                float scale;
                if (t < 0.7f)
                {
                    scale = Mathf.Lerp(_iconScaleFrom, _iconOvershootScale, eased);
                }
                else
                {
                    float settleT = (t - 0.7f) / 0.3f;
                    scale = Mathf.Lerp(_iconOvershootScale, _iconScaleTo, EaseInOutQuad(settleT));
                }

                _boosterIconTransform.localScale = _iconOriginalScale * scale;
                yield return GetAnimationYield();
            }

            _boosterIconTransform.localScale = _iconOriginalScale * _iconScaleTo;
        }

        // ── Phase 3: Counter Count-Up ──────────────────────────────────

        private IEnumerator Phase_CounterCountUp()
        {
            SetCanvasGroupAlpha(_counterCanvasGroup, 1f);

            float elapsed = 0f;
            while (elapsed < _counterDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _counterDuration);
                float curveT = _counterCurve != null ? _counterCurve.Evaluate(t) : t;

                long currentValue = (long)(_targetRewardAmount * curveT);
                SetCounterText(currentValue);
                yield return GetAnimationYield();
            }

            SetCounterText(_targetRewardAmount);
        }

        // ── Phase 4: Claim Button Reveal ───────────────────────────────

        private IEnumerator Phase_ClaimButtonReveal()
        {
            float fadeDuration = 0.3f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                SetCanvasGroupAlpha(_claimButtonCanvasGroup, EaseOutQuad(t));
                yield return GetAnimationYield();
            }

            SetCanvasGroupAlpha(_claimButtonCanvasGroup, 1f);
            SetClaimButtonInteractable(true);
        }

        // ── Continuous Rotation ────────────────────────────────────────

        private void StartRotation()
        {
            StopRotation();
            if (_rotatingTransform != null)
                _rotationCoroutine = StartCoroutine(RotationLoop());
        }

        private void StopRotation()
        {
            if (_rotationCoroutine != null)
            {
                StopCoroutine(_rotationCoroutine);
                _rotationCoroutine = null;
            }
        }

        private IEnumerator RotationLoop()
        {
            float angle = 0f;
            while (true)
            {
                angle += _rotationSpeed * GetDeltaTime();
                if (angle > 360f) angle -= 360f;
                _rotatingTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
                yield return GetAnimationYield();
            }
        }

        // ── Glow Pulse ────────────────────────────────────────────────

        private void StartGlowPulse()
        {
            StopGlowPulse();
            if (_glowCanvasGroup != null)
                _glowPulseCoroutine = StartCoroutine(GlowPulseLoop());
        }

        private void StopGlowPulse()
        {
            if (_glowPulseCoroutine != null)
            {
                StopCoroutine(_glowPulseCoroutine);
                _glowPulseCoroutine = null;
            }
        }

        private IEnumerator GlowPulseLoop()
        {
            while (true)
            {
                // Pulse down
                yield return StartCoroutine(LerpCanvasGroupAlpha(_glowCanvasGroup, _glowPulseMax, _glowPulseMin, _glowPulseDuration * 0.5f));
                // Pulse up
                yield return StartCoroutine(LerpCanvasGroupAlpha(_glowCanvasGroup, _glowPulseMin, _glowPulseMax, _glowPulseDuration * 0.5f));
            }
        }

        private IEnumerator LerpCanvasGroupAlpha(CanvasGroup cg, float from, float to, float duration)
        {
            if (cg == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / duration);
                cg.alpha = Mathf.Lerp(from, to, EaseInOutQuad(t));
                yield return GetAnimationYield();
            }

            cg.alpha = to;
        }

        // ── Helpers ────────────────────────────────────────────────────

        private void SetCounterText(long value)
        {
            if (_rewardCounterText != null)
                _rewardCounterText.text = FormatNumberAbbreviated(value);
        }

        private static string FormatNumberAbbreviated(long value)
        {
            if (value >= 1_000_000)
            {
                double m = value / 1_000_000.0;
                return m % 1 == 0 ? $"{(long)m}M" : $"{m:0.#}M";
            }

            if (value >= 1_000)
            {
                double k = value / 1_000.0;
                return k % 1 == 0 ? $"{(long)k}K" : $"{k:0.#}K";
            }

            return value.ToString("N0");
        }

        private void SetCanvasGroupAlpha(CanvasGroup cg, float alpha)
        {
            if (cg != null) cg.alpha = alpha;
        }

        private float GetDeltaTime()
        {
            return _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        private object GetAnimationYield()
        {
            if (_isWebGL)
                return _waitForEndOfFrame;
            return null;
        }

        private object WaitSeconds(float seconds)
        {
            if (_useUnscaledTime)
                return new WaitForSecondsRealtime(seconds);
            return new WaitForSeconds(seconds);
        }

        // ── Easing Functions ───────────────────────────────────────────

        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float tm1 = t - 1f;
            return 1f + c3 * tm1 * tm1 * tm1 + c1 * tm1 * tm1;
        }

        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        // ── Event Handlers ─────────────────────────────────────────────

        private void HandleBackClick()
        {
            OnBackClicked?.Invoke();
        }

        private void HandleClaimClick()
        {
            OnClaimClicked?.Invoke();
        }

        // ── Skinning ──────────────────────────────────────────────────

        private void OnSkinChanged(MainMenuSkinDefinition skin)
        {
            if (_themeManager != null)
                _themeManager.ApplySkin(skin, _skinBindings, this);
        }
    }
}

