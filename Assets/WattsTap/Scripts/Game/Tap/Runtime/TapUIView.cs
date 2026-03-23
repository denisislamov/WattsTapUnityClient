using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.GameLoop;
using WattsTap.Game.SpineView;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Scripts.Game.Tap.Runtime
{
    public class TapUIView : MonoBehaviour, IUpdatable
    {
        [Header("Spine Animation")]
        [SerializeField] private BlacksmithSpineController spineController;
        
        [Header("Hit Particles")]
        [SerializeField] private ParticleSystem hitParticles;
        
        [Header("Old Effects (will be disabled)")]
        [Tooltip("Old coin particles — will be disabled on Start")]
        [SerializeField] private GameObject[] oldEffectsToDisable;

        [Header("Legacy (unused)")]
        [SerializeField] private Animator animator;

        private ITapControllerService _tapControllerService;
        private IUpdateService _updateService;
        
        private bool _isTapping;
        private float _tapCooldown;
        private const float TapCooldownDuration = 0.5f;
        
        private ParticleSystem[] _cachedHitParticles;

        private void Start()
        {
            // Disable old coin particle effects
            if (oldEffectsToDisable != null)
            {
                foreach (var obj in oldEffectsToDisable)
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }
            
            if (hitParticles != null)
                _cachedHitParticles = hitParticles.GetComponentsInChildren<ParticleSystem>(true);
            
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
                    // Spine controller auto-returns to Idle after Hit, no extra action needed
                }
            }
        }

        private void OnTap(Vector2 screenPos, int coinsEarned)
        {
            _isTapping = true;
            _tapCooldown = TapCooldownDuration;
            
            // Play Spine Hit animation (auto-returns to Idle on complete)
            if (spineController != null)
            {
                spineController.PlayHit();
            }
            
            // Play hit particle effect
            PlayHitParticles();
        }

        private void PlayHitParticles()
        {
            if (_cachedHitParticles == null) return;
            
            foreach (var ps in _cachedHitParticles)
            {
                ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play(false);
            }
        }
    }
}
