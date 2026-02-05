using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core.UI;
using WattsTap.Game.Shop;

namespace WattsTap.Game.UI
{
    public class ShopChestItemUIViewFinal : UIBaseView<ShopChestItemUIPresenterFinal>
    {
        [Header("Controls")]
        [SerializeField] private Button _closeButton;

        public Button CloseButton => _closeButton;

        /// <summary>
        /// Update the view based on the config.
        /// This can be extended later if you need to display config data.
        /// </summary>
        public void UpdateFromConfig(ShopChestItemConfig config)
        {
            // Здесь можно добавить отображение данных из config, если понадобится
            // Например: название предмета, иконка и т.д.
        }
    }
}
