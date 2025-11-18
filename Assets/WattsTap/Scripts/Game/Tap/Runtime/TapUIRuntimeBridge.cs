using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Scripts.Game.Tap.Runtime
{
    public class TapUIRuntimeBridge : MonoBehaviour
    {
        public GameObject targetUI;
        public GraphicRaycaster[] raycasters;

        private IInputService _input;
        private ITapControllerService _tapController;
        private EventSystem _eventSystem;

        private void Start()
        {
            _input = ServiceLocator.Get<IInputService>();
            _tapController = ServiceLocator.Get<ITapControllerService>();
            _eventSystem = EventSystem.current;

            if (_input != null)
            {
                _input.OnTap += OnTap;
            }
        }

        private void Update()
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
            if (targetUI == null)
            {
                return false;
            }

            var eventSystem = _eventSystem ?? EventSystem.current;
            if (eventSystem == null)
            {
                return false;
            }

            var eventData = new PointerEventData(eventSystem)
            {
                position = screenPos
            };

            var results = new System.Collections.Generic.List<RaycastResult>();

            if (raycasters != null && raycasters.Length > 0)
            {
                foreach (var raycaster in raycasters)
                {
                    if (raycaster == null)
                    {
                        continue;
                    }
                    raycaster.Raycast(eventData, results);
                }
            }
            else
            {
                var all = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
                foreach (var rc in all)
                {
                    if (rc == null)
                    {
                        continue;
                    }
                    rc.Raycast(eventData, results);
                }
            }

            foreach (var result in results)
            {
                if (result.gameObject == targetUI)
                {
                    return true;
                }

                if (result.gameObject.transform.IsChildOf(targetUI.transform))
                {
                    return true;
                }
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
