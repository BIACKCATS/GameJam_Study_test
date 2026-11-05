using DG.Tweening;
using JSH.Core;
using JSH.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JSH.UI
{
    /// <summary>
    /// Last beat of the ending: a line fades in while sliding down into place (put it at screen center in the Editor),
    /// then the restart button ("Re?") fades in at the bottom. Wire EndingMessageSequence.onFinished -> Play,
    /// and Button.onClick -> Restart. Unscaled time.
    /// </summary>
    public sealed class EndingRestartPrompt : MonoBehaviour
    {
        [Header("Line (Editor position = final, e.g. screen center)")]
        [SerializeField] private RectTransform _line;
        [SerializeField, Min(0f)] private float _lineDelay = 1f;
        [SerializeField, Min(0.01f)] private float _lineDuration = 1.5f;
        [Tooltip("Line starts this far above its Editor position and slides down.")]
        [SerializeField] private float _lineOffsetY = 150f;
        [SerializeField] private Ease _lineEase = Ease.OutCubic;

        [Header("Dark overlay (optional, fades in with the line)")]
        [SerializeField] private CanvasGroup _overlay;
        [SerializeField, Range(0f, 1f)] private float _overlayAlpha = 0.85f;

        [Header("Restart button")]
        [SerializeField] private Button _restartButton;
        [SerializeField, Min(0f)] private float _buttonDelay = 0.8f;
        [SerializeField, Min(0.01f)] private float _buttonDuration = 0.6f;

        [Header("Restart target")]
        [Tooltip("Scene to load on restart (must be in Build Settings). Empty = build index 0.")]
        [SerializeField] private string _restartSceneName = "Title";

        private CanvasGroup _lineGroup;
        private CanvasGroup _buttonGroup;
        private Vector2 _lineBasePos;
        private Sequence _sequence;
        private bool _played;
        private bool _restarting;

        private void Awake()
        {
            if (_line != null)
            {
                _lineBasePos = _line.anchoredPosition;
                _lineGroup = GetOrAddGroup(_line.gameObject);
            }

            if (_restartButton != null)
            {
                _buttonGroup = GetOrAddGroup(_restartButton.gameObject);
            }

            ApplyHidden();
        }

        private void OnDisable()
        {
            _sequence?.Kill();
            _sequence = null;
        }

        /// <summary>Play once (wire to EndingMessageSequence.onFinished).</summary>
        public void Play()
        {
            if (_played)
            {
                return;
            }

            _played = true;
            ApplyHidden();
            _sequence = DOTween.Sequence().SetUpdate(true);
            _sequence.AppendInterval(_lineDelay);
            if (_line != null)
            {
                _line.gameObject.SetActive(true);
                _sequence.Append(_line.DOAnchorPos(_lineBasePos, _lineDuration).SetEase(_lineEase));
                _sequence.Join(_lineGroup.DOFade(1f, _lineDuration).SetEase(Ease.Linear));
            }

            if (_overlay != null)
            {
                _overlay.gameObject.SetActive(true);
                _sequence.Join(_overlay.DOFade(_overlayAlpha, _lineDuration).SetEase(Ease.Linear));
            }

            if (_buttonGroup != null)
            {
                _sequence.AppendInterval(_buttonDelay);
                _sequence.AppendCallback(() => _restartButton.gameObject.SetActive(true));
                _sequence.Append(_buttonGroup.DOFade(1f, _buttonDuration));
                _sequence.AppendCallback(() =>
                {
                    _buttonGroup.interactable = true;
                    _buttonGroup.blocksRaycasts = true;
                });
            }
        }

        /// <summary>Wire to the "Re?" Button.onClick.</summary>
        public void Restart()
        {
            if (_restarting)
            {
                return;
            }

            _restarting = true;
            EndingStats.ClearLast();
            if (GameManager.HasInstance)
            {
                GameManager.Instance.GoToTitle();
            }

            bool useName = !string.IsNullOrEmpty(_restartSceneName);
            if (SceneLoader.HasInstance)
            {
                if (useName)
                {
                    SceneLoader.Instance.Load(_restartSceneName);
                }
                else
                {
                    SceneLoader.Instance.Load(0);
                }

                return;
            }

            if (useName)
            {
                SceneManager.LoadScene(_restartSceneName);
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

        private void ApplyHidden()
        {
            if (_line != null)
            {
                _line.anchoredPosition = _lineBasePos + new Vector2(0f, _lineOffsetY);
                _lineGroup.alpha = 0f;
            }

            if (_overlay != null)
            {
                _overlay.alpha = 0f;
                _overlay.blocksRaycasts = false;
            }

            if (_buttonGroup != null)
            {
                _buttonGroup.alpha = 0f;
                _buttonGroup.interactable = false;
                _buttonGroup.blocksRaycasts = false;
            }
        }

        private static CanvasGroup GetOrAddGroup(GameObject go)
        {
            if (!go.TryGetComponent(out CanvasGroup group))
            {
                group = go.AddComponent<CanvasGroup>();
            }

            return group;
        }
    }
}
