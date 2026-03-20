using System;
using System.Collections;
using UnityEngine;
using WattsTap.Core.API;

namespace WattsTap.Scripts.Game.GlobalConfigs
{
    /// <summary>
    /// Loads MiningBalanceConfig data from the remote server.
    /// If the server is available, overwrites the local ScriptableObject fields with server data.
    /// If the server is unavailable, the local (CSV-imported) defaults remain in use.
    /// 
    /// Usage:
    ///   Call LoadFromServer() as a coroutine during game initialization, 
    ///   BEFORE services like PlayerService and TapControllerService access the config.
    /// </summary>
    public class MiningBalanceRemoteLoader
    {
        private const string Tag = "[MiningBalance]";
        private const string ColorServer = "#00FF00";
        private const string ColorLocal = "#FFFF00";
        private const string ColorError = "#FF6600";
        
        private readonly MiningBalanceConfig _config;
        private readonly IReferralAPIService _apiService;
        private readonly ICoreServerService _coreService;

        /// <summary>Whether the last load attempt succeeded from server.</summary>
        public bool LoadedFromServer { get; private set; }

        /// <summary>Whether the load operation has completed (success or failure).</summary>
        public bool IsLoadComplete { get; private set; }
        
        /// <summary>Source description for logging.</summary>
        public string Source => LoadedFromServer ? "SERVER" : "LOCAL (client)";

        /// <summary>Constructor for legacy IReferralAPIService.</summary>
        public MiningBalanceRemoteLoader(MiningBalanceConfig config, IReferralAPIService apiService)
        {
            _config = config;
            _apiService = apiService;
        }
        
        /// <summary>Constructor for new ICoreServerService.</summary>
        public MiningBalanceRemoteLoader(MiningBalanceConfig config, ICoreServerService coreService)
        {
            _config = config;
            _coreService = coreService;
        }

        /// <summary>
        /// Attempt to load mining balance from the remote server.
        /// Falls back silently to local config on any error.
        /// </summary>
        public IEnumerator LoadFromServer(Action<bool> onComplete = null)
        {
            IsLoadComplete = false;
            LoadedFromServer = false;
            
            Debug.Log($"<color={ColorServer}>{Tag} ========== MINING BALANCE LOAD START ==========</color>");
            LogCurrentLocalValues("BEFORE (local defaults)");

            if (_config == null)
            {
                Debug.LogWarning($"{Tag} Config is null — using local defaults");
                LogFinalSource(false);
                IsLoadComplete = true;
                onComplete?.Invoke(false);
                yield break;
            }

            if (_apiService == null && _coreService == null)
            {
                Debug.LogWarning($"{Tag} API service not available — using local defaults");
                LogFinalSource(false);
                IsLoadComplete = true;
                onComplete?.Invoke(false);
                yield break;
            }

            bool requestDone = false;
            bool requestSuccess = false;

            System.Action<MiningBalancePublicResponse> onSuccess = response =>
            {
                try
                {
                    ApplyServerData(response.balance);
                    LoadedFromServer = true;
                    requestSuccess = true;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"<color={ColorError}>{Tag} Failed to apply server data: {ex.Message}</color>");
                }
                requestDone = true;
            };
            
            System.Action<string> onError = error =>
            {
                Debug.LogWarning($"<color={ColorError}>{Tag} Server request failed: {error}</color>");
                requestDone = true;
            };

            if (_coreService != null)
                yield return _coreService.LoadMiningBalance(onSuccess, onError);
            else
                yield return _apiService.LoadMiningBalance(onSuccess, onError);

            while (!requestDone)
            {
                yield return null;
            }
            
            LogFinalSource(requestSuccess);
            LogCurrentLocalValues("ACTIVE VALUES");
            Debug.Log($"<color={ColorServer}>{Tag} ========== MINING BALANCE LOAD END ==========</color>");

