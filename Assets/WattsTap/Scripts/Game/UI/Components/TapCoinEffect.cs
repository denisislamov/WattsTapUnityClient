using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WattsTap.Core;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Scripts.Game.UI.Components
{
    /// <summary>
    /// Компонент для отображения эффекта монетки при тапе.
    /// Монетка вылетает из места тапа и плавно исчезает с твин-анимацией.
    /// </summary>
    public class TapCoinEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform effectContainer;
        
        [Header("Coin Prefab")]
        [SerializeField] private GameObject coinEffectPrefab;
        
        [Header("Animation Settings")]
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private float floatDistance = 100f;
        [SerializeField] private float randomOffsetX = 30f;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 1, 1, 0);
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
            
            // Add random horizontal offset
            float randomX = Random.Range(-randomOffsetX, randomOffsetX);
            localPoint.x += randomX;
            
            // Setup instance
            instance.RectTransform.anchoredPosition = localPoint;
            instance.StartPosition = localPoint;
            instance.TargetPosition = localPoint + Vector2.up * floatDistance;
            instance.ElapsedTime = 0f;
            instance.Duration = duration;
            
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
                
                // Apply movement
                float moveT = moveCurve.Evaluate(t);
                instance.RectTransform.anchoredPosition = Vector2.Lerp(
                    instance.StartPosition, 
                    instance.TargetPosition, 
                    moveT);
                
                // Apply fade
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
            public float ElapsedTime;
            public float Duration;
        }
    }
}
