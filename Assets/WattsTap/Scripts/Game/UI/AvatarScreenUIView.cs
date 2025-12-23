using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class AvatarScreenUIView : UIBaseView<AvatarScreenUIPresenter>
    {
        [Header("Controls")]
        [SerializeField] private Button _backButton;

        public Button BackButton => _backButton;
    }
}

