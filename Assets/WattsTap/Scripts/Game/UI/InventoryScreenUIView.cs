using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.Inventory;
using WattsTap.Core.UI;
using WattsTap.Game.UI.Components.Inventory;

namespace WattsTap.Game.UI
{
    public class InventoryScreenUIView : UIBaseView<InventoryScreenUIPresenter>
    {
        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;
        
        [Header("Navigation")]
        [SerializeField] private Button _miningButton;
        [SerializeField] private Button _friendsReferralButton;
        [SerializeField] private Button _questsButton;
        
        [Header("Hits Display")]
        [SerializeField] private TMP_Text _currentHitsText;
        [SerializeField] private TMP_Text _maxHitsText;
        
        [Header("Currency Display")]
        [SerializeField] private TMP_Text _coinsPerTapText;
        
        [Header("Inventory Grid")]
        [SerializeField] private Transform _inventoryContainer;
        [SerializeField] private InventoryItemElementView _itemPrefab;
        
        [Header("Equipment Slots")]
        [SerializeField] private EquipmentSlotView _weaponSlot;
        [SerializeField] private EquipmentSlotView _bodyArmorSlot;
        [SerializeField] private EquipmentSlotView _armsArmorSlot;
        [SerializeField] private EquipmentSlotView _legsArmorSlot;
        
        [Header("Stats Display")]
        [SerializeField] private TMP_Text _totalBonusText;
        
        private readonly List<InventoryItemElementView> _itemViews = new List<InventoryItemElementView>();
        
        public event Action<InventoryItemElementView> OnItemDoubleClicked;
        public event Action<EquipmentSlotView> OnEquipmentSlotClicked;

        public Button MiningButton => _miningButton;
        public Button FriendsReferralButton => _friendsReferralButton;
        public Button QuestsButton => _questsButton;
        
        public EquipmentSlotView WeaponSlot => _weaponSlot;
        public EquipmentSlotView BodyArmorSlot => _bodyArmorSlot;
        public EquipmentSlotView ArmsArmorSlot => _armsArmorSlot;
        public EquipmentSlotView LegsArmorSlot => _legsArmorSlot;

        private void OnEnable()
        {
            _themeManager = ServiceLocator.Get<MainMenuThemeManager>();
            
            if (_themeManager != null)
            {
                _themeManager.SkinChanged += OnSkinChanged;

                if (_themeManager.CurrentSkin != null)
                {
                    _themeManager.ApplySkin(_themeManager.CurrentSkin, _skinBindings, this);
                }
                else
                {
                    _themeManager.ApplySkin(null, _skinBindings, this);
                }
            }
            
            // Subscribe to equipment slot events
            SubscribeToSlots();
        }

        private void OnDisable()
        {
            if (_themeManager != null)
            {
                _themeManager.SkinChanged -= OnSkinChanged;
            }
            
            UnsubscribeFromSlots();
        }
        
        private void OnSkinChanged(MainMenuSkinDefinition skin)
        {
            if (_themeManager != null)
            {
                _themeManager.ApplySkin(skin, _skinBindings, this);
            }
        }
        
        private void SubscribeToSlots()
        {
            if (_weaponSlot != null) _weaponSlot.OnSlotClicked += HandleEquipmentSlotClicked;
            if (_bodyArmorSlot != null) _bodyArmorSlot.OnSlotClicked += HandleEquipmentSlotClicked;
            if (_armsArmorSlot != null) _armsArmorSlot.OnSlotClicked += HandleEquipmentSlotClicked;
            if (_legsArmorSlot != null) _legsArmorSlot.OnSlotClicked += HandleEquipmentSlotClicked;
        }
        
        private void UnsubscribeFromSlots()
        {
            if (_weaponSlot != null) _weaponSlot.OnSlotClicked -= HandleEquipmentSlotClicked;
            if (_bodyArmorSlot != null) _bodyArmorSlot.OnSlotClicked -= HandleEquipmentSlotClicked;
            if (_armsArmorSlot != null) _armsArmorSlot.OnSlotClicked -= HandleEquipmentSlotClicked;
            if (_legsArmorSlot != null) _legsArmorSlot.OnSlotClicked -= HandleEquipmentSlotClicked;
        }
        
        private void HandleEquipmentSlotClicked(EquipmentSlotView slot)
        {
            OnEquipmentSlotClicked?.Invoke(slot);
        }
        
        public void UpdateHits(int current, int max)
        {
            if (_currentHitsText != null)
            {
                _currentHitsText.text = $"{current}";
            }
            
            if (_maxHitsText != null)
            {
                _maxHitsText.text = $"{max}";
            }
        }
        
        public void UpdateCoinsPerTap(int coinsPerTap)
        {
            if (_coinsPerTapText != null)
            {
                _coinsPerTapText.text = $"+{coinsPerTap}";
            }
        }
        
        /// <summary>
        /// Populate the inventory grid with items.
        /// </summary>
        public void PopulateInventory(IReadOnlyList<InventoryItem> items)
        {
            ClearInventoryViews();
            
            if (_itemPrefab == null || _inventoryContainer == null)
            {
                Debug.LogWarning("[InventoryScreenUIView] Item prefab or container not set");
                return;
            }
            
            foreach (var item in items)
            {
                var itemView = Instantiate(_itemPrefab, _inventoryContainer);
                itemView.Setup(item);
                itemView.OnDoubleClick += HandleItemDoubleClick;
                _itemViews.Add(itemView);
            }
        }
        
        /// <summary>
        /// Update a specific item's equipped state.
        /// </summary>
        public void UpdateItemEquippedState(string instanceId, bool isEquipped)
        {
            foreach (var view in _itemViews)
            {
                if (view.InventoryItem?.InstanceId == instanceId)
                {
                    view.UpdateEquippedState(isEquipped);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Update equipment slot with item.
        /// </summary>
        public void UpdateEquipmentSlot(ItemType slotType, InventoryItem item)
        {
            var slot = GetSlotForType(slotType);
            if (slot != null)
            {
                if (item != null)
                {
                    slot.SetEquippedItem(item);
                }
                else
                {
                    slot.ClearSlot();
                }
            }
        }
        
        /// <summary>
        /// Update total bonus display.
        /// </summary>
        public void UpdateTotalBonus(float totalBonus)
        {
            if (_totalBonusText != null)
            {
                _totalBonusText.text = $"+{totalBonus:F1}/tap";
            }
        }
        
        /// <summary>
        /// Get equipment slot for item type.
        /// </summary>
        public EquipmentSlotView GetSlotForType(ItemType type)
        {
            switch (type)
            {
                case ItemType.Weapon:
                    return _weaponSlot;
                case ItemType.ArmorBody:
                    return _bodyArmorSlot;
                case ItemType.ArmorArms:
                    return _armsArmorSlot;
                case ItemType.ArmorLegs:
                    return _legsArmorSlot;
                default:
                    return null;
            }
        }
        
        private void HandleItemDoubleClick(InventoryItemElementView itemView)
        {
            OnItemDoubleClicked?.Invoke(itemView);
        }
        
        private void ClearInventoryViews()
        {
            foreach (var view in _itemViews)
            {
                if (view != null)
                {
                    view.OnDoubleClick -= HandleItemDoubleClick;
                    Destroy(view.gameObject);
                }
            }
            _itemViews.Clear();
        }
        
        protected override void OnDestroy()
        {
            ClearInventoryViews();
            base.OnDestroy();
        }
    }
}
