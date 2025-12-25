using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.Avatars;

namespace WattsTap.Game.UI
{
    public class AvatarScreenUIPresenter : UIBasePresenter<AvatarScreenUIView, AvatarScreenUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;
        private IAvatarsService _avatarsService;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _hapticService);
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _avatarsService);

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
            
            Model.AvatarPresenters.Add(presenter);
        }
        
        private void CreateAvatarItem(AvatarConfig config)
        {
            var view = Object.Instantiate(View.AvatarItemPrefab, View.AvatarsContainer);
            var state = GetAvatarState(config.AvatarId);
            
            var model = new AvatarItemUIModel(config.AvatarId, config.AvatarSprite, state);
            var presenter = new AvatarItemUIPresenter(view, model);
            presenter.OnAvatarClicked += OnAvatarItemClicked;
            
            Model.AvatarPresenters.Add(presenter);
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
            _hapticService?.ButtonPressed();
            
            if (currentState == AvatarItemState.Locked)
            {
                // TODO: Show unlock requirements popup
                Debug.Log($"[AvatarScreenUIPresenter] Avatar '{avatarId}' is locked");
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
            
            // Equip the selected avatar via service
            if (_avatarsService.SelectAvatar(selectedId))
            {
                Model.CurrentAvatarId.Value = selectedId;
                UpdateAllAvatarStates();
                UpdateEquipButtonState();
                
                Debug.Log($"[AvatarScreenUIPresenter] Avatar '{selectedId}' equipped!");
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

        private void OnBackClicked()
        {
            _hapticService?.ButtonPressed();

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
            }
            
            // Cleanup avatar presenters
            foreach (var presenter in Model.AvatarPresenters)
            {
                presenter.OnAvatarClicked -= OnAvatarItemClicked;
            }
        }
    }
}

