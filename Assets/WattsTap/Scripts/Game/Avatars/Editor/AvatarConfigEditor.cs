#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace WattsTap.Game.Avatars.Editor
{
    /// <summary>
    /// Custom editor for AvatarConfig with avatar preview
    /// </summary>
    [CustomEditor(typeof(AvatarConfig))]
    public class AvatarConfigEditor : UnityEditor.Editor
    {
        private const float PreviewSize = 128f;
        private const float SmallPreviewSize = 64f;
        
        // Serialized properties
        private SerializedProperty _avatarId;
        private SerializedProperty _displayName;
        private SerializedProperty _avatarSprite;
        private SerializedProperty _isUnlockedByDefault;
        private SerializedProperty _unlockRequirement;
        private SerializedProperty _unlockType;
        private SerializedProperty _requiredLevel;
        private SerializedProperty _coinPrice;
        private SerializedProperty _btnPrice;
        
        // Styles
        private GUIStyle _previewBoxStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _priceStyle;
        
        private void OnEnable()
        {
            _avatarId = serializedObject.FindProperty("_avatarId");
            _displayName = serializedObject.FindProperty("_displayName");
            _avatarSprite = serializedObject.FindProperty("_avatarSprite");
            _isUnlockedByDefault = serializedObject.FindProperty("_isUnlockedByDefault");
            _unlockRequirement = serializedObject.FindProperty("_unlockRequirement");
            _unlockType = serializedObject.FindProperty("_unlockType");
            _requiredLevel = serializedObject.FindProperty("_requiredLevel");
            _coinPrice = serializedObject.FindProperty("_coinPrice");
            _btnPrice = serializedObject.FindProperty("_btnPrice");
        }
        
        private void InitStyles()
        {
            if (_previewBoxStyle == null)
            {
                _previewBoxStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(10, 10, 10, 10)
                };
            }
            
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter
                };
            }
            
            if (_priceStyle == null)
            {
                _priceStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
            }
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            InitStyles();
            
            var avatarConfig = (AvatarConfig)target;
            
            // Preview section
            DrawPreviewSection(avatarConfig);
            
            EditorGUILayout.Space(10);
            
            // Avatar Info section
            DrawAvatarInfoSection();
            
            EditorGUILayout.Space(5);
            
            // Availability section
            DrawAvailabilitySection();
            
            EditorGUILayout.Space(5);
            
            // Unlock Requirements section
            DrawUnlockRequirementsSection();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawPreviewSection(AvatarConfig config)
        {
            EditorGUILayout.BeginVertical(_previewBoxStyle);
            
            // Header with name
            string displayName = !string.IsNullOrEmpty(config.DisplayName) ? config.DisplayName : config.AvatarId;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = "New Avatar";
            }
            
            EditorGUILayout.LabelField(displayName, _headerStyle);
            EditorGUILayout.Space(5);
            
            // Avatar preview
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            Rect previewRect = GUILayoutUtility.GetRect(PreviewSize, PreviewSize, GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
            
            // Draw background
            EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.2f, 1f));
            
            // Draw sprite preview
            if (config.AvatarSprite != null)
            {
                Texture2D texture = AssetPreview.GetAssetPreview(config.AvatarSprite);
                if (texture != null)
                {
                    GUI.DrawTexture(previewRect, texture, ScaleMode.ScaleToFit);
                }
                else
                {
                    // Fallback to sprite texture
                    GUI.DrawTexture(previewRect, config.AvatarSprite.texture, ScaleMode.ScaleToFit);
                }
            }
            else
            {
                // Draw placeholder
                EditorGUI.LabelField(previewRect, "No Sprite", new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                });
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Quick info
            DrawQuickInfo(config);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawQuickInfo(AvatarConfig config)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            // Unlock status
            if (config.IsUnlockedByDefault)
            {
                DrawInfoBadge("✓ FREE", new Color(0.2f, 0.7f, 0.2f));
            }
            else
            {
                switch (config.UnlockType)
                {
                    case AvatarUnlockType.Level:
                        DrawInfoBadge($"LVL {config.RequiredLevel}", new Color(0.3f, 0.5f, 0.9f));
                        break;
                    case AvatarUnlockType.Coins:
                        DrawInfoBadge($"⚡ {config.CoinPrice:N0}", new Color(0.9f, 0.7f, 0.1f));
                        if (config.RequiredLevel > 1)
                        {
                            DrawInfoBadge($"LVL {config.RequiredLevel}", new Color(0.3f, 0.5f, 0.9f));
                        }
                        break;
                    case AvatarUnlockType.BTN:
                        DrawInfoBadge($"BTN {config.BtnPrice:N0}", new Color(0.9f, 0.3f, 0.3f));
                        if (config.RequiredLevel > 1)
                        {
                            DrawInfoBadge($"LVL {config.RequiredLevel}", new Color(0.3f, 0.5f, 0.9f));
                        }
                        break;
                    case AvatarUnlockType.Free:
                        DrawInfoBadge("FREE", new Color(0.2f, 0.7f, 0.2f));
                        break;
                }
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawInfoBadge(string text, Color color)
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            
            GUILayout.Label(text, new GUIStyle(EditorStyles.miniButton)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            }, GUILayout.Height(20));
            
            GUI.backgroundColor = oldColor;
        }
        
        private void DrawAvatarInfoSection()
        {
            EditorGUILayout.LabelField("Avatar Info", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(_avatarId, new GUIContent("Avatar ID", "Unique identifier for this avatar"));
            EditorGUILayout.PropertyField(_displayName, new GUIContent("Display Name", "Name shown to players"));
            EditorGUILayout.PropertyField(_avatarSprite, new GUIContent("Sprite", "Avatar image"));
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawAvailabilitySection()
        {
            EditorGUILayout.LabelField("Availability", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(_isUnlockedByDefault, new GUIContent("Unlocked By Default", "If true, avatar is available from the start"));
            EditorGUILayout.PropertyField(_unlockRequirement, new GUIContent("Unlock Description", "Text describing how to unlock this avatar"));
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawUnlockRequirementsSection()
        {
            EditorGUILayout.LabelField("Unlock Requirements", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            // Unlock Type
            EditorGUILayout.PropertyField(_unlockType, new GUIContent("Unlock Type", "How this avatar can be unlocked"));
            
            var unlockType = (AvatarUnlockType)_unlockType.enumValueIndex;
            
            // Level requirement (shown for all types except Free)
            if (unlockType != AvatarUnlockType.Free)
            {
                EditorGUILayout.PropertyField(_requiredLevel, new GUIContent("Required Level", "Minimum player level to unlock/purchase"));
            }
            
            // Price fields based on unlock type
            switch (unlockType)
            {
                case AvatarUnlockType.Coins:
                    EditorGUILayout.PropertyField(_coinPrice, new GUIContent("Coin Price (Watts)", "Price in Watts currency"));
                    break;
                    
                case AvatarUnlockType.BTN:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(_btnPrice, new GUIContent("BTN Price", "Price in BTN tokens"));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.HelpBox("BTN purchases are not yet available.", MessageType.Info);
                    break;
                    
                case AvatarUnlockType.Level:
                    EditorGUILayout.HelpBox("This avatar will be automatically unlocked when player reaches the required level.", MessageType.Info);
                    break;
                    
                case AvatarUnlockType.Free:
                    EditorGUILayout.HelpBox("This avatar is free but not unlocked by default. Use IsUnlockedByDefault if it should be available from start.", MessageType.Info);
                    break;
            }
            
            EditorGUI.indentLevel--;
        }
        
        // Custom preview in Project window
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var config = (AvatarConfig)target;
            
            if (config.AvatarSprite == null)
            {
                return base.RenderStaticPreview(assetPath, subAssets, width, height);
            }
            
            // Create preview texture
            Texture2D preview = new Texture2D(width, height);
            Texture2D spriteTexture = config.AvatarSprite.texture;
            
            if (spriteTexture != null)
            {
                // Copy sprite texture to preview
                EditorUtility.CopySerialized(AssetPreview.GetAssetPreview(config.AvatarSprite) ?? spriteTexture, preview);
            }
            
            return preview;
        }
        
        public override bool HasPreviewGUI()
        {
            return true;
        }
        
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            var config = (AvatarConfig)target;
            
            if (config.AvatarSprite != null)
            {
                Texture2D texture = AssetPreview.GetAssetPreview(config.AvatarSprite);
                if (texture != null)
                {
                    GUI.DrawTexture(r, texture, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUI.DrawTexture(r, config.AvatarSprite.texture, ScaleMode.ScaleToFit);
                }
            }
        }
    }
}
#endif