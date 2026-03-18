#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using WattsTap.Scripts.Game.UI.Components;

namespace WattsTap.Scripts.Game.UI.Editor
{
    [CustomEditor(typeof(ConfettiAnimation))]
    public class ConfettiAnimationEditor : UnityEditor.Editor
    {
        private int _previewCount = 30;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var confetti = (ConfettiAnimation)target;

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Editor Preview", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Particle Count", GUILayout.Width(100));
            _previewCount = EditorGUILayout.IntSlider(_previewCount, 1, 100);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            bool isAnimating = confetti.IsPreviewingInEditor || confetti.HasActiveParticles;

            EditorGUILayout.BeginHorizontal();

            // ▶ Play
            GUI.backgroundColor = new Color(0.4f, 0.9f, 0.5f);
            if (GUILayout.Button("▶  Play", GUILayout.Height(32)))
            {
                if (Application.isPlaying)
                    confetti.Play(_previewCount);
                else
                    confetti.PlayEditorPreview(_previewCount);
            }

            // ⏹ Stop
            GUI.backgroundColor = isAnimating ? new Color(1f, 0.45f, 0.4f) : Color.gray;
            GUI.enabled = isAnimating;
            if (GUILayout.Button("⏹  Stop", GUILayout.Height(32)))
            {
                if (Application.isPlaying)
                    confetti.Stop();
                else
                    confetti.StopEditorPreview();
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            // Статус
            if (isAnimating)
            {
                EditorGUILayout.HelpBox(
                    $"Анимация активна  •  частиц: {(confetti.HasActiveParticles ? "▸" : "завершено")}",
                    MessageType.Info);
                Repaint(); // непрерывный Repaint во время анимации
            }
        }
    }
}
#endif




