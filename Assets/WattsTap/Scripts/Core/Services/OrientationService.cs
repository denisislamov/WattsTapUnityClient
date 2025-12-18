using System;
using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core.GameLoop;
using WattsTap.Core.UI;

namespace WattsTap.Core.Services
{
    public class OrientationService : IOrientationService, IUpdatable
    {
        public event Action<bool> OnOrientationChanged;

        public int InitializationOrder => 250;
        public bool IsInitialized { get; private set; }
        public bool IsLandscape { get; private set; }

        private IUIService _uiService;
        private IUpdateService _updateService;
        private IUIView _rotateScreenView;
        
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        public void Initialize()
        {
            if (IsInitialized) return;

            _uiService = ServiceLocator.Get<IUIService>();
            _updateService = ServiceLocator.Get<IUpdateService>();

            // Get initial orientation
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            IsLandscape = _lastScreenWidth > _lastScreenHeight;

            // Register for update loop
            _updateService.Register(this);

            // Open rotate screen on overlay (always on top)
            _rotateScreenView = _uiService.Open(UIConstants.RotateScreen, _uiService.OverlayRoot);

            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;

            if (_updateService != null)
            {
                _updateService.Unregister(this);
            }

            if (_rotateScreenView != null && _uiService != null)
            {
                _uiService.Close(_rotateScreenView);
            }

            IsInitialized = false;
        }

        public void OnUpdate()
        {
            int currentWidth = Screen.width;
            int currentHeight = Screen.height;

            // Only check if screen dimensions have changed
            if (currentWidth == _lastScreenWidth && currentHeight == _lastScreenHeight)
            {
                return;
            }

            _lastScreenWidth = currentWidth;
            _lastScreenHeight = currentHeight;

            bool newIsLandscape = currentWidth > currentHeight;

            if (newIsLandscape != IsLandscape)
            {
                IsLandscape = newIsLandscape;
                OnOrientationChanged?.Invoke(IsLandscape);
            }
        }
    }
}

