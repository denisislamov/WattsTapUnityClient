using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.Telegram;

namespace WattsTap.Game.UI.Components
{
    /// <summary>
    /// Utility class to add haptic feedback to buttons programmatically
    /// </summary>
    public static class HapticButtonExtensions
    {
        /// <summary>
        /// Adds haptic feedback listener to a button
        /// </summary>
        public static void AddHapticFeedback(this Button button)
        {
            if (button == null) return;
            
            button.onClick.AddListener(() =>
            {
                if (ServiceLocator.TryGet<IHapticFeedbackService>(out var hapticService))
                {
                    hapticService.ButtonPressed();
                }
            });
        }

        /// <summary>
        /// Adds haptic feedback listener with custom style to a button
        /// </summary>
        public static void AddHapticFeedback(this Button button, HapticImpactStyle style)
        {
            if (button == null) return;
            
            button.onClick.AddListener(() =>
            {
                if (ServiceLocator.TryGet<IHapticFeedbackService>(out var hapticService))
                {
                    hapticService.Impact(style);
                }
            });
        }

        /// <summary>
        /// Adds HapticButton component to a button if not already present
        /// </summary>
        public static HapticButton EnsureHapticButton(this Button button)
        {
            if (button == null) return null;
            
            var hapticButton = button.GetComponent<HapticButton>();
            if (hapticButton == null)
            {
                hapticButton = button.gameObject.AddComponent<HapticButton>();
            }
            return hapticButton;
        }
    }
}

