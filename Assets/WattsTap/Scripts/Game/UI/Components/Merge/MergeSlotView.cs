using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WattsTap.Core.Inventory;

namespace WattsTap.Game.UI.Components.Inventory
{
    /// <summary>
    /// UI component for a merge selection slot.
    /// Displays a visual duplicate of the selected item.
    /// Clicking a filled slot removes the item from selection.
    /// </summary>
    public class MergeSlotView : MonoBehaviour
    {
        [Header("Display Elements")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private GameObject _filledState;
        [SerializeField] private Button _slotButton;

        [Header("Rarity Colors")]
        [SerializeField] private RarityColorMapping[] _rarityColors;

        private InventoryItem _item;

        /// <summary>
        /// Fired when a filled slot is clicked (to remove the item).
        /// </summary>
        public event Action<MergeSlotView> OnSlotClicked;

        public InventoryItem Item => _item;
        public bool HasItem => _item != null;

        private void Awake()
        {
            if (_slotButton != null)
            {
                _slotButton.onClick.AddListener(HandleSlotClick);
            }
        }

        private void OnDestroy()
        {
            if (_slotButton != null)
            {
                _slotButton.onClick.RemoveListener(HandleSlotClick);
            }
        }

        /// <summary>
        /// Set the item displayed in this slot.
        /// </summary>
        public void SetItem(InventoryItem item)
        {
            _item = item;

            if (item?.Data == null)
            {
                ShowEmptyState();
                return;
            }

            ShowFilledState(item);
        }

        /// <summary>
        /// Clear the slot.
        /// </summary>
        public void ClearSlot()
        {
            _item = null;
            ShowEmptyState();
        }

        private void ShowEmptyState()
        {
            if (_emptyState != null)
                _emptyState.SetActive(true);

            if (_filledState != null)
                _filledState.SetActive(false);

            if (_iconImage != null)
                _iconImage.enabled = false;

            if (_levelText != null)
                _levelText.gameObject.SetActive(false);

            if (_backgroundImage != null)
                _backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        private void ShowFilledState(InventoryItem item)
        {
            var data = item.Data;

            if (_emptyState != null)
                _emptyState.SetActive(false);

            if (_filledState != null)
                _filledState.SetActive(true);

            if (_iconImage != null)
            {
                _iconImage.sprite = data.Icon;
                _iconImage.enabled = data.Icon != null;
            }

            if (_levelText != null)
            {
                _levelText.text = $"Lv.{data.RequiredLevel}";
                _levelText.gameObject.SetActive(true);
            }

            SetRarityBackground(data.Rarity);
        }

        private void SetRarityBackground(ItemRarity rarity)
        {
            if (_backgroundImage == null || _rarityColors == null) return;

            foreach (var mapping in _rarityColors)
            {
                if (mapping.Rarity == rarity)
                {
                    _backgroundImage.color = mapping.Color;
                    return;
                }
            }

            _backgroundImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        private void HandleSlotClick()
        {
            if (_item != null)
            {
                OnSlotClicked?.Invoke(this);
            }
        }
    }
}

