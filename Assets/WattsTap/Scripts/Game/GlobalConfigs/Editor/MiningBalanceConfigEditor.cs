#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace WattsTap.Scripts.Game.GlobalConfigs.Editor
{
    /// <summary>
    /// Кастомный инспектор для MiningBalanceConfig с функцией импорта из CSV
    /// </summary>
    [CustomEditor(typeof(MiningBalanceConfig))]
    public class MiningBalanceConfigEditor : UnityEditor.Editor
    {
        private string csvPath = "Assets/CvsConfigs/WattsBalanceMining.csv";
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            MiningBalanceConfig config = (MiningBalanceConfig)target;
            
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("CSV Import Tools", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Импорт данных из WattsBalanceMining.csv. " +
                "Это перезапишет все текущие значения в конфигурации.", 
                MessageType.Info);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CSV File Path:", GUILayout.Width(100));
            csvPath = EditorGUILayout.TextField(csvPath);
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Import from CSV", GUILayout.Height(30)))
            {
                ImportFromCSV(config);
            }
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Validate Configuration", GUILayout.Height(25)))
            {
                ValidateConfig(config);
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
            
            if (config.dailyProgression != null && config.dailyProgression.Length > 0)
            {
                EditorGUILayout.LabelField($"Days configured: {config.dailyProgression.Length}");
                
                // Показываем статистику последнего дня
                var lastDay = config.dailyProgression[config.dailyProgression.Length - 1];
                EditorGUILayout.LabelField($"Total profit (Day 30): {lastDay.cumulativeProfitCoins:F1}");
                EditorGUILayout.LabelField($"Total exp (Day 30): {lastDay.cumulativeExp}");
            }
        }
        
        private void ImportFromCSV(MiningBalanceConfig config)
        {
            if (!File.Exists(csvPath))
            {
                EditorUtility.DisplayDialog(
                    "Error", 
                    $"CSV file not found at path:\n{csvPath}\n\nPlease check the path and try again.", 
                    "OK");
                return;
            }
            
            if (EditorUtility.DisplayDialog(
                "Import CSV", 
                "This will overwrite all current configuration values.\n\nAre you sure you want to continue?", 
                "Yes, Import", 
                "Cancel"))
            {
                try
                {
                    Undo.RecordObject(config, "Import CSV Data");
                    
                    string csvText = File.ReadAllText(csvPath);
                    MiningBalanceCsvConverter.ParseCsvToConfig(csvText, config);
                    
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                    
                    EditorUtility.DisplayDialog(
                        "Success", 
                        "CSV data imported successfully!\n\nCheck the console for any warnings.", 
                        "OK");
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog(
                        "Error", 
                        $"Failed to import CSV:\n\n{e.Message}", 
                        "OK");
                    Debug.LogError($"[MiningBalanceConfigEditor] Import failed: {e}");
                }
            }
        }
        
        private void ValidateConfig(MiningBalanceConfig config)
        {
            bool hasErrors = false;
            int errorCount = 0;
            int warningCount = 0;
            
            // Проверка стартовых параметров
            if (config.coinsPerTap <= 0)
            {
                Debug.LogError("[MiningBalanceConfig] Coins per tap must be greater than 0");
                hasErrors = true;
                errorCount++;
            }
            
            if (config.startCapacityHits <= 0)
            {
                Debug.LogError("[MiningBalanceConfig] Start capacity hits must be greater than 0");
                hasErrors = true;
                errorCount++;
            }
            
            if (config.cooldownPerHitSec <= 0)
            {
                Debug.LogError("[MiningBalanceConfig] Cooldown per hit must be greater than 0");
                hasErrors = true;
                errorCount++;
            }
            
            // Проверка прогрессии
            if (config.dailyProgression == null || config.dailyProgression.Length == 0)
            {
                Debug.LogWarning("[MiningBalanceConfig] Daily progression is empty");
                warningCount++;
            }
            else
            {
                for (int i = 0; i < config.dailyProgression.Length; i++)
                {
                    var day = config.dailyProgression[i];
                    if (day.tapsPerDay <= 0)
                    {
                        Debug.LogWarning($"[MiningBalanceConfig] Day {i + 1}: Taps per day is 0 or negative");
                        warningCount++;
                    }
                }
            }
            
            if (!hasErrors && warningCount == 0)
            {
                EditorUtility.DisplayDialog(
                    "Validation Success", 
                    "Configuration is valid! No errors or warnings found.", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Validation Complete", 
                    $"Validation completed with:\n\nErrors: {errorCount}\nWarnings: {warningCount}\n\nCheck the console for details.", 
                    "OK");
            }
        }
    }
}
#endif