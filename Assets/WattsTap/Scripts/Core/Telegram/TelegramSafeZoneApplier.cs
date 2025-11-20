using System;
using UnityEngine;

namespace WattsTap.Core.React
{
    public class TelegramSafeZoneApplier : MonoBehaviour
    {
        [Serializable]
        public class AdditionalOffsets
        {
            public float Top;
            public float Bottom;
            public float Left;
            public float Right;
        }

        [System.Flags]
        public enum SafeZoneSide
        {
            Top = 1 << 0,
            Bottom = 1 << 1,
            Left = 1 << 2,
            Right = 1 << 3
        }

        public enum ApplyMode
        {
            Padding,        // Adds padding to RectTransform (changes offsetMin/offsetMax)
            SizeIncrease,   // Increases size of RectTransform
            Position        // Moves position of RectTransform
        }

        [Header("Safe Zone Settings")]
        [SerializeField] private SafeZoneSide _sides = SafeZoneSide.Top;
        [SerializeField] private ApplyMode _applyMode = ApplyMode.Padding;
        [Header("Additional Mobile Client Offsets (pixels)")]
        [SerializeField] private AdditionalOffsets _mobileClientAdditionalOffsets = new AdditionalOffsets();

        [Header("Target Components")]
        [SerializeField] private RectTransform _targetRectTransform;

        private Vector2 _originalOffsetMin;
        private Vector2 _originalOffsetMax;
        private Vector2 _originalSizeDelta;
        private Vector2 _originalAnchoredPosition;
        
        private ITelegramService _telegramService;
        
        private void Awake()
        {
            _telegramService = ServiceLocator.Get<ITelegramService>();
            
            if (_targetRectTransform == null)
            {
                _targetRectTransform = GetComponent<RectTransform>();
            }

            if (_targetRectTransform != null)
            {
                // Store original values
                _originalOffsetMin = _targetRectTransform.offsetMin;
                _originalOffsetMax = _targetRectTransform.offsetMax;
                _originalSizeDelta = _targetRectTransform.sizeDelta;
                _originalAnchoredPosition = _targetRectTransform.anchoredPosition;
            }
            
            _telegramService.OnReceivedSafeAreaInsets += ApplySafeZone;
            _telegramService.OnReceivedAppEnvironment += HandleEnvironmentChanged;
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (_telegramService.DebugSafeAreaInsets)
            {
                ApplySafeZone(_telegramService.SafeAreaInsets);
            }
#endif
        }

        private void OnDestroy()
        {
            if (_telegramService == null)
            {
                return;
            }
            
            _telegramService.OnReceivedSafeAreaInsets -= ApplySafeZone;
            _telegramService.OnReceivedAppEnvironment -= HandleEnvironmentChanged;
        }

