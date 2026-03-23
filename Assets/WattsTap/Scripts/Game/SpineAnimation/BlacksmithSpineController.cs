using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace WattsTap.Game.SpineView
{
    /// <summary>
    /// Controls the Blacksmith Spine animation.
    /// Attach to the same GameObject that has SkeletonAnimation.
    /// 
    /// Setup in Unity Editor:
    ///   1. Create a GameObject (in Canvas or world-space).
    ///   2. Add SkeletonGraphic (for Canvas UI) — it will auto-add SkeletonAnimation.
    ///      Or add SkeletonAnimation directly for world-space.
    ///   3. Drag Blacksmith__SkeletonData asset into the "Skeleton Data Asset" field.
    ///   4. Add this component (BlacksmithSpineController).
    /// </summary>
    public class BlacksmithSpineController : MonoBehaviour
    {
        [Header("Animation Names")]
        [SpineAnimation] [SerializeField] private string _idleAnimation = "Idle";
        [SpineAnimation] [SerializeField] private string _hitAnimation = "Hit";

        [Header("Skin")]
        [SpineSkin] [SerializeField] private string _defaultSkin = "default";

        [Header("Settings")]
        [SerializeField] private bool _playIdleOnStart = true;
        [SerializeField] private bool _loopIdle = true;

        private SkeletonAnimation _skeletonAnimation;

        public Skeleton Skeleton => _skeletonAnimation?.Skeleton;
        public Spine.AnimationState AnimationState => _skeletonAnimation?.AnimationState;

        public event Action OnHitAnimationComplete;

        private void Awake()
        {
            _skeletonAnimation = GetComponent<SkeletonAnimation>();

            if (_skeletonAnimation == null)
            {
                Debug.LogError("[BlacksmithSpineController] No SkeletonAnimation found on this GameObject!", this);
            }
        }

        private void Start()
        {
            SetSkin(_defaultSkin);

            if (_playIdleOnStart)
            {
                PlayIdle();
            }
        }

        /// <summary>
        /// Play the Idle animation (looping by default).
        /// </summary>
        [Button]
        public void PlayIdle()
        {
            var state = AnimationState;
            if (state == null) return;

            state.SetAnimation(0, _idleAnimation, _loopIdle);
        }

        /// <summary>
        /// Play the Hit animation once, then return to Idle.
        /// </summary>
        [Button]
        public void PlayHit()
        {
            var state = AnimationState;
            if (state == null) return;

            var entry = state.SetAnimation(0, _hitAnimation, false);
            entry.Complete += _ =>
            {
                OnHitAnimationComplete?.Invoke();
                PlayIdle();
            };
        }

        /// <summary>
        /// Play the Hit animation once and queue Idle to continue afterwards.
        /// Uses Spine's built-in queue instead of a callback.
        /// </summary>
        public void PlayHitQueued()
        {
            var state = AnimationState;
            if (state == null) return;

            state.SetAnimation(0, _hitAnimation, false);
            state.AddAnimation(0, _idleAnimation, _loopIdle, 0f);
        }

        /// <summary>
        /// Switch the skin at runtime.
        /// Available skins: "default", "skin 1", "skin 2", "skin 3".
        /// </summary>
        public void SetSkin(string skinName)
        {
            var skeleton = Skeleton;
            if (skeleton == null) return;

            var skin = skeleton.Data.FindSkin(skinName);
            if (skin == null)
            {
                Debug.LogWarning($"[BlacksmithSpineController] Skin '{skinName}' not found!", this);
                return;
            }

            skeleton.SetSkin(skin);
            skeleton.SetupPoseSlots();
        }

        /// <summary>
        /// Play any animation by name.
        /// </summary>
        public TrackEntry PlayAnimation(string animationName, bool loop, int trackIndex = 0)
        {
            var state = AnimationState;
            if (state == null) return null;

            return state.SetAnimation(trackIndex, animationName, loop);
        }

        /// <summary>
        /// Queue an animation to play after the current one on a given track.
        /// </summary>
        public TrackEntry QueueAnimation(string animationName, bool loop, float delay = 0f, int trackIndex = 0)
        {
            var state = AnimationState;
            if (state == null) return null;

            return state.AddAnimation(trackIndex, animationName, loop, delay);
        }
    }
}
