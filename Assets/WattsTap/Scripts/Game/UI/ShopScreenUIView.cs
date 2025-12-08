using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class ShopScreenUIView : UIBaseView<ShopScreenUIPresenter>
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button _chestsButton;
        [SerializeField] private Button _boostersButton;

        [Header("Tab Content")]
        [SerializeField] private GameObject[] _chestsPanelElements;
        [SerializeField] private GameObject[] _boostersPanelElements;

        [Header("Controls")]
        [SerializeField] private Button[] _closeButtons;

        public Button ChestsButton => _chestsButton;
        public Button BoostersButton => _boostersButton;
        public Button[] CloseButtons => _closeButtons;

        /// <summary>
        /// Shows chests tab content and hides boosters content
        /// </summary>
        public void ShowChestsTab()
        {
            SetElementsActive(_chestsPanelElements, true);
            SetElementsActive(_boostersPanelElements, false);
        }

        /// <summary>
        /// Shows boosters tab content and hides chests content
        /// </summary>
        public void ShowBoostersTab()
        {
            SetElementsActive(_chestsPanelElements, false);
            SetElementsActive(_boostersPanelElements, true);
        }

        private static void SetElementsActive(GameObject[] elements, bool isActive)
        {
            if (elements == null)
            {
                return;
            }

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] != null)
                {
                    elements[i].SetActive(isActive);
                }
            }
        }
    }
}








