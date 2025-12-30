using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    /// <summary>
    /// View component for Bug Report UI
    /// Handles visual elements and user input
    /// </summary>
    public class BugReportUIView : UIBaseView<BugReportUIPresenter>
    {
        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _sendButton;

        [Header("Input")]
        [SerializeField] private TMP_InputField _descriptionInput;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _deviceInfoText;

        [Header("Loading")]
        [SerializeField] private GameObject _loadingIndicator;
        [SerializeField] private CanvasGroup _contentCanvasGroup;

        #region Properties

        public Button CloseButton => _closeButton;
        public Button SendButton => _sendButton;
        public TMP_InputField DescriptionInput => _descriptionInput;

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the send button interactable state
        /// </summary>
        public void SetSendButtonInteractable(bool isInteractable)
        {
            if (_sendButton != null)
            {
                // _sendButton.interactable = isInteractable;
                _sendButton.interactable = true;
            }
        }

        /// <summary>
        /// Shows or hides loading state
        /// </summary>
        public void SetLoading(bool isLoading)
        {
            if (_loadingIndicator != null)
            {
                _loadingIndicator.SetActive(isLoading);
            }

            if (_contentCanvasGroup != null)
            {
                _contentCanvasGroup.interactable = !isLoading;
                _contentCanvasGroup.alpha = isLoading ? 0.5f : 1f;
            }
        }

        /// <summary>
        /// Updates status message text
        /// </summary>
        public void UpdateStatusMessage(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
                _statusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
            }
        }

        /// <summary>
        /// Updates device info preview text
        /// </summary>
        public void UpdateDeviceInfo(string deviceInfo)
        {
            if (_deviceInfoText != null)
            {
                _deviceInfoText.text = deviceInfo;
            }
        }

        /// <summary>
        /// Clears the description input field
        /// </summary>
        public void ClearDescription()
        {
            if (_descriptionInput != null)
            {
                _descriptionInput.text = string.Empty;
            }
        }

        /// <summary>
        /// Gets the current description text
        /// </summary>
        public string GetDescription()
        {
            return _descriptionInput != null ? _descriptionInput.text : string.Empty;
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Initialize default states
            SetLoading(false);
            UpdateStatusMessage(string.Empty);
        }

        #endregion
    }
}

