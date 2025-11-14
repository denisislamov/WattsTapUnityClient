#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace WattsTap.Scripts.Game.GlobalConfigs.Editor
{
    /// <summary>
    /// Кастомный инспектор для PlayerLevelConfig с функцией импорта из CSV
    /// </summary>
    [CustomEditor(typeof(PlayerLevelConfig))]
    public class PlayerLevelConfigEditor : UnityEditor.Editor
    {
        private string csvPath = "Assets/CvsConfigs/WattsBalanceLevelupProfile.csv";
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            PlayerLevelConfig config = (PlayerLevelConfig)target;
            
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("CSV Import Tools", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Импорт данных из WattsBalanceLevelupProfile.csv. " +
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
            
            if (config.levels != null && config.levels.Length > 0)
            {
                EditorGUILayout.LabelField($"Levels configured: {config.levels.Length}");
                
                // Показываем статистику последнего уровня
                var lastLevel = config.levels[config.levels.Length - 1];
                EditorGUILayout.LabelField($"Max level: {lastLevel.level}");
                EditorGUILayout.LabelField($"Total exp required: {lastLevel.sumExp:N0}");
                
                // Подсчитываем общие награды
                long totalCoins = 0;
                int totalDraws = 0;
                int totalChests = 0;
                
                foreach (var level in config.levels)
                {
                    if (level != null && level.rewards != null)
                    {
                        totalCoins += level.rewards.coins;
                        totalDraws += level.rewards.draws;
                        totalChests += level.rewards.GetTotalChests();
                    }
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Total Rewards:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Coins: {totalCoins:N0}");
                EditorGUILayout.LabelField($"Draws: {totalDraws}");
                EditorGUILayout.LabelField($"Chests: {totalChests}");
            }
        }
        
        private void ImportFromCSV(PlayerLevelConfig config)
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
                    PlayerLevelCsvConverter.ParseCsvToConfig(csvText, config);
                    
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                    
                    EditorUtility.DisplayDialog(
                        "Success", 
                        $"CSV data imported successfully!\n\n{config.levels.Length} levels loaded.\n\nCheck the console for any warnings.", 
                        "OK");
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog(
                        "Error", 
                        $"Failed to import CSV:\n\n{e.Message}", 
                        "OK");
                    Debug.LogError($"[PlayerLevelConfigEditor] Import failed: {e}");
                }
            }
        }
        
        private void ValidateConfig(PlayerLevelConfig config)
        {
            bool hasErrors = false;
            int errorCount = 0;
            int warningCount = 0;
            
            // Проверка наличия уровней
            if (config.levels == null || config.levels.Length == 0)
            {
                Debug.LogError("[PlayerLevelConfig] No levels configured");
                hasErrors = true;
                errorCount++;
            }
            else
            {
                // Проверка последовательности уровней
                for (int i = 0; i < config.levels.Length; i++)
                {
                    var level = config.levels[i];
                    
                    if (level == null)
                    {
                        Debug.LogError($"[PlayerLevelConfig] Level at index {i} is null");
                        hasErrors = true;
                        errorCount++;
                        continue;
                    }
                    
                    if (level.level != i + 1)
                    {
                        Debug.LogWarning($"[PlayerLevelConfig] Level mismatch at index {i}: expected {i + 1}, got {level.level}");
                        warningCount++;
                    }
                    
                    // Проверка прогрессии опыта
                    if (i > 0)
                    {
                        var prevLevel = config.levels[i - 1];
                        if (level.sumExp <= prevLevel.sumExp)
                        {
                            Debug.LogError($"[PlayerLevelConfig] Level {level.level}: Sum exp ({level.sumExp}) must be greater than previous level ({prevLevel.sumExp})");
                            hasErrors = true;
                            errorCount++;
                        }
                    }
                    
                    // Проверка наград
                    if (level.rewards == null)
                    {
                        Debug.LogWarning($"[PlayerLevelConfig] Level {level.level}: No rewards configured");
                        warningCount++;
                    }
                }
                
                // Проверка что первый уровень имеет sumExp = 0
                if (config.levels[0].sumExp != 0)
                {
                    Debug.LogWarning($"[PlayerLevelConfig] Level 1 should have sumExp = 0, got {config.levels[0].sumExp}");
                    warningCount++;
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

