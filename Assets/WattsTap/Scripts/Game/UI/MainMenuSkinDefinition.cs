using System;
using UnityEngine;

namespace WattsTap.Game.UI
{
    [CreateAssetMenu(menuName = "WattsTap/UI/Main Menu Skin", fileName = "MainMenuSkin")]
    public class MainMenuSkinDefinition : ScriptableObject
    {
        [SerializeField] private string skinId = "default";
        [SerializeField] private SkinToken[] tokens;

        public string SkinId => skinId;

        public bool TryGetToken(string tokenId, out SkinToken token)
        {
            token = null;

            if (tokens == null || tokens.Length == 0 || string.IsNullOrEmpty(tokenId))
            {
                return false;
            }

            for (int i = 0; i < tokens.Length; i++)
            {
                var current = tokens[i];
                if (current == null || string.IsNullOrEmpty(current.TokenId))
                {
                    continue;
                }

                if (string.Equals(current.TokenId, tokenId, StringComparison.OrdinalIgnoreCase))
                {
                    token = current;
                    return true;
                }
            }

            return false;
        }

        [Serializable]
        public class SkinToken
        {
            [SerializeField] private string tokenId;
            [SerializeField] private Color color = Color.white;
            [SerializeField] private Material material;

            public string TokenId => tokenId;
            public Color Color => color;
            public Material Material => material;
        }
    }
}

