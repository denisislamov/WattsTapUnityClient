using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.Inventory;
using WattsTap.Core.UI;
using WattsTap.Game.UI.Components.Inventory;

namespace WattsTap.Game.UI
{
    public class MergeScreenUIView : UIBaseView<MergeScreenUIPresenter>
    {
        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;
        
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _inventoryButton;

        [Header("Inventory Grid")]
        [SerializeField] private Transform _inventoryContainer;
        [SerializeField] private MergeItemElementView _itemPrefab;

        private readonly List<MergeItemElementView> _itemViews = new List<MergeItemElementView>();

        public event Action<MergeItemElementView> OnItemClicked;
        public event Action<MergeItemElementView, bool> OnItemSelectionChanged;

        public Button BackButton => _backButton;
        public Button InventoryButton => _inventoryButton;

        /// <summary>
        /// Populate the inventory grid with items.
        /// </summary>
        public void PopulateInventory(IReadOnlyList<InventoryItem> items)
        {
            ClearInventoryViews();
            
            if (_itemPrefab == null || _inventoryContainer == null)
            {
                Debug.LogWarning("[MergeScreenUIView] Item prefab or container not set");
                return;
            }
            
            foreach (var item in items)
            {
                var itemView = Instantiate(_itemPrefab, _inventoryContainer);
                itemView.Setup(item);
                itemView.OnSingleClick += HandleItemClick;
                itemView.OnSelectionChanged += HandleItemSelectionChanged;
                _itemViews.Add(itemView);
            }
        }

        private void HandleItemClick(MergeItemElementView itemView)
        {
            OnItemClicked?.Invoke(itemView);
        }

        private void HandleItemSelectionChanged(MergeItemElementView itemView, bool isSelected)
        {
            OnItemSelectionChanged?.Invoke(itemView, isSelected);
        }

        private void ClearInventoryViews()
        {
            foreach (var view in _itemViews)
            {
                if (view != null)
                {
                    view.OnSingleClick -= HandleItemClick;
                    view.OnSelectionChanged -= HandleItemSelectionChanged;
                    Destroy(view.gameObject);
                }
            }
            _itemViews.Clear();
        }

        #region Skinning

        private void OnEnable()
        {
            _themeManager = ServiceLocator.Get<MainMenuThemeManager>();
            
            if (_themeManager != null)
            {
                _themeManager.SkinChanged += OnSkinChanged;

                if (_themeManager.CurrentSkin != null)
                {
                    _themeManager.ApplySkin(_themeManager.CurrentSkin, _skinBindings, this);
                    return;
                }
                
                _themeManager.ApplySkin(null, _skinBindings, this);
            }
        }

        private void OnDisable()
        {
            if (_themeManager != null)
            {
                _themeManager.SkinChanged -= OnSkinChanged;
            }
        }
        
        private void OnSkinChanged(MainMenuSkinDefinition skin)
        {
            if (_themeManager != null)
            {
                _themeManager.ApplySkin(skin, _skinBindings, this);
            }
        }

        #endregion

        protected override void OnDestroy()
        {
            ClearInventoryViews();
            base.OnDestroy();
        }
    }
}

