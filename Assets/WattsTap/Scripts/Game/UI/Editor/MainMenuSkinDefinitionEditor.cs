#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace WattsTap.Game.UI.Editor
{
    [CustomEditor(typeof(MainMenuSkinDefinition))]
    public class MainMenuSkinDefinitionEditor : UnityEditor.Editor
    {
        private string searchQuery = "";
        private SerializedProperty skinIdProperty;
        private SerializedProperty tokensProperty;

        private void OnEnable()
        {
            skinIdProperty = serializedObject.FindProperty("skinId");
            tokensProperty = serializedObject.FindProperty("tokens");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(skinIdProperty);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Search by Sprite Name", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            searchQuery = EditorGUILayout.TextField("Search", searchQuery);
            if (EditorGUI.EndChangeCheck())
            {
                // Force repaint when search query changes
                Repaint();
            }

            EditorGUILayout.Space(5);

            if (string.IsNullOrEmpty(searchQuery))
            {
                EditorGUILayout.PropertyField(tokensProperty, true);
            }
            else
            {
                DrawFilteredTokens();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFilteredTokens()
        {
            EditorGUILayout.LabelField("Filtered Tokens", EditorStyles.boldLabel);

            int matchCount = 0;

            for (int i = 0; i < tokensProperty.arraySize; i++)
            {
                var tokenProperty = tokensProperty.GetArrayElementAtIndex(i);
                var spriteProperty = tokenProperty.FindPropertyRelative("sprite");
                var tokenIdProperty = tokenProperty.FindPropertyRelative("tokenId");

                string spriteName = "";
                string tokenId = tokenIdProperty.stringValue ?? "";

                if (spriteProperty.objectReferenceValue != null)
                {
                    spriteName = spriteProperty.objectReferenceValue.name;
                }

                bool matchesSprite = !string.IsNullOrEmpty(spriteName) &&
                                     spriteName.IndexOf(searchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0;
                bool matchesTokenId = !string.IsNullOrEmpty(tokenId) &&
                                      tokenId.IndexOf(searchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0;

                if (matchesSprite || matchesTokenId)
                {
                    matchCount++;

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUILayout.LabelField($"Element {i}", EditorStyles.miniLabel);
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(tokenProperty.FindPropertyRelative("tokenId"));
                    EditorGUILayout.PropertyField(tokenProperty.FindPropertyRelative("color"));
                    EditorGUILayout.PropertyField(tokenProperty.FindPropertyRelative("material"));
                    EditorGUILayout.PropertyField(spriteProperty);

                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space(5);
                }
            }
            if (matchCount == 0)
            {
                EditorGUILayout.HelpBox($"No tokens found matching '{searchQuery}'", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField($"Found {matchCount} token(s)", EditorStyles.miniLabel);
            }
        }
    }
}
#endif
