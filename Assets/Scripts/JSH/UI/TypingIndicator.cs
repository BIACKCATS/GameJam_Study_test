using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace JSH.UI
{
    /// <summary>
    /// Messenger "typing..." indicator: the dots wave softly for a while, then disappear and the content
    /// (e.g. the chat title) pops in at once. Unscaled time.
    /// Use alone (Play / playOnStart) or from EndingMessageSequence (PlayRoutine).
    /// </summary>
    public sealed class TypingIndicator : MonoBehaviour
    {
        [Tooltip("Parent of the dots (its own small bubble). Hidden when typing ends.")]
        [SerializeField] private RectTransform _dotsRoot;
        [SerializeField] private RectTransform[] _dots = Array.Empty<RectTransform>();
        [Tooltip("Shown with a pop after typing (the message itself).")]
        [SerializeField] private RectTransform _content;

        [Header("Typing")]
        [SerializeField, Min(0f)] private float _typingDuration = 1.6f;
        [Tooltip("How high each dot rises (UI units).")]
        [SerializeField] private float _waveHeight = 6f;
        [Tooltip("Seconds for one rise (and again for the fall).")]
        [SerializeField, Min(0.05f)] private float _waveHalfPeriod = 0.3f;
        [Tooltip("Delay between dots, makes the wave travel left to right.")]
        [SerializeField, Min(0f)] private float _dotPhase = 0.15f;
        [Tooltip("Dot alpha at the bottom of the wave (1 at the top).")]
        [SerializeField, Range(0f, 1f)] private float _dotMinAlpha = 0.35f;

        [Header("Pop")]
        [SerializeField, Min(0.01f)] private float _dotsHideDuration = 0.1f;
        [SerializeField, Min(0.01f)] private float _popDuration = 0.35f;
        [SerializeField] private Ease _popEase = Ease.OutBack;
        [SerializeField] private AudioSource _sfx;
        [SerializeField] private AudioClip _popClip;

        [SerializeField] private bool _playOnStart;
        [SerializeField] private UnityEvent _onFinished;

        private Vector2[] _dotBasePos = Array.Empty<Vector2>();
        private Graphic[] _dotGraphics = Array.Empty<Graphic>();
        private Coroutine _routine;
        private bool _initialized;

        public bool IsPlaying => _routine != null;
        public bool IsDone { get; private set; }

        private void Awake() => Initialize();

        private void Start()
        {
            if (_playOnStart)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            StopRoutine();
            KillTweens();
        }

        public void Play()
        {
            StopRoutine();
            _routine = StartCoroutine(PlayAndClear());
        }

        /// <summary>Yield this from another coroutine. Complete() can still finish it early.</summary>
        public IEnumerator PlayRoutine()
        {
            Initialize();
            IsDone = false;
            KillTweens();
            ApplyStartState();

            for (int i = 0; i < _dots.Length; i++)
            {
                RectTransform dot = _dots[i];
                if (dot == null)
                {
                    continue;
                }

                float delay = i * _dotPhase;
                dot.DOAnchorPosY(_dotBasePos[i].y + _waveHeight, _waveHalfPeriod)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetDelay(delay).SetUpdate(true);
                if (_dotGraphics[i] != null)
                {
                    _dotGraphics[i].DOFade(1f, _waveHalfPeriod)
                        .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetDelay(delay).SetUpdate(true);
                }
            }

            yield return WaitUnscaled(_typingDuration);

            KillTweens();
            if (_dotsRoot != null)
            {
                _dotsRoot.DOScale(0f, _dotsHideDuration).SetEase(Ease.InBack).SetUpdate(true);
                yield return WaitUnscaled(_dotsHideDuration);
                _dotsRoot.gameObject.SetActive(false);
            }

            if (_content != null)
            {
                _content.gameObject.SetActive(true);
                _content.localScale = Vector3.zero;
                _content.DOScale(1f, _popDuration).SetEase(_popEase).SetUpdate(true);
            }

            PlayClip(_popClip);
            yield return WaitUnscaled(_popDuration);
            ApplyFinalState();
        }

        /// <summary>Jump to the end state (dots gone, content shown).</summary>
        public void Complete()
        {
            Initialize();
            StopRoutine();
            KillTweens();
            ApplyFinalState();
        }

        /// <summary>Hide the content and show idle dots (call before PlayRoutine to avoid a visible flash).</summary>
        public void PrepareHidden()
        {
            Initialize();
            ApplyStartState();
        }

        private IEnumerator PlayAndClear()
        {
            yield return PlayRoutine();
            _routine = null;
        }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _dotBasePos = new Vector2[_dots.Length];
            _dotGraphics = new Graphic[_dots.Length];
            for (int i = 0; i < _dots.Length; i++)
            {
                if (_dots[i] == null)
                {
                    continue;
                }

                _dotBasePos[i] = _dots[i].anchoredPosition;
                _dots[i].TryGetComponent(out _dotGraphics[i]);
            }

            if (_playOnStart)
            {
                ApplyStartState();
            }
        }

        private void ApplyStartState()
        {
            if (_dotsRoot != null)
            {
                _dotsRoot.gameObject.SetActive(true);
                _dotsRoot.localScale = Vector3.one;
            }

            ResetDots(_dotMinAlpha);
            if (_content != null)
            {
                _content.gameObject.SetActive(false);
            }
        }

        private void ApplyFinalState()
        {
            ResetDots(1f);
            if (_dotsRoot != null)
            {
                _dotsRoot.gameObject.SetActive(false);
            }

            if (_content != null)
            {
                _content.gameObject.SetActive(true);
                _content.localScale = Vector3.one;
            }

            if (!IsDone)
            {
                IsDone = true;
                _onFinished?.Invoke();
            }
        }

        private void ResetDots(float alpha)
        {
            for (int i = 0; i < _dots.Length; i++)
            {
                if (_dots[i] == null)
                {
                    continue;
                }

                _dots[i].anchoredPosition = _dotBasePos[i];
                if (_dotGraphics[i] != null)
                {
                    Color c = _dotGraphics[i].color;
                    c.a = alpha;
                    _dotGraphics[i].color = c;
                }
            }
        }

        private void KillTweens()
        {
            for (int i = 0; i < _dots.Length; i++)
            {
                if (_dots[i] != null)
                {
                    _dots[i].DOKill();
                }

                if (i < _dotGraphics.Length && _dotGraphics[i] != null)
                {
                    _dotGraphics[i].DOKill();
                }
            }

            if (_dotsRoot != null)
            {
                _dotsRoot.DOKill();
            }

            if (_content != null)
            {
                _content.DOKill();
            }
        }

        private void StopRoutine()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        private void PlayClip(AudioClip clip)
        {
            if (_sfx != null && clip != null)
            {
                _sfx.PlayOneShot(clip);
            }
        }

        private static IEnumerator WaitUnscaled(float seconds)
        {
            while (seconds > 0f)
            {
                seconds -= Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
