using DG.Tweening;
using JSH.Core;
using JSH.Data;
using JSH.Gameplay;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JSH.UI
{
    /// <summary>
    /// Screen for the Timing (설득) minigame, Undertale attack-bar style: a wide box, the target zone,
    /// and a vertical line sweeping left/right. Reads MinigameDirector.ActiveTiming every frame (no logic here;
    /// input stays in MinigameDirector). On finish the line freezes and flashes the result, then the panel hides
    /// (on fail the Counterattack event starts underneath at the same time).
    /// Put on an always-active object of the gameplay Canvas (not on the panel it shows/hides).
    /// Result feedback (screen shake / flash) = optional MMF_Players.
    /// </summary>
    public sealed class TimingMinigameView : MonoBehaviour
    {
        [SerializeField] private UIPanel _panel;
        [Tooltip("The wide judgment box. Its width maps the normalized range [-1, 1]. Pivot/anchor X must be centered.")]
        [SerializeField] private RectTransform _track;
        [Tooltip("Child of _track (anchor + pivot center). Positioned/sized from TargetMin..TargetMax each round.")]
        [SerializeField] private RectTransform _targetZone;
        [Tooltip("The sweeping vertical line. Child of _track (anchor + pivot center).")]
        [SerializeField] private RectTransform _cursor;
        [SerializeField] private Graphic _cursorGraphic;
        [Tooltip("Image Type = Filled. Remaining time (1 = full).")]
        [SerializeField] private Image _timeFill;
        [SerializeField] private TMP_Text _promptText;

        [Header("Look")]
        [SerializeField] private Color _cursorColor = Color.white;
        [SerializeField] private Color _cursorReadyColor = new Color(1f, 1f, 1f, 0.4f);
        [SerializeField] private Color _successColor = new Color(1f, 0.85f, 0.1f, 1f);
        [SerializeField] private Color _failColor = new Color(0.9f, 0.15f, 0.15f, 1f);
        [SerializeField] private string _readyMessage = "준비...";
        [SerializeField] private string _playMessage = "클릭으로 멈추기!";
        [SerializeField] private string _successMessage = "설득 성공!";
        [SerializeField] private string _missMessage = "설득 실패...";
        [Tooltip("Used when the Timing minigame decides a 퇴치 (MinigameDirector.CurrentGoal = Defeat).")]
        [SerializeField] private string _defeatSuccessMessage = "퇴치 성공!";
        [SerializeField] private string _defeatMissMessage = "퇴치 실패...";
        [SerializeField] private string _timeoutMessage = "시간 초과...";

        [Header("Result")]
        [Tooltip("Seconds the frozen result stays on screen before the panel hides.")]
        [SerializeField, Min(0f)] private float _resultHoldTime = 0.7f;
        [SerializeField, Min(0f)] private float _cursorBlinkInterval = 0.08f;
        [SerializeField] private MMF_Player _successFeedback;
        [SerializeField] private MMF_Player _failFeedback;

        private TimingMinigame _current;
        private float _timeLimit = 1f;
        private float _resultLeft;
        private float _blinkLeft;
        private bool _showingResult;
        private bool _resultSuccess;
        private bool _wasReady;
        private Tween _punch;

        private void OnEnable() => EventBus<MinigameFinishedEvent>.Subscribe(OnFinished);

        private void OnDisable()
        {
            EventBus<MinigameFinishedEvent>.Unsubscribe(OnFinished);
            _punch?.Kill();
        }

        private void Start()
        {
            if (_panel != null) _panel.SetVisible(false, true);
        }

        private void Update()
        {
            if (_showingResult)
            {
                TickResult(Time.deltaTime);
                return;
            }

            TimingMinigame timing = MinigameDirector.HasInstance ? MinigameDirector.Instance.ActiveTiming : null;
            if (timing != _current)
            {
                _current = timing;
                if (timing != null) BeginRound(timing);
                else Hide(); // session cancelled (ending, scene change) without a finish event
            }
            if (_current == null) return;

            SetCursor(_current.Position);
            if (_timeFill != null) _timeFill.fillAmount = Mathf.Clamp01(_current.TimeLeft / _timeLimit);
            if (_current.IsReady != _wasReady)
            {
                _wasReady = _current.IsReady;
                ApplyReadyLook(_wasReady);
            }
        }

        private void BeginRound(TimingMinigame timing)
        {
            _timeLimit = Mathf.Max(0.01f, timing.TimeLeft); // TimeLeft starts at the full limit
            _wasReady = timing.IsReady;
            LayoutTargetZone(timing.TargetMin, timing.TargetMax);
            ApplyReadyLook(_wasReady);
            if (_timeFill != null) _timeFill.fillAmount = 1f;
            if (_cursor != null) _cursor.localScale = Vector3.one;
            SetCursor(timing.Position);
            if (_panel != null) _panel.Show();
        }

        // Raised by MinigameDirector before it clears ActiveTiming, so the final state is still readable here.
        private void OnFinished(MinigameFinishedEvent evt)
        {
            if (evt.Type != MinigameType.Timing || _current == null) return;

            SetCursor(_current.Position);
            _resultSuccess = evt.Success;
            bool defeat = MinigameDirector.HasInstance && MinigameDirector.Instance.CurrentGoal == DialogueOutcome.Defeat;
            if (_promptText != null)
                _promptText.text = evt.Success ? (defeat ? _defeatSuccessMessage : _successMessage)
                    : _current.TimedOut ? _timeoutMessage
                    : defeat ? _defeatMissMessage : _missMessage;
            if (_cursorGraphic != null) _cursorGraphic.color = evt.Success ? _successColor : _failColor;

            if (_cursor != null && _cursor.gameObject.activeInHierarchy)
            {
                _punch?.Kill();
                _cursor.localScale = Vector3.one;
                _punch = _cursor.DOPunchScale(new Vector3(0.6f, 0.15f, 0f), 0.25f, 8, 0.5f);
            }

            MMF_Player feedback = evt.Success ? _successFeedback : _failFeedback;
            if (feedback != null) feedback.PlayFeedbacks();

            _current = null;
            _showingResult = true;
            _resultLeft = _resultHoldTime;
            _blinkLeft = _cursorBlinkInterval;
        }

        private void TickResult(float dt)
        {
            _resultLeft -= dt;
            if (_cursorGraphic != null && _cursorBlinkInterval > 0f)
            {
                _blinkLeft -= dt;
                if (_blinkLeft <= 0f)
                {
                    _blinkLeft = _cursorBlinkInterval;
                    _cursorGraphic.enabled = !_cursorGraphic.enabled;
                }
            }
            if (_resultLeft > 0f) return;

            _showingResult = false;
            if (_cursorGraphic != null) _cursorGraphic.enabled = true;
            Hide();
        }

        private void Hide()
        {
            if (_panel != null && _panel.IsVisible) _panel.Hide();
        }

        private void ApplyReadyLook(bool ready)
        {
            if (_cursorGraphic != null)
            {
                _cursorGraphic.enabled = true;
                _cursorGraphic.color = ready ? _cursorColor : _cursorReadyColor;
            }
            if (_promptText != null) _promptText.text = ready ? _playMessage : _readyMessage;
        }

        private void LayoutTargetZone(float min, float max)
        {
            if (_targetZone == null || _track == null) return;
            float half = _track.rect.width * 0.5f;
            Vector2 pos = _targetZone.anchoredPosition;
            pos.x = (min + max) * 0.5f * half;
            _targetZone.anchoredPosition = pos;
            _targetZone.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (max - min) * half);
        }

        private void SetCursor(float normalized)
        {
            if (_cursor == null || _track == null) return;
            Vector2 pos = _cursor.anchoredPosition;
            pos.x = normalized * _track.rect.width * 0.5f;
            _cursor.anchoredPosition = pos;
        }
    }
}
