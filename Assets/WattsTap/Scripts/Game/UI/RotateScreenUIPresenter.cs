using WattsTap.Core;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class RotateScreenUIPresenter : UIBasePresenter<RotateScreenUIView, RotateScreenUIModel>
    {
        private IOrientationService _orientationService;

        protected override void OnInit()
        {
            if (ServiceLocator.TryGet(out _orientationService))
            {
                _orientationService.OnOrientationChanged += OnOrientationChanged;
                
                // Apply initial state
                UpdateVisibility(_orientationService.IsLandscape);
            }
        }

        private void OnOrientationChanged(bool isLandscape)
        {
            UpdateVisibility(isLandscape);
        }

        private void UpdateVisibility(bool isLandscape)
        {
            if (isLandscape)
            {
                View.Show();
            }
            else
            {
                View.Hide();
            }
        }

        protected override void OnDispose()
        {
            if (_orientationService != null)
            {
                _orientationService.OnOrientationChanged -= OnOrientationChanged;
            }
        }
    }
}

