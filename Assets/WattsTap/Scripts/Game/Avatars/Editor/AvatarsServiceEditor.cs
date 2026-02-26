#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace WattsTap.Game.Avatars.Editor
{
    /// <summary>
    /// Custom editor for AvatarsService with visual avatar grid and drag-reorderable custom sort list.
    /// </summary>
    [CustomEditor(typeof(AvatarsService))]
    public class AvatarsServiceEditor : UnityEditor.Editor
    {
        private const int Columns = 3;
        private const float CellSize = 110f;
        private const float SpriteSize = 72f;

        // Serialized properties
        private SerializedProperty _avatarConfigs;
        private SerializedProperty _customSortOrder;
        private SerializedProperty _defaultAvatarConfig;

        // Foldouts
        private bool _avatarConfigsFoldout = true;
        private bool _customSortFoldout = true;
        private bool _defaultsFoldout = true;

        // Styles (lazy init)
        private GUIStyle _cellStyle;
        private GUIStyle _nameLabelStyle;
        private GUIStyle _indexLabelStyle;

        // Reorderable lists
        private ReorderableList _configsReorderableList;
        private ReorderableList _sortReorderableList;

        private void OnEnable()
        {
            _avatarConfigs = serializedObject.FindProperty("_avatarConfigs");
            _customSortOrder = serializedObject.FindProperty("_customSortOrder");
            _defaultAvatarConfig = serializedObject.FindProperty("_defaultAvatarConfig");

            RebuildConfigsReorderableList();
            RebuildSortReorderableList();
        }

        private void InitStyles()
        {
            if (_cellStyle != null) return;

            _cellStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(4, 4, 4, 4),
                margin = new RectOffset(2, 2, 2, 2)
            };

            _nameLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fontSize = 10
            };

            _indexLabelStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                normal = { textColor = new Color(0.7f, 0.85f, 1f) }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            InitStyles();

            // --- Avatar Configurations grid ---
            DrawAvatarConfigsSection();

            EditorGUILayout.Space(8);

            // --- Custom Sort Order ---
            DrawCustomSortSection();

            EditorGUILayout.Space(8);

            // --- Default avatar config ---
            _defaultsFoldout = EditorGUILayout.Foldout(_defaultsFoldout, "Default Avatar", true, EditorStyles.foldoutHeader);
            if (_defaultsFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_defaultAvatarConfig, new GUIContent("Default Avatar Config", "Конфиг аватара по умолчанию. Экипируется при первом запуске."));
                
                // Show visual preview of the default avatar config
                var defaultConfig = _defaultAvatarConfig.objectReferenceValue as AvatarConfig;
                if (defaultConfig != null)
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 15f);
                    
                    // Sprite preview
                    Rect previewRect = GUILayoutUtility.GetRect(SpriteSize, SpriteSize, GUILayout.Width(SpriteSize), GUILayout.Height(SpriteSize));
                    EditorGUI.DrawRect(previewRect, new Color(0.15f, 0.15f, 0.15f, 1f));
                    
                    if (defaultConfig.AvatarSprite != null)
                    {
                        Texture2D tex = AssetPreview.GetAssetPreview(defaultConfig.AvatarSprite);
                        if (tex != null)
                            GUI.DrawTexture(previewRect, tex, ScaleMode.ScaleToFit);
                        else
                            GUI.DrawTexture(previewRect, defaultConfig.AvatarSprite.texture, ScaleMode.ScaleToFit);
                    }
                    
                    // Info next to sprite
                    EditorGUILayout.BeginVertical();
                    string displayName = !string.IsNullOrEmpty(defaultConfig.DisplayName) ? defaultConfig.DisplayName : defaultConfig.AvatarId;
                    EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"ID: {defaultConfig.AvatarId}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField("✓ Экипируется по умолчанию", EditorStyles.miniLabel);
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.HelpBox("Не указан дефолтный аватар! Будет использован Telegram аватар как fallback.", MessageType.Warning);
                }
                
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        #region Avatar Configs Grid

        private void RebuildConfigsReorderableList()
        {
            const float elementHeight = 52f;

            _configsReorderableList = new ReorderableList(serializedObject, _avatarConfigs, true, true, true, true)
            {
                elementHeightCallback = _ => elementHeight,
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, $"Avatar Configs ({_avatarConfigs.arraySize})  — drag to reorder, +/− to add/remove");
                },
                drawElementCallback = DrawConfigElement,
                onAddCallback = list =>
                {
                    int index = list.serializedProperty.arraySize;
                    list.serializedProperty.InsertArrayElementAtIndex(index);
                    list.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = null;
                }
            };
        }

        private void DrawConfigElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _avatarConfigs.GetArrayElementAtIndex(index);
            var config = element.objectReferenceValue as AvatarConfig;

            float x = rect.x;
            float y = rect.y + 2f;
            float spriteSmall = 46f;

            // Index label
            float idxWidth = 22f;
            EditorGUI.LabelField(new Rect(x, y + (spriteSmall - 16f) * 0.5f, idxWidth, 16f),
                $"{index}", EditorStyles.miniBoldLabel);
            x += idxWidth + 2f;

            // Sprite preview
            Rect spriteRect = new Rect(x, y, spriteSmall, spriteSmall);
            EditorGUI.DrawRect(spriteRect, new Color(0.15f, 0.15f, 0.15f, 1f));

            if (config != null && config.AvatarSprite != null)
            {
                Texture2D tex = AssetPreview.GetAssetPreview(config.AvatarSprite);
                if (tex != null)
                    GUI.DrawTexture(spriteRect, tex, ScaleMode.ScaleToFit);
                else
                    GUI.DrawTexture(spriteRect, config.AvatarSprite.texture, ScaleMode.ScaleToFit);
            }

            x += spriteSmall + 6f;
            float fieldWidth = rect.xMax - x;

            // Object field
            EditorGUI.PropertyField(
                new Rect(x, y, fieldWidth, EditorGUIUtility.singleLineHeight),
                element, GUIContent.none);

            // Info under the field
            if (config != null)
            {
                string displayName = !string.IsNullOrEmpty(config.DisplayName) ? config.DisplayName : config.AvatarId;
                string badge = GetBadgeText(config);
                EditorGUI.LabelField(
                    new Rect(x, y + EditorGUIUtility.singleLineHeight + 2f, fieldWidth, EditorGUIUtility.singleLineHeight),
                    $"{displayName}  •  {badge}", EditorStyles.miniLabel);
            }
        }

        private void DrawAvatarConfigsSection()
        {
            _avatarConfigsFoldout = EditorGUILayout.Foldout(_avatarConfigsFoldout,
                $"Avatar Configurations  ({_avatarConfigs.arraySize})", true, EditorStyles.foldoutHeader);

            if (!_avatarConfigsFoldout) return;

            // Reorderable list with +/- buttons
            _configsReorderableList.DoLayoutList();

            // Grid preview below
            if (_avatarConfigs.arraySize > 0)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                DrawAvatarGrid(_avatarConfigs, showSortIndex: false);
            }
        }

        private void DrawAvatarGrid(SerializedProperty arrayProp, bool showSortIndex)
        {
            int count = arrayProp.arraySize;
            if (count == 0)
            {
                EditorGUILayout.HelpBox("No avatars configured.", MessageType.Info);
                return;
            }

            int col = 0;
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < count; i++)
            {
                var element = arrayProp.GetArrayElementAtIndex(i);

                AvatarConfig config = null;
                int sortIndex = i;

                if (showSortIndex)
                {
                    // element is AvatarSortEntry
                    var configProp = element.FindPropertyRelative("AvatarConfig");
                    var indexProp = element.FindPropertyRelative("SortIndex");
                    config = configProp?.objectReferenceValue as AvatarConfig;
                    sortIndex = indexProp?.intValue ?? i;
                }
                else
                {
                    config = element.objectReferenceValue as AvatarConfig;
                }

                DrawAvatarCell(config, i, sortIndex, showSortIndex);

                col++;
                if (col >= Columns)
                {
                    col = 0;
                    EditorGUILayout.EndHorizontal();
                    if (i < count - 1)
                        EditorGUILayout.BeginHorizontal();
                    else
                        return; // ended cleanly
                }
            }

            // Fill remaining cells in last row
            if (col > 0)
            {
                for (int f = col; f < Columns; f++)
                    GUILayout.Space(CellSize + 4);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawAvatarCell(AvatarConfig config, int arrayIndex, int sortIndex, bool showSortIndex)
        {
            EditorGUILayout.BeginVertical(_cellStyle, GUILayout.Width(CellSize), GUILayout.Height(CellSize + 30));

            if (config == null)
            {
                EditorGUILayout.LabelField("null", _nameLabelStyle, GUILayout.Height(SpriteSize));
                EditorGUILayout.LabelField($"[{arrayIndex}]", _indexLabelStyle);
            }
            else
            {
                // Sprite preview
                Rect spriteRect = GUILayoutUtility.GetRect(SpriteSize, SpriteSize,
                    GUILayout.Width(SpriteSize), GUILayout.Height(SpriteSize));

                // Center the rect inside the cell
                spriteRect.x += (CellSize - SpriteSize) * 0.5f - 6f;

                EditorGUI.DrawRect(spriteRect, new Color(0.15f, 0.15f, 0.15f, 1f));

                if (config.AvatarSprite != null)
                {
                    Texture2D tex = AssetPreview.GetAssetPreview(config.AvatarSprite);
                    if (tex != null)
                        GUI.DrawTexture(spriteRect, tex, ScaleMode.ScaleToFit);
                    else
                        GUI.DrawTexture(spriteRect, config.AvatarSprite.texture, ScaleMode.ScaleToFit);
                }
                else
                {
                    EditorGUI.LabelField(spriteRect, "—", _nameLabelStyle);
                }

                // Name
                string label = !string.IsNullOrEmpty(config.DisplayName) ? config.DisplayName : config.AvatarId;
                EditorGUILayout.LabelField(label, _nameLabelStyle);

                // Badge row: unlock type + sort index
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                DrawUnlockBadge(config);

                if (showSortIndex)
                {
                    GUILayout.Label($"#{sortIndex}", _indexLabelStyle);
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                // Ping on click
                if (Event.current.type == EventType.MouseDown && spriteRect.Contains(Event.current.mousePosition))
                {
                    EditorGUIUtility.PingObject(config);
                    Selection.activeObject = config;
                    Event.current.Use();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawUnlockBadge(AvatarConfig config)
        {
            string text;
            Color color;

            if (config.IsUnlockedByDefault)
            {
                text = "FREE";
                color = new Color(0.2f, 0.72f, 0.2f);
            }
            else
            {
                switch (config.UnlockType)
                {
                    case AvatarUnlockType.Level:
                        text = $"LVL {config.RequiredLevel}";
                        color = new Color(0.3f, 0.5f, 0.9f);
                        break;
                    case AvatarUnlockType.Coins:
                        text = $"⚡{config.CoinPrice:N0}";
                        color = new Color(0.9f, 0.7f, 0.1f);
                        break;
                    case AvatarUnlockType.BTN:
                        text = $"BTN {config.BtnPrice:N0}";
                        color = new Color(0.9f, 0.3f, 0.3f);
                        break;
                    default:
                        text = "FREE";
                        color = new Color(0.2f, 0.72f, 0.2f);
                        break;
                }
            }

            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUILayout.Label(text, new GUIStyle(EditorStyles.miniButton)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 9,
                normal = { textColor = Color.white },
                fixedHeight = 16
            });
            GUI.backgroundColor = oldBg;
        }

        #endregion

        #region Custom Sort Order

        private void RebuildSortReorderableList()
        {
            _sortReorderableList = new ReorderableList(serializedObject, _customSortOrder, true, true, true, true)
            {
                elementHeightCallback = _ => SpriteSize + 12f,
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Custom Sort Order (drag to reorder)");
                },
                drawElementCallback = DrawSortElement,
                onAddCallback = list =>
                {
                    int index = list.serializedProperty.arraySize;
                    list.serializedProperty.InsertArrayElementAtIndex(index);
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    element.FindPropertyRelative("AvatarConfig").objectReferenceValue = null;
                    element.FindPropertyRelative("SortIndex").intValue = index;
                },
                onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
                {
                    // After drag-reorder, recalculate SortIndex values to match visual order
                    RecalculateSortIndices();
                }
            };
        }

        private void DrawSortElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _customSortOrder.GetArrayElementAtIndex(index);
            var configProp = element.FindPropertyRelative("AvatarConfig");
            var indexProp = element.FindPropertyRelative("SortIndex");

            float x = rect.x;
            float y = rect.y + 2f;

            // Sort index label
            float idxWidth = 28f;
            EditorGUI.LabelField(new Rect(x, y + (SpriteSize - 16f) * 0.5f, idxWidth, 16f),
                $"#{indexProp.intValue}", EditorStyles.miniBoldLabel);
            x += idxWidth + 4f;

            // Sprite preview
            AvatarConfig config = configProp.objectReferenceValue as AvatarConfig;
            Rect spriteRect = new Rect(x, y, SpriteSize, SpriteSize);
            EditorGUI.DrawRect(spriteRect, new Color(0.15f, 0.15f, 0.15f, 1f));

            if (config != null && config.AvatarSprite != null)
            {
                Texture2D tex = AssetPreview.GetAssetPreview(config.AvatarSprite);
                if (tex != null)
                    GUI.DrawTexture(spriteRect, tex, ScaleMode.ScaleToFit);
                else
                    GUI.DrawTexture(spriteRect, config.AvatarSprite.texture, ScaleMode.ScaleToFit);
            }

            x += SpriteSize + 8f;

            float fieldWidth = rect.xMax - x;

            // Config object field
            EditorGUI.PropertyField(
                new Rect(x, y, fieldWidth, EditorGUIUtility.singleLineHeight),
                configProp, GUIContent.none);

            // Display name
            if (config != null)
            {
                string displayName = !string.IsNullOrEmpty(config.DisplayName)
                    ? config.DisplayName
                    : config.AvatarId;
                EditorGUI.LabelField(
                    new Rect(x, y + EditorGUIUtility.singleLineHeight + 2f, fieldWidth, EditorGUIUtility.singleLineHeight),
                    displayName, EditorStyles.boldLabel);

                // Unlock badge text
                string badgeText = GetBadgeText(config);
                EditorGUI.LabelField(
                    new Rect(x, y + (EditorGUIUtility.singleLineHeight + 2f) * 2, fieldWidth, EditorGUIUtility.singleLineHeight),
                    badgeText, EditorStyles.miniLabel);
            }

            // Sort index field
            EditorGUI.PropertyField(
                new Rect(x, y + SpriteSize - EditorGUIUtility.singleLineHeight, 60f, EditorGUIUtility.singleLineHeight),
                indexProp, GUIContent.none);
        }

        private string GetBadgeText(AvatarConfig config)
        {
            if (config.IsUnlockedByDefault) return "Free (default)";
            return config.UnlockType switch
            {
                AvatarUnlockType.Level => $"Level {config.RequiredLevel}",
                AvatarUnlockType.Coins => $"⚡ {config.CoinPrice:N0} Watts",
                AvatarUnlockType.BTN => $"BTN {config.BtnPrice:N0}",
                AvatarUnlockType.Free => "Free",
                _ => "—"
            };
        }

        private void RecalculateSortIndices()
        {
            for (int i = 0; i < _customSortOrder.arraySize; i++)
            {
                _customSortOrder.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("SortIndex").intValue = i;
            }
        }

        private void DrawCustomSortSection()
        {
            _customSortFoldout = EditorGUILayout.Foldout(_customSortFoldout,
                $"Custom Sort Order  ({_customSortOrder.arraySize})", true, EditorStyles.foldoutHeader);

            if (!_customSortFoldout) return;

            EditorGUILayout.HelpBox(
                "Перетаскивайте элементы для изменения порядка. SortIndex обновляется автоматически.\n" +
                "Если список пуст — используется автоматическая сортировка по типу/цене.",
                MessageType.Info);

            // Auto-fill button
            if (_avatarConfigs.arraySize > 0 && _customSortOrder.arraySize == 0)
            {
                if (GUILayout.Button("Auto-fill from Avatar Configs", GUILayout.Height(24)))
                {
                    AutoFillCustomSort();
                }
            }
            else if (_avatarConfigs.arraySize > 0 && _customSortOrder.arraySize > 0)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Re-fill from Configs", GUILayout.Height(20)))
                {
                    if (EditorUtility.DisplayDialog("Re-fill Custom Sort",
                        "Это перезапишет текущий кастомный порядок. Продолжить?", "Да", "Отмена"))
                    {
                        AutoFillCustomSort();
                    }
                }
                if (GUILayout.Button("Clear", GUILayout.Height(20)))
                {
                    _customSortOrder.ClearArray();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(4);

            // Visual grid preview of sorted result
            if (_customSortOrder.arraySize > 0)
            {
                EditorGUILayout.LabelField("Sorted Preview", EditorStyles.boldLabel);
                DrawAvatarGrid(_customSortOrder, showSortIndex: true);
                EditorGUILayout.Space(4);
            }

            // Reorderable list
            _sortReorderableList.DoLayoutList();
        }

        private void AutoFillCustomSort()
        {
            _customSortOrder.ClearArray();

            for (int i = 0; i < _avatarConfigs.arraySize; i++)
            {
                var configRef = _avatarConfigs.GetArrayElementAtIndex(i).objectReferenceValue;
                if (configRef == null) continue;

                int idx = _customSortOrder.arraySize;
                _customSortOrder.InsertArrayElementAtIndex(idx);
                var entry = _customSortOrder.GetArrayElementAtIndex(idx);
                entry.FindPropertyRelative("AvatarConfig").objectReferenceValue = configRef;
                entry.FindPropertyRelative("SortIndex").intValue = idx;
            }
        }

        #endregion
    }
}
#endif

