#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace WattsTap.Core.Inventory.Editor
{
    /// <summary>
    /// Custom editor for ItemData that provides a rich visual preview
    /// with icon display, rarity coloring, and organized layout.
    /// </summary>
    [CustomEditor(typeof(ItemData))]
    public class ItemDataEditor : UnityEditor.Editor
    {
        // Serialized properties
        private SerializedProperty _idProp;
        private SerializedProperty _displayNameProp;
        private SerializedProperty _descriptionProp;
        private SerializedProperty _itemTypeProp;
        private SerializedProperty _rarityProp;
        private SerializedProperty _requiredLevelProp;
        private SerializedProperty _iconProp;
        private SerializedProperty _perTapBonusProp;

        private void OnEnable()
        {
            _idProp = serializedObject.FindProperty("_id");
            _displayNameProp = serializedObject.FindProperty("_displayName");
            _descriptionProp = serializedObject.FindProperty("_description");
            _itemTypeProp = serializedObject.FindProperty("_itemType");
            _rarityProp = serializedObject.FindProperty("_rarity");
            _requiredLevelProp = serializedObject.FindProperty("_requiredLevel");
            _iconProp = serializedObject.FindProperty("_icon");
            _perTapBonusProp = serializedObject.FindProperty("_perTapBonus");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ItemData itemData = (ItemData)target;
            ItemRarity rarity = itemData.Rarity;

            DrawHeaderWithIcon(itemData, rarity);
            EditorGUILayout.Space(10);
            DrawIdentificationSection();
            EditorGUILayout.Space(5);
            DrawClassificationSection(rarity);
            EditorGUILayout.Space(5);
            DrawStatsSection(itemData);
            EditorGUILayout.Space(5);
            DrawVisualSection();
            EditorGUILayout.Space(10);
            DrawSummaryBox(itemData, rarity);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeaderWithIcon(ItemData itemData, ItemRarity rarity)
        {
            Color rarityColor = GetRarityColor(rarity);

            // Header background
            Rect headerRect = EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(headerRect, new Color(rarityColor.r * 0.2f, rarityColor.g * 0.2f, rarityColor.b * 0.2f, 0.3f));

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);

            // Icon preview
            if (itemData.Icon != null)
            {
                Rect iconRect = GUILayoutUtility.GetRect(80, 80, GUILayout.Width(80));
                
                // Draw rarity border
                Rect borderRect = new Rect(iconRect.x - 2, iconRect.y - 2, iconRect.width + 4, iconRect.height + 4);
                EditorGUI.DrawRect(borderRect, rarityColor);
                
                // Draw icon background
                EditorGUI.DrawRect(iconRect, new Color(0.15f, 0.15f, 0.15f, 1f));
                
                // Draw icon
                GUI.DrawTexture(iconRect, itemData.Icon.texture, ScaleMode.ScaleToFit);
            }
            else
            {
                Rect iconRect = GUILayoutUtility.GetRect(80, 80, GUILayout.Width(80));
                EditorGUI.DrawRect(iconRect, new Color(0.2f, 0.2f, 0.2f, 1f));
                
                GUIStyle noIconStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                    fontSize = 10
                };
                GUI.Label(iconRect, "No Icon\nAssigned", noIconStyle);
            }

            GUILayout.Space(15);

            // Title & rarity info
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            // Display name
            string displayName = string.IsNullOrEmpty(itemData.DisplayName) ? itemData.name : itemData.DisplayName;
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                normal = { textColor = rarityColor },
                richText = true
            };
            EditorGUILayout.LabelField(displayName, titleStyle, GUILayout.Height(24));

            // Rarity badge
            GUIStyle rarityBadgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 11,
                normal = { textColor = rarityColor },
                fontStyle = FontStyle.Bold
            };
            string raritySymbol = GetRaritySymbol(rarity);
            EditorGUILayout.LabelField($"{raritySymbol} {rarity}", rarityBadgeStyle);

            // Type
            if (itemData.ItemType != ItemType.None)
            {
                GUIStyle typeStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontSize = 10,
                    normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
                };
                string typeIcon = GetItemTypeIcon(itemData.ItemType);
                EditorGUILayout.LabelField($"{typeIcon} {itemData.ItemType}", typeStyle);
            }

            // Level
            GUIStyle levelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.6f, 0.8f, 1f) }
            };
            EditorGUILayout.LabelField($"Lv. {itemData.RequiredLevel}", levelStyle);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();

            // Rarity color strip
            Rect stripRect = GUILayoutUtility.GetRect(0, 3);
            EditorGUI.DrawRect(stripRect, rarityColor);
        }

        private void DrawIdentificationSection()
        {
            EditorGUILayout.LabelField("🆔 Identification", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(_idProp, new GUIContent("ID"));
            EditorGUILayout.PropertyField(_displayNameProp, new GUIContent("Display Name"));
            EditorGUILayout.PropertyField(_descriptionProp, new GUIContent("Description"));

            EditorGUILayout.EndVertical();
        }

        private void DrawClassificationSection(ItemRarity rarity)
        {
            EditorGUILayout.LabelField("📋 Classification", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Item Type with icon
            EditorGUILayout.PropertyField(_itemTypeProp, new GUIContent("Item Type"));

            // Rarity with colored label
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_rarityProp, new GUIContent("Rarity"));
            
            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = GetRarityColor(rarity);
            GUILayout.Label("  ■  ", GUILayout.Width(30));
            GUI.backgroundColor = prevColor;
            
            EditorGUILayout.EndHorizontal();

            // Required level
            EditorGUILayout.PropertyField(_requiredLevelProp, new GUIContent("Required Level"));

            EditorGUILayout.EndVertical();
        }

        private void DrawStatsSection(ItemData itemData)
        {
            EditorGUILayout.LabelField("⚔️ Stats", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(_perTapBonusProp, new GUIContent("Per Tap Bonus"));

            // Visual stat bar
            if (itemData.PerTapBonus > 0)
            {
                EditorGUILayout.Space(3);
                Rect barBgRect = GUILayoutUtility.GetRect(0, 12);
                barBgRect.x += 4;
                barBgRect.width -= 8;
                
                EditorGUI.DrawRect(barBgRect, new Color(0.15f, 0.15f, 0.15f, 1f));
                
                float fillPercent = Mathf.Clamp01(itemData.PerTapBonus / 100f);
                Rect barFillRect = new Rect(barBgRect.x, barBgRect.y, barBgRect.width * fillPercent, barBgRect.height);
                
                Color barColor = Color.Lerp(new Color(0.2f, 0.8f, 0.2f), new Color(1f, 0.3f, 0.1f), fillPercent);
                EditorGUI.DrawRect(barFillRect, barColor);
                
                GUIStyle barLabelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    normal = { textColor = Color.white },
                    fontSize = 9
                };
                GUI.Label(barBgRect, $"+{itemData.PerTapBonus:F1} / tap", barLabelStyle);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawVisualSection()
        {
            EditorGUILayout.LabelField("🎨 Visual", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(_iconProp, new GUIContent("Icon"));

            // Large icon preview
            ItemData itemData = (ItemData)target;
            if (itemData.Icon != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Preview:", EditorStyles.miniLabel);
                
                Rect previewRect = GUILayoutUtility.GetRect(0, 128);
                previewRect.x = previewRect.x + (previewRect.width - 128) / 2f;
                previewRect.width = 128;
                
                // Background
                EditorGUI.DrawRect(previewRect, new Color(0.12f, 0.12f, 0.12f, 1f));
                
                // Checkerboard pattern indicator
                Rect borderRect = new Rect(previewRect.x - 1, previewRect.y - 1, previewRect.width + 2, previewRect.height + 2);
                Color rarityColor = GetRarityColor(itemData.Rarity);
                EditorGUI.DrawRect(borderRect, rarityColor);
                EditorGUI.DrawRect(previewRect, new Color(0.12f, 0.12f, 0.12f, 1f));
                
                // Draw texture
                GUI.DrawTexture(previewRect, itemData.Icon.texture, ScaleMode.ScaleToFit);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSummaryBox(ItemData itemData, ItemRarity rarity)
        {
            Color rarityColor = GetRarityColor(rarity);

            Rect summaryRect = EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(summaryRect, new Color(rarityColor.r * 0.1f, rarityColor.g * 0.1f, rarityColor.b * 0.1f, 0.4f));
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("📝 Summary", EditorStyles.boldLabel);

            GUIStyle summaryStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                richText = true,
                fontSize = 11
            };

            string summary = $"<b>{(string.IsNullOrEmpty(itemData.DisplayName) ? itemData.name : itemData.DisplayName)}</b>\n" +
                             $"Type: {itemData.ItemType}  |  Rarity: {rarity}  |  Level: {itemData.RequiredLevel}\n" +
                             $"Per Tap Bonus: +{itemData.PerTapBonus:F1}";

            if (!string.IsNullOrEmpty(itemData.Description))
            {
                summary += $"\n<i>{itemData.Description}</i>";
            }

            EditorGUILayout.LabelField(summary, summaryStyle);
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        #region Utility Methods

        private static Color GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return new Color(0.7f, 0.7f, 0.7f); // Grey
                case ItemRarity.Uncommon:
                    return new Color(0.3f, 0.9f, 0.3f); // Green
                case ItemRarity.Rare:
                    return new Color(0.3f, 0.5f, 1f);   // Blue
                case ItemRarity.Epic:
                    return new Color(0.7f, 0.3f, 1f);   // Purple
                case ItemRarity.Legendary:
                    return new Color(1f, 0.75f, 0.15f);  // Gold
                default:
                    return Color.white;
            }
        }

        private static string GetRaritySymbol(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common:   return "●";
                case ItemRarity.Uncommon: return "◆";
                case ItemRarity.Rare:     return "★";
                case ItemRarity.Epic:     return "✦";
                case ItemRarity.Legendary: return "♛";
                default:                  return "○";
            }
        }

        private static string GetItemTypeIcon(ItemType type)
        {
            switch (type)
            {
                case ItemType.Weapon:    return "⚔️";
                case ItemType.ArmorBody: return "🛡️";
                case ItemType.ArmorLegs: return "👢";
                case ItemType.ArmorArms: return "🧤";
                case ItemType.ArmorHead: return "⛑️";
                default:                 return "📦";
            }
        }

        #endregion

        // Custom preview in the project window
        public override bool HasPreviewGUI()
        {
            ItemData itemData = (ItemData)target;
            return itemData.Icon != null;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            ItemData itemData = (ItemData)target;
            if (itemData.Icon == null) return;

            // Draw rarity-colored background
            Color rarityColor = GetRarityColor(itemData.Rarity);
            EditorGUI.DrawRect(r, new Color(rarityColor.r * 0.15f, rarityColor.g * 0.15f, rarityColor.b * 0.15f, 1f));

            // Draw icon centered
            float size = Mathf.Min(r.width, r.height) - 10;
            Rect iconRect = new Rect(
                r.x + (r.width - size) / 2f,
                r.y + (r.height - size) / 2f,
                size, size);

            GUI.DrawTexture(iconRect, itemData.Icon.texture, ScaleMode.ScaleToFit);

            // Draw name label at bottom
            Rect labelRect = new Rect(r.x, r.yMax - 20, r.width, 20);
            EditorGUI.DrawRect(labelRect, new Color(0, 0, 0, 0.6f));
            
            GUIStyle labelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                normal = { textColor = rarityColor },
                fontStyle = FontStyle.Bold
            };
            GUI.Label(labelRect, itemData.DisplayName, labelStyle);
        }

        public override GUIContent GetPreviewTitle()
        {
            ItemData itemData = (ItemData)target;
            return new GUIContent($"{itemData.DisplayName} [{itemData.Rarity}]");
        }
    }
}
#endif

