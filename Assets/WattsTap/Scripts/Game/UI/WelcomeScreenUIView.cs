using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class WelcomeScreenUIView : UIBaseView<WelcomeScreenUIPresenter>
    {
        [Header("Controls")]
        [SerializeField] private Button _startButton;

        public Button StartButton => _startButton;
    }
}
