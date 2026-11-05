using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace JSH.UI
{
    /// <summary>
    /// Title screen intro: UI elements fly in one after another (DOTween Sequence).
    /// Per entry, mode decides what the Editor position means:
    ///  - FromOffset: Editor = FINAL, starts at final + fromOffset.
    ///  - MoveBy / ToX / ToY / ToXY: Editor = START, moves to start + to / absolute x,y of "to".
    /// Stagger elements with startTime (seconds from intro start).
    /// Uses unscaled time. Any click / key skips to the end.
    /// </summary>
    public sealed class TitleIntroSequence : MonoBehaviour
    {
        private enum MoveMode
        {
            FromOffset, // Editor = final; start = final + fromOffset
            MoveBy,     // Editor = start; final = start + to
            ToX,        // Editor = start; final.x = to.x
            ToY,        // Editor = start; final.y = to.y
            ToXY,       // Editor = start; final = to
        }

        [Serializable]
        private sealed class Entry
        {
            public RectTransform target;
            [Tooltip("FromOffset: Editor pos = final. Others: Editor pos = start, moves using 'to'.")]
            public MoveMode mode = MoveMode.FromOffset;
            [Tooltip("FromOffset only. Start position = final anchoredPosition + this. e.g. (0, 1200) = from above.")]
            public Vector2 fromOffset = new Vector2(0f, 1200f);
            [Tooltip("MoveBy: added to start. ToX/ToY/ToXY: absolute anchoredPosition (Inspector Pos X/Y).")]
            public Vector2 to;
            [Tooltip("Start scale (1 = no scale animation).")]
            public float fromScale = 1f;
            [Tooltip("Fade alpha 0 -> 1 (adds a CanvasGroup if missing).")]
            public bool fade = true;
            [Tooltip("Seconds from intro start when this element begins moving.")]
            [Min(0f)] public float startTime;
            [Min(0.01f)] public float duration = 0.6f;
            public Ease ease = Ease.OutBack;

            [NonSerialized] public Vector2 startPos;
            [NonSerialized] public Vector2 finalPos;
            [NonSerialized] public Vector3 finalScale;
            [NonSerialized] public CanvasGroup group;
        }

        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();
        [SerializeField] private bool _playOnStart = true;
        [SerializeField, Min(0f)] private float _initialDelay = 0.2f;
        [Tooltip("Click / any key completes the intro immediately.")]
        [SerializeField] private bool _skipOnInput = true;
        [Tooltip("Optional: buttons root. Not interactable until the intro finishes.")]
        [SerializeField] private CanvasGroup _interactionGroup;
        [SerializeField] private UnityEvent _onFinished;

        private Sequence _sequence;

        public bool IsPlaying => _sequence != null && _sequence.IsActive() && _sequence.IsPlaying();

        private void Awake()
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                Entry e = _entries[i];
                if (e.target == null)
                {
                    continue;
                }

                Vector2 editorPos = e.target.anchoredPosition;
                switch (e.mode)
                {
                    case MoveMode.MoveBy:
                        e.startPos = editorPos;
                        e.finalPos = editorPos + e.to;
                        break;
                    case MoveMode.ToX:
                        e.startPos = editorPos;
                        e.finalPos = new Vector2(e.to.x, editorPos.y);
                        break;
                    case MoveMode.ToY:
                        e.startPos = editorPos;
                        e.finalPos = new Vector2(editorPos.x, e.to.y);
                        break;
                    case MoveMode.ToXY:
                        e.startPos = editorPos;
                        e.finalPos = e.to;
                        break;
                    default:
                        e.finalPos = editorPos;
                        e.startPos = editorPos + e.fromOffset;
                        break;
                }

                e.finalScale = e.target.localScale;
                if (e.fade && !e.target.TryGetComponent(out e.group))
                {
                    e.group = e.target.gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Hide everything at its start pose before the first rendered frame.
            if (_playOnStart)
            {
                ApplyStartPose();
            }
        }

        private void Start()
        {
            if (_playOnStart)
            {
                Play();
            }
        }

        private void Update()
        {
            if (!_skipOnInput || !IsPlaying)
            {
                return;
            }

            bool pressed = (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
                || (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame);
            if (pressed)
            {
                Skip();
            }
        }

        private void OnDisable() => KillSequence();

        /// <summary>Restart the intro from the beginning.</summary>
        public void Play()
        {
            KillSequence();
            ApplyStartPose();
            SetInteractable(false);

            _sequence = DOTween.Sequence().SetUpdate(true);
            for (int i = 0; i < _entries.Length; i++)
            {
                Entry e = _entries[i];
                if (e.target == null)
                {
                    continue;
                }

                float t = _initialDelay + e.startTime;
                _sequence.Insert(t, e.target.DOAnchorPos(e.finalPos, e.duration).SetEase(e.ease));
                if (!Mathf.Approximately(e.fromScale, 1f))
                {
                    _sequence.Insert(t, e.target.DOScale(e.finalScale, e.duration).SetEase(e.ease));
                }

                if (e.group != null)
                {
                    // Linear fade over the first half so overshoot eases don't flicker alpha.
                    _sequence.Insert(t, e.group.DOFade(1f, e.duration * 0.5f).SetEase(Ease.Linear));
                }
            }

            _sequence.OnComplete(Finish);
        }

        /// <summary>Jump to the final layout immediately.</summary>
        public void Skip()
        {
            if (IsPlaying)
            {
                _sequence.Complete(true); // fires OnComplete -> Finish
            }
        }

        private void Finish()
        {
            _sequence = null;
            SetInteractable(true);
            _onFinished?.Invoke();
        }

        private void ApplyStartPose()
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                Entry e = _entries[i];
                if (e.target == null)
                {
                    continue;
                }

                e.target.anchoredPosition = e.startPos;
                e.target.localScale = e.finalScale * e.fromScale;
                if (e.group != null)
                {
                    e.group.alpha = 0f;
                }
            }
        }

        private void SetInteractable(bool value)
        {
            if (_interactionGroup == null)
            {
                return;
            }

            _interactionGroup.interactable = value;
            _interactionGroup.blocksRaycasts = value;
        }

        private void KillSequence()
        {
            if (_sequence != null && _sequence.IsActive())
            {
                _sequence.Kill();
            }

            _sequence = null;
        }
    }
}
