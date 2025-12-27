using UnityEngine;

namespace WattsTap.Game.UI
{
    /// <summary>
    /// Animates a tap hint with smooth zoom in/out and subtle vertical movement
    /// </summary>
    public class TapHintAnimator : MonoBehaviour
    {
        [Header("Scale Animation")]
        [SerializeField] private float _minScale = 0.9f;
        [SerializeField] private float _maxScale = 1.1f;
        [SerializeField] private float _scaleSpeed = 2f;
        
        [Header("Vertical Movement")]
        [SerializeField] private float _verticalOffset = 5f;
        [SerializeField] private float _verticalSpeed = 1.5f;

        private Vector3 _initialPosition;
        private Vector3 _initialScale;
        private float _scaleTime;
        private float _verticalTime;

        private void Awake()
        {
            _initialPosition = transform.localPosition;
            _initialScale = transform.localScale;
        }

        private void OnEnable()
        {
            // Reset animation state when re-enabled
            _scaleTime = 0f;
            _verticalTime = 0f;
            transform.localPosition = _initialPosition;
            transform.localScale = _initialScale;
        }

        private void Update()
        {
            AnimateScale();
            AnimateVerticalMovement();
        }

        private void AnimateScale()
        {
            _scaleTime += Time.deltaTime * _scaleSpeed;
            
            // Smooth ping-pong between min and max scale using sine wave
            float t = (Mathf.Sin(_scaleTime) + 1f) * 0.5f; // 0 to 1
            float scale = Mathf.Lerp(_minScale, _maxScale, t);
            
            transform.localScale = _initialScale * scale;
        }

        private void AnimateVerticalMovement()
        {
            _verticalTime += Time.deltaTime * _verticalSpeed;
            
            // Subtle up-down movement using sine wave
            float offset = Mathf.Sin(_verticalTime) * _verticalOffset;
            
            transform.localPosition = _initialPosition + new Vector3(0f, offset, 0f);
        }
    }
}