        private void ApplySafeZone(TelegramService.SafeArea insets)
        {
            if (_targetRectTransform == null)
            {
                Debug.LogWarning("[TelegramSafeZone] Target RectTransform is not assigned!");
                return;
            }
            
            if (insets == null)
            {
                Debug.LogWarning("[TelegramSafeZone] Safe area insets not available!");
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[TelegramSafeZone] Canvas not found!");
                return;
            }

            // Convert pixels to canvas units
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            float canvasHeight = canvasRect.rect.height;
            float canvasWidth = canvasRect.rect.width;
            float pixelHeight = canvas.pixelRect.height;
            float pixelWidth = canvas.pixelRect.width;

            // Combine safeAreaInset.top with contentSafeAreaInset.top for proper Android support
            float combinedTopInset = insets.Top + insets.ContentSafeAreaTop;
            
            float topOffset = (_sides & SafeZoneSide.Top) != 0 ? (combinedTopInset / pixelHeight) * canvasHeight : 0;
            float bottomOffset = (_sides & SafeZoneSide.Bottom) != 0 ? (insets.Bottom / pixelHeight) * canvasHeight : 0;
            float leftOffset = (_sides & SafeZoneSide.Left) != 0 ? (insets.Left / pixelWidth) * canvasWidth : 0;
            float rightOffset = (_sides & SafeZoneSide.Right) != 0 ? (insets.Right / pixelWidth) * canvasWidth : 0;

            if (IsMobileClient())
            {
                if ((_sides & SafeZoneSide.Top) != 0)
                {
                    topOffset += (_mobileClientAdditionalOffsets.Top / pixelHeight) * canvasHeight;
                }
                if ((_sides & SafeZoneSide.Bottom) != 0)
                {
                    bottomOffset += (_mobileClientAdditionalOffsets.Bottom / pixelHeight) * canvasHeight;
                }
                if ((_sides & SafeZoneSide.Left) != 0)
                {
                    leftOffset += (_mobileClientAdditionalOffsets.Left / pixelWidth) * canvasWidth;
                }
                if ((_sides & SafeZoneSide.Right) != 0)
                {
                    rightOffset += (_mobileClientAdditionalOffsets.Right / pixelWidth) * canvasWidth;
                }
            }

            Debug.LogFormat("[TelegramSafeZone] Safe Area Insets (pixels) - Top: {0}, Bottom: {1}, Left: {2}, Right: {3}, ContentSafeAreaTop: {4}, Combined Top: {5}", 
                insets.Top, insets.Bottom, insets.Left, insets.Right, insets.ContentSafeAreaTop, combinedTopInset);
            switch (_applyMode)
            {
                case ApplyMode.Padding:
                    ApplyPadding(leftOffset, rightOffset, bottomOffset, topOffset);
                    break;
                case ApplyMode.SizeIncrease:
                    ApplySizeIncrease(leftOffset, rightOffset, bottomOffset, topOffset);
                    break;
                case ApplyMode.Position:
                    ApplyPosition(leftOffset, rightOffset, bottomOffset, topOffset);
                    break;
            }

            Debug.Log($"[TelegramSafeZone] Applied safe zone: Top={topOffset:F2}, Bottom={bottomOffset:F2}, Left={leftOffset:F2}, Right={rightOffset:F2}");
        }

        private bool IsMobileClient()
        {
            return _telegramService != null &&
                   _telegramService.AppEnvironment == TelegramService.TelegramAppEnvironment.MobileClient;
        }

        private void HandleEnvironmentChanged(TelegramService.TelegramAppEnvironment environment)
        {
            if (_telegramService?.SafeAreaInsets != null)
            {
                ApplySafeZone(_telegramService.SafeAreaInsets);
            }
        }
        
        private void ApplyPadding(float left, float right, float bottom, float top)
        {
            _targetRectTransform.offsetMin = _originalOffsetMin + new Vector2(left, bottom);
            _targetRectTransform.offsetMax = _originalOffsetMax - new Vector2(right, top);
        }

        private void ApplySizeIncrease(float left, float right, float bottom, float top)
        {
            Vector2 sizeIncrease = new Vector2(left + right, bottom + top);
            _targetRectTransform.sizeDelta = _originalSizeDelta + sizeIncrease;
        }

        private void ApplyPosition(float left, float right, float bottom, float top)
        {
            Vector2 positionOffset = new Vector2(
                (left - right) * 0.5f,
                (bottom - top) * 0.5f
            );
            _targetRectTransform.anchoredPosition = _originalAnchoredPosition + positionOffset;
        }

        /// <summary>
        /// Resets to original values
        /// </summary>
        public void ResetToOriginal()
        {
            if (_targetRectTransform == null) return;

            _targetRectTransform.offsetMin = _originalOffsetMin;
            _targetRectTransform.offsetMax = _originalOffsetMax;
            _targetRectTransform.sizeDelta = _originalSizeDelta;
            _targetRectTransform.anchoredPosition = _originalAnchoredPosition;
        }
    }
}