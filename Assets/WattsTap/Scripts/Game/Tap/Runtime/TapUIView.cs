using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.GameLoop;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Scripts.Game.Tap.Runtime
{
    public class TapUIView : MonoBehaviour, IUpdatable
    {
        [SerializeField] private Animator animator;
        
        private static readonly int TapHash = Animator.StringToHash("Tap");

        private ITapControllerService _tapControllerService;
        private IUpdateService _updateService;
        
        private bool _isTapping;
        private float _tapCooldown;
        private const float TapCooldownDuration = 0.5f;

        private void Start()
        {
            _tapControllerService = ServiceLocator.Get<ITapControllerService>();
            
            if (_tapControllerService != null)
            {
                _tapControllerService.OnTapPerformedWithPosition += OnTap;
            }
        }

        private void OnDestroy()
        {
            if (_tapControllerService != null)
            {
                _tapControllerService.OnTapPerformedWithPosition -= OnTap;
            }
        }

        private void OnEnable()
        {
            _updateService = ServiceLocator.Get<IUpdateService>();
            _updateService?.Register(this);
        }

        private void OnDisable()
        {
            _updateService?.Unregister(this);
        }

        public void OnUpdate()
        {
            if (_tapCooldown > 0)
            {
                _tapCooldown -= Time.deltaTime;
                
                if (_tapCooldown <= 0 && _isTapping)
                {
                    Debug.Log($"[TapUIView] Stopping animation (SetBool false)");
                    _isTapping = false;
                    SetTapAnimation(false);
                }
            }
        }

        private void OnTap(Vector2 screenPos, int i)
        {
            // Only start animation if not already tapping
            // This prevents animation from restarting on each tap
            bool wasAlreadyTapping = _isTapping;
            
            Debug.Log($"[TapUIView] OnTap called. wasAlreadyTapping={wasAlreadyTapping}, instanceId={GetInstanceID()}");
            
            _isTapping = true;
            _tapCooldown = TapCooldownDuration;
            
            // Only set animation to true when starting a new tap sequence
            // Subsequent taps just reset the cooldown without restarting animation
            if (!wasAlreadyTapping)
            {
                Debug.Log($"[TapUIView] Starting animation (SetBool true)");
                SetTapAnimation(true);
            }
        }

        private void SetTapAnimation(bool isTapping)
        {
            if (animator != null)
            {
                animator.SetBool(TapHash, isTapping);
            }
        }
    }
}
