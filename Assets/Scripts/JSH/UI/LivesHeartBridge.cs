using JSH.Core;
using JSH.Gameplay;
using UnityEngine;
using UnityEngine.Events;

namespace JSH.UI
{
    /// <summary>
    /// Connects LifeSystem to the teammate's heart HUD (HeartAnimationPlayer in TempFolderStage).
    /// Data only: all heart visuals/sounds belong to the teammate's component.
    /// Hearts are numbered 1..N left to right; heart n is shown full while lives >= n.
    ///  - Life lost   -> HeartAnimationPlayer.PlayBreak(n), rightmost first
    ///  - Life gained -> HeartAnimationPlayer.PlayHeal(n) (+ optional On Heart Restored(n) hook)
    /// The visual state is reconciled every frame: HeartAnimationPlayer rejects a break while that heart is still
    /// playing its heal animation, so such a break waits until the heal has finished (e.g. persuade success then
    /// an immediate counterattack).
    /// </summary>
    public sealed class LivesHeartBridge : MonoBehaviour
    {
        private const int HeartSheetFrames = 10; // HeartAnimationPlayer plays 10 frames per animation

        [SerializeField, Min(1)] private int _heartCount = 3;
        [Tooltip("Empty = HeartAnimationPlayer on this GameObject. Only used to read the heal animation length.")]
        [SerializeField] private HeartAnimationPlayer _heartPlayer;
        [Tooltip("Extra wait after the heal animation before a pending break is allowed (scaled seconds).")]
        [SerializeField, Min(0f)] private float _healMargin = 0.05f;
        [Tooltip("Optional extra hook when heart n is restored (the heal animation itself is already played).")]
        [SerializeField] private UnityEvent<int> _onHeartRestored = new UnityEvent<int>();

        private bool[] _broken;
        private float[] _healLeft;
        private int _lives;

        private void Awake()
        {
            _broken = new bool[_heartCount];
            _healLeft = new float[_heartCount];
            if (_heartPlayer == null) TryGetComponent(out _heartPlayer);
        }

        private void OnEnable() => EventBus<LivesChangedEvent>.Subscribe(OnLivesChanged);

        private void OnDisable() => EventBus<LivesChangedEvent>.Unsubscribe(OnLivesChanged);

        // After every Awake (LifeSystem sets its count in Awake). Hearts start full.
        private void Start()
        {
            _lives = LifeSystem.HasInstance ? LifeSystem.Instance.Lives : _heartCount;
            Reconcile();
        }

        private void OnLivesChanged(LivesChangedEvent evt)
        {
            _lives = evt.Current;
            if (evt.Delta < 0) HeartAnimationPlayer.PlayBreakSound();
            Reconcile();
        }

        private void Update()
        {
            float dt = Time.deltaTime; // same clock as HeartAnimationPlayer
            bool waiting = false;
            for (int i = 0; i < _heartCount; i++)
            {
                if (_healLeft[i] <= 0f) continue;
                _healLeft[i] -= dt;
                waiting = true;
            }
            if (waiting) Reconcile();
        }

        private void Reconcile()
        {
            for (int n = _heartCount; n >= 1; n--) // rightmost first
            {
                int i = n - 1;
                bool shouldBreak = n > _lives;
                if (shouldBreak && !_broken[i])
                {
                    if (_healLeft[i] > 0f) continue; // still healing: break once it has finished
                    if (HeartAnimationPlayer.TryPlayBreak(n, false)) _broken[i] = true;
                }
                else if (!shouldBreak && _broken[i])
                {
                    _broken[i] = false;
                    _healLeft[i] = HealDuration() + _healMargin;
                    HeartAnimationPlayer.PlayHeal(n);
                    _onHeartRestored.Invoke(n);
                }
            }
        }

        private float HealDuration()
        {
            float fps = _heartPlayer != null ? Mathf.Max(0.1f, _heartPlayer.healFps) : 15f;
            return HeartSheetFrames / fps;
        }
    }
}
