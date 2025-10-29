using UnityEngine;
using WattsTap.Core;
using WattsTap.Game.Tap.Services;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Game.Tap.Runtime
{
    /// <summary>
    /// Bridge MonoBehaviour: вызывает Update у InputService и TapControllerService и связывает OnTap -> HandleTap.
    /// Поместите на любой GameObject в сцене с ApplicationEntry.
    /// </summary>
    public class TapRuntimeBridge : MonoBehaviour
    {
        private IInputService _input;
        private ITapControllerService _tapController;

        void Start()
        {
            // Services should be registered by ApplicationEntry
            _input = ServiceLocator.Get<IInputService>();
            _tapController = ServiceLocator.Get<ITapControllerService>();

            if (_input != null)
            {
                _input.OnTap += OnTap;
            }
        }

        void Update()
        {
            var dt = Time.deltaTime;
            _input?.Update(dt);
            _tapController?.Update(dt);
        }

        private void OnTap(Vector2 screenPos)
        {
            _tapController?.HandleTap();
        }

        void OnDestroy()
        {
            if (_input != null)
            {
                _input.OnTap -= OnTap;
            }
        }
    }
}

