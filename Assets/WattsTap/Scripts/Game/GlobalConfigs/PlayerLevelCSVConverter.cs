using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace WattsTap.Scripts.Game.GlobalConfigs
{
    /// <summary>
    /// Утилита для конвертации данных из WattsBalanceLevelupProfile.csv в PlayerLevelConfig
    /// </summary>
    public static class PlayerLevelCsvConverter
    {
        private const char CsvSeparator = ',';
        
        /// <summary>
        /// Парсит CSV файл и заполняет конфигурацию
        /// </summary>
        /// <param name="csvText">Текст CSV файла</param>
        /// <param name="config">Конфигурация для заполнения</param>
        public static void ParseCsvToConfig(string csvText, PlayerLevelConfig config)
        {
            if (string.IsNullOrEmpty(csvText))
            {
                Debug.LogError("[PlayerLevelCsvConverter] CSV text is null or empty");
                return;
            }

            if (config == null)
            {
                Debug.LogError("[PlayerLevelCsvConverter] Config is null");
                return;
            }

            try
            {
                var lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Парсим данные уровней
                ParseLevelData(lines, config);
                
                Debug.Log("[PlayerLevelCsvConverter] Successfully parsed CSV data");
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerLevelCsvConverter] Error parsing CSV: {e.Message}\n{e.StackTrace}");
            }
        }
        
        /// <summary>
        /// Парсит данные уровней из CSV
        /// </summary>
        private static void ParseLevelData(string[] lines, PlayerLevelConfig config)
        {
            // Находим заголовок таблицы
            int headerIndex = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("LEVEL #") && lines[i].Contains("NEED EXP"))
                {
                    headerIndex = i;
                    break;
                }
            }

            if (headerIndex == -1)
            {
                Debug.LogWarning("[PlayerLevelCsvConverter] Level data header not found");
                return;
            }

            // Парсим заголовок для определения колонок
            var headerParts = SplitCsvLine(lines[headerIndex]);
            var columnMap = new Dictionary<string, int>();
            
            for (int i = 0; i < headerParts.Length; i++)
            {
                var columnName = headerParts[i].Trim();
                if (!string.IsNullOrEmpty(columnName))
                {
                    columnMap[columnName] = i;
                }
            }

            // Подсчитываем количество уровней
            var levelDataLines = new List<string[]>();
            for (int i = headerIndex + 2; i < lines.Length; i++) // +2 чтобы пропустить строку с суммами наград
            {
                var parts = SplitCsvLine(lines[i]);
                if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                {
                    // Проверяем что это строка с уровнем
                    if (int.TryParse(parts[0].Trim(), out int level) && level > 0)
                    {
                        levelDataLines.Add(parts);
                    }
                }
            }

            // Инициализируем массив уровней
            int maxLevel = levelDataLines.Count;
            config.levels = new PlayerLevelData[maxLevel];

            // Заполняем данные для каждого уровня
            for (int i = 0; i < levelDataLines.Count; i++)
            {
                var parts = levelDataLines[i];
                var levelData = new PlayerLevelData
                {
                    level = i + 1
                };

                // Level #
                if (columnMap.TryGetValue("LEVEL #", out int levelCol) && levelCol < parts.Length)
                {
                    levelData.level = ParseInt(parts[levelCol], i + 1);
                }

                // Cof Exp
                if (columnMap.TryGetValue("Cof Exp", out int cofExpCol) && cofExpCol < parts.Length)
                {
                    levelData.expCoefficient = ParseFloat(parts[cofExpCol], 1.0f);
                }

                // NEED EXP
                if (columnMap.TryGetValue("NEED EXP", out int needExpCol) && needExpCol < parts.Length)
                {
                    levelData.needExp = ParseLong(parts[needExpCol], 0);
                }

                // SUM EXP
                if (columnMap.TryGetValue("SUM EXP", out int sumExpCol) && sumExpCol < parts.Length)
                {
                    levelData.sumExp = ParseLong(parts[sumExpCol], 0);
                }

                // Rewards
                levelData.rewards = new LevelRewards();

                // COINS
                if (columnMap.TryGetValue("COINS", out int coinsCol) && coinsCol < parts.Length)
                {
                    levelData.rewards.coins = ParseLong(parts[coinsCol], 0);
                }

                // DRAW
                if (columnMap.TryGetValue("DRAW", out int drawCol) && drawCol < parts.Length)
                {
                    levelData.rewards.draws = ParseInt(parts[drawCol], 0);
                }

                // C CHEST (Common)
                if (columnMap.TryGetValue("C CHEST", out int cChestCol) && cChestCol < parts.Length)
                {
                    levelData.rewards.commonChests = ParseInt(parts[cChestCol], 0);
                }

                // U CHEST (Uncommon)
                if (columnMap.TryGetValue("U CHEST", out int uChestCol) && uChestCol < parts.Length)
                {
                    levelData.rewards.uncommonChests = ParseInt(parts[uChestCol], 0);
                }

                // R CHEST (Rare)
                if (columnMap.TryGetValue("R CHEST", out int rChestCol) && rChestCol < parts.Length)
                {
                    levelData.rewards.rareChests = ParseInt(parts[rChestCol], 0);
                }

                // L CHEST (Legendary)
                if (columnMap.TryGetValue("L CHEST", out int lChestCol) && lChestCol < parts.Length)
                {
                    levelData.rewards.legendaryChests = ParseInt(parts[lChestCol], 0);
                }

                config.levels[i] = levelData;
            }

            Debug.Log($"[PlayerLevelCsvConverter] Parsed {config.levels.Length} levels");
        }
        
        /// <summary>
        /// Разбивает CSV строку на части, учитывая кавычки
        /// </summary>
        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            var currentField = "";
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == CsvSeparator && !inQuotes)
                {
                    result.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }
            
            result.Add(currentField);
            return result.ToArray();
        }
        
        /// <summary>
        /// Парсит целое число с обработкой ошибок
        /// </summary>
        private static int ParseInt(string value, int defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            value = value.Trim().Replace(" ", "");
            
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
                return result;

            return defaultValue;
        }
        
        /// <summary>
        /// Парсит длинное целое число с обработкой ошибок
        /// </summary>
        private static long ParseLong(string value, long defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            value = value.Trim().Replace(" ", "");
            
            if (long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out long result))
                return result;

            return defaultValue;
        }
        
        /// <summary>
        /// Парсит дробное число с обработкой ошибок и различных форматов
        /// </summary>
        private static float ParseFloat(string value, float defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            value = value.Trim().Replace(" ", "").Replace(",", ".");
            
            if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
                return result;

            return defaultValue;
        }
    }
}
