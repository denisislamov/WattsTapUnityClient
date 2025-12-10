using System.ComponentModel;
using WattsTap.Core;
using WattsTap.Core.Telegram;

public partial class SROptions
{
    private const string HapticCategory = "Haptic Feedback";

    #region Global Settings

    private bool _hapticEnabled = true;

    [Category(HapticCategory)]
    [DisplayName("Haptic Enabled")]
    [Sort(0)]
    public bool HapticEnabled
    {
        get => _hapticEnabled;
        set
        {
            _hapticEnabled = value;
            ApplyHapticSettings();
            OnPropertyChanged(nameof(HapticEnabled));
        }
    }

    #endregion

    #region Button Haptic

    private bool _buttonHapticEnabled = true;
    private int _buttonHapticStyle = 0; // Light

    [Category(HapticCategory)]
    [DisplayName("Button Haptic Enabled")]
    [Sort(10)]
    public bool ButtonHapticEnabled
    {
        get => _buttonHapticEnabled;
        set
        {
            _buttonHapticEnabled = value;
            ApplyHapticSettings();
            OnPropertyChanged(nameof(ButtonHapticEnabled));
        }
    }

    [Category(HapticCategory)]
    [DisplayName("Button Style (0=Light, 1=Medium, 2=Heavy, 3=Rigid, 4=Soft)")]
    [NumberRange(0, 4)]
    [Sort(11)]
    public int ButtonHapticStyle
    {
        get => _buttonHapticStyle;
        set
        {
            _buttonHapticStyle = value;
            ApplyHapticSettings();
            OnPropertyChanged(nameof(ButtonHapticStyle));
        }
    }

    #endregion

    #region Tap Haptic

    private bool _tapHapticEnabled = true;
    private int _tapHapticStyle = 0; // Light

    [Category(HapticCategory)]
    [DisplayName("Tap Haptic Enabled")]
    [Sort(20)]
    public bool TapHapticEnabled
    {
        get => _tapHapticEnabled;
        set
        {
            _tapHapticEnabled = value;
            ApplyHapticSettings();
            OnPropertyChanged(nameof(TapHapticEnabled));
        }
    }

    [Category(HapticCategory)]
    [DisplayName("Tap Style (0=Light, 1=Medium, 2=Heavy, 3=Rigid, 4=Soft)")]
    [NumberRange(0, 4)]
    [Sort(21)]
    public int TapHapticStyle
    {
        get => _tapHapticStyle;
        set
        {
            _tapHapticStyle = value;
            ApplyHapticSettings();
            OnPropertyChanged(nameof(TapHapticStyle));
        }
    }

    #endregion

    #region Level Up Haptic

    private bool _levelUpHapticEnabled = true;
    private int _levelUpNotificationType = 0; // Success

    [Category(HapticCategory)]
    [DisplayName("Level Up Haptic Enabled")]
    [Sort(30)]
    public bool LevelUpHapticEnabled
    {
        get => _levelUpHapticEnabled;
        set
        {
            _levelUpHapticEnabled = value;
            ApplyHapticSettings();
            OnPropertyChanged(nameof(LevelUpHapticEnabled));
        }
    }

    [Category(HapticCategory)]
    [DisplayName("Level Up Type (0=Success, 1=Warning, 2=Error)")]
    [NumberRange(0, 2)]
    [Sort(31)]
    public int LevelUpNotificationType
    {
        get => _levelUpNotificationType;
        set
        {
            _levelUpNotificationType = value;
            ApplyHapticSettings();
            OnPropertyChanged(nameof(LevelUpNotificationType));
        }
    }

    #endregion

    #region Test Buttons

    [Category(HapticCategory)]
    [DisplayName("Test Button Haptic")]
    [Sort(100)]
    public void TestButtonHaptic()
    {
        if (ServiceLocator.TryGet<IHapticFeedbackService>(out var hapticService))
        {
            hapticService.ButtonPressed();
        }
    }

    [Category(HapticCategory)]
    [DisplayName("Test Tap Haptic")]
    [Sort(101)]
    public void TestTapHaptic()
    {
        if (ServiceLocator.TryGet<IHapticFeedbackService>(out var hapticService))
        {
            hapticService.TapPerformed();
        }
    }

    [Category(HapticCategory)]
    [DisplayName("Test Level Up Haptic")]
    [Sort(102)]
    public void TestLevelUpHaptic()
    {
        if (ServiceLocator.TryGet<IHapticFeedbackService>(out var hapticService))
        {
            hapticService.LevelUp();
        }
    }

    #endregion

    private void ApplyHapticSettings()
    {
        if (!ServiceLocator.TryGet<IHapticFeedbackService>(out var service))
        {
            return;
        }

        if (service is HapticFeedbackService hapticService)
        {
            hapticService.IsEnabled = _hapticEnabled;
            hapticService.EnableButtonHaptic = _buttonHapticEnabled;
            hapticService.EnableTapHaptic = _tapHapticEnabled;
            hapticService.EnableLevelUpHaptic = _levelUpHapticEnabled;

            hapticService.ButtonPressStyle = (HapticImpactStyle)_buttonHapticStyle;
            hapticService.TapStyle = (HapticImpactStyle)_tapHapticStyle;
            hapticService.LevelUpNotificationType = (HapticNotificationType)_levelUpNotificationType;
        }
    }
}

