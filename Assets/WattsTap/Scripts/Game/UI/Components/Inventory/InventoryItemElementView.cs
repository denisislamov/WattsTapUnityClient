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
    /// UI representation of an inventory item.
    /// Displays item icon, level, type indicator, and rarity-based background.
    /// </summary>
    public class InventoryItemElementView : MonoBehaviour, IPointerClickHandler
    {
        [Header("Display Elements")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _smallIconBackground;
        [SerializeField] private Image _typeIconImage;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private GameObject _equippedIndicator;
        
        [Header("Type Icons")]
        [SerializeField] private List<ItemTypeIconMapping> _typeIcons = new List<ItemTypeIconMapping>();
        
        [Header("Rarity Colors")]
        [SerializeField] private List<RarityColorMapping> _rarityColors = new List<RarityColorMapping>();
        
        [Header("Selection")]
        [SerializeField] private GameObject _selectionFrame;
        
        private InventoryItem _inventoryItem;
        private float _lastClickTime;
        private const float DoubleClickThreshold = 0.3f;
        
        public event Action<InventoryItemElementView> OnDoubleClick;
        public event Action<InventoryItemElementView> OnSingleClick;
        
        public InventoryItem InventoryItem => _inventoryItem;
        public bool IsEquipped => _inventoryItem?.IsEquipped ?? false;
        
        /// <summary>
        /// Setup the view with inventory item data.
        /// </summary>
        public void Setup(InventoryItem inventoryItem)
        {
            _inventoryItem = inventoryItem;
            
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
            
            // Update equipped indicator
            UpdateEquippedState(inventoryItem.IsEquipped);
            
            // Deselect by default
            SetSelected(false);
        }
        
        /// <summary>
        /// Update the equipped visual state.
        /// </summary>
        public void UpdateEquippedState(bool isEquipped)
        {
            if (_equippedIndicator != null)
            {
                _equippedIndicator.SetActive(isEquipped);
            }
        }
        
        /// <summary>
        /// Set selection visual state.
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (_selectionFrame != null)
            {
                _selectionFrame.SetActive(selected);
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
                    _smallIconBackground.color = mapping.Color;
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
                // Double click
                OnDoubleClick?.Invoke(this);
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
    
    /// <summary>
    /// Mapping between item type and its icon.
    /// </summary>
    [Serializable]
    public class ItemTypeIconMapping
    {
        public ItemType Type;
        public Sprite Icon;
    }
    
    /// <summary>
    /// Mapping between item rarity and background color.
    /// </summary>
    [Serializable]
    public class RarityColorMapping
    {
        public ItemRarity Rarity;
        public Color Color = Color.white;
    }
}

