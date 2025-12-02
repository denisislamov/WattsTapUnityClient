using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class FriendsReferralScreenUIView : UIBaseView<FriendsReferralScreenUIPresenter>
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button _referralButton;
        [SerializeField] private Button _friendsButton;

        [Header("Tab Content")]
        [SerializeField] private GameObject[] _referralElements;
        [SerializeField] private GameObject[] _friendsElements;

        [Header("Controls")]
        [SerializeField] private Button _miningButton;

        public Button ReferralButton => _referralButton;
        public Button FriendsButton => _friendsButton;
        public Button MiningButton => _miningButton;

        /// <summary>
        /// Shows referral tab content and hides friends content
        /// </summary>
        public void ShowReferralTab()
        {
            SetElementsActive(_referralElements, true);
            SetElementsActive(_friendsElements, false);
        }

        /// <summary>
        /// Shows friends tab content and hides referral content
        /// </summary>
        public void ShowFriendsTab()
        {
            SetElementsActive(_referralElements, false);
            SetElementsActive(_friendsElements, true);
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



