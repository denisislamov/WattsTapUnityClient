namespace WattsTap.Core.Telegram
{
    /// <summary>
    /// Haptic impact styles for physical feedback
    /// Based on Telegram WebApp HapticFeedback API
    /// https://core.telegram.org/bots/webapps#hapticfeedback
    /// </summary>
    public enum HapticImpactStyle
    {
        /// <summary>Light impact - indicates a collision between small or lightweight UI objects</summary>
        Light,
        /// <summary>Medium impact - indicates a collision between medium-sized or medium-weight UI objects</summary>
        Medium,
        /// <summary>Heavy impact - indicates a collision between large or heavyweight UI objects</summary>
        Heavy,
        /// <summary>Rigid impact - indicates a collision between hard or inflexible UI objects</summary>
        Rigid,
        /// <summary>Soft impact - indicates a collision between soft or flexible UI objects</summary>
        Soft
    }

    /// <summary>
    /// Notification types for haptic feedback
    /// </summary>
    public enum HapticNotificationType
    {
        /// <summary>Indicates that a task or action has completed successfully</summary>
        Success,
        /// <summary>Indicates that a task or action has produced a warning</summary>
        Warning,
        /// <summary>Indicates that a task or action has failed</summary>
        Error
    }

    /// <summary>
    /// Interface for Telegram haptic feedback service
    /// </summary>
    public interface IHapticFeedbackService : IService
    {
        /// <summary>
        /// Whether haptic feedback is globally enabled
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Triggers an impact haptic feedback
        /// </summary>
        /// <param name="style">The impact style</param>
        void Impact(HapticImpactStyle style);

        /// <summary>
        /// Triggers a notification haptic feedback
        /// </summary>
        /// <param name="type">The notification type</param>
        void Notification(HapticNotificationType type);

        /// <summary>
        /// Triggers a selection changed haptic feedback
        /// Used when the user changes a selection
        /// </summary>
        void SelectionChanged();

        /// <summary>
        /// Triggers haptic feedback for button press
        /// </summary>
        void ButtonPressed();

        /// <summary>
        /// Triggers haptic feedback for tap (coin earning)
        /// </summary>
        void TapPerformed();

        /// <summary>
        /// Triggers haptic feedback for level up
        /// </summary>
        void LevelUp();
    }
}

