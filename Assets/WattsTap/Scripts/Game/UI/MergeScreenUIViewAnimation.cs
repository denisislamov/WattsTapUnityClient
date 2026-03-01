using System;
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
    /// Displays the merge slots with selected items and plays merge animation.
    /// Opened when the user presses the Merge button on MergeScreenUIView.
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
        [SerializeField] private TMP_Text _resultNameText;
        [SerializeField] private TMP_Text _resultDescriptionText;
        [SerializeField] private GameObject _resultContainer;

        [Header("Animation")]
        [SerializeField] private Button _confirmMergeButton;
        [SerializeField] private TMP_Text _statusText;

        /// <summary>Fired when back button is clicked.</summary>
        public event Action OnBackClicked;

        /// <summary>Fired when confirm merge button is clicked.</summary>
        public event Action OnConfirmMergeClicked;

        /// <summary>Fired when the merge animation completes.</summary>
        public event Action OnAnimationComplete;

        public Button BackButton => _backButton;
        public Button ConfirmMergeButton => _confirmMergeButton;

        private MergeSlotView[] MergeSlots => new[] { _mergeSlot1, _mergeSlot2, _mergeSlot3 };

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

            // Hide result initially
            if (_resultContainer != null)
                _resultContainer.SetActive(false);

            if (_statusText != null)
                _statusText.text = "Ready to merge!";
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

        /// <summary>
        /// Update status text during merge process.
        /// </summary>
        public void SetStatusText(string text)
        {
            if (_statusText != null)
                _statusText.text = text;
        }

        /// <summary>
        /// Set the confirm merge button interactable state.
        /// </summary>
        public void SetConfirmButtonInteractable(bool interactable)
        {
            if (_confirmMergeButton != null)
                _confirmMergeButton.interactable = interactable;
        }

        #region Skinning

        private void OnEnable()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(HandleBackClick);

            if (_confirmMergeButton != null)
                _confirmMergeButton.onClick.AddListener(HandleConfirmMergeClick);

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

        /// <summary>
        /// Notify that the animation has completed (call from animation event or coroutine).
        /// </summary>
        public void NotifyAnimationComplete()
        {
            OnAnimationComplete?.Invoke();
        }
    }
}


