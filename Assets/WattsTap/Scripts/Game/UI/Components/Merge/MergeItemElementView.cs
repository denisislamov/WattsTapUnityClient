using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using WattsTap.Core.Inventory;

namespace WattsTap.Game.UI.Components.Inventory
{
    /// <summary>
    /// UI representation of an inventory item in the Merge screen.
    /// Displays item icon, level, type indicator, and rarity-based background.
    /// Double-tap toggles selection (activates/deactivates the outline).
    /// If the item is already selected, double-tap deselects it.
    /// </summary>
    public class MergeItemElementView : MonoBehaviour, IPointerClickHandler
    {
        [Header("Display Elements")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _typeIconImage;
        [SerializeField] private TMP_Text _levelText;
        
        [Header("Type Icons")]
        [SerializeField] private List<ItemTypeIconMapping> _typeIcons = new List<ItemTypeIconMapping>();
        
        [Header("Rarity Colors")]
        [SerializeField] private List<RarityColorMapping> _rarityColors = new List<RarityColorMapping>();
        
        [Header("Selection")]
        [SerializeField] private GameObject _selectionFrame;
        
        private InventoryItem _inventoryItem;
        private bool _isSelected;
        private float _lastClickTime;
        private const float DoubleClickThreshold = 0.3f;
        
        /// <summary>
        /// Fired when the item selection state changes via double-tap.
        /// Parameters: this view, new selected state.
        /// </summary>
        public event Action<MergeItemElementView, bool> OnSelectionChanged;
        
        /// <summary>
        /// Fired on a single tap/click.
        /// </summary>
        public event Action<MergeItemElementView> OnSingleClick;
        
        public InventoryItem InventoryItem => _inventoryItem;
        public bool IsSelected => _isSelected;
        
        /// <summary>
        /// Setup the view with inventory item data.
        /// </summary>
        public void Setup(InventoryItem inventoryItem)
        {
            _inventoryItem = inventoryItem;
            _isSelected = false;
            
            if (inventoryItem?.Data == null)
            {
                gameObject.SetActive(false);
                return;
            }
            
            var data = inventoryItem.Data;
            
            // Set icon
            if (_iconImage != null)
            {
                _iconImage.sprite = data.Icon;
                _iconImage.enabled = data.Icon != null;
            }
            
            // Set level
            if (_levelText != null)
            {
                _levelText.text = $"Lv.{data.RequiredLevel}";
            }
            
            // Set type icon
            SetTypeIcon(data.ItemType);
            
            // Set rarity background color
            SetRarityBackground(data.Rarity);
            
            // Deselect by default
            SetSelected(false, notify: false);
        }
        
        /// <summary>
        /// Set selection visual state.
        /// </summary>
        /// <param name="selected">Whether the item is selected.</param>
        /// <param name="notify">Whether to fire OnSelectionChanged event.</param>
        public void SetSelected(bool selected, bool notify = true)
        {
            _isSelected = selected;
            
            if (_selectionFrame != null)
            {
                _selectionFrame.SetActive(_isSelected);
            }
            
            if (notify)
            {
                OnSelectionChanged?.Invoke(this, _isSelected);
            }
        }
        
        private void SetTypeIcon(ItemType type)
        {
            if (_typeIconImage == null) return;
            
            foreach (var mapping in _typeIcons)
            {
                if (mapping.Type == type)
                {
                    _typeIconImage.sprite = mapping.Icon;
                    _typeIconImage.enabled = mapping.Icon != null;
                    return;
                }
            }
            
            // No mapping found, hide icon
            _typeIconImage.enabled = false;
        }
        
        private void SetRarityBackground(ItemRarity rarity)
        {
            if (_backgroundImage == null) return;
            
            foreach (var mapping in _rarityColors)
            {
                if (mapping.Rarity == rarity)
                {
                    _backgroundImage.color = mapping.Color;
                    return;
                }
            }
            
            // Default color if no mapping found
            _backgroundImage.color = Color.gray;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            float currentTime = Time.unscaledTime;
            
            if (currentTime - _lastClickTime < DoubleClickThreshold)
            {
                // Double click — toggle selection
                SetSelected(!_isSelected);
                _lastClickTime = 0f; // Reset to prevent triple-click
            }
            else
            {
                // Single click
                OnSingleClick?.Invoke(this);
                _lastClickTime = currentTime;
            }
        }
    }
}

