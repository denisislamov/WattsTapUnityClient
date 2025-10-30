using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Scripts.Game.Tap.Runtime
{
    /// <summary>
    /// Bridge MonoBehaviour: вызывает Update у InputService и TapControllerService и связывает OnTap -> HandleTap
    /// только если палец или мышь были над указанным UI-объектом.
    /// Поместите на любой GameObject в сцене с ApplicationEntry и назначьте targetUI (UI GameObject).
    /// </summary>
    public class TapUIRuntimeBridge : MonoBehaviour
    {
        [Tooltip("UI GameObject (или его дочерний объект). Тапы будут учитываться только если произошли над этим объектом.")]
        public GameObject targetUI;

        [Tooltip("Необязательные GraphicRaycasters. Если не заданы, будут найдены автоматически.")]
        public GraphicRaycaster[] raycasters;

        private IInputService _input;
        private ITapControllerService _tapController;
        private EventSystem _eventSystem;

        void Start()
        {
            // Services should be registered by ApplicationEntry
            _input = ServiceLocator.Get<IInputService>();
            _tapController = ServiceLocator.Get<ITapControllerService>();
            _eventSystem = EventSystem.current;

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
            if (IsPointerOverTarget(screenPos))
            {
                _tapController?.HandleTap();
            }
        }

        private bool IsPointerOverTarget(Vector2 screenPos)
        {
            if (targetUI == null) return false;

            // Ensure we have an EventSystem
            var es = _eventSystem ?? EventSystem.current;
            if (es == null) return false;

            var eventData = new PointerEventData(es)
            {
                position = screenPos
            };

            var results = new System.Collections.Generic.List<RaycastResult>();

            if (raycasters != null && raycasters.Length > 0)
            {
                foreach (var rc in raycasters)
                {
                    if (rc == null) continue;
                    rc.Raycast(eventData, results);
                }
            }
            else
            {
                // Find all GraphicRaycasters in the scene (canvases)
                var all = Object.FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
                foreach (var rc in all)
                {
                    if (rc == null) continue;
                    rc.Raycast(eventData, results);
                }
            }

            foreach (var r in results)
            {
                if (r.gameObject == targetUI) return true;
                if (r.gameObject.transform.IsChildOf(targetUI.transform)) return true;
            }

            return false;
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
