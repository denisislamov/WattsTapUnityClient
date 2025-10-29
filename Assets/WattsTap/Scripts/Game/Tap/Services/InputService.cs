using System;
using UnityEngine;

namespace WattsTap.Game.Tap.Services
{
    /// <summary>
    /// Простая небезопасная реализация сервиса ввода для платформы Unity.
    /// Не зависит от UI — слушает Input.GetMouseButtonDown(0) / Touch.
    /// Этот класс не наследует MonoBehaviour — предполагается, что внешний MonoBehaviour вызывает Update(deltaTime).
    /// </summary>
    public class InputService : IInputService
    {
        public int InitializationOrder => 5;
        public bool IsInitialized { get; private set; }

        public event Action<Vector2> OnTap;

        public void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;
            Debug.Log("[InputService] Initialized");
        }

        public void Shutdown()
        {
            IsInitialized = false;
        }

        public void Update(float deltaTime)
        {
            if (!IsInitialized) return;

            // Desktop / Editor
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                var pos = UnityEngine.Input.mousePosition;
                OnTap?.Invoke(pos);
                return;
            }

            // Touch (mobile)
            if (UnityEngine.Input.touchCount > 0)
            {
                var touch = UnityEngine.Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    OnTap?.Invoke(touch.position);
                }
            }
        }

        public void SimulateTap(Vector2 screenPosition)
        {
            OnTap?.Invoke(screenPosition);
        }
    }
}
