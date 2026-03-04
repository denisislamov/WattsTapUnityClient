using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WattsTap.Core;
using WattsTap.Core.Inventory;
using WattsTap.Core.UI;
using WattsTap.Game.UI.Components.Inventory;

namespace WattsTap.Game.UI
{
    /// <summary>
    /// View for the Merge Animation screen.
    /// Displays the merge slots with selected items and plays the full merge animation sequence.
    /// After the animation completes, a tap on the tap zone closes the screen.
    /// </summary>
    public class MergeScreenUIViewAnimation : UIBaseView<MergeScreenUIPresenterAnimation>
    {
        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;

        private MainMenuThemeManager _themeManager;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Merge Slots Display")]
        [SerializeField] private MergeSlotView _mergeSlot1;
        [SerializeField] private MergeSlotView _mergeSlot2;
        [SerializeField] private MergeSlotView _mergeSlot3;

        [Header("Result Display")]
        [SerializeField] private Image _resultIconImage;
        [SerializeField] private Image _resultBackgroundImage;
        [SerializeField] private TMP_Text _resultNameText;
        [SerializeField] private TMP_Text _resultLevelText;
        [SerializeField] private TMP_Text _resultDescriptionText;
        [SerializeField] private GameObject _resultContainer;
        [SerializeField] private RarityColorMapping[] _resultRarityColors;

        [Header("Animation Controls")]
        [SerializeField] private Button _confirmMergeButton;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private Button _tapZoneButton;

        [Header("Merge Animation — Merge Machine")]
        [SerializeField] private RectTransform _mergeFrameTransform;
        [SerializeField] private Image _mainSlotBackground;
        [SerializeField] private Image _mainSlotItemIcon;
        [SerializeField] private CanvasGroup _mainSlotCanvasGroup;

        [Header("Merge Animation — Small Cards (Source Items)")]
        [SerializeField] private RectTransform _smallCardLeftTransform;
        [SerializeField] private CanvasGroup _smallCardLeftCanvasGroup;
        [SerializeField] private Image _smallCardLeftIcon;
        [SerializeField] private RectTransform _smallCardRightTransform;
        [SerializeField] private CanvasGroup _smallCardRightCanvasGroup;
        [SerializeField] private Image _smallCardRightIcon;

        [Header("Merge Animation — New Item Card")]
        [SerializeField] private RectTransform _newItemCardTransform;
        [SerializeField] private CanvasGroup _newItemCardCanvasGroup;
        [SerializeField] private Image _newItemCardBackground;
        [SerializeField] private Image _newItemCardIcon;
        [SerializeField] private TMP_Text _newItemLevelBadge;

        [Header("Merge Animation — VFX")]
        [SerializeField] private CanvasGroup _flashOverlayCanvasGroup;
        [SerializeField] private CanvasGroup _glowBorderCanvasGroup;
        [SerializeField] private CanvasGroup _lightRaysCanvasGroup;
        [SerializeField] private CanvasGroup _topTriangleVFXCanvasGroup;
        [SerializeField] private RectTransform _shineEffectTransform;
        [SerializeField] private CanvasGroup _shineEffectCanvasGroup;

        [Header("Merge Animation — Text")]
        [SerializeField] private CanvasGroup _successTextCanvasGroup;
        [SerializeField] private CanvasGroup _itemNameLabelCanvasGroup;
        [SerializeField] private TMP_Text _itemNameLabel;

        [Header("Merge Animation — Stats Panel")]
        [SerializeField] private CanvasGroup _statLine1CanvasGroup;
        [SerializeField] private RectTransform _statLine1Transform;
        [SerializeField] private CanvasGroup _statLine2CanvasGroup;
        [SerializeField] private RectTransform _statLine2Transform;
        [SerializeField] private CanvasGroup _statLine3CanvasGroup;
        [SerializeField] private RectTransform _statLine3Transform;

