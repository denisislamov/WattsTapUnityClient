using UnityEngine;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class RotateScreenUIView : UIBaseView<RotateScreenUIPresenter>
    {
        [Header("Optional Elements")]
        [SerializeField] private GameObject _rotateIcon;
        [SerializeField] private GameObject _messageText;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

