using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        
        public void ApplySkin(MainMenuSkinDefinition skin, SkinTokenBinding[] skinBindings, MonoBehaviour context)
        {
            var skinToApply = skin ?? fallbackSkin;
            
            if (skinToApply == null)
            {
                Debug.LogWarning("MainMenuThemeManager: No skin provided to apply.");
                return;
            }

            if (skinBindings == null || skinBindings.Length == 0)
            {
                Debug.LogWarning("MainMenuThemeManager: No skin bindings configured.");
                return;
            }

            foreach (var binding in skinBindings)
            {
                binding?.Apply(skinToApply, context);
            }
        }
        
        [Serializable]
        public class SkinTokenBinding
        {
            [SerializeField] private string tokenId;
            [SerializeField] private Image[] imageTargets;
            [SerializeField] private TMP_Text[] textTargets;
            [SerializeField] private bool suppressMissingTokenWarning;

            public void Apply(MainMenuSkinDefinition skin, MonoBehaviour context)
            {
                if (skin == null || string.IsNullOrEmpty(tokenId))
                {
                    return;
                }

                if (!skin.TryGetToken(tokenId, out var token))
                {
                    if (!suppressMissingTokenWarning)
                    {
                        Debug.LogWarning($"MainMenuThemeManager: Token '{tokenId}' was not found in skin '{skin.name}'.", context);
                    }
                    
                    return;
                }

                ApplyToImages(token);
                ApplyToTexts(token);
            }

            private void ApplyToImages(MainMenuSkinDefinition.SkinToken token)
            {
                if (imageTargets == null)
                {
                    return;
                }
                
                foreach (var image in imageTargets)
                {
                    if (image == null)
                    {
                        continue;
                    }

                    image.color = token.Color;
                    
                    if (token.Sprite != null)
                    {
                        image.sprite = token.Sprite;
                    }
                    
                    if (token.Material != null)
                    {
                        image.material = token.Material;
                    }
                }
            }

            private void ApplyToTexts(MainMenuSkinDefinition.SkinToken token)
            {
                if (textTargets == null)
                {
                    return;
                }
                
                foreach (var text in textTargets)
                {
                    if (text == null)
                    {
                        continue;
                    }

                    text.color = token.Color;
                    if (token.Material != null)
                    {
                        text.material = token.Material;
                    }
                }
            }
        }
    }
}

