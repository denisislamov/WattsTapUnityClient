using System;

namespace WattsTap.Core.API
{
    #region Progress DTOs
    
    /// <summary>
    /// Player progress data from server
    /// </summary>
    [Serializable]
    public class PlayerProgressDTO
    {
        public int level;
        public long watts;
        public long currentXp;
        public long totalXp;
    }
    
    /// <summary>
    /// Request to save player progress
    /// </summary>
    [Serializable]
    public class SaveProgressRequest
    {
        public int level;
        public long watts;
        public long currentXp;
        public long totalXp;
    }
    
    /// <summary>
    /// Response after saving progress
    /// </summary>
    [Serializable]
    public class SaveProgressResponse
    {
        public bool success;
        public PlayerProgressDTO progress;
        public string message;
    }
    
    /// <summary>
    /// Response when loading player progress
    /// </summary>
    [Serializable]
    public class LoadProgressResponse
    {
        public bool success;
        public PlayerProgressDTO progress;
        public bool isNewPlayer;
    }
    
    /// <summary>
    /// Request to reset player progress
    /// </summary>
    [Serializable]
    public class ResetProgressRequest
    {
        public bool confirm;
    }
    
    /// <summary>
    /// Response after resetting progress
    /// </summary>
    [Serializable]
    public class ResetProgressResponse
    {
        public bool success;
        public PlayerProgressDTO progress;
        public string message;
    }
    
    #endregion
    
    #region Debug DTOs
    
    /// <summary>
    /// Request to add XP and/or watts to the player (debug)
    /// </summary>
    [Serializable]
    public class AddResourcesRequest
    {
        public int watts;
        public int xp;
    }
    
    /// <summary>
    /// Response after adding resources (debug)
    /// </summary>
    [Serializable]
    public class AddResourcesResponse
    {
        public bool success;
        public PlayerProgressDTO progress;
        public int addedWatts;
        public int addedXp;
        public string message;
    }
    
    #endregion
}

