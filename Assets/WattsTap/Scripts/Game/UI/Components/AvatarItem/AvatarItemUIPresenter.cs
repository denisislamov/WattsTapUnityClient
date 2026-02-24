using System;
using UnityEngine;

namespace WattsTap.Game.UI
{
    /// <summary>
    /// Presenter for AvatarItem, connects Model and View.
    /// Handles the logic for avatar item interactions.
    /// </summary>
    public class AvatarItemUIPresenter : IDisposable
    {
        private readonly AvatarItemUIView _view;
        private readonly AvatarItemUIModel _model;
        
        /// <summary>
        /// Event fired when avatar is clicked.
        /// Provides the avatar ID and current state.
        /// </summary>
        public event Action<string, AvatarItemState> OnAvatarClicked;
        
        /// <summary>
        /// Gets the avatar's unique identifier.
        /// </summary>
        public string AvatarId => _model.AvatarId;
        
        /// <summary>
        /// Gets the current state of the avatar.
        /// </summary>
        public AvatarItemState CurrentState => _model.State;
        
        /// <summary>
        /// Gets the view component.
        /// </summary>
        public AvatarItemUIView View => _view;
        
        /// <summary>
        /// Creates a new AvatarItemUIPresenter.
        /// </summary>
        public AvatarItemUIPresenter(AvatarItemUIView view, AvatarItemUIModel model)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            
            Initialize();
        }
        
        private void Initialize()
        {
            // Set initial view state from model
            _view.SetAvatarId(_model.AvatarId);
            _view.SetAvatarSprite(_model.AvatarSprite);
            _view.ForceSetState(_model.State);
            
            // Subscribe to view events
            _view.OnAvatarClicked += HandleViewClicked;
            
            // Set interactable based on locked state
            UpdateInteractable();
        }
        
        private void HandleViewClicked(AvatarItemUIView view)
        {
            // Allow clicking on locked avatars - the presenter will check affordability
            OnAvatarClicked?.Invoke(_model.AvatarId, _model.State);
        }
        
        /// <summary>
        /// Updates the avatar's state.
        /// </summary>
        public void SetState(AvatarItemState newState)
        {
            _model.SetState(newState);
            _view.SetState(newState);
            UpdateInteractable();
        }
        
        /// <summary>
        /// Shows or hides the selection overlay independently of avatar state.
        /// </summary>
        public void SetSelected(bool selected)
        {
            _view.SetSelected(selected);
        }
        
        /// <summary>
        /// Updates the avatar's sprite.
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            _model.SetSprite(sprite);
            _view.SetAvatarSprite(sprite);
        }
        
        private void UpdateInteractable()
        {
            // Locked avatars can still be clicked (to show unlock requirements)
            // but you can change this behavior if needed
            _view.SetInteractable(true);
        }
        
        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnAvatarClicked -= HandleViewClicked;
            }
        }
    }
}

