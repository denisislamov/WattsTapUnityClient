using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Inventory;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.UI.Components.Inventory;

namespace WattsTap.Game.UI
{
    public class InventoryScreenUIPresenter : UIBasePresenter<InventoryScreenUIView, InventoryScreenUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;
        private Coroutine _refreshCoroutine;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);

            // Subscribe to model changes
            Model.HitsCurrent.OnValueChanged += OnHitsChanged;
            Model.HitsMax.OnValueChanged += OnHitsChanged;
            Model.CoinsPerTap.OnValueChanged += OnCoinsPerTapChanged;
            Model.TotalEquipmentBonus.OnValueChanged += OnTotalBonusChanged;
            
            // Subscribe to per-stat equipment bonuses
            Model.BonusCoinsPerTap.OnValueChanged += OnEquippedStatsChanged;
            Model.BonusXpPerTap.OnValueChanged += OnEquippedStatsChanged;
            Model.BonusCapacityHits.OnValueChanged += OnEquippedStatsChanged;
            Model.BonusRecoverySpeed.OnValueChanged += OnEquippedStatsChanged;
            Model.BonusCritChance.OnValueChanged += OnEquippedStatsChanged;
            Model.BonusCritMultiplier.OnValueChanged += OnEquippedStatsChanged;
            Model.BonusOfflinePercent.OnValueChanged += OnEquippedStatsChanged;
            
            // Subscribe to profit summary
            Model.ProfitPerTap.OnValueChanged += OnProfitSummaryChanged;
            Model.ProfitPerHour.OnValueChanged += OnProfitSummaryChanged;
            Model.TotalRecovery.OnValueChanged += OnProfitSummaryChangedFloat;
            
            // Initialize view with current values
            View.UpdateHits(Model.HitsCurrent.Value, Model.HitsMax.Value);
            View.UpdateCoinsPerTap(Model.CoinsPerTap.Value);
            View.UpdateTotalBonus(Model.TotalEquipmentBonus.Value);
            RefreshEquippedStatsView();
            RefreshProfitSummaryView();
            
            // Show cached inventory immediately (so UI is not blank)
            PopulateInventory();
            InitializeEquipmentSlots();

            // Subscribe to inventory reload event (fires when server data arrives)
            if (Model.InventoryService != null)
            {
                Model.InventoryService.OnInventoryLoaded += OnInventoryRefreshed;
            }

            // Refresh inventory from server in background
            _refreshCoroutine = ((MonoBehaviour)View).StartCoroutine(Model.RefreshFromServer());

            // Subscribe to view events
            View.OnItemDoubleClicked += OnItemDoubleClicked;
            View.OnEquipmentSlotClicked += OnEquipmentSlotClicked;

            // Navigation buttons
            if (View.MiningButton != null)
            {
                View.MiningButton.onClick.AddListener(OnMiningButtonClicked);
            }

            if (View.FriendsReferralButton != null)
            {
                View.FriendsReferralButton.onClick.AddListener(OnFriendsReferralButtonClicked);
            }

            if (View.QuestsButton != null)
            {
                View.QuestsButton.onClick.AddListener(OnQuestsButtonClicked);
            }

            if (View.UpdatesButton != null)
            {
                View.UpdatesButton.onClick.AddListener(OnUpdatesButtonClicked);
            }

            if (View.MergeButton != null)
            {
                View.MergeButton.onClick.AddListener(OnMergeButtonClicked);
            }
        }
        
        private void PopulateInventory()
        {
            var items = Model.GetAllItems();
            View.PopulateInventory(items);
        }
        
        private void InitializeEquipmentSlots()
        {
            var equippedItems = Model.GetEquippedItems();
            
            // Clear all slots first
            View.UpdateEquipmentSlot(ItemType.Weapon, null);
            View.UpdateEquipmentSlot(ItemType.ArmorBody, null);
            View.UpdateEquipmentSlot(ItemType.ArmorArms, null);
            View.UpdateEquipmentSlot(ItemType.ArmorLegs, null);
            
            // Set equipped items
            foreach (var item in equippedItems)
            {
                if (item?.Data != null)
                {
                    View.UpdateEquipmentSlot(item.Data.ItemType, item);
                }
            }
        }

        #region Event Handlers - Model
        
        private void OnHitsChanged(int value)
        {
            View.UpdateHits(Model.HitsCurrent.Value, Model.HitsMax.Value);
        }

        private void OnCoinsPerTapChanged(int value)
        {
            View.UpdateCoinsPerTap(value);
        }
        
        private void OnTotalBonusChanged(float value)
        {
            View.UpdateTotalBonus(value);
        }
        
        private void OnEquippedStatsChanged(float _)
        {
            RefreshEquippedStatsView();
        }
        
        private void OnProfitSummaryChanged(int _)
        {
            RefreshProfitSummaryView();
        }
        
        private void OnProfitSummaryChangedFloat(float _)
        {
            RefreshProfitSummaryView();
        }
        
        private void RefreshEquippedStatsView()
        {
            View.UpdateEquippedStats(
                Model.BonusCoinsPerTap.Value,
                Model.BonusXpPerTap.Value,
                Model.BonusCapacityHits.Value,
                Model.BonusRecoverySpeed.Value,
                Model.BonusCritChance.Value,
                Model.BonusCritMultiplier.Value,
                Model.BonusOfflinePercent.Value
            );
        }
        
        private void RefreshProfitSummaryView()
        {
            View.UpdateProfitSummary(
                Model.ProfitPerTap.Value,
                Model.ProfitPerHour.Value,
                Model.TotalRecovery.Value
            );
        }
        
        /// <summary>
        /// Called when InventoryService finishes loading data from server.
        /// Re-populates the entire inventory UI with fresh data.
        /// </summary>
        private void OnInventoryRefreshed()
        {
            Debug.Log("<color=#00AAFF>[InventoryScreenUIPresenter] Server data arrived — refreshing UI</color>");
            PopulateInventory();
            InitializeEquipmentSlots();
            RefreshEquippedStatsView();
            RefreshProfitSummaryView();
        }
        
        #endregion

        #region Event Handlers - Buttons

        private void OnItemDoubleClicked(InventoryItemElementView itemView)
        {
            _hapticService?.ButtonPressed();
            
            var item = itemView.InventoryItem;
            if (item?.Data == null) return;
            
            // Equip the item
            if (!item.IsEquipped)
            {
                Model.EquipItem(item.InstanceId);
                
                // Update view
                View.UpdateItemEquippedState(item.InstanceId, true);
                View.UpdateEquipmentSlot(item.Data.ItemType, item);
            }
        }
        
        private void OnEquipmentSlotClicked(EquipmentSlotView slot)
        {
            _hapticService?.ButtonPressed();
            
            var item = slot.EquippedItem;
            if (item == null) return;
            
            // Unequip the item
            Model.UnequipItem(item.InstanceId);
            
            // Update view
            View.UpdateItemEquippedState(item.InstanceId, false);
            View.UpdateEquipmentSlot(slot.SlotType, null);
        }

        private void OnMiningButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close InventoryScreen to return to MainMenu
            _uiService?.Close(UIConstants.InventoryScreen);
        }

        private void OnFriendsReferralButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close InventoryScreen and open FriendsReferralScreen
            _uiService?.Close(UIConstants.InventoryScreen);
            _uiService?.Open(UIConstants.FriendsReferralScreen);
        }

        private void OnQuestsButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close InventoryScreen and open QuestsScreen
            _uiService?.Close(UIConstants.InventoryScreen);
            _uiService?.Open(UIConstants.QuestsScreen);
        }

        private void OnUpdatesButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close InventoryScreen and open UpdatesPopup
            _uiService?.Close(UIConstants.InventoryScreen);
            _uiService?.Open(UIConstants.UpdatesPopup);
        }

        private void OnMergeButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close InventoryScreen and open MergeScreen
            _uiService?.Close(UIConstants.InventoryScreen);
            _uiService?.Open(UIConstants.MergeScreen);
        }

        #endregion

        protected override void OnDispose()
        {
            // Stop pending server refresh
            if (_refreshCoroutine != null && View is MonoBehaviour mb && mb != null)
            {
                mb.StopCoroutine(_refreshCoroutine);
                _refreshCoroutine = null;
            }

            // Unsubscribe from inventory server reload
            if (Model?.InventoryService != null)
            {
                Model.InventoryService.OnInventoryLoaded -= OnInventoryRefreshed;
            }

            // Unsubscribe from model changes
            if (Model != null)
            {
                Model.HitsCurrent.OnValueChanged -= OnHitsChanged;
                Model.HitsMax.OnValueChanged -= OnHitsChanged;
                Model.CoinsPerTap.OnValueChanged -= OnCoinsPerTapChanged;
                Model.TotalEquipmentBonus.OnValueChanged -= OnTotalBonusChanged;
                
                Model.BonusCoinsPerTap.OnValueChanged -= OnEquippedStatsChanged;
                Model.BonusXpPerTap.OnValueChanged -= OnEquippedStatsChanged;
                Model.BonusCapacityHits.OnValueChanged -= OnEquippedStatsChanged;
                Model.BonusRecoverySpeed.OnValueChanged -= OnEquippedStatsChanged;
                Model.BonusCritChance.OnValueChanged -= OnEquippedStatsChanged;
                Model.BonusCritMultiplier.OnValueChanged -= OnEquippedStatsChanged;
                Model.BonusOfflinePercent.OnValueChanged -= OnEquippedStatsChanged;
                
                Model.ProfitPerTap.OnValueChanged -= OnProfitSummaryChanged;
                Model.ProfitPerHour.OnValueChanged -= OnProfitSummaryChanged;
                Model.TotalRecovery.OnValueChanged -= OnProfitSummaryChangedFloat;
            }
            
            // Unsubscribe from view events
            if (View != null)
            {
                View.OnItemDoubleClicked -= OnItemDoubleClicked;
                View.OnEquipmentSlotClicked -= OnEquipmentSlotClicked;
            }
            
            // Remove button listeners
            if (View?.MiningButton != null)
            {
                View.MiningButton.onClick.RemoveListener(OnMiningButtonClicked);
            }

            if (View?.FriendsReferralButton != null)
            {
                View.FriendsReferralButton.onClick.RemoveListener(OnFriendsReferralButtonClicked);
            }

            if (View?.QuestsButton != null)
            {
                View.QuestsButton.onClick.RemoveListener(OnQuestsButtonClicked);
            }

            if (View?.UpdatesButton != null)
            {
                View.UpdatesButton.onClick.RemoveListener(OnUpdatesButtonClicked);
            }

            if (View?.MergeButton != null)
            {
                View.MergeButton.onClick.RemoveListener(OnMergeButtonClicked);
            }
        }
    }
}

