using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.Avatars;
using WattsTap.Game.Player;

namespace WattsTap.Game.UI
{
    public class AvatarScreenUIPresenter : UIBasePresenter<AvatarScreenUIView, AvatarScreenUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;
        private IAvatarsService _avatarsService;
        private IPlayerService _playerService;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _hapticService);
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _avatarsService);
            ServiceLocator.TryGet(out _playerService);

            if (View.BackButton != null)
            {
                View.BackButton.onClick.AddListener(OnBackClicked);
            }
            
            if (View.EquipButton != null)
            {
                View.EquipButton.onClick.AddListener(OnEquipClicked);
            }
            
            if (_avatarsService != null)
            {
                // Set initial current avatar
                Model.CurrentAvatarId.Value = _avatarsService.GetCurrentAvatarId();
                // Initially, selected is same as current (no pending selection)
                Model.SelectedAvatarId.Value = Model.CurrentAvatarId.Value;
                
                // Subscribe to avatar changes from external sources
                _avatarsService.OnAvatarChanged += OnAvatarChangedExternally;
                _avatarsService.OnTelegramAvatarLoaded += OnTelegramAvatarLoaded;
                _avatarsService.OnAvatarPurchased += OnAvatarPurchased;
                
                // Create avatar items
                CreateAvatarItems();
                
                // Update equip button visibility
                UpdateEquipButtonState();
            }
        }
        
        private void CreateAvatarItems()
        {
            if (View.AvatarItemPrefab == null || View.AvatarsContainer == null)
            {
                Debug.LogWarning("[AvatarScreenUIPresenter] Avatar prefab or container is not set");
                return;
            }
            
            // Create Telegram avatar first (default)
            CreateTelegramAvatarItem();
            
            // Create avatars from configs
            var avatarConfigs = _avatarsService.GetAllAvatarConfigs();
            foreach (var config in avatarConfigs)
            {
                if (config != null)
                {
                    CreateAvatarItem(config);
                }
            }
        }
        
        private void CreateTelegramAvatarItem()
        {
            var view = Object.Instantiate(View.AvatarItemPrefab, View.AvatarsContainer);
            var sprite = _avatarsService.GetTelegramAvatarSprite();
            var state = GetAvatarState(_avatarsService.TelegramAvatarId);
            
            var model = new AvatarItemUIModel(_avatarsService.TelegramAvatarId, sprite, state);
            var presenter = new AvatarItemUIPresenter(view, model);
            presenter.OnAvatarClicked += OnAvatarItemClicked;
            
            // Telegram avatar is always free - hide cost
            view.SetCostVisible(false);
            
            Model.AvatarPresenters.Add(presenter);
        }
        
        private void CreateAvatarItem(AvatarConfig config)
        {
            var view = Object.Instantiate(View.AvatarItemPrefab, View.AvatarsContainer);
            var state = GetAvatarState(config.AvatarId);
            
            var model = new AvatarItemUIModel(config.AvatarId, config.AvatarSprite, state);
            var presenter = new AvatarItemUIPresenter(view, model);
            presenter.OnAvatarClicked += OnAvatarItemClicked;
            
            // Set cost display based on unlock type
            if (config.IsUnlockedByDefault || _avatarsService.IsAvatarUnlocked(config.AvatarId))
            {
                // Already unlocked - hide cost
                view.SetCostVisible(false);
            }
            else
            {
                // Show cost based on unlock type
                long price = config.UnlockType == AvatarUnlockType.BTN ? config.BtnPrice : config.CoinPrice;
                view.SetCost(config.UnlockType, price, config.RequiredLevel);
                view.SetCostVisible(true);
                
                // Set affordability (opacity)
                bool canAfford = CanAffordAvatar(config);
                view.SetCostAffordable(canAfford);
            }
            
            Model.AvatarPresenters.Add(presenter);
        }
        
        private bool CanAffordAvatar(AvatarConfig config)
        {
            if (_playerService == null)
            {
                return false;
            }
            
            var playerData = _playerService.GetPlayerData();
            
            // Check level requirement first
            if (playerData.level < config.RequiredLevel)
            {
                return false;
            }
            
            // Check currency based on unlock type
            switch (config.UnlockType)
            {
                case AvatarUnlockType.Coins:
                    return playerData.resources.watts >= config.CoinPrice;
                    
                case AvatarUnlockType.BTN:
                    // BTN not implemented yet
                    return false;
                    
                case AvatarUnlockType.Level:
                    // Level unlock is free if level requirement is met
                    return true;
                    
                case AvatarUnlockType.Free:
                    return true;
                    
                default:
                    return false;
            }
        }
        
        private AvatarItemState GetAvatarState(string avatarId)
        {
            if (!_avatarsService.IsAvatarUnlocked(avatarId))
            {
                return AvatarItemState.Locked;
            }
            
            if (avatarId == Model.CurrentAvatarId.Value)
            {
                return AvatarItemState.Current;
            }
            
            if (avatarId == Model.SelectedAvatarId.Value)
            {
                return AvatarItemState.Selected;
            }
            
            return AvatarItemState.Unselected;
        }
        
        private void OnAvatarItemClicked(string avatarId, AvatarItemState currentState)
        {
            // _hapticService?.ButtonPressed();
            
            if (currentState == AvatarItemState.Locked)
            {
                // Check if we can afford this locked avatar
                var config = _avatarsService.GetAvatarConfig(avatarId);
                if (config != null && CanAffordAvatar(config))
                {
                    // Select locked avatar so user can click Equip to purchase
                    Model.SelectedAvatarId.Value = avatarId;
                    UpdateAllAvatarStates();
                    UpdateEquipButtonState();
                    Debug.Log($"[AvatarScreenUIPresenter] Locked avatar '{avatarId}' selected for purchase");
                }
                else
                {
                    Debug.Log($"[AvatarScreenUIPresenter] Avatar '{avatarId}' is locked and cannot be afforded");
                    // TODO: Show "not enough resources" feedback
                }
                return;
            }
            
            if (currentState == AvatarItemState.Current)
            {
                // Already current - deselect (go back to current as selected)
                Model.SelectedAvatarId.Value = Model.CurrentAvatarId.Value;
                UpdateAllAvatarStates();
                UpdateEquipButtonState();
                return;
            }
            
            if (currentState == AvatarItemState.Selected)
            {
                // Already selected - deselect
                Model.SelectedAvatarId.Value = Model.CurrentAvatarId.Value;
                UpdateAllAvatarStates();
                UpdateEquipButtonState();
                return;
            }
            
            // Select this avatar (not equip yet, just visual selection)
            Model.SelectedAvatarId.Value = avatarId;
            UpdateAllAvatarStates();
            UpdateEquipButtonState();
            
            Debug.Log($"[AvatarScreenUIPresenter] Avatar '{avatarId}' selected (not equipped yet)");
        }
        
        private void OnEquipClicked()
        {
            _hapticService?.ButtonPressed();
            
            var selectedId = Model.SelectedAvatarId.Value;
            
            // Don't equip if selected is same as current
            if (selectedId == Model.CurrentAvatarId.Value)
            {
                Debug.Log("[AvatarScreenUIPresenter] Selected avatar is already equipped");
                return;
            }
            
            // Check if avatar is locked - try to purchase
            if (!_avatarsService.IsAvatarUnlocked(selectedId))
            {
                TryPurchaseAvatar(selectedId);
                return;
            }
            
            // Equip the selected avatar via service
            if (_avatarsService.SelectAvatar(selectedId))
            {
                Model.CurrentAvatarId.Value = selectedId;
                UpdateAllAvatarStates();
                UpdateEquipButtonState();
                
                Debug.Log($"[AvatarScreenUIPresenter] Avatar '{selectedId}' equipped!");
            }
        }
        
        private void TryPurchaseAvatar(string avatarId)
        {
            var config = _avatarsService.GetAvatarConfig(avatarId);
            if (config == null)
            {
                Debug.LogWarning($"[AvatarScreenUIPresenter] Avatar config '{avatarId}' not found");
                return;
            }
            
            if (!CanAffordAvatar(config))
            {
                Debug.Log($"[AvatarScreenUIPresenter] Cannot afford avatar '{avatarId}'");
                // TODO: Show "not enough resources" feedback
                return;
            }
            
            AvatarPurchaseResult result;
            
            switch (config.UnlockType)
            {
                case AvatarUnlockType.Coins:
                    result = _avatarsService.PurchaseAvatarWithCoins(avatarId);
                    break;
                    
                case AvatarUnlockType.BTN:
                    result = _avatarsService.PurchaseAvatarWithBTN(avatarId);
                    break;
                    
                case AvatarUnlockType.Level:
                    result = _avatarsService.UnlockAvatarByLevel(avatarId);
                    break;
                    
                case AvatarUnlockType.Free:
                    _avatarsService.UnlockAvatar(avatarId);
                    result = AvatarPurchaseResult.Success;
                    break;
                    
                default:
                    result = AvatarPurchaseResult.Error;
                    break;
            }
            
            if (result == AvatarPurchaseResult.Success)
            {
                Debug.Log($"[AvatarScreenUIPresenter] Avatar '{avatarId}' purchased successfully!");
                
                // Auto-equip after purchase
                if (_avatarsService.SelectAvatar(avatarId))
                {
                    Model.CurrentAvatarId.Value = avatarId;
                    UpdateAllAvatarStates();
                    UpdateEquipButtonState();
                }
                
                // Update all affordability states (resources changed)
                UpdateAllAffordabilityStates();
            }
            else
            {
                Debug.Log($"[AvatarScreenUIPresenter] Failed to purchase avatar '{avatarId}': {result}");
            }
        }
        
        private void UpdateAllAffordabilityStates()
        {
            var avatarConfigs = _avatarsService.GetAllAvatarConfigs();
            
            foreach (var presenter in Model.AvatarPresenters)
            {
                // Skip telegram avatar
                if (presenter.AvatarId == _avatarsService.TelegramAvatarId)
                {
                    continue;
                }
                
                // Skip unlocked avatars
                if (_avatarsService.IsAvatarUnlocked(presenter.AvatarId))
                {
                    continue;
                }
                
                // Find config and update affordability
                foreach (var config in avatarConfigs)
                {
                    if (config.AvatarId == presenter.AvatarId)
                    {
                        bool canAfford = CanAffordAvatar(config);
                        presenter.View.SetCostAffordable(canAfford);
                        break;
                    }
                }
            }
        }
        
        private void UpdateAllAvatarStates()
        {
            foreach (var presenter in Model.AvatarPresenters)
            {
                var state = GetAvatarState(presenter.AvatarId);
                presenter.SetState(state);
            }
        }
        
        private void UpdateEquipButtonState()
        {
            // Show equip button only if selected avatar is different from current
            bool hasNewSelection = Model.SelectedAvatarId.Value != Model.CurrentAvatarId.Value;
            // View.SetEquipButtonVisible(hasNewSelection);
            View.SetEquipButtonInteractable(hasNewSelection);
        }
        
        private void OnAvatarChangedExternally(string newAvatarId)
        {
            // Avatar was changed from outside (e.g., another screen)
            Model.CurrentAvatarId.Value = newAvatarId;
            Model.SelectedAvatarId.Value = newAvatarId;
            
            UpdateAllAvatarStates();
            UpdateEquipButtonState();
        }
        
        private void OnTelegramAvatarLoaded(Sprite sprite)
        {
            // Update Telegram avatar sprite
            foreach (var presenter in Model.AvatarPresenters)
            {
                if (presenter.AvatarId == _avatarsService.TelegramAvatarId)
                {
                    presenter.SetSprite(sprite);
                    break;
                }
            }
        }
        
        private void OnAvatarPurchased(string avatarId, AvatarPurchaseResult result)
        {
            if (result != AvatarPurchaseResult.Success)
            {
                return;
            }
            
            // Find the presenter for this avatar and hide cost
            foreach (var presenter in Model.AvatarPresenters)
            {
                if (presenter.AvatarId == avatarId)
                {
                    presenter.View.SetCostVisible(false);
                    presenter.SetState(AvatarItemState.Unselected);
                    break;
                }
            }
            
            UpdateAllAvatarStates();
        }

        private void OnBackClicked()
        {
            // _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.AvatarScreen);
        }

        protected override void OnDispose()
        {
            if (View?.BackButton != null)
            {
                View.BackButton.onClick.RemoveListener(OnBackClicked);
            }
            
            if (View?.EquipButton != null)
            {
                View.EquipButton.onClick.RemoveListener(OnEquipClicked);
            }
            
            if (_avatarsService != null)
            {
                _avatarsService.OnAvatarChanged -= OnAvatarChangedExternally;
                _avatarsService.OnTelegramAvatarLoaded -= OnTelegramAvatarLoaded;
                _avatarsService.OnAvatarPurchased -= OnAvatarPurchased;
            }
            
            // Cleanup avatar presenters
            foreach (var presenter in Model.AvatarPresenters)
            {
                presenter.OnAvatarClicked -= OnAvatarItemClicked;
            }
        }
    }
}

