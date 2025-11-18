using System;
using UnityEngine;
using WattsTap.Core;

namespace WattsTap.Game.UI
{
    [DisallowMultipleComponent]
    public class MainMenuThemeManager : MonoBehaviour, IService
    {
        [SerializeField] private MainMenuSkinDefinition fallbackSkin;
        [SerializeField] private MainMenuSkinDefinition[] availableSkins;

        private int _currentIndex;

        public MainMenuSkinDefinition CurrentSkin { get; private set; }

        public event Action<MainMenuSkinDefinition> SkinChanged;

        public int InitializationOrder => 10;
        public bool IsInitialized { get; private set; }
        public void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;
        }

        public void Shutdown()
        {
            IsInitialized = false;
        }
        
        
        private void Awake()
        {
            if (CurrentSkin == null)
            {
                SetDefaultSkin();
            }
        }

        public MainMenuSkinDefinition SetDefaultSkin()
        {
            if (availableSkins == null || availableSkins.Length == 0)
            {
                return ActivateSkin(fallbackSkin);
            }

            return SetSkinByIndex(0);
        }

        public MainMenuSkinDefinition SetNextSkin()
        {
            if (availableSkins == null || availableSkins.Length == 0)
            {
                return ActivateSkin(fallbackSkin);
            }

            var nextIndex = (_currentIndex + 1) % availableSkins.Length;
            return SetSkinByIndex(nextIndex);
        }

        public MainMenuSkinDefinition SetSkinById(string skinId)
        {
            if (string.IsNullOrEmpty(skinId) || availableSkins == null)
            {
                return CurrentSkin;
            }

            for (int i = 0; i < availableSkins.Length; i++)
            {
                var skin = availableSkins[i];
                if (skin == null)
                {
                    continue;
                }

                if (string.Equals(skin.SkinId, skinId, StringComparison.OrdinalIgnoreCase))
                {
                    return SetSkinByIndex(i);
                }
            }

            Debug.LogWarning($"MainMenuThemeManager: skin with id '{skinId}' was not found.");
            return CurrentSkin;
        }

        private MainMenuSkinDefinition SetSkinByIndex(int index)
        {
            if (availableSkins == null || availableSkins.Length == 0)
            {
                return ActivateSkin(fallbackSkin);
            }

            index = Mathf.Clamp(index, 0, availableSkins.Length - 1);

            var nextSkin = availableSkins[index];
            if (nextSkin == null)
            {
                Debug.LogWarning($"MainMenuThemeManager: skin at index {index} is null.");
                return CurrentSkin;
            }

            _currentIndex = index;

            return ActivateSkin(nextSkin);
        }

        private MainMenuSkinDefinition ActivateSkin(MainMenuSkinDefinition skin)
        {
            if (skin == null)
            {
                Debug.LogWarning("MainMenuThemeManager: Attempted to activate a null skin.");
                return CurrentSkin;
            }

            if (ReferenceEquals(CurrentSkin, skin))
            {
                // Force refresh so listeners can reapply when needed.
                SkinChanged?.Invoke(CurrentSkin);
                return CurrentSkin;
            }

            CurrentSkin = skin;
            SkinChanged?.Invoke(CurrentSkin);
            return CurrentSkin;
        }
    }
}

