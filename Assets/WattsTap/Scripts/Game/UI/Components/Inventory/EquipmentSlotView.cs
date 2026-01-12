using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WattsTap.Core.Inventory;

namespace WattsTap.Game.UI.Components.Inventory
{
    /// <summary>
    /// UI component for an equipment slot.
    /// Displays the currently equipped item and handles unequip on click.
    /// </summary>
    public class EquipmentSlotView : MonoBehaviour
    {
        [Header("Slot Configuration")]
        [SerializeField] private ItemType _slotType = ItemType.Weapon;
        
        [Header("Display Elements")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _slotTypeIcon;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private GameObject _filledState;
        [SerializeField] private Button _slotButton;
        
        [Header("Rarity Colors")]
        [SerializeField] private RarityColorMapping[] _rarityColors;
        
        private InventoryItem _equippedItem;
        
        public event Action<EquipmentSlotView> OnSlotClicked;
        
        public ItemType SlotType => _slotType;
        public InventoryItem EquippedItem => _equippedItem;
        public bool HasItem => _equippedItem != null;
        
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
        /// Set the equipped item in this slot.
        /// </summary>
        public void SetEquippedItem(InventoryItem item)
        {
            _equippedItem = item;
            
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
            _equippedItem = null;
            ShowEmptyState();
        }
        
        private void ShowEmptyState()
        {
            if (_emptyState != null)
            {
                _emptyState.SetActive(true);
            }
            
            if (_filledState != null)
            {
                _filledState.SetActive(false);
            }
            
            if (_iconImage != null)
            {
                _iconImage.enabled = false;
            }
            
            if (_levelText != null)
            {
                _levelText.gameObject.SetActive(false);
            }
            
            // Reset background to default
            if (_backgroundImage != null)
            {
                _backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            }
        }
        
        private void ShowFilledState(InventoryItem item)
        {
            var data = item.Data;
            
            if (_emptyState != null)
            {
                _emptyState.SetActive(false);
            }
            
            if (_filledState != null)
            {
                _filledState.SetActive(true);
            }
            
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
                _levelText.gameObject.SetActive(true);
            }
            
            // Set rarity background
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
            
            // Default color
            _backgroundImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
        
        private void HandleSlotClick()
        {
            if (_equippedItem != null)
            {
                OnSlotClicked?.Invoke(this);
            }
        }
    }
}

