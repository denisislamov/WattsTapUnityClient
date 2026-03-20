using System;

namespace WattsTap.Core.API
{
    #region Tap Progress (NEW server)

    /// <summary>
    /// Request to send taps to the new server.
    /// Replaces SaveProgressRequest for the new API.
    /// </summary>
    [Serializable]
    public class TapProgressRequest
    {
        public int tapCount;
    }

    /// <summary>
    /// Full player state from the new server.
    /// Returned inside GET /progress and POST /progress responses.
    /// </summary>
    [Serializable]
    public class PlayerStateDTO
    {
        public string id;
        public string userId;
        public string gameConfigId;
        public int level;
        public long experience;
        public long coins;
        public int energy;
        public string energyUpdatedAt;
        public string createdAt;
        public string updatedAt;
    }

    /// <summary>
    /// Tap conversion info from boosters.
    /// </summary>
    [Serializable]
    public class TapConversionDTO
    {
        public int validatedTapCount;
        public int effectiveTapCount;
        public int energyTapCostCount;
        public int baseAppliedCount;
    }

    /// <summary>
    /// Boosters state returned from progress endpoints.
    /// </summary>
    [Serializable]
    public class BoostersDTO
    {
        public string tapMode;
        public TapConversionDTO tapConversion;
    }

    /// <summary>
    /// Tap result details from POST /progress response.
    /// </summary>
    [Serializable]
    public class TapResultDTO
    {
        public int energyBefore;
        public int energyAfter;
        public int xpEarned;
        public int coinsEarned;
        public int appliedTapCount;
        public TapMetaDTO tapMeta;
    }

    /// <summary>
    /// Meta information about the tap operation.
    /// </summary>
    [Serializable]
    public class TapMetaDTO
    {
        public int requestedTapCount;
        public int appliedTapCount;
        /// <summary>If > 0, taps were dropped because player has no energy.</summary>
        public int droppedTapCount;
    }

    /// <summary>
    /// Full response from POST /progress on the new server.
    /// Extends SaveProgressResponse with tap data.
    /// </summary>
    [Serializable]
    public class TapProgressResponse
    {
        public bool success;
        public string message;
        public PlayerProgressDTO progress;
        public bool isNewPlayer;
        public PlayerStateDTO playerState;
        public string nextRegenAt;
        public BoostersDTO boosters;
        public TapResultDTO tap;
    }

    /// <summary>
    /// Extended LoadProgressResponse that includes new server fields.
    /// </summary>
    [Serializable]
    public class ExtendedLoadProgressResponse
    {
        public bool success;
        public PlayerProgressDTO progress;
        public bool isNewPlayer;
        public PlayerStateDTO playerState;
        public string nextRegenAt;
        public BoostersDTO boosters;
    }

    #endregion
}