            IsLoadComplete = true;
            onComplete?.Invoke(requestSuccess);
        }
        
        private void LogFinalSource(bool fromServer)
        {
            if (fromServer)
            {
                Debug.Log($"<color={ColorServer}>{Tag} >>> SOURCE: SERVER (remote) <<<</color>");
                Debug.Log($"<color={ColorServer}>{Tag} Balance loaded from remote /balance/mining endpoint</color>");
            }
            else
            {
                Debug.Log($"<color={ColorLocal}>{Tag} >>> SOURCE: LOCAL (client ScriptableObject) <<<</color>");
                Debug.Log($"<color={ColorLocal}>{Tag} Server was unavailable — using local CSV-imported defaults</color>");
            }
        }
        
        private void LogCurrentLocalValues(string header)
        {
            if (_config == null) return;

            var c = _config;
            Debug.Log($"<color=#00FFFF>{Tag} --- {header} ---</color>");
            Debug.Log($"<color=#00FFFF>{Tag}   coinsPerTap={c.coinsPerTap}, expPerTap={c.expPerTap}, energyCost={c.energyCostPerTap}</color>");
            Debug.Log($"<color=#00FFFF>{Tag}   startCapacityHits={c.startCapacityHits}, cooldown={c.cooldownPerHitSec}s</color>");
            Debug.Log($"<color=#00FFFF>{Tag}   critMultiplier={c.critMultiplier}, chanceCrit={c.chanceCritPercent}%</color>");
            Debug.Log($"<color=#00FFFF>{Tag}   profitPerHour={c.profitPerHour}, maxHoursOffline={c.maxHoursOffline}</color>");
            Debug.Log($"<color=#00FFFF>{Tag}   tapsPerSecond={c.tapsPerSecond}, sessionsPerDay={c.sessionsPerDay}</color>");
            Debug.Log($"<color=#00FFFF>{Tag}   dailyProgression days={c.dailyProgression?.Length ?? 0}</color>");
        }

        /// <summary>
        /// Apply server data to the local MiningBalanceConfig ScriptableObject.
        /// </summary>
        private void ApplyServerData(MiningBalanceDTO data)
        {
            if (data == null)
            {
                Debug.LogWarning($"{Tag} Server data is null");
                return;
            }

            _config.coinsPerTap = data.coinsPerTap;
            _config.expPerTap = data.expPerTap;
            _config.energyCostPerTap = data.energyCostPerTap;
            _config.startCapacityHits = data.startCapacityHits;
            _config.cooldownPerHitSec = data.cooldownPerHitSec;
            _config.critMultiplier = data.critMultiplier;
            _config.chanceCritPercent = data.chanceCritPercent;
            _config.avgPlaytimeMinutes = data.avgPlaytimeMinutes;
            _config.tapsPerSecond = data.tapsPerSecond;
            _config.profitPerHour = data.profitPerHour;
            _config.maxHoursOffline = data.maxHoursOffline;
            _config.sessionsPerDay = data.sessionsPerDay;

            if (data.dailyProgression != null && data.dailyProgression.Count > 0)
            {
                _config.dailyProgression = new DailyProgressionData[data.dailyProgression.Count];

                for (int i = 0; i < data.dailyProgression.Count; i++)
                {
                    var src = data.dailyProgression[i];
                    _config.dailyProgression[i] = new DailyProgressionData
                    {
                        day = src.day,
                        playtimeSec = src.playtimeSec,
                        tapsPerSession = src.tapsPerSession,
                        tapsPerDay = src.tapsPerDay,
                        expPerDay = src.expPerDay,
                        coinsFromTaps = src.coinsFromTaps,
                        coinsFromOfflineBonus = src.coinsFromOfflineBonus,
                        profitCoins = src.profitCoins,
                        cumulativeProfitCoins = src.cumulativeProfitCoins,
                        cumulativeExp = src.cumulativeExp,
                    };
                }
            }
        }
    }
}
