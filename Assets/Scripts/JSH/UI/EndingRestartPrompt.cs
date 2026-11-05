using System.Collections;
using DG.Tweening;
using JSH.Core;
using JSH.Gameplay;
using UnityEngine;
using BC.Core;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JSH.UI
{
    /// <summary>
    /// Ending frame around EndingMessageSequence:
    ///  1. Intro (Play, on start): screen fades to black while the line fades in sliding down into place
    ///     (put it at screen center in the Editor), holds (stays visible by default), then onIntroFinished
    ///     (wire to EndingMessageSequence.Play and turn off its Play On Start).
    ///  2. Outro (ShowRestart, wire to EndingMessageSequence.onFinished): the "Re?" button fades in at the bottom.
    /// Button.onClick -> Restart. Keep the overlay behind the bubble UI (first sibling) and the line in front.
    /// Unscaled time.
    /// </summary>
    public sealed class EndingRestartPrompt : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField] private bool _playOnStart = true;
        [SerializeField] private UnityEvent _onIntroFinished;

        [Header("Black overlay (fades in with the line, stays black)")]
        [SerializeField] private CanvasGroup _overlay;
        [SerializeField, Range(0f, 1f)] private float _overlayAlpha = 1f;

        [Header("Line (Editor position = final, e.g. screen center)")]
        [SerializeField] private RectTransform _line;
        [SerializeField, Min(0f)] private float _lineDelay = 0.5f;
        [SerializeField, Min(0.01f)] private float _lineDuration = 1.5f;
        [Tooltip("Line starts this far above its Editor position and slides down.")]
        [SerializeField] private float _lineOffsetY = 150f;
        [SerializeField] private Ease _lineEase = Ease.OutCubic;
        [SerializeField, Min(0f)] private float _lineHold = 1.5f;
        [Tooltip("0 = the line stays on screen.")]
        [SerializeField, Min(0f)] private float _lineFadeOut;
        [SerializeField, Min(0f)] private float _gapAfterIntro = 0.5f;

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
        private Sequence _introSequence;
        private Sequence _buttonSequence;
        private bool _introPlayed;
        private bool _buttonShown;
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

        private void OnDisable()
        {
            _introSequence?.Kill();
            _introSequence = null;
            _buttonSequence?.Kill();
            _buttonSequence = null;
        }

        /// <summary>Intro: black screen + line, then onIntroFinished. Plays once.</summary>
        public void Play()
        {
            if (_introPlayed)
            {
                return;
            }

            _introPlayed = true;
            _introSequence = DOTween.Sequence().SetUpdate(true);
            _introSequence.AppendInterval(_lineDelay);
            if (_line != null)
            {
                _line.gameObject.SetActive(true);
                _introSequence.AppendCallback(()=>{if(SoundManager.Instance)SoundManager.Instance.PlaySound(SoundNames.PaperStitching,0.8f);});
                _introSequence.Append(_line.DOAnchorPos(_lineBasePos, _lineDuration).SetEase(_lineEase));
                _introSequence.Join(_lineGroup.DOFade(1f, _lineDuration).SetEase(Ease.Linear));
            }

            if (_overlay != null)
            {
                _overlay.gameObject.SetActive(true);
                _introSequence.Join(_overlay.DOFade(_overlayAlpha, _lineDuration).SetEase(Ease.Linear));
            }

            _introSequence.AppendInterval(_lineHold);
            if (_line != null && _lineFadeOut > 0f)
            {
                _introSequence.Append(_lineGroup.DOFade(0f, _lineFadeOut));
            }

            _introSequence.AppendInterval(_gapAfterIntro);
            _introSequence.AppendCallback(() =>
            {
                _introSequence = null;
                _onIntroFinished?.Invoke();
            });
        }

        /// <summary>Fade in the "Re?" button (wire to EndingMessageSequence.onFinished). Plays once.</summary>
        public void ShowRestart()
        {
            if (_buttonShown || _buttonGroup == null)
            {
                return;
            }

            _buttonShown = true;
            _buttonSequence = DOTween.Sequence().SetUpdate(true);
            _buttonSequence.AppendInterval(_buttonDelay);
            _buttonSequence.AppendCallback(() => _restartButton.gameObject.SetActive(true));
            _buttonSequence.Append(_buttonGroup.DOFade(1f, _buttonDuration));
            _buttonSequence.AppendCallback(() =>
            {
                _buttonGroup.interactable = true;
                _buttonGroup.blocksRaycasts = true;
                _buttonSequence = null;
            });
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
