using System;

namespace WattsTap.Core
{
    public interface IOrientationService : IService
    {
        /// <summary>
        /// Event fired when device orientation changes.
        /// Parameter is true if landscape, false if portrait.
        /// </summary>
        event Action<bool> OnOrientationChanged;
        
        /// <summary>
        /// Returns true if device is currently in landscape orientation.
        /// </summary>
        bool IsLandscape { get; }
    }
}

