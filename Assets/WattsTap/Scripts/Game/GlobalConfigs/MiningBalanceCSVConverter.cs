using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WattsTap.Scripts.Game.GlobalConfigs
{
    /// <summary>
    /// Утилита для конвертации данных из WattsBalanceMining.csv в MiningBalanceConfig
    /// </summary>
    public static class MiningBalanceCsvConverter
    {
        private const char CsvSeparator = ',';
        
        /// <summary>
        /// Парсит CSV файл и заполняет конфигурацию
        /// </summary>
        /// <param name="csvText">Текст CSV файла</param>
        /// <param name="config">Конфигурация для заполнения</param>
        public static void ParseCsvToConfig(string csvText, MiningBalanceConfig config)
        {
            if (string.IsNullOrEmpty(csvText))
            {
                Debug.LogError("[MiningBalanceCsvConverter] CSV text is null or empty");
                return;
            }

            if (config == null)
            {
                Debug.LogError("[MiningBalanceCsvConverter] Config is null");
                return;
            }

            try
            {
                var lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Парсим стартовые параметры
                ParseStartParameters(lines, config);
                
                // Парсим данные прогрессии по дням
                ParseDailyProgression(lines, config);
                
                Debug.Log("[MiningBalanceCsvConverter] Successfully parsed CSV data");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MiningBalanceCsvConverter] Error parsing CSV: {e.Message}\n{e.StackTrace}");
            }
        }
        
        /// <summary>
        /// Парсит стартовые параметры из CSV
        /// </summary>
        private static void ParseStartParameters(string[] lines, MiningBalanceConfig config)
        {
            // Карта параметров: название в CSV -> действие по установке значения
            var parameterMap = new Dictionary<string, Action<string>>
            {
                { "Coins Per tap", value => config.coinsPerTap = ParseInt(value, 1) },
                { "Exp per Tap", value => config.expPerTap = ParseInt(value, 1) },
                { "Start Capacity Hits", value => config.startCapacityHits = ParseInt(value, 1500) },
                { "Coldown 1hits  sec", value => config.cooldownPerHitSec = ParseFloat(value, 2f) },
                { "Crit multiplier", value => config.critMultiplier = ParseFloat(value, 1.2f) },
                { "Chance Crit %", value => config.chanceCritPercent = ParseFloat(value, 1f) },
                { "Avg Playtime (mins)", value => config.avgPlaytimeMinutes = ParseInt(value, 15) },
                { "Taps per second", value => config.tapsPerSecond = ParseInt(value, 10) },
                { "Profit per Hour", value => config.profitPerHour = ParseInt(value, 500) },
                { "Max Hours offline", value => config.maxHoursOffline = ParseInt(value, 3) },
                { "Sessions per day", value => config.sessionsPerDay = ParseInt(value, 2) }
            };

            foreach (var line in lines)
            {
                var parts = SplitCsvLine(line);
                if (parts.Length < 2) continue;

                var paramName = parts[0].Trim();
                var paramValue = parts[1].Trim();

                if (parameterMap.TryGetValue(paramName, out var setter))
                {
                    setter(paramValue);
                }
            }
        }
        
        /// <summary>
        /// Парсит данные прогрессии по дням из CSV
        /// </summary>
        private static void ParseDailyProgression(string[] lines, MiningBalanceConfig config)
        {
            // Находим строку с заголовками дней
            int headerIndex = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("Day 1") && lines[i].Contains("Day 2"))
                {
                    headerIndex = i;
                    break;
                }
            }

            if (headerIndex == -1)
            {
                Debug.LogWarning("[MiningBalanceCsvConverter] Daily progression header not found");
                return;
            }

            // Инициализируем массив для 30 дней
            config.dailyProgression = new DailyProgressionData[30];
            for (int i = 0; i < 30; i++)
            {
                config.dailyProgression[i] = new DailyProgressionData { day = i + 1 };
            }

            // Парсим строки данных после заголовка
            var dataRows = new Dictionary<string, int>
            {
                { "Playtime (sec)", -1 },
                { "Taps per session", -1 },
                { "Taps per day", -1 },
                { "Exp per day", -1 },
                { "Coins from taps", -1 },
                { "Coins from offline bonus", -1 },
                { "Profit Coins", -1 },
                { "Cumulative profit coins", -1 },
                { "Cumulative exp", -1 }
            };

            // Находим индексы строк данных
            for (int i = headerIndex + 1; i < lines.Length && i < headerIndex + 15; i++)
            {
                var parts = SplitCsvLine(lines[i]);
                if (parts.Length < 2) continue;

                var rowName = parts[0].Trim();
                foreach (var key in dataRows.Keys.ToList())
                {
                    if (rowName == key)
                    {
                        dataRows[key] = i;
                        break;
                    }
                }
            }

            // Заполняем данные для каждого дня
            for (int day = 0; day < 30; day++)
            {
                int columnIndex = day + 1; // +1 потому что первая колонка - название строки
                
                if (dataRows["Playtime (sec)"] != -1)
                {
                    var parts = SplitCsvLine(lines[dataRows["Playtime (sec)"]]);
                    if (columnIndex < parts.Length)
                        config.dailyProgression[day].playtimeSec = ParseInt(parts[columnIndex], 900);
                }
                
                if (dataRows["Taps per session"] != -1)
                {
                    var parts = SplitCsvLine(lines[dataRows["Taps per session"]]);
                    if (columnIndex < parts.Length)
                        config.dailyProgression[day].tapsPerSession = ParseInt(parts[columnIndex], 1950);
                }
                
                if (dataRows["Taps per day"] != -1)
                {
                    var parts = SplitCsvLine(lines[dataRows["Taps per day"]]);
                    if (columnIndex < parts.Length)
                        config.dailyProgression[day].tapsPerDay = ParseInt(parts[columnIndex], 3900);
                }
                
                if (dataRows["Exp per day"] != -1)
                {
                    var parts = SplitCsvLine(lines[dataRows["Exp per day"]]);
                    if (columnIndex < parts.Length)
                        config.dailyProgression[day].expPerDay = ParseLong(parts[columnIndex], 3900);
                }
                
                if (dataRows["Coins from taps"] != -1)
                {
                    var parts = SplitCsvLine(lines[dataRows["Coins from taps"]]);
                    if (columnIndex < parts.Length)
                        config.dailyProgression[day].coinsFromTaps = ParseFloat(parts[columnIndex], 3946.8f);
                }
                
                if (dataRows["Coins from offline bonus"] != -1)
                {
                    var parts = SplitCsvLine(lines[dataRows["Coins from offline bonus"]]);
                    if (columnIndex < parts.Length)
                        config.dailyProgression[day].coinsFromOfflineBonus = ParseFloat(parts[columnIndex], 3000f);
                }
                
                if (dataRows["Profit Coins"] != -1)
                {
                    var parts = SplitCsvLine(lines[dataRows["Profit Coins"]]);
                    if (columnIndex < parts.Length)
                        config.dailyProgression[day].profitCoins = ParseFloat(parts[columnIndex], 6946.8f);
                }
                
                if (dataRows["Cumulative profit coins"] != -1)
                {
                    var parts = SplitCsvLine(lines[dataRows["Cumulative profit coins"]]);
                    if (columnIndex < parts.Length)
                        config.dailyProgression[day].cumulativeProfitCoins = ParseFloat(parts[columnIndex], 6946.8f);
                }
                
                if (dataRows["Cumulative exp"] != -1)
                {
                    var parts = SplitCsvLine(lines[dataRows["Cumulative exp"]]);
                    if (columnIndex < parts.Length)
                        config.dailyProgression[day].cumulativeExp = ParseLong(parts[columnIndex], 3900);
                }
            }
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
        
        /// <summary>
        /// Загружает CSV файл из ресурсов и конвертирует в конфигурацию
        /// </summary>
        public static void LoadFromCsvFile(string csvFilePath, MiningBalanceConfig config)
        {
            try
            {
                if (!File.Exists(csvFilePath))
                {
                    Debug.LogError($"[MiningBalanceCsvConverter] CSV file not found: {csvFilePath}");
                    return;
                }

                string csvText = File.ReadAllText(csvFilePath);
                ParseCsvToConfig(csvText, config);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MiningBalanceCsvConverter] Error loading CSV file: {e.Message}");
            }
        }
    }
}