        [Header("Merge Animation — Phase 2 Settings (Pull-In)")]
        [SerializeField] private float _pullInDuration = 0.5f;
        [SerializeField] private float _pullInRotationAngle = 15f;

        [Header("Merge Animation — Phase 3 Settings (Flash)")]
        [SerializeField] private float _flashDuration = 0.12f;

        [Header("Merge Animation — Phase 4 Settings (Reveal)")]
        [SerializeField] private float _revealDuration = 0.33f;
        [SerializeField] private float _revealOvershootScale = 1.2f;
        [SerializeField] private float _machineOvershootScale = 1.15f;
        [SerializeField] private float _lightRaysFadeOutDuration = 0.25f;

        [Header("Merge Animation — Phase 5 Settings (Stats)")]
        [SerializeField] private float _statsSlideInDuration = 0.2f;
        [SerializeField] private float _statsStaggerDelay = 0.2f;
        [SerializeField] private float _statsSlideOffsetY = 30f;

        [Header("Merge Animation — Phase 6 Settings (Shine)")]
        [SerializeField] private float _shineSweepDuration = 0.4f;
        [SerializeField] private float _shineSweepDelay = 0.3f;
        [SerializeField] private float _shineStartX = -200f;
        [SerializeField] private float _shineEndX = 200f;

        [Header("WebGL/Mobile Optimization")]
        [SerializeField] private bool _useUnscaledTime = true;

        public event Action OnBackClicked;
        public event Action OnConfirmMergeClicked;
        public event Action OnAnimationComplete;
        public event Action OnTapZoneClicked;

        public Button BackButton => _backButton;
        public Button ConfirmMergeButton => _confirmMergeButton;
        public Button TapZoneButton => _tapZoneButton;

        private MergeSlotView[] MergeSlots => new[] { _mergeSlot1, _mergeSlot2, _mergeSlot3 };

        private Coroutine _mergeAnimationCoroutine;
        private bool _animationPlaying;
        private bool _animationFinished;

        private Vector2 _smallCardLeftOrigin;
        private Vector2 _smallCardRightOrigin;
        private Vector3 _mergeFrameOriginalScale;
        private Vector3 _newItemCardOriginalScale;
        private float _statLine1OriginalY;
        private float _statLine2OriginalY;
        private float _statLine3OriginalY;
        private float _shineOriginalX;

        private bool _isWebGL;
        private WaitForEndOfFrame _waitForEndOfFrame;

        private void Awake()
        {
            _isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
            _waitForEndOfFrame = new WaitForEndOfFrame();
        }

        /// <summary>
        /// Display the items that are placed in merge slots.
        /// </summary>
        public void DisplayMergeItems(IReadOnlyList<InventoryItem> items)
        {
            var slots = MergeSlots;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;

                if (i < items.Count && items[i] != null)
                    slots[i].SetItem(items[i]);
                else
                    slots[i].ClearSlot();
            }

            if (_resultContainer != null)
                _resultContainer.SetActive(false);

            if (_statusText != null)
                _statusText.text = "Ready to merge!";
        }

