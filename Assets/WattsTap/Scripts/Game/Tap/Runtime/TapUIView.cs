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
        private const float TapCooldownDuration = 1.0f;

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
                    _isTapping = false;
                    SetTapAnimation(false);
                }
            }
        }

        private void OnTap(Vector2 screenPos, int i)
        {
            _isTapping = true;
            _tapCooldown = TapCooldownDuration;
            SetTapAnimation(true);
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
