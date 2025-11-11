using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WattsTap.Core;
using WattsTap.Game.Player;

namespace WattsTap.Game.API
{
    /// <summary>
    /// Интерфейс API сервиса для WattsTap
    /// Определяет контракт для работы с бэкендом (реальным или mock)
    /// </summary>
    public interface IWattsTapAPIService : IService
    {
        /// <summary>
        /// Аутентификация через Telegram WebApp
        /// </summary>
        /// <param name="initData">initDataRaw от Telegram WebApp SDK</param>
        /// <param name="onSuccess">Callback при успешной аутентификации</param>
        /// <param name="onError">Callback при ошибке</param>
        IEnumerator Authenticate(string initData, Action<AuthResponse> onSuccess, Action<string> onError);

        /// <summary>
        /// Отправка пакета тапов на сервер
        /// </summary>
        /// <param name="taps">Список тапов для обработки</param>
        /// <param name="onSuccess">Callback с результатом обработки</param>
        /// <param name="onError">Callback при ошибке</param>
        IEnumerator SendTapBatch(List<TapData> taps, Action<TapBatchResponse> onSuccess, Action<string> onError);

        /// <summary>
        /// Получение полных данных игрока
        /// </summary>
        /// <param name="onSuccess">Callback с данными игрока</param>
        /// <param name="onError">Callback при ошибке</param>
        IEnumerator GetPlayerData(Action<PlayerData> onSuccess, Action<string> onError);

        /// <summary>
        /// Синхронизация состояния игры (при переподключении)
        /// </summary>
        /// <param name="onSuccess">Callback с обновленным состоянием</param>
        /// <param name="onError">Callback при ошибке</param>
        IEnumerator SyncGameState(Action<SyncResponse> onSuccess, Action<string> onError);

        /// <summary>
        /// Получение оффлайн бонуса
        /// </summary>
        /// <param name="onSuccess">Callback с наградами</param>
        /// <param name="onError">Callback при ошибке</param>
        IEnumerator ClaimOfflineBonus(Action<OfflineRewards> onSuccess, Action<string> onError);
    }

    #region DTO Classes

    // Authentication
    [Serializable]
    public class AuthResponse
    {
        public string token;
        public int expiresIn;
        public PlayerInfo player;
    }

    [Serializable]
    public class PlayerInfo
    {
        public string playerId;
        public string nickname;
        public int level;
        public bool isNewPlayer;
    }

    // Tap System
    [Serializable]
    public class TapData
    {
        public long clientTimestamp;
        public Vector2Data screenPosition;
    }

    [Serializable]
    public class Vector2Data
    {
        public float x;
        public float y;

        public Vector2Data() { }

        public Vector2Data(Vector2 vec)
        {
            x = vec.x;
            y = vec.y;
        }
    }

    [Serializable]
    public class TapBatchResponse
    {
        public int validTapsCount;
        public int invalidTapsCount;
        public long wattsEarned;
        public long xpEarned;
        public PlayerResources resources;
        public List<TapEffect> effects;
        public LevelUpInfo levelUp;
    }

    [Serializable]
    public class TapEffect
    {
        public string type; // tap_success, combo, critical, level_up
        public Vector2Data position;
        public float value;
        public float multiplier;
    }

    [Serializable]
    public class LevelUpInfo
    {
        public int newLevel;
        public Rewards rewards;
    }

    // Sync
    [Serializable]
    public class SyncResponse
    {
        public PlayerResources resources;
        public OfflineRewards offlineRewards;
        public int hitsRecovered;
    }

    // Rewards
    [Serializable]
    public class OfflineRewards
    {
        public long wattsEarned;
        public int offlineDuration;
        public float multiplier;
        public int maxDuration;
    }

    [Serializable]
    public class Rewards
    {
        public long watts;
        public long xp;
        public string kiloWattTokens;
        public List<InventoryItem> items;
    }

    #endregion
}