        /// <summary>
        /// Display the expected merge result from computed MergeResultInfo.
        /// </summary>
        public void DisplayMergeResult(MergeResultInfo result)
        {
            if (result == null)
            {
                if (_resultContainer != null)
                    _resultContainer.SetActive(false);
                return;
            }

            if (_resultContainer != null)
                _resultContainer.SetActive(true);

            if (_resultIconImage != null)
            {
                _resultIconImage.sprite = result.Icon;
                _resultIconImage.enabled = result.Icon != null;
            }

            if (_resultNameText != null)
                _resultNameText.text = result.DisplayName;

            if (_resultLevelText != null)
                _resultLevelText.text = $"Lv.{result.Level}";

            if (_resultDescriptionText != null)
                _resultDescriptionText.text = $"{result.ResultRarity} {result.ItemType}";

            if (_resultBackgroundImage != null && _resultRarityColors != null)
            {
                bool found = false;
                foreach (var mapping in _resultRarityColors)
                {
                    if (mapping.Rarity == result.ResultRarity)
                    {
                        _resultBackgroundImage.color = mapping.Color;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    _resultBackgroundImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
            }
        }

        /// <summary>
        /// Display the result of the merge operation.
        /// </summary>
        public void ShowMergeResult(InventoryItem resultItem)
        {
            if (_resultContainer != null)
                _resultContainer.SetActive(true);

            if (resultItem?.Data != null)
            {
                if (_resultIconImage != null)
                {
                    _resultIconImage.sprite = resultItem.Data.Icon;
                    _resultIconImage.enabled = resultItem.Data.Icon != null;
                }

                if (_resultNameText != null)
                    _resultNameText.text = resultItem.Data.DisplayName;

                if (_resultDescriptionText != null)
                    _resultDescriptionText.text = resultItem.Data.Description;
            }

            if (_statusText != null)
                _statusText.text = "Merge complete!";
        }

        public void SetStatusText(string text)
        {
            if (_statusText != null)
                _statusText.text = text;
        }

        public void SetConfirmButtonInteractable(bool interactable)
        {
            if (_confirmMergeButton != null)
                _confirmMergeButton.interactable = interactable;
        }

        public void SetTapZoneActive(bool active)
        {
            if (_tapZoneButton != null)
                _tapZoneButton.gameObject.SetActive(active);
        }

        /// <summary>
        /// Whether the merge animation has finished playing and the screen is waiting for a dismiss tap.
        /// </summary>
        public bool IsAnimationFinished => _animationFinished;

        #region Merge Animation

        /// <summary>
        /// Plays the full merge animation sequence (6 phases from MERGE_ANIMATION_SPEC).
        /// </summary>
        public void PlayMergeAnimation()
        {
            if (_animationPlaying) return;

            StopMergeAnimation();
            SaveAnimationOrigins();
            InitializeMergeAnimationState();
            _animationPlaying = true;
            _animationFinished = false;
            _mergeAnimationCoroutine = StartCoroutine(MergeAnimationSequence());
        }

        public void StopMergeAnimation()
        {
            if (_mergeAnimationCoroutine != null)
            {
                StopCoroutine(_mergeAnimationCoroutine);
                _mergeAnimationCoroutine = null;
            }
            _animationPlaying = false;
        }

        private void SaveAnimationOrigins()
        {
            if (_smallCardLeftTransform != null)
                _smallCardLeftOrigin = _smallCardLeftTransform.anchoredPosition;
            if (_smallCardRightTransform != null)
                _smallCardRightOrigin = _smallCardRightTransform.anchoredPosition;
            if (_mergeFrameTransform != null)
                _mergeFrameOriginalScale = _mergeFrameTransform.localScale;
            if (_newItemCardTransform != null)
                _newItemCardOriginalScale = _newItemCardTransform.localScale;
            if (_statLine1Transform != null)
                _statLine1OriginalY = _statLine1Transform.anchoredPosition.y;
            if (_statLine2Transform != null)
                _statLine2OriginalY = _statLine2Transform.anchoredPosition.y;
            if (_statLine3Transform != null)
                _statLine3OriginalY = _statLine3Transform.anchoredPosition.y;
            if (_shineEffectTransform != null)
                _shineOriginalX = _shineEffectTransform.anchoredPosition.x;
        }

        private void InitializeMergeAnimationState()
        {
            SetCanvasGroupAlpha(_smallCardLeftCanvasGroup, 1f);
            SetCanvasGroupAlpha(_smallCardRightCanvasGroup, 1f);
            SetCanvasGroupAlpha(_flashOverlayCanvasGroup, 0f);
            SetCanvasGroupAlpha(_glowBorderCanvasGroup, 0f);
            SetCanvasGroupAlpha(_lightRaysCanvasGroup, 0f);
            SetCanvasGroupAlpha(_topTriangleVFXCanvasGroup, 0f);
            SetCanvasGroupAlpha(_shineEffectCanvasGroup, 0f);
            SetCanvasGroupAlpha(_successTextCanvasGroup, 0f);
            SetCanvasGroupAlpha(_itemNameLabelCanvasGroup, 0f);
            SetCanvasGroupAlpha(_newItemCardCanvasGroup, 0f);
            SetCanvasGroupAlpha(_statLine1CanvasGroup, 0f);
            SetCanvasGroupAlpha(_statLine2CanvasGroup, 0f);
            SetCanvasGroupAlpha(_statLine3CanvasGroup, 0f);

            if (_newItemCardTransform != null)
                _newItemCardTransform.localScale = Vector3.zero;

            if (_smallCardLeftTransform != null)
            {
                _smallCardLeftTransform.anchoredPosition = _smallCardLeftOrigin;
                _smallCardLeftTransform.localRotation = Quaternion.identity;
                _smallCardLeftTransform.localScale = Vector3.one;
            }

            if (_smallCardRightTransform != null)
            {
                _smallCardRightTransform.anchoredPosition = _smallCardRightOrigin;
                _smallCardRightTransform.localRotation = Quaternion.identity;
                _smallCardRightTransform.localScale = Vector3.one;
            }

            if (_mergeFrameTransform != null)
                _mergeFrameTransform.localScale = _mergeFrameOriginalScale;

            if (_shineEffectTransform != null)
            {
                var pos = _shineEffectTransform.anchoredPosition;
                pos.x = _shineStartX;
                _shineEffectTransform.anchoredPosition = pos;
            }
        }

        private IEnumerator MergeAnimationSequence()
        {
            // PHASE 1: Idle — short hold so the player sees the items (0.2s)
            yield return WaitSeconds(0.2f);

            // PHASE 2: Pull-In — small cards fly to center
            yield return StartCoroutine(Phase2_PullIn());

            // PHASE 3: Flash — white/cyan burst
            yield return StartCoroutine(Phase3_Flash());

            // PHASE 4: Reveal — new item scales in with light rays
            yield return StartCoroutine(Phase4_Reveal());

            // PHASE 5: Stats — lines slide in sequentially
            yield return StartCoroutine(Phase5_StatsSlideIn());

            // PHASE 6: Hold — shine sweep
            yield return StartCoroutine(Phase6_ShineSweep());

            _animationPlaying = false;
            _animationFinished = true;
            OnAnimationComplete?.Invoke();
        }

        #region Phase 2 — Pull-In

        private IEnumerator Phase2_PullIn()
        {
            if (_mergeFrameTransform == null) yield break;

            Vector2 targetCenter = _mergeFrameTransform.anchoredPosition;
            Vector2 leftStart = _smallCardLeftOrigin;
            Vector2 rightStart = _smallCardRightOrigin;
            float elapsed = 0f;

            while (elapsed < _pullInDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _pullInDuration);
                float eased = EaseInBack(t);

                if (_smallCardLeftTransform != null)
                {
                    Vector2 arcPos = ArcLerp(leftStart, targetCenter, eased, 40f);
                    _smallCardLeftTransform.anchoredPosition = arcPos;
                    _smallCardLeftTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.3f, eased);
                    _smallCardLeftTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, _pullInRotationAngle, eased));
                }

