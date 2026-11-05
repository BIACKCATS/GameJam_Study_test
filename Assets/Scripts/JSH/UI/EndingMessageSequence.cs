using System;
using System.Collections;
using DG.Tweening;
using JSH.Gameplay;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace JSH.UI
{
    /// <summary>
    /// Ending "complaint received" text message, played in order:
    ///  header (typing dots -> pop) -> bubble + "[Web]" label pop -> typed line -> stat rows slide in one by one
    ///  -> stat values stamp in one by one -> grade pops softly and keeps wobbling -> typed closing lines.
    /// Write the fixed text in each TMP in the Editor; this only drives visibility. Values use EndingStats.Last
    /// (set by gameplay), or the test values below if none. Unscaled time. Click / any key skips to the end.
    /// </summary>
    public sealed class EndingMessageSequence : MonoBehaviour
    {
        [Serializable]
        private sealed class StatRow
        {
            [Tooltip("Whole row (label + value). Slides in.")]
            public RectTransform row;
            [Tooltip("Number text. Stamped in.")]
            public TMP_Text value;
            [Tooltip("{0} = number. Type the unit word after it in the Inspector.")]
            public string format = "{0}";
        }

        [Header("Header (optional)")]
        [SerializeField] private TypingIndicator _header;
        [SerializeField, Min(0f)] private float _gapAfterHeader = 0.5f;

        [Header("Bubble + [Web] label")]
        [SerializeField] private RectTransform _bubble;
        [SerializeField] private RectTransform _webLabel;
        [SerializeField, Min(0.01f)] private float _popDuration = 0.35f;
        [SerializeField] private Ease _popEase = Ease.OutBack;

        [Header("Typed line")]
        [SerializeField] private TMP_Text _receivedLine;
        [Tooltip("Seconds per character.")]
        [SerializeField, Min(0.005f)] private float _typeInterval = 0.05f;
        [SerializeField, Min(0f)] private float _sectionGap = 0.4f;

        [Header("Stat rows (top to bottom: reports, embraced, not embraced, mistakes)")]
        [SerializeField] private StatRow[] _statRows = Array.Empty<StatRow>();
        [SerializeField] private RectTransform _gradeRow;
        [SerializeField] private TMP_Text _gradeValue;
        [SerializeField, Min(0f)] private float _rowInterval = 0.08f;
        [SerializeField, Min(0.01f)] private float _rowDuration = 0.25f;
        [Tooltip("Rows start this far above and slide down into place.")]
        [SerializeField] private float _rowOffsetY = 30f;

        [Header("Stamp")]
        [SerializeField, Min(0f)] private float _stampInterval = 0.35f;
        [SerializeField, Min(0.01f)] private float _stampDuration = 0.15f;
        [SerializeField, Min(1f)] private float _stampFromScale = 2.5f;
        [Tooltip("Optional Feel feedback per stamp (e.g. small screen shake).")]
        [SerializeField] private MMF_Player _stampFeedback;

        [Header("Grade (soft)")]
        [SerializeField, Min(0.01f)] private float _gradePopDuration = 0.9f;
        [SerializeField] private Ease _gradePopEase = Ease.OutElastic;
        [Tooltip("Idle wobble scale after the pop (1 = none).")]
        [SerializeField, Min(1f)] private float _gradeWobbleScale = 1.06f;
        [SerializeField, Min(0.05f)] private float _gradeWobbleHalfPeriod = 0.6f;

        [Header("Closing lines")]
        [SerializeField] private TMP_Text _closingLine;
        [SerializeField] private TMP_Text _thanksLine;
        [Tooltip("Seconds per character for the last line (slower).")]
        [SerializeField, Min(0.005f)] private float _thanksInterval = 0.15f;

        [Header("Sound (optional)")]
        [SerializeField] private AudioSource _sfx;
        [SerializeField] private AudioClip _popClip;
        [SerializeField] private AudioClip _typeClip;
        [SerializeField] private AudioClip _stampClip;
        [SerializeField] private AudioClip _gradeClip;

        [Header("Flow")]
        [SerializeField] private bool _playOnStart = true;
        [Tooltip("Used only when there is no header.")]
        [SerializeField, Min(0f)] private float _initialDelay = 0.3f;
        [SerializeField] private bool _skipOnInput = true;
        [SerializeField] private UnityEvent _onFinished;

        [Header("Test values (used when gameplay set no EndingStats)")]
        [SerializeField, Min(0)] private int _testReportCount = 12;
        [SerializeField, Min(0)] private int _testEmbracedCount = 8;
        [SerializeField, Min(0)] private int _testNotEmbracedCount = 4;
        [SerializeField, Min(0)] private int _testMistakeCount = 2;
        [SerializeField] private PeaceGrade _testGrade = PeaceGrade.A;

        private const int AllVisible = 99999;

        private Vector2[] _rowBasePos = Array.Empty<Vector2>();
        private CanvasGroup[] _rowGroups = Array.Empty<CanvasGroup>();
        private RectTransform[] _rows = Array.Empty<RectTransform>();
        private EndingStats _stats;
        private Coroutine _routine;
        private bool _initialized;
        private bool _finished;

        public bool IsPlaying => _routine != null;

        private void Awake()
        {
            Initialize();
            if (_playOnStart)
            {
                ApplyStartState();
            }
        }

        private IEnumerator Start()
        {
            if (!_playOnStart)
            {
                yield break;
            }

            // Skip the scene-load frames (huge unscaled deltaTime would fast-forward the start).
            yield return null;
            yield return null;
            Play();
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

        private void OnDisable()
        {
            StopRoutine();
            KillTweens();
        }

        /// <summary>Play with EndingStats.Last (or the test values).</summary>
        public void Play()
        {
            if (!EndingStats.TryGetLast(out EndingStats stats))
            {
                stats = new EndingStats(_testReportCount, _testEmbracedCount, _testNotEmbracedCount,
                    _testMistakeCount, _testGrade);
            }

            Play(stats);
        }

        public void Play(EndingStats stats)
        {
            Initialize();
            StopRoutine();
            KillTweens();
            _stats = stats;
            _finished = false;
            ApplyStartState();
            _routine = StartCoroutine(Run());
        }

        /// <summary>Show everything in its final state immediately.</summary>
        public void Skip()
        {
            if (!IsPlaying)
            {
                return;
            }

            StopRoutine();
            KillTweens();
            if (_header != null)
            {
                _header.Complete();
            }

            ApplyFinalState();
            Finish();
        }

        private IEnumerator Run()
        {
            if (_header != null)
            {
                yield return _header.PlayRoutine();
                yield return Wait(_gapAfterHeader);
            }
            else
            {
                yield return Wait(_initialDelay);
            }

            // Bubble + [Web] label pop at once.
            Pop(_bubble, _popDuration, _popEase);
            Pop(_webLabel, _popDuration, _popEase);
            PlayClip(_popClip);
            yield return Wait(_popDuration);

            yield return Type(_receivedLine, _typeInterval);
            yield return Wait(_sectionGap);

            // Rows slide down one after another.
            for (int i = 0; i < _rows.Length; i++)
            {
                RectTransform row = _rows[i];
                if (row == null)
                {
                    continue;
                }

                row.DOAnchorPos(_rowBasePos[i], _rowDuration).SetEase(Ease.OutCubic).SetUpdate(true);
                _rowGroups[i].DOFade(1f, _rowDuration).SetUpdate(true);
                yield return Wait(_rowInterval);
            }

            yield return Wait(_rowDuration + _sectionGap * 0.5f);

            // Values stamp in one by one.
            for (int i = 0; i < _statRows.Length; i++)
            {
                StatRow s = _statRows[i];
                if (s == null || s.value == null)
                {
                    continue;
                }

                SetValueText(s, i);
                s.value.alpha = 1f;
                RectTransform rt = s.value.rectTransform;
                rt.localScale = Vector3.one * _stampFromScale;
                rt.DOScale(1f, _stampDuration).SetEase(Ease.InQuad).SetUpdate(true)
                    .OnComplete(() => rt.DOPunchScale(Vector3.one * 0.15f, 0.2f, 6, 0.5f).SetUpdate(true));
                PlayClip(_stampClip);
                if (_stampFeedback != null)
                {
                    _stampFeedback.PlayFeedbacks();
                }

                yield return Wait(_stampInterval);
            }

            // Grade: soft elastic pop, then idle wobble.
            if (_gradeValue != null)
            {
                _gradeValue.text = _stats.Grade.ToString();
                _gradeValue.alpha = 1f;
                RectTransform rt = _gradeValue.rectTransform;
                rt.localScale = Vector3.zero;
                rt.DOScale(1f, _gradePopDuration).SetEase(_gradePopEase).SetUpdate(true)
                    .OnComplete(StartGradeWobble);
                PlayClip(_gradeClip);
                yield return Wait(_gradePopDuration);
            }

            yield return Wait(_sectionGap);
            yield return Type(_closingLine, _typeInterval);
            yield return Wait(_sectionGap);
            yield return Type(_thanksLine, _thanksInterval);

            _routine = null;
            Finish();
        }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            int count = _statRows.Length + (_gradeRow != null ? 1 : 0);
            _rows = new RectTransform[count];
            _rowBasePos = new Vector2[count];
            _rowGroups = new CanvasGroup[count];
            for (int i = 0; i < _statRows.Length; i++)
            {
                _rows[i] = _statRows[i] != null ? _statRows[i].row : null;
            }

            if (_gradeRow != null)
            {
                _rows[count - 1] = _gradeRow;
            }

            for (int i = 0; i < count; i++)
            {
                RectTransform row = _rows[i];
                if (row == null)
                {
                    continue;
                }

                _rowBasePos[i] = row.anchoredPosition;
                if (!row.TryGetComponent(out _rowGroups[i]))
                {
                    _rowGroups[i] = row.gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void ApplyStartState()
        {
            if (_header != null)
            {
                _header.PrepareHidden();
            }

            Hide(_bubble);
            Hide(_webLabel);
            HideText(_receivedLine);
            HideText(_closingLine);
            HideText(_thanksLine);

            for (int i = 0; i < _rows.Length; i++)
            {
                if (_rows[i] == null)
                {
                    continue;
                }

                _rows[i].anchoredPosition = _rowBasePos[i] + new Vector2(0f, _rowOffsetY);
                _rowGroups[i].alpha = 0f;
            }

            for (int i = 0; i < _statRows.Length; i++)
            {
                if (_statRows[i] != null && _statRows[i].value != null)
                {
                    _statRows[i].value.alpha = 0f;
                    _statRows[i].value.rectTransform.localScale = Vector3.one;
                }
            }

            if (_gradeValue != null)
            {
                _gradeValue.alpha = 0f;
                _gradeValue.rectTransform.localScale = Vector3.one;
            }
        }

        private void ApplyFinalState()
        {
            Show(_bubble);
            Show(_webLabel);
            ShowText(_receivedLine);
            ShowText(_closingLine);
            ShowText(_thanksLine);

            for (int i = 0; i < _rows.Length; i++)
            {
                if (_rows[i] == null)
                {
                    continue;
                }

                _rows[i].anchoredPosition = _rowBasePos[i];
                _rowGroups[i].alpha = 1f;
            }

            for (int i = 0; i < _statRows.Length; i++)
            {
                StatRow s = _statRows[i];
                if (s != null && s.value != null)
                {
                    SetValueText(s, i);
                    s.value.alpha = 1f;
                    s.value.rectTransform.localScale = Vector3.one;
                }
            }

            if (_gradeValue != null)
            {
                _gradeValue.text = _stats.Grade.ToString();
                _gradeValue.alpha = 1f;
                _gradeValue.rectTransform.localScale = Vector3.one;
                StartGradeWobble();
            }
        }

        private void Finish()
        {
            if (_finished)
            {
                return;
            }

            _finished = true;
            _onFinished?.Invoke();
        }

        private void SetValueText(StatRow s, int index)
        {
            int number = index switch
            {
                0 => _stats.ReportCount,
                1 => _stats.EmbracedCount,
                2 => _stats.NotEmbracedCount,
                3 => _stats.MistakeCount,
                _ => 0,
            };
            s.value.text = string.Format(string.IsNullOrEmpty(s.format) ? "{0}" : s.format, number);
        }

        private void StartGradeWobble()
        {
            if (_gradeValue == null || _gradeWobbleScale <= 1f)
            {
                return;
            }

            RectTransform rt = _gradeValue.rectTransform;
            rt.DOKill();
            rt.localScale = Vector3.one;
            rt.DOScale(_gradeWobbleScale, _gradeWobbleHalfPeriod)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        }

        private IEnumerator Type(TMP_Text text, float interval)
        {
            if (text == null)
            {
                yield break;
            }

            text.gameObject.SetActive(true);
            text.maxVisibleCharacters = 0;
            text.ForceMeshUpdate();
            int total = text.textInfo.characterCount;
            int shown = 0;
            float timer = 0f;
            while (shown < total)
            {
                timer += Time.unscaledDeltaTime;
                bool ticked = false;
                while (timer >= interval && shown < total)
                {
                    timer -= interval;
                    if (!char.IsWhiteSpace(text.textInfo.characterInfo[shown].character))
                    {
                        ticked = true;
                    }

                    shown++;
                }

                text.maxVisibleCharacters = shown;
                if (ticked)
                {
                    PlayClip(_typeClip);
                }

                yield return null;
            }

            text.maxVisibleCharacters = AllVisible;
        }

        private static void Pop(RectTransform target, float duration, Ease ease)
        {
            if (target == null)
            {
                return;
            }

            target.gameObject.SetActive(true);
            target.localScale = Vector3.zero;
            target.DOScale(1f, duration).SetEase(ease).SetUpdate(true);
        }

        private static void Hide(RectTransform target)
        {
            if (target != null)
            {
                target.gameObject.SetActive(false);
            }
        }

        private static void Show(RectTransform target)
        {
            if (target != null)
            {
                target.gameObject.SetActive(true);
                target.localScale = Vector3.one;
            }
        }

        private static void HideText(TMP_Text text)
        {
            if (text != null)
            {
                text.maxVisibleCharacters = 0;
            }
        }

        private static void ShowText(TMP_Text text)
        {
            if (text != null)
            {
                text.gameObject.SetActive(true);
                text.maxVisibleCharacters = AllVisible;
            }
        }

        private void KillTweens()
        {
            KillOn(_bubble);
            KillOn(_webLabel);
            for (int i = 0; i < _rows.Length; i++)
            {
                KillOn(_rows[i]);
                if (_rowGroups[i] != null)
                {
                    _rowGroups[i].DOKill();
                }
            }

            for (int i = 0; i < _statRows.Length; i++)
            {
                if (_statRows[i] != null && _statRows[i].value != null)
                {
                    _statRows[i].value.rectTransform.DOKill();
                }
            }

            if (_gradeValue != null)
            {
                _gradeValue.rectTransform.DOKill();
            }
        }

        private static void KillOn(RectTransform target)
        {
            if (target != null)
            {
                target.DOKill();
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

        private static IEnumerator Wait(float seconds)
        {
            while (seconds > 0f)
            {
                seconds -= Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
