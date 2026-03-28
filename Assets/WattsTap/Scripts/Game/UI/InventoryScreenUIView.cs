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
        [SerializeField] private Button _updatesButton;
        [SerializeField] private Button _mergeButton;
        
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
        
        [Header("Profit Summary")]
        [SerializeField] private TMP_Text _profitPerTapText;
        [SerializeField] private TMP_Text _profitPerHourText;
        [SerializeField] private TMP_Text _totalRecoveryText;
        
        [Header("Equipment Stats")]
        [SerializeField] private TMP_Text _bonusCoinsPerTapText;
        [SerializeField] private TMP_Text _bonusXpPerTapText;
        [SerializeField] private TMP_Text _bonusCapacityText;
        [SerializeField] private TMP_Text _bonusRecoveryText;
        [SerializeField] private TMP_Text _bonusCritChanceText;
        [SerializeField] private TMP_Text _bonusCritMultText;
        [SerializeField] private TMP_Text _bonusOfflineText;
        
        private readonly List<InventoryItemElementView> _itemViews = new List<InventoryItemElementView>();
        
        public event Action<InventoryItemElementView> OnItemDoubleClicked;
        public event Action<EquipmentSlotView> OnEquipmentSlotClicked;

        public Button MiningButton => _miningButton;
        public Button FriendsReferralButton => _friendsReferralButton;
        public Button QuestsButton => _questsButton;
        public Button UpdatesButton => _updatesButton;
        public Button MergeButton => _mergeButton;
        
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
        /// Update profit summary fields: per tap, per hour, recovery.
        /// </summary>
        public void UpdateProfitSummary(int profitPerTap, int profitPerHour, float totalRecovery)
        {
            if (_profitPerTapText != null)
            {
                _profitPerTapText.text = $"+{profitPerTap}";
            }
            
            if (_profitPerHourText != null)
            {
                _profitPerHourText.text = $"+{profitPerHour}";
            }
            
            if (_totalRecoveryText != null)
            {
                _totalRecoveryText.text = $"{totalRecovery:F1}";
            }
        }
        
        /// <summary>
        /// Update per-stat equipment bonuses display.
        /// Only shows stats that have a non-zero value.
        /// </summary>
        public void UpdateEquippedStats(
            float coinsPerTap, float xpPerTap, float capacityHits,
            float recoverySpeed, float critChance, float critMult, float offlinePercent)
        {
            SetStatText(_bonusCoinsPerTapText, "Coins/Tap", coinsPerTap, false);
            SetStatText(_bonusXpPerTapText, "XP/Tap", xpPerTap, false);
            SetStatText(_bonusCapacityText, "Capacity", capacityHits, false);
            SetStatText(_bonusRecoveryText, "Recovery", recoverySpeed, false);
            SetStatText(_bonusCritChanceText, "Crit Chance", critChance, true);
            SetStatText(_bonusCritMultText, "Crit Mult", critMult, true);
            SetStatText(_bonusOfflineText, "Offline", offlinePercent, true);
        }
        
        private void SetStatText(TMP_Text text, string label, float value, bool isPercent)
        {
            if (text == null) return;
            
            if (value == 0f)
            {
                text.gameObject.SetActive(false);
                return;
            }
            
            text.gameObject.SetActive(true);
            text.text = isPercent
                ? $"{label}: +{value:F1}%"
                : $"{label}: +{value:F0}";
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