                if (_smallCardRightTransform != null)
                {
                    Vector2 arcPos = ArcLerp(rightStart, targetCenter, eased, 40f);
                    _smallCardRightTransform.anchoredPosition = arcPos;
                    _smallCardRightTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.3f, eased);
                    _smallCardRightTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -_pullInRotationAngle, eased));
                }

                yield return GetAnimationYield();
            }

            SetCanvasGroupAlpha(_smallCardLeftCanvasGroup, 0f);
            SetCanvasGroupAlpha(_smallCardRightCanvasGroup, 0f);
        }

        #endregion

        #region Phase 3 — Flash

        private IEnumerator Phase3_Flash()
        {
            SetCanvasGroupAlpha(_successTextCanvasGroup, 1f);
            SetCanvasGroupAlpha(_itemNameLabelCanvasGroup, 1f);

            float elapsed = 0f;
            float halfDuration = _flashDuration * 0.5f;

            while (elapsed < halfDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / halfDuration);

                SetCanvasGroupAlpha(_flashOverlayCanvasGroup, EaseOutQuad(t));
                SetCanvasGroupAlpha(_glowBorderCanvasGroup, EaseOutQuad(t) * 0.8f);
                SetCanvasGroupAlpha(_topTriangleVFXCanvasGroup, EaseOutQuad(t));

                yield return GetAnimationYield();
            }

            SetCanvasGroupAlpha(_flashOverlayCanvasGroup, 1f);
        }

        #endregion

        #region Phase 4 — Reveal

        private IEnumerator Phase4_Reveal()
        {
            SetCanvasGroupAlpha(_newItemCardCanvasGroup, 1f);

            float elapsed = 0f;

            SetCanvasGroupAlpha(_lightRaysCanvasGroup, 1f);

            while (elapsed < _revealDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _revealDuration);
                float easedCard = EaseOutBack(t);
                float easedFlash = EaseOutQuad(t);

                if (_newItemCardTransform != null)
                {
                    float cardScale = easedCard * _revealOvershootScale;
                    if (t > 0.6f)
                    {
                        float settleT = (t - 0.6f) / 0.4f;
                        cardScale = Mathf.Lerp(_revealOvershootScale, 1f, EaseInOutQuad(settleT));
                    }
                    _newItemCardTransform.localScale = _newItemCardOriginalScale * cardScale;
                }

                if (_mergeFrameTransform != null)
                {
                    float machineScale;
                    if (t < 0.45f)
                    {
                        machineScale = Mathf.Lerp(1f, _machineOvershootScale, EaseOutQuad(t / 0.45f));
                    }
                    else
                    {
                        float settleT = (t - 0.45f) / 0.55f;
                        machineScale = Mathf.Lerp(_machineOvershootScale, 1f, EaseInOutQuad(settleT));
                    }
                    _mergeFrameTransform.localScale = _mergeFrameOriginalScale * machineScale;
                }

                SetCanvasGroupAlpha(_flashOverlayCanvasGroup, Mathf.Lerp(1f, 0f, easedFlash));

                yield return GetAnimationYield();
            }

            if (_newItemCardTransform != null)
                _newItemCardTransform.localScale = _newItemCardOriginalScale;
            if (_mergeFrameTransform != null)
                _mergeFrameTransform.localScale = _mergeFrameOriginalScale;
            SetCanvasGroupAlpha(_flashOverlayCanvasGroup, 0f);

            yield return StartCoroutine(FadeOutVFX());
        }

        private IEnumerator FadeOutVFX()
        {
            float elapsed = 0f;
            float startGlow = _glowBorderCanvasGroup != null ? _glowBorderCanvasGroup.alpha : 0f;
            float startTriangle = _topTriangleVFXCanvasGroup != null ? _topTriangleVFXCanvasGroup.alpha : 0f;

            while (elapsed < _lightRaysFadeOutDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _lightRaysFadeOutDuration);

                SetCanvasGroupAlpha(_lightRaysCanvasGroup, Mathf.Lerp(1f, 0f, t));
                SetCanvasGroupAlpha(_glowBorderCanvasGroup, Mathf.Lerp(startGlow, 0f, t));
                SetCanvasGroupAlpha(_topTriangleVFXCanvasGroup, Mathf.Lerp(startTriangle, 0f, t));

                yield return GetAnimationYield();
            }

            SetCanvasGroupAlpha(_lightRaysCanvasGroup, 0f);
            SetCanvasGroupAlpha(_glowBorderCanvasGroup, 0f);
            SetCanvasGroupAlpha(_topTriangleVFXCanvasGroup, 0f);
        }

        #endregion

        #region Phase 5 — Stats Slide-In

        private IEnumerator Phase5_StatsSlideIn()
        {
            yield return StartCoroutine(SlideInStatLine(_statLine1CanvasGroup, _statLine1Transform, _statLine1OriginalY));

            yield return WaitSeconds(_statsStaggerDelay);

            yield return StartCoroutine(SlideInStatLine(_statLine2CanvasGroup, _statLine2Transform, _statLine2OriginalY));

            yield return WaitSeconds(_statsStaggerDelay);

            yield return StartCoroutine(SlideInStatLine(_statLine3CanvasGroup, _statLine3Transform, _statLine3OriginalY));
        }

        private IEnumerator SlideInStatLine(CanvasGroup canvasGroup, RectTransform rectTransform, float targetY)
        {
            if (canvasGroup == null || rectTransform == null) yield break;

            float startY = targetY - _statsSlideOffsetY;
            float elapsed = 0f;

            var pos = rectTransform.anchoredPosition;
            pos.y = startY;
            rectTransform.anchoredPosition = pos;

            while (elapsed < _statsSlideInDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _statsSlideInDuration);
                float eased = EaseOutQuad(t);

                canvasGroup.alpha = eased;
                pos = rectTransform.anchoredPosition;
                pos.y = Mathf.Lerp(startY, targetY, eased);
                rectTransform.anchoredPosition = pos;

                yield return GetAnimationYield();
            }

            canvasGroup.alpha = 1f;
            pos = rectTransform.anchoredPosition;
            pos.y = targetY;
            rectTransform.anchoredPosition = pos;
        }

        #endregion

        #region Phase 6 — Shine Sweep

        private IEnumerator Phase6_ShineSweep()
        {
            if (_shineEffectTransform == null || _shineEffectCanvasGroup == null) yield break;

            yield return WaitSeconds(_shineSweepDelay);

            SetCanvasGroupAlpha(_shineEffectCanvasGroup, 1f);

            float elapsed = 0f;
            while (elapsed < _shineSweepDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / _shineSweepDuration);
                float eased = EaseInOutQuad(t);

                var pos = _shineEffectTransform.anchoredPosition;
                pos.x = Mathf.Lerp(_shineStartX, _shineEndX, eased);
                _shineEffectTransform.anchoredPosition = pos;

                yield return GetAnimationYield();
            }

            SetCanvasGroupAlpha(_shineEffectCanvasGroup, 0f);
        }

        #endregion

        #endregion

        #region Animation Utilities

        private Vector2 ArcLerp(Vector2 from, Vector2 to, float t, float arcHeight)
        {
            Vector2 linear = Vector2.Lerp(from, to, t);
            float arc = 4f * arcHeight * t * (1f - t);
            linear.y += arc;
            return linear;
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

        private float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float tm1 = t - 1f;
            return 1f + c3 * tm1 * tm1 * tm1 + c1 * tm1 * tm1;
        }

        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        #endregion

        #region Skinning

        private void OnEnable()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(HandleBackClick);

            if (_confirmMergeButton != null)
                _confirmMergeButton.onClick.AddListener(HandleConfirmMergeClick);

            if (_tapZoneButton != null)
                _tapZoneButton.onClick.AddListener(HandleTapZoneClick);

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

            if (_confirmMergeButton != null)
                _confirmMergeButton.onClick.RemoveListener(HandleConfirmMergeClick);

            if (_tapZoneButton != null)
                _tapZoneButton.onClick.RemoveListener(HandleTapZoneClick);

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

        #endregion

        private void HandleBackClick()
        {
            OnBackClicked?.Invoke();
        }

        private void HandleConfirmMergeClick()
        {
            OnConfirmMergeClicked?.Invoke();
        }

        private void HandleTapZoneClick()
        {
            OnTapZoneClicked?.Invoke();
        }

        /// <summary>
        /// Notify that the animation has completed (call from animation event or coroutine).
        /// </summary>
        public void NotifyAnimationComplete()
        {
            OnAnimationComplete?.Invoke();
        }
    }
}
