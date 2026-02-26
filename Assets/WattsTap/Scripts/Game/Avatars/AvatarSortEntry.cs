using System;
using UnityEngine;

namespace WattsTap.Game.Avatars
{
    /// <summary>
    /// Запись для кастомной сортировки аватара в списке.
    /// </summary>
    [Serializable]
    public class AvatarSortEntry
    {
        [Tooltip("Конфиг аватара")]
        public AvatarConfig AvatarConfig;
        
        [Tooltip("Кастомный индекс для сортировки (меньше = выше в списке)")]
        public int SortIndex;
    }
}

