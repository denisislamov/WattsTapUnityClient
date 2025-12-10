using System.Runtime.InteropServices;
using UnityEngine;

namespace WattsTap.Core.Telegram
{
    /// <summary>
    /// Telegram WebApp Haptic Feedback Service implementation
    /// Based on https://core.telegram.org/bots/webapps#hapticfeedback
    /// </summary>
    public class HapticFeedbackService : MonoBehaviour, IHapticFeedbackService
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void TelegramHapticImpact(string style);

        [DllImport("__Internal")]
        private static extern void TelegramHapticNotification(string type);

        [DllImport("__Internal")]
        private static extern void TelegramHapticSelectionChanged();
#endif

        public int InitializationOrder => 50;
        public bool IsInitialized { get; private set; }
        
        public bool IsEnabled { get; set; } = true;

        // Settings for different haptic scenarios
        public HapticImpactStyle ButtonPressStyle { get; set; } = HapticImpactStyle.Light;
        public HapticImpactStyle TapStyle { get; set; } = HapticImpactStyle.Light;
        public HapticNotificationType LevelUpNotificationType { get; set; } = HapticNotificationType.Success;

        // Enable/disable individual haptic events
        public bool EnableButtonHaptic { get; set; } = true;
        public bool EnableTapHaptic { get; set; } = true;
        public bool EnableLevelUpHaptic { get; set; } = true;

        public void Initialize()
        {
            if (IsInitialized) return;
            
            IsInitialized = true;
            Debug.Log("[HapticFeedbackService] Initialized");
        }

        public void Shutdown()
        {
            IsInitialized = false;
        }

        public void Impact(HapticImpactStyle style)
        {
            if (!IsEnabled) return;

#if UNITY_WEBGL && !UNITY_EDITOR
            string styleString = style switch
            {
                HapticImpactStyle.Light => "light",
                HapticImpactStyle.Medium => "medium",
                HapticImpactStyle.Heavy => "heavy",
                HapticImpactStyle.Rigid => "rigid",
                HapticImpactStyle.Soft => "soft",
                _ => "light"
            };
            
            TelegramHapticImpact(styleString);
#else
            Debug.Log($"[HapticFeedbackService] Impact: {style}");
#endif
        }

        public void Notification(HapticNotificationType type)
        {
            if (!IsEnabled) return;

#if UNITY_WEBGL && !UNITY_EDITOR
            string typeString = type switch
            {
                HapticNotificationType.Success => "success",
                HapticNotificationType.Warning => "warning",
                HapticNotificationType.Error => "error",
                _ => "success"
            };
            
            TelegramHapticNotification(typeString);
#else
            Debug.Log($"[HapticFeedbackService] Notification: {type}");
#endif
        }

        public void SelectionChanged()
        {
            if (!IsEnabled) return;

#if UNITY_WEBGL && !UNITY_EDITOR
            TelegramHapticSelectionChanged();
#else
            Debug.Log("[HapticFeedbackService] SelectionChanged");
#endif
        }

        public void ButtonPressed()
        {
            if (!EnableButtonHaptic) return;
            Impact(ButtonPressStyle);
        }

        public void TapPerformed()
        {
            if (!EnableTapHaptic) return;
            Impact(TapStyle);
        }

        public void LevelUp()
        {
            if (!EnableLevelUpHaptic) return;
            Notification(LevelUpNotificationType);
        }
    }
}

