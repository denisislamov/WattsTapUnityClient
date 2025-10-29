using System;
using UnityEngine;

namespace WattsTap.Game.Tap.Services
{
    /// <summary>
    /// Обёртка для получения входных событий (тап/клик) без привязки к UI.
    /// Сервис отвечает только за обнаружение тапов и генерацию события OnTap.
    /// </summary>
    public interface IInputService : Core.IService
    {
        /// <summary>
        /// Событие на тап (или клик). Параметр - позиция в экранных пикселях.
        /// </summary>
        event Action<Vector2> OnTap;

        /// <summary>
        /// Обновление (должно вызываться извне в Update, если реализация не использует MonoBehaviour)
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// Опционально: вручную вызвать событие тап (полезно для тестов)
        /// </summary>
        void SimulateTap(Vector2 screenPosition);
    }
}
