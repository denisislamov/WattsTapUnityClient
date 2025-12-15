using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WattsTap.Core;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Scripts.Game.UI.Components
{
    /// <summary>
    /// Компонент для отображения эффекта монетки при тапе.
    /// Монетка вылетает из места тапа и летит к целевому RectTransform, плавно исчезая.
    /// </summary>
    public class TapCoinEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform effectContainer;
        [SerializeField] private RectTransform target; // Целевой RectTransform куда летят монетки
        
        [Header("Coin Prefab")]
        [SerializeField] private GameObject coinEffectPrefab;
        
        [Header("Animation Settings")]
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private float randomOffsetX = 30f;
        [SerializeField] private float randomOffsetY = 20f;
        [SerializeField] private float curveHeight = 50f; // Высота дуги полёта
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0.7f, 1, 1, 0); // Начинает исчезать ближе к концу
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.5f, 0.3f, 1f);
        
        [Header("Pool Settings")]
        [SerializeField] private int poolSize = 10;
        
        private ITapControllerService _tapController;
        private Camera _uiCamera;
        private readonly Queue<CoinEffectInstance> _pool = new Queue<CoinEffectInstance>();
        private readonly List<CoinEffectInstance> _activeEffects = new List<CoinEffectInstance>();

        private void Awake()
        {
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
            
            if (effectContainer == null)
            {
                effectContainer = transform as RectTransform;
            }
            
            // Get camera for Screen Space - Camera canvas
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _uiCamera = canvas.worldCamera;
            }
            
            InitializePool();
        }

        private void Start()
        {
            _tapController = ServiceLocator.Get<ITapControllerService>();
            
            if (_tapController != null)
            {
                _tapController.OnTapPerformedWithPosition += OnTapPerformed;
            }
        }

        private void OnDestroy()
        {
            if (_tapController != null)
            {
                _tapController.OnTapPerformedWithPosition -= OnTapPerformed;
            }
        }

        private void Update()
        {
            UpdateActiveEffects();
        }

        private void InitializePool()
        {
            if (coinEffectPrefab == null)
            {
                CreateDefaultPrefab();
            }
            
            for (int i = 0; i < poolSize; i++)
            {
                var instance = CreateCoinInstance();
                instance.GameObject.SetActive(false);
                _pool.Enqueue(instance);
            }
        }

        private void CreateDefaultPrefab()
        {
            // Create a default coin effect if no prefab is assigned
            var go = new GameObject("CoinEffect");
            go.transform.SetParent(effectContainer, false);
            
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(60, 60);
            
            // Add coin icon (using text as placeholder)
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = "+1";
            text.fontSize = 32;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.84f, 0f); // Gold color
            
            // Add CanvasGroup for fading
            go.AddComponent<CanvasGroup>();
            
            coinEffectPrefab = go;
            go.SetActive(false);
        }

        private CoinEffectInstance CreateCoinInstance()
        {
            var go = Instantiate(coinEffectPrefab, effectContainer);
            var instance = new CoinEffectInstance
            {
                GameObject = go,
                RectTransform = go.GetComponent<RectTransform>(),
                CanvasGroup = go.GetComponent<CanvasGroup>(),
                Text = go.GetComponentInChildren<TMP_Text>()
            };
            
            if (instance.CanvasGroup == null)
            {
                instance.CanvasGroup = go.AddComponent<CanvasGroup>();
            }
            
            return instance;
        }

        private void OnTapPerformed(Vector2 screenPosition, int coinsEarned)
        {
            SpawnCoinEffect(screenPosition, coinsEarned);
        }

        public void SpawnCoinEffect(Vector2 screenPosition, int coinsAmount)
        {
            var instance = GetFromPool();
            if (instance == null) return;
            
            // Convert screen position to canvas local position
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                effectContainer, 
                screenPosition, 
                _uiCamera, 
                out localPoint);
            
            // Add random offset to start position
            float randomX = Random.Range(-randomOffsetX, randomOffsetX);
            float randomY = Random.Range(-randomOffsetY, randomOffsetY);
            localPoint.x += randomX;
            localPoint.y += randomY;
            
            // Calculate target position
            Vector2 targetPosition;
            if (target != null)
            {
                // Get target position in effectContainer's local space
                Vector3 targetWorldPos = target.position;
                Vector3 localTargetPos = effectContainer.InverseTransformPoint(targetWorldPos);
                targetPosition = new Vector2(localTargetPos.x, localTargetPos.y);
            }
            else
            {
                // Fallback: fly upward if no target is set
                targetPosition = localPoint + Vector2.up * 100f;
            }
            
            // Setup instance
            instance.RectTransform.anchoredPosition = localPoint;
            instance.StartPosition = localPoint;
            instance.TargetPosition = targetPosition;
            instance.ElapsedTime = 0f;
            instance.Duration = duration;
            
            // Calculate curve control point for bezier-like movement
            Vector2 midPoint = (localPoint + targetPosition) / 2f;
            instance.ControlPoint = midPoint + Vector2.up * curveHeight;
            
            if (instance.Text != null)
            {
                instance.Text.text = $"+{coinsAmount}";
            }
            
            // Reset visual state
            instance.CanvasGroup.alpha = 1f;
            instance.RectTransform.localScale = Vector3.one * 0.5f;
            instance.GameObject.SetActive(true);
            
            _activeEffects.Add(instance);
        }

        private void UpdateActiveEffects()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var instance = _activeEffects[i];
                instance.ElapsedTime += Time.deltaTime;
                
                float t = Mathf.Clamp01(instance.ElapsedTime / instance.Duration);
                
                // Apply curved movement using quadratic bezier
                float moveT = moveCurve.Evaluate(t);
                Vector2 position = CalculateQuadraticBezierPoint(
                    moveT,
                    instance.StartPosition,
                    instance.ControlPoint,
                    instance.TargetPosition);
                instance.RectTransform.anchoredPosition = position;
                
                // Apply fade (starts fading closer to the end)
                float fadeT = fadeCurve.Evaluate(t);
                instance.CanvasGroup.alpha = fadeT;
                
                // Apply scale
                float scaleT = scaleCurve.Evaluate(t);
                instance.RectTransform.localScale = Vector3.one * scaleT;
                
                // Check if animation is complete
                if (t >= 1f)
                {
                    ReturnToPool(instance);
                    _activeEffects.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Вычисляет точку на квадратичной кривой Безье
        /// </summary>
        private Vector2 CalculateQuadraticBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            
            Vector2 point = uu * p0; // (1-t)^2 * P0
            point += 2 * u * t * p1; // 2(1-t)t * P1
            point += tt * p2;        // t^2 * P2
            
            return point;
        }

        private CoinEffectInstance GetFromPool()
        {
            if (_pool.Count > 0)
            {
                return _pool.Dequeue();
            }
            
            // Create new instance if pool is empty
            return CreateCoinInstance();
        }

        private void ReturnToPool(CoinEffectInstance instance)
        {
            instance.GameObject.SetActive(false);
            _pool.Enqueue(instance);
        }

        private class CoinEffectInstance
        {
            public GameObject GameObject;
            public RectTransform RectTransform;
            public CanvasGroup CanvasGroup;
            public TMP_Text Text;
            public Vector2 StartPosition;
            public Vector2 TargetPosition;
            public Vector2 ControlPoint; // Контрольная точка для кривой Безье
            public float ElapsedTime;
            public float Duration;
        }
    }
}
