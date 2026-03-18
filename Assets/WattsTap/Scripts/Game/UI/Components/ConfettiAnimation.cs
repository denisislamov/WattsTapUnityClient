using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WattsTap.Scripts.Game.UI.Components
{
    /// <summary>
    /// Компонент для анимации конфетти, разлетающихся из указанного RectTransform.
    /// Спрайты Confetti_1..Confetti_16 загружаются из Resources или задаются вручную.
    /// Паттерн аналогичен TapCoinEffect / MergeScreenUIViewAnimation.
    /// Поддерживает превью в Edit Mode через кастомный Editor (ConfettiAnimationEditor).
    /// </summary>
    [ExecuteAlways]
    public class ConfettiAnimation : MonoBehaviour
    {
        [Header("Spawn Source")]
        [Tooltip("RectTransform, из которого разлетаются конфетти. Если не задан — используется собственный RectTransform.")]
        [SerializeField] private RectTransform _spawnSource;

        [Header("Container")]
        [Tooltip("Родительский RectTransform для порождаемых конфетти. Если не задан — используется собственный RectTransform.")]
        [SerializeField] private RectTransform _container;

        [Header("Confetti Sprites")]
        [Tooltip("Массив спрайтов конфетти (Confetti_1..Confetti_16). Если пуст — загружаются из Resources.")]
        [SerializeField] private Sprite[] _confettiSprites;

        [Header("Spawn Settings")]
        [Tooltip("Количество конфетти за один «взрыв».")]
        [SerializeField] private int _spawnCount = 30;
        [Tooltip("Случайное смещение от центра источника при порождении (px).")]
        [SerializeField] private float _spawnRandomOffset = 20f;

        [Header("Flight Settings")]
        [Tooltip("Общая длительность полёта одной частицы (сек).")]
        [SerializeField] private float _flightDuration = 1.4f;
        [Tooltip("Разброс длительности ±(сек).")]
        [SerializeField] private float _flightDurationVariance = 0.4f;
        [Tooltip("Минимальная дальность разлёта (px).")]
        [SerializeField] private float _minDistance = 150f;
        [Tooltip("Максимальная дальность разлёта (px).")]
        [SerializeField] private float _maxDistance = 500f;
        [Tooltip("Высота подъёма дуги полёта (px). 0 — прямолинейный. (Не используется в режиме салюта.)")]
        [SerializeField] private float _arcHeight = 0f;
        [Tooltip("Гравитация — ускорение вниз (px/s²). Имитирует падение после разлёта.")]
        [SerializeField] private float _gravity = 200f;

        [Header("Rotation Settings")]
        [Tooltip("Минимальная скорость вращения (°/s).")]
        [SerializeField] private float _minRotationSpeed = 180f;
        [Tooltip("Максимальная скорость вращения (°/s).")]
        [SerializeField] private float _maxRotationSpeed = 720f;

        [Header("Scale Settings")]
        [Tooltip("Начальный масштаб конфетти.")]
        [SerializeField] private float _startScale = 0.6f;
        [Tooltip("Конечный масштаб конфетти.")]
        [SerializeField] private float _endScale = 0.3f;
        [Tooltip("Размер Image конфетти (px).")]
        [SerializeField] private Vector2 _confettiSize = new Vector2(40f, 40f);

        [Header("Fade Settings")]
        [Tooltip("Доля полёта, после которой начинается затухание (0..1).")]
        [SerializeField] [Range(0f, 1f)] private float _fadeStartNormalized = 0.5f;

        [Header("Pool Settings")]
        [SerializeField] private int _poolSize = 40;

        [Header("WebGL / Mobile Optimization")]
        [SerializeField] private bool _useUnscaledTime = true;

        // ── Internal State ─────────────────────────────────────────────
        private readonly Queue<ConfettiInstance> _pool = new Queue<ConfettiInstance>();
        private readonly List<ConfettiInstance> _activeParticles = new List<ConfettiInstance>();
        private bool _isPlaying;
        private bool _isPreviewingInEditor;

        // ── Unity Lifecycle ────────────────────────────────────────────

        private void Awake()
        {
            if (_spawnSource == null)
                _spawnSource = transform as RectTransform;

            if (_container == null)
                _container = transform as RectTransform;

            LoadSpritesIfNeeded();
            InitializePool();
        }

        private void Update()
        {
#if UNITY_EDITOR
            // В Edit Mode обновляем только если идёт превью
            if (!Application.isPlaying && !_isPreviewingInEditor)
                return;
#endif
            UpdateActiveParticles();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            ClearAll();
        }

#if UNITY_EDITOR
        private void OnDisable()
        {
            // Очистка при выключении компонента / выходе из Play Mode
            if (_isPreviewingInEditor)
                StopEditorPreview();
        }
#endif

        // ── Public API ─────────────────────────────────────────────────

        /// <summary>
        /// Запускает одиночный «взрыв» конфетти из _spawnSource.
        /// </summary>
        public void Play()
        {
            SpawnBurst(_spawnCount);
        }

        /// <summary>
        /// Запускает взрыв конфетти с указанным количеством частиц.
        /// </summary>
        public void Play(int count)
        {
            SpawnBurst(count);
        }

        /// <summary>
        /// Запускает взрыв конфетти из указанной мировой позиции.
        /// </summary>
        public void PlayAt(Vector3 worldPosition)
        {
            Vector2 localPoint = WorldToContainerLocal(worldPosition);
            SpawnBurstAt(localPoint, _spawnCount);
        }

        /// <summary>
        /// Запускает взрыв конфетти из указанной мировой позиции с заданным количеством.
        /// </summary>
        public void PlayAt(Vector3 worldPosition, int count)
        {
            Vector2 localPoint = WorldToContainerLocal(worldPosition);
            SpawnBurstAt(localPoint, count);
        }

        /// <summary>
        /// Запускает повторяющийся эффект конфетти (несколько волн).
        /// </summary>
        public Coroutine PlayRepeating(int waves, float interval)
        {
            return StartCoroutine(RepeatingBurstCoroutine(waves, interval));
        }

        /// <summary>
        /// Останавливает повторяющийся эффект и очищает все активные частицы.
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            StopAllCoroutines();
            ClearAll();
        }

        /// <summary>
        /// Задать спрайты конфетти программно (если не заданы через Inspector).
        /// </summary>
        public void SetSprites(Sprite[] sprites)
        {
            _confettiSprites = sprites;
        }

        /// <summary>
        /// Задать RectTransform источника конфетти программно.
        /// </summary>
        public void SetSpawnSource(RectTransform source)
        {
            _spawnSource = source;
        }

        // ── Editor Preview API ────────────────────────────────────────

#if UNITY_EDITOR
        /// <summary>
        /// Запускает превью конфетти в Edit Mode (вызывается из кастомного Editor).
        /// </summary>
        public void PlayEditorPreview()
        {
            EnsureInitialized();
            _isPreviewingInEditor = true;
            SpawnBurst(_spawnCount);
            UnityEditor.EditorApplication.update -= EditorUpdate;
            UnityEditor.EditorApplication.update += EditorUpdate;
        }

        /// <summary>
        /// Запускает превью конфетти с указанным количеством в Edit Mode.
        /// </summary>
        public void PlayEditorPreview(int count)
        {
            EnsureInitialized();
            _isPreviewingInEditor = true;
            SpawnBurst(count);
            UnityEditor.EditorApplication.update -= EditorUpdate;
            UnityEditor.EditorApplication.update += EditorUpdate;
        }

        /// <summary>
        /// Останавливает превью и удаляет все порождённые объекты в Edit Mode.
        /// </summary>
        public void StopEditorPreview()
        {
            _isPreviewingInEditor = false;
            UnityEditor.EditorApplication.update -= EditorUpdate;
            ClearAllImmediate();
        }

        /// <summary>
        /// Есть ли активные частицы (для Editor GUI).
        /// </summary>
        public bool HasActiveParticles => _activeParticles.Count > 0;

        /// <summary>
        /// Идёт ли превью в редакторе.
        /// </summary>
        public bool IsPreviewingInEditor => _isPreviewingInEditor;

        private double _lastEditorTime;

        private void EditorUpdate()
        {
            if (!_isPreviewingInEditor && _activeParticles.Count == 0)
            {
                UnityEditor.EditorApplication.update -= EditorUpdate;
                return;
            }

            double currentTime = UnityEditor.EditorApplication.timeSinceStartup;
            float dt = (_lastEditorTime > 0) ? (float)(currentTime - _lastEditorTime) : 0.016f;
            _lastEditorTime = currentTime;

            // Ограничиваем dt чтобы не было скачков при фокусе/расфокусе окна
            dt = Mathf.Min(dt, 0.05f);

            UpdateActiveParticlesWithDt(dt);

            // Перерисовка Scene/Game view
            UnityEditor.SceneView.RepaintAll();

            if (_activeParticles.Count == 0)
            {
                _isPreviewingInEditor = false;
                UnityEditor.EditorApplication.update -= EditorUpdate;
            }
        }

        /// <summary>
        /// Убеждаемся что пул и спрайты инициализированы (для Edit Mode, где Awake мог не вызваться).
        /// </summary>
        private void EnsureInitialized()
        {
            if (_spawnSource == null)
                _spawnSource = transform as RectTransform;
            if (_container == null)
                _container = transform as RectTransform;

            LoadSpritesIfNeeded();

            if (_pool.Count == 0 && _activeParticles.Count == 0)
                InitializePool();
        }

        /// <summary>
        /// Удаление объектов через DestroyImmediate (для Edit Mode).
        /// </summary>
        private void ClearAllImmediate()
        {
            for (int i = _activeParticles.Count - 1; i >= 0; i--)
            {
                if (_activeParticles[i].GameObject != null)
                    DestroyImmediate(_activeParticles[i].GameObject);
            }
            _activeParticles.Clear();

            while (_pool.Count > 0)
            {
                var inst = _pool.Dequeue();
                if (inst.GameObject != null)
                    DestroyImmediate(inst.GameObject);
            }
        }

        /// <summary>
        /// Обновление частиц с явным dt (для EditorApplication.update).
        /// </summary>
        private void UpdateActiveParticlesWithDt(float dt)
        {
            for (int i = _activeParticles.Count - 1; i >= 0; i--)
            {
                var p = _activeParticles[i];
                p.ElapsedTime += dt;

                float t = Mathf.Clamp01(p.ElapsedTime / p.Duration);

                float easedT = EaseOutQuad(t);
                float x = p.StartPosition.x + p.Direction.x * p.Distance * easedT;
                float y = p.StartPosition.y + p.Direction.y * p.Distance * easedT;

                float elapsed = p.ElapsedTime;
                y -= 0.5f * _gravity * elapsed * elapsed;

                p.RectTransform.anchoredPosition = new Vector2(x, y);

                float currentRotation = p.StartRotation + p.RotationSpeed * p.ElapsedTime;
                p.RectTransform.localRotation = Quaternion.Euler(0f, 0f, currentRotation);

                float scale = Mathf.Lerp(_startScale, _endScale, t) * p.ScaleMultiplier;
                p.RectTransform.localScale = Vector3.one * scale;

                if (t > _fadeStartNormalized)
                {
                    float fadeT = (t - _fadeStartNormalized) / (1f - _fadeStartNormalized);
                    p.CanvasGroup.alpha = Mathf.Lerp(1f, 0f, EaseInQuad(fadeT));
                }
                else
                {
                    p.CanvasGroup.alpha = 1f;
                }

                if (t >= 1f)
                {
                    if (Application.isPlaying)
                    {
                        ReturnToPool(p);
                    }
                    else
                    {
                        if (p.GameObject != null)
                            DestroyImmediate(p.GameObject);
                    }
                    _activeParticles.RemoveAt(i);
                }
            }
        }

        [ContextMenu("▶ Play Confetti")]
        private void ContextMenuPlay()
        {
            if (Application.isPlaying)
                Play();
            else
                PlayEditorPreview();
        }

        [ContextMenu("⏹ Stop Confetti")]
        private void ContextMenuStop()
        {
            if (Application.isPlaying)
                Stop();
            else
                StopEditorPreview();
        }
#endif

        // ── Sprite Loading ─────────────────────────────────────────────

        private void LoadSpritesIfNeeded()
        {
            if (_confettiSprites != null && _confettiSprites.Length > 0)
                return;

            // Попытка загрузки из Resources (если спрайты положены в Assets/Resources/Confetti/)
            var loaded = new List<Sprite>();
            for (int i = 1; i <= 16; i++)
            {
                var sprite = Resources.Load<Sprite>($"Confetti/Confetti_{i}");
                if (sprite != null)
                    loaded.Add(sprite);
            }

            if (loaded.Count > 0)
            {
                _confettiSprites = loaded.ToArray();
            }
            else
            {
                Debug.LogWarning("[ConfettiAnimation] Спрайты конфетти не назначены. " +
                                 "Перетащите Confetti_1..16 из Assets/Content/Sprites/v6/Update/ в поле Confetti Sprites в Inspector, " +
                                 "или скопируйте их в Assets/Resources/Confetti/ для автозагрузки.");
            }
        }

        // ── Object Pool ───────────────────────────────────────────────

        private void InitializePool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                var instance = CreateConfettiInstance();
                instance.GameObject.SetActive(false);
                _pool.Enqueue(instance);
            }
        }

        private ConfettiInstance CreateConfettiInstance()
        {
            var go = new GameObject("Confetti");
            go.transform.SetParent(_container, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = _confettiSize;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            var image = go.AddComponent<Image>();
            image.raycastTarget = false;

            var canvasGroup = go.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            return new ConfettiInstance
            {
                GameObject = go,
                RectTransform = rect,
                Image = image,
                CanvasGroup = canvasGroup
            };
        }

        private ConfettiInstance GetFromPool()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();

            // Пул пуст — создаём новый экземпляр
            return CreateConfettiInstance();
        }

        private void ReturnToPool(ConfettiInstance instance)
        {
            instance.GameObject.SetActive(false);
            _pool.Enqueue(instance);
        }

        private void ClearAll()
        {
            for (int i = _activeParticles.Count - 1; i >= 0; i--)
            {
                ReturnToPool(_activeParticles[i]);
            }
            _activeParticles.Clear();
        }

        // ── Spawn Logic ───────────────────────────────────────────────

        private void SpawnBurst(int count)
        {
            if (_spawnSource == null) return;

            // Центр источника в локальных координатах контейнера
            Vector3 worldCenter = _spawnSource.position;
            Vector2 localCenter = WorldToContainerLocal(worldCenter);

            SpawnBurstAt(localCenter, count);
        }

        private void SpawnBurstAt(Vector2 localCenter, int count)
        {
            bool hasSprites = _confettiSprites != null && _confettiSprites.Length > 0;

            for (int i = 0; i < count; i++)
            {
                var instance = GetFromPool();

                // Случайный спрайт
                if (hasSprites)
                {
                    instance.Image.sprite = _confettiSprites[Random.Range(0, _confettiSprites.Length)];
                    instance.Image.color = Color.white;
                }
                else
                {
                    // Фолбэк — цветной квадрат
                    instance.Image.sprite = null;
                    instance.Image.color = GetRandomConfettiColor();
                }

                // Случайное начальное смещение
                float offsetX = Random.Range(-_spawnRandomOffset, _spawnRandomOffset);
                float offsetY = Random.Range(-_spawnRandomOffset, _spawnRandomOffset);
                Vector2 startPos = localCenter + new Vector2(offsetX, offsetY);

                // Случайное направление полёта (360°)
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(_minDistance, _maxDistance);
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                // Длительность с вариацией
                float duration = _flightDuration + Random.Range(-_flightDurationVariance, _flightDurationVariance);
                duration = Mathf.Max(duration, 0.3f);

                // Вращение
                float rotSpeed = Random.Range(_minRotationSpeed, _maxRotationSpeed);
                if (Random.value > 0.5f) rotSpeed = -rotSpeed;

                // Начальное вращение
                float startRotation = Random.Range(0f, 360f);

                // Начальный масштаб с небольшой вариацией
                float scaleMult = Random.Range(0.8f, 1.2f);

                // Настройка экземпляра
                instance.StartPosition = startPos;
                instance.Direction = direction;
                instance.Distance = distance;
                instance.Duration = duration;
                instance.ElapsedTime = 0f;
                instance.RotationSpeed = rotSpeed;
                instance.StartRotation = startRotation;
                instance.ScaleMultiplier = scaleMult;
                instance.InitialVelocityY = 0f; // Салют: не используется, позиция считается радиально

                // Начальное состояние визуала
                instance.RectTransform.anchoredPosition = startPos;
                instance.RectTransform.localScale = Vector3.one * (_startScale * scaleMult);
                instance.RectTransform.localRotation = Quaternion.Euler(0f, 0f, startRotation);
                instance.CanvasGroup.alpha = 1f;

                instance.GameObject.SetActive(true);

                // Поднять наверх в иерархии, чтобы последние рисовались поверх
                instance.RectTransform.SetAsLastSibling();

                _activeParticles.Add(instance);
            }
        }

        // ── Update Loop ───────────────────────────────────────────────

        private void UpdateActiveParticles()
        {
            float dt = GetDeltaTime();

            for (int i = _activeParticles.Count - 1; i >= 0; i--)
            {
                var p = _activeParticles[i];
                p.ElapsedTime += dt;

                float t = Mathf.Clamp01(p.ElapsedTime / p.Duration);

                // ── Позиция (салют: радиальный разлёт от центра) ──
                // Горизонтальное и вертикальное движение — радиальное с замедлением (EaseOutQuad)
                float easedT = EaseOutQuad(t);
                float x = p.StartPosition.x + p.Direction.x * p.Distance * easedT;
                float y = p.StartPosition.y + p.Direction.y * p.Distance * easedT;

                // Гравитация плавно тянет вниз после разлёта
                float elapsed = p.ElapsedTime;
                y -= 0.5f * _gravity * elapsed * elapsed;

                p.RectTransform.anchoredPosition = new Vector2(x, y);

                // ── Вращение ──
                float currentRotation = p.StartRotation + p.RotationSpeed * p.ElapsedTime;
                p.RectTransform.localRotation = Quaternion.Euler(0f, 0f, currentRotation);

                // ── Масштаб ──
                float scale = Mathf.Lerp(_startScale, _endScale, t) * p.ScaleMultiplier;
                p.RectTransform.localScale = Vector3.one * scale;

                // ── Прозрачность ──
                if (t > _fadeStartNormalized)
                {
                    float fadeT = (t - _fadeStartNormalized) / (1f - _fadeStartNormalized);
                    p.CanvasGroup.alpha = Mathf.Lerp(1f, 0f, EaseInQuad(fadeT));
                }
                else
                {
                    p.CanvasGroup.alpha = 1f;
                }

                // ── Завершение ──
                if (t >= 1f)
                {
                    ReturnToPool(p);
                    _activeParticles.RemoveAt(i);
                }
            }
        }

        // ── Repeating Burst Coroutine ─────────────────────────────────

        private IEnumerator RepeatingBurstCoroutine(int waves, float interval)
        {
            _isPlaying = true;
            for (int w = 0; w < waves; w++)
            {
                if (!_isPlaying) yield break;
                SpawnBurst(_spawnCount);
                yield return WaitSeconds(interval);
            }
            _isPlaying = false;
        }

        // ── Coordinate Helpers ────────────────────────────────────────

        private Vector2 WorldToContainerLocal(Vector3 worldPosition)
        {
            Vector3 localPos = _container.InverseTransformPoint(worldPosition);
            return new Vector2(localPos.x, localPos.y);
        }

        // ── Timing Helpers (same pattern as MergeScreenUIViewAnimation) ──

        private float GetDeltaTime()
        {
            return _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        private object WaitSeconds(float seconds)
        {
            if (_useUnscaledTime)
                return new WaitForSecondsRealtime(seconds);
            return new WaitForSeconds(seconds);
        }

        // ── Easing Functions ──────────────────────────────────────────

        private static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private static float EaseInQuad(float t)
        {
            return t * t;
        }

        // ── Fallback Colors ───────────────────────────────────────────

        private static readonly Color[] FallbackColors =
        {
            new Color(1f, 0.22f, 0.33f),   // красный
            new Color(1f, 0.65f, 0f),       // оранжевый
            new Color(1f, 0.92f, 0.23f),    // жёлтый
            new Color(0.3f, 0.85f, 0.4f),   // зелёный
            new Color(0.25f, 0.6f, 1f),     // синий
            new Color(0.65f, 0.35f, 1f),    // фиолетовый
            new Color(1f, 0.42f, 0.72f),    // розовый
            new Color(0f, 0.9f, 0.85f),     // бирюзовый
        };

        private static Color GetRandomConfettiColor()
        {
            return FallbackColors[Random.Range(0, FallbackColors.Length)];
        }

        // ── Instance Data ─────────────────────────────────────────────

        private class ConfettiInstance
        {
            public GameObject GameObject;
            public RectTransform RectTransform;
            public Image Image;
            public CanvasGroup CanvasGroup;

            public Vector2 StartPosition;
            public Vector2 Direction;
            public float Distance;
            public float Duration;
            public float ElapsedTime;
            public float RotationSpeed;
            public float StartRotation;
            public float ScaleMultiplier;
            public float InitialVelocityY;
        }
    }
}




