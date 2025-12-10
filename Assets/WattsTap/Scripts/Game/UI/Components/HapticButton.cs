using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.Telegram;

namespace WattsTap.Game.UI.Components
{
    /// <summary>
    /// Component that adds haptic feedback to UI buttons
    /// Attach this to any Button to automatically trigger haptic on click
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class HapticButton : MonoBehaviour
    {
        [SerializeField] private bool _useCustomStyle = false;
        [SerializeField] private HapticImpactStyle _customStyle = HapticImpactStyle.Light;

        private Button _button;
        private IHapticFeedbackService _hapticService;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked()
        {
            if (_hapticService == null)
            {
                ServiceLocator.TryGet(out _hapticService);
            }

            if (_hapticService == null) return;

            if (_useCustomStyle)
            {
                _hapticService.Impact(_customStyle);
            }
            else
            {
                _hapticService.ButtonPressed();
            }
        }
    }
}

