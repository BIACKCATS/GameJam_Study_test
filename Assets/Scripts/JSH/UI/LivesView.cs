using DG.Tweening;
using JSH.Core;
using JSH.Gameplay;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

namespace JSH.UI
{
    /// <summary>
    /// Heart HUD driven by LifeSystem (LivesChangedEvent). Hearts are RawImages animated from sprite sheets
    /// (shimmer loop while full, break sheet on loss, break sheet reversed on heal). Hearts are ordered left to right;
    /// the rightmost full heart breaks first. Uses unscaled time so the last heart still breaks during pause/game over.
    /// </summary>
    public sealed class LivesView : MonoBehaviour
    {
        private enum HeartState { Full, Breaking, Empty, Restoring }

        [Header("Hearts (left to right)")]
        [SerializeField] private RawImage[] _hearts = new RawImage[3];

        [Header("Sprite Sheets (grid, frames read left-to-right, top-to-bottom)")]
        [SerializeField] private Texture2D _shimmerSheet;
        [SerializeField] private Texture2D _breakSheet;
        [SerializeField, Min(1)] private int _sheetColumns = 4;
        [SerializeField, Min(1)] private int _sheetRows = 3;
        [SerializeField, Min(1)] private int _sheetFrameCount = 10;
        [SerializeField, Min(0.1f)] private float _shimmerFps = 6f;
        [SerializeField, Min(0.1f)] private float _breakFps = 18f;
        [SerializeField, Min(0.1f)] private float _restoreFps = 24f;

        [Header("Punch")]
        [SerializeField] private Vector3 _damagePunch = new Vector3(0.5f, 0.5f, 0f);
        [SerializeField] private Vector3 _healPunch = new Vector3(0.35f, 0.35f, 0f);
        [SerializeField, Min(0.01f)] private float _punchDuration = 0.3f;
        [SerializeField, Min(0)] private int _punchVibrato = 8;

        [Header("Audio (optional)")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _breakSound;
        [SerializeField] private AudioClip _healSound;
        [SerializeField, Range(0f, 1f)] private float _soundVolume = 0.4f;

        [Header("Feedback (optional, e.g. screen shake / red flash)")]
        [SerializeField] private MMF_Player _damageFeedback;
        [SerializeField] private MMF_Player _healFeedback;

        private HeartState[] _states;
        private float[] _frames;
        private Tween[] _punches;
        private int _shownLives;

        private void Awake()
        {
            int count = _hearts != null ? _hearts.Length : 0;
            _states = new HeartState[count];
            _frames = new float[count];
            _punches = new Tween[count];
        }

        private void OnEnable() => EventBus<LivesChangedEvent>.Subscribe(OnLivesChanged);

        private void OnDisable()
        {
            EventBus<LivesChangedEvent>.Unsubscribe(OnLivesChanged);
            for (int i = 0; i < _punches.Length; i++) _punches[i]?.Kill();
        }

        // Snap after every Awake has run (LifeSystem initializes its count in its own Awake).
        private void Start()
        {
            if (_shimmerSheet == null || _breakSheet == null)
                Debug.LogError("LivesView: assign both Shimmer Sheet and Break Sheet.", this);

            Snap(LifeSystem.HasInstance ? LifeSystem.Instance.Lives : _states.Length);
        }

        private void Snap(int lives)
        {
            _shownLives = Mathf.Clamp(lives, 0, _states.Length);
            for (int i = 0; i < _states.Length; i++)
            {
                _states[i] = i < _shownLives ? HeartState.Full : HeartState.Empty;
                _frames[i] = 0f;
                Render(i);
            }
        }

        private void OnLivesChanged(LivesChangedEvent evt)
        {
            int target = Mathf.Clamp(evt.Current, 0, _states.Length);
            if (target == _shownLives) return;

            bool lost = target < _shownLives;
            if (lost)
            {
                for (int i = target; i < _shownLives; i++) StartHeart(i, HeartState.Breaking, 0f, _damagePunch);
            }
            else
            {
                for (int i = _shownLives; i < target; i++) StartHeart(i, HeartState.Restoring, _sheetFrameCount - 1, _healPunch);
            }
            _shownLives = target;

            PlaySound(lost ? _breakSound : _healSound);
            MMF_Player feedback = lost ? _damageFeedback : _healFeedback;
            if (feedback != null) feedback.PlayFeedbacks();
        }

        private void StartHeart(int index, HeartState state, float startFrame, Vector3 punch)
        {
            _states[index] = state;
            _frames[index] = startFrame;
            Render(index);

            RawImage heart = _hearts[index];
            if (heart == null || !heart.gameObject.activeInHierarchy) return;
            _punches[index]?.Kill();
            heart.rectTransform.localScale = Vector3.one;
            _punches[index] = heart.rectTransform
                .DOPunchScale(punch, _punchDuration, _punchVibrato, 0.5f)
                .SetUpdate(true);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            for (int i = 0; i < _states.Length; i++)
            {
                switch (_states[i])
                {
                    case HeartState.Empty:
                        continue;
                    case HeartState.Full:
                        _frames[i] = Mathf.Repeat(_frames[i] + dt * _shimmerFps, _sheetFrameCount);
                        break;
                    case HeartState.Breaking:
                        _frames[i] += dt * _breakFps;
                        if (_frames[i] >= _sheetFrameCount) _states[i] = HeartState.Empty;
                        break;
                    case HeartState.Restoring:
                        _frames[i] -= dt * _restoreFps;
                        if (_frames[i] < 0f)
                        {
                            _states[i] = HeartState.Full;
                            _frames[i] = 0f;
                        }
                        break;
                }
                Render(i);
            }
        }

        private void Render(int index)
        {
            RawImage heart = _hearts[index];
            if (heart == null) return;

            HeartState state = _states[index];
            heart.enabled = state != HeartState.Empty;
            if (!heart.enabled) return;

            heart.texture = state == HeartState.Full ? _shimmerSheet : _breakSheet;
            int frame = Mathf.Clamp(Mathf.FloorToInt(_frames[index]), 0, _sheetFrameCount - 1);
            float w = 1f / _sheetColumns;
            float h = 1f / _sheetRows;
            heart.uvRect = new Rect(frame % _sheetColumns * w, 1f - (frame / _sheetColumns + 1) * h, w, h);
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null && _audioSource.isActiveAndEnabled)
                _audioSource.PlayOneShot(clip, _soundVolume);
        }
    }
}
