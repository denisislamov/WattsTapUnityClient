using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace WattsTap.Scripts.Game.GlobalConfigs
{
    /// <summary>
    /// Утилита для конвертации данных из WattsBalanceUpgrades.csv в UpgradesConfig
    /// </summary>
    public static class UpgradesCsvConverter
    {
        private const char CsvSeparator = ',';
        
        /// <summary>
        /// Парсит CSV файл и заполняет конфигурацию
        /// </summary>
        /// <param name="csvText">Текст CSV файла</param>
        /// <param name="config">Конфигурация для заполнения</param>
        public static void ParseCsvToConfig(string csvText, UpgradesConfig config)
        {
            if (string.IsNullOrEmpty(csvText))
            {
                Debug.LogError("[UpgradesCsvConverter] CSV text is null or empty");
                return;
            }

            if (config == null)
            {
                Debug.LogError("[UpgradesCsvConverter] Config is null");
                return;
            }

            try
            {
                var lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Парсим данные апгрейдов
                ParseUpgradeData(lines, config);
                
                Debug.Log("[UpgradesCsvConverter] Successfully parsed CSV data");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UpgradesCsvConverter] Error parsing CSV: {e.Message}\n{e.StackTrace}");
            }
        }
        
        /// <summary>
        /// Парсит данные апгрейдов из CSV
        /// </summary>
        private static void ParseUpgradeData(string[] lines, UpgradesConfig config)
        {
            if (lines.Length < 8)
            {
                Debug.LogError("[UpgradesCsvConverter] CSV file has too few lines");
                return;
            }
            
            // Строка 2 (индекс 1): SKILL TITTLE - названия скиллов
            var titleLine = SplitCsvLine(lines[1]);
            
            // Строка 3 (индекс 2): DESCRIPTION - описания
            var descriptionLine = SplitCsvLine(lines[2]);
            
            // Строка 5 (индекс 4): PARAMETRS AND PRICE - заголовок
            // Строка 6 (индекс 5): step_cost, step_parameter - множители
            var stepLine = SplitCsvLine(lines[5]);
            
            // Строка 7 (индекс 6): start_cost, start_parameter - стартовые значения
            var startLine = SplitCsvLine(lines[6]);
            
            // Строка 8 (индекс 7): LEVEL, PRICE, PARAMETR... - заголовок данных
            // Строки 9+ (индекс 8+): данные уровней
            
            // Определяем количество скиллов (каждые 2 колонки = 1 скилл: PRICE, PARAMETR)
            // Первая колонка - LEVEL, остальные парами
            int numSkills = (titleLine.Length - 1) / 2;
            
            // Создаем список скиллов
            config.skills = new List<UpgradeSkillData>();
            
            // Маппинг названий на типы апгрейдов
            var upgradeTypeMap = new Dictionary<string, UpgradeType>
            {
                { "GOLD HAMMER", UpgradeType.GoldHammer },
                { "FAST TIME", UpgradeType.FastTime },
                { "ENDURANCE", UpgradeType.Endurance },
                { "CRITICAL CHANCE", UpgradeType.CriticalChance },
                { "CRITICAL MULTIPLIER", UpgradeType.CriticalMultiplier },
                { "WORK EXPERIENCE", UpgradeType.WorkExperience },
                { "ECONOMIST", UpgradeType.Economist },
                { "STRONG FRIENDSHIP", UpgradeType.StrongFriendship },
                { "INVESTOR", UpgradeType.Investor },
                { "ITEM MASTER", UpgradeType.ItemMaster },
                { "SHARE PROFIT", UpgradeType.ShareProfit },
                { "GOLD FRIENDS", UpgradeType.GoldFriends }
            };
            
            // Парсим каждый скилл
            for (int skillIndex = 0; skillIndex < numSkills; skillIndex++)
            {
                int priceCol = 1 + skillIndex * 2; // Колонка с ценой
                int paramCol = priceCol + 1;        // Колонка с параметром
                
                // Получаем название скилла
                string title = titleLine.Length > priceCol ? titleLine[priceCol].Trim() : "";
                if (string.IsNullOrEmpty(title))
                {
                    Debug.LogWarning($"[UpgradesCsvConverter] Empty skill title at index {skillIndex}, skipping");
                    continue;
                }
                
                // Определяем тип апгрейда
                UpgradeType upgradeType = UpgradeType.GoldHammer;
                if (upgradeTypeMap.ContainsKey(title.ToUpper()))
                {
                    upgradeType = upgradeTypeMap[title.ToUpper()];
                }
                else
                {
                    Debug.LogWarning($"[UpgradesCsvConverter] Unknown upgrade type: {title}");
                }
                
                var skill = new UpgradeSkillData
                {
                    upgradeType = upgradeType,
                    title = title,
                    description = descriptionLine.Length > priceCol ? descriptionLine[priceCol].Trim() : "",
                    stepCostMultiplier = stepLine.Length > priceCol ? ParseFloat(stepLine[priceCol], 1.3f) : 1.3f,
                    stepParameter = stepLine.Length > paramCol ? ParseFloat(stepLine[paramCol], 0.1f) : 0.1f,
                    startCost = startLine.Length > priceCol ? ParseLong(startLine[priceCol], 1000) : 1000,
                    startParameter = startLine.Length > paramCol ? ParseFloat(startLine[paramCol], 0f) : 0f,
                    levels = new List<UpgradeLevelData>()
                };
                
                // Парсим уровни для этого скилла
                for (int lineIndex = 9; lineIndex < lines.Length; lineIndex++)
                {
                    var levelParts = SplitCsvLine(lines[lineIndex]);
                    
                    // Проверяем что это строка с данными уровня
                    if (levelParts.Length > 0 && int.TryParse(levelParts[0].Trim(), out int level) && level > 0)
                    {
                        // Проверяем что у нас есть данные для этого скилла
                        if (levelParts.Length <= paramCol)
                        {
                            break; // Данных больше нет
                        }
                        
                        // Проверяем что price не пустой (для некоторых скиллов данные кончаются раньше)
                        string priceStr = levelParts[priceCol].Trim();
                        if (string.IsNullOrEmpty(priceStr))
                        {
                            break; // Данные для этого скилла закончились
                        }
                        
                        var levelData = new UpgradeLevelData
                        {
                            level = level,
                            price = ParseLong(priceStr, 0),
                            parameter = ParseFloat(levelParts[paramCol], 0f)
                        };
                        
                        skill.levels.Add(levelData);
                    }
                }
                
                config.skills.Add(skill);
                Debug.Log($"[UpgradesCsvConverter] Parsed skill: {skill.title} with {skill.levels.Count} levels");
            }
            
            Debug.Log($"[UpgradesCsvConverter] Parsed {config.skills.Count} skills");
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

            value = value.Trim().Replace(" ", "").Replace(",", "");
            
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

            // Удаляем пробелы и заменяем запятую на пустую строку (европейский разделитель тысяч)
            value = value.Trim().Replace(" ", "").Replace(",", "");
            
            // Проверка на "Start" или другие строковые значения
            if (value.Equals("Start", StringComparison.OrdinalIgnoreCase))
                return defaultValue;
            
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

            // Заменяем запятую на точку (европейский десятичный разделитель)
            value = value.Trim().Replace(" ", "").Replace(",", ".");
            
            if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
                return result;

            return defaultValue;
        }
    }
}
