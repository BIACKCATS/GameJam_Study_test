using Febucci.TextAnimatorForUnity;
using JSH.Core;
using JSH.Data;
using JSH.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JSH.UI
{
    /// <summary>
    /// JRPG-style dialogue screen: portrait in the center (hexagon), dialogue box at the bottom,
    /// radial choices around the portrait. Listens to DialogueRunner; owns dialogue input:
    ///  - node with choices: Left Click confirms the choice selected by mouse direction (RadialChoiceSelector)
    ///  - node without choices: Left Click / Space / Enter = next
    ///  - while text is still being typed (Text Animator typewriter): the same input reveals it all instead
    /// Choices and the "next" marker appear only after the body text is fully shown.
    /// Text supports DialogueTextTags (e.g. &lt;angry&gt;) and any Text Animator / TMP tag.
    /// Put on an always-active object of the gameplay Canvas (not on the panel it shows/hides).
    /// </summary>
    public sealed class DialogueView : MonoBehaviour
    {
        [Tooltip("Whole dialogue screen (portrait + ring + box). Faded in/out.")]
        [SerializeField] private UIPanel _panel;
        [SerializeField] private TMP_Text _speakerText;
        [SerializeField] private TMP_Text _bodyText;
        [Tooltip("Text Animator typewriter on the Speaker text (optional). Retypes only when the name changes.")]
        [SerializeField] private TypewriterComponent _speakerTypewriter;
        [Tooltip("Text Animator typewriter on the Body text (optional). Empty = text appears instantly.")]
        [SerializeField] private TypewriterComponent _bodyTypewriter;
        [SerializeField] private Image _portraitImage;
        [Tooltip("Used when the Talkable has no portrait.")]
        [SerializeField] private Sprite _fallbackPortrait;
        [Tooltip("\"Next\" marker shown on nodes without choices (optional).")]
        [SerializeField] private GameObject _nextIndicator;
        [SerializeField] private RadialChoiceSelector _choices;

        private DialogueRunner _runner;
        private Talkable _target;
        private int _openedFrame = -1;
        private bool _started;
        private string _shownSpeaker;
        private DialogueNode _pendingNode; // choices / next marker wait until the body is fully typed
        private int _pendingFrame = -1;

        private void Start()
        {
            _started = true;
            Subscribe();
            if (_panel != null) _panel.SetVisible(false, true);
            if (_choices != null) _choices.Hide();
            if (_nextIndicator != null) _nextIndicator.SetActive(false);
        }

        private void OnEnable()
        {
            if (_started) Subscribe();
        }

        private void OnDisable() => Unsubscribe();

        // Runner subscription happens in Start so the DialogueRunner singleton has run Awake.
        private void Subscribe()
        {
            if (_runner != null || !DialogueRunner.HasInstance) return;
            _runner = DialogueRunner.Instance;
            _runner.Started += OnStarted;
            _runner.NodeShown += OnNodeShown;
            _runner.Ended += OnEnded;
        }

        private void Unsubscribe()
        {
            if (_runner == null) return;
            _runner.Started -= OnStarted;
            _runner.NodeShown -= OnNodeShown;
            _runner.Ended -= OnEnded;
            _runner = null;
        }

        private void OnStarted(Talkable target)
        {
            _target = target;
            _shownSpeaker = null;
            _pendingNode = null;
            _openedFrame = Time.frameCount; // the Shift+Click that opened the dialogue must not confirm a choice

            if (_portraitImage != null)
            {
                Sprite portrait = target != null && target.Portrait != null ? target.Portrait : _fallbackPortrait;
                _portraitImage.sprite = portrait;
                _portraitImage.enabled = portrait != null;
            }
            if (_panel != null) _panel.Show();
        }

        private void OnNodeShown(DialogueNode node)
        {
            string speaker = string.IsNullOrEmpty(node.Speaker) && _target != null ? _target.DisplayName : node.Speaker;
            if (speaker != _shownSpeaker)
            {
                _shownSpeaker = speaker;
                ShowText(_speakerText, _speakerTypewriter, speaker);
            }
            ShowText(_bodyText, _bodyTypewriter, node.Text);

            // Hide the previous node's choices / marker; they come back once this body is fully shown.
            if (_choices != null) _choices.Hide();
            if (_nextIndicator != null) _nextIndicator.SetActive(false);
            _pendingNode = node;
            _pendingFrame = Time.frameCount;
            // Without typewriters show controls now; with them, Update decides from the next frame on
            // (so a typewriter that starts a frame late can't let the choices pop up early).
            if (_bodyTypewriter == null && _speakerTypewriter == null) ShowNodeControls();
        }

        private static void ShowText(TMP_Text label, TypewriterComponent typewriter, string text)
        {
            string shown = DialogueTextTags.Expand(text);
            if (typewriter != null) typewriter.ShowText(shown);
            else if (label != null) label.text = shown;
        }

        private bool IsBodyTyping => (_bodyTypewriter != null && _bodyTypewriter.IsShowingText)
                                     || (_speakerTypewriter != null && _speakerTypewriter.IsShowingText);

        private void SkipBodyTyping()
        {
            if (_speakerTypewriter != null && _speakerTypewriter.IsShowingText) _speakerTypewriter.SkipTypewriter();
            if (_bodyTypewriter != null && _bodyTypewriter.IsShowingText) _bodyTypewriter.SkipTypewriter();
        }

        private void ShowNodeControls()
        {
            DialogueNode node = _pendingNode;
            _pendingNode = null;
            if (node == null) return;
            bool hasChoices = node.ChoiceCount > 0;
            if (_choices != null && hasChoices) _choices.Show(node);
            if (_nextIndicator != null) _nextIndicator.SetActive(!hasChoices);
        }

        private void OnEnded(Talkable target)
        {
            _target = null;
            _pendingNode = null;
            _shownSpeaker = null;
            if (_choices != null) _choices.Hide();
            if (_nextIndicator != null) _nextIndicator.SetActive(false);
            if (_panel != null) _panel.Hide();
        }

        private void Update()
        {
            if (_runner == null || !_runner.IsActive) return;
            if (Time.frameCount == _openedFrame) return;
            if (GameManager.HasInstance && GameManager.Instance.IsPaused) return;

            DialogueNode node = _runner.CurrentNode;
            if (node == null) return;

            Mouse mouse = Mouse.current;
            bool click = mouse != null && mouse.leftButton.wasPressedThisFrame;
            Keyboard kb = Keyboard.current;
            bool key = kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame);

            // 1) Body still typing: input reveals it; choices wait.
            if (IsBodyTyping)
            {
                if (click || key) SkipBodyTyping();
                return;
            }
            if (_pendingNode != null)
            {
                if (Time.frameCount > _pendingFrame) ShowNodeControls();
                return; // no input on the frame the choices appear
            }

            if (node.ChoiceCount > 0)
            {
                if (_choices == null) return;
                // 2) Choice labels still typing: input reveals them instead of choosing.
                if (_choices.IsTyping)
                {
                    if (click || key) _choices.SkipTyping();
                    return;
                }
                if (click && _choices.SelectedIndex >= 0) _runner.Choose(_choices.SelectedIndex);
                return;
            }

            if (click || key) _runner.Advance();
        }
    }
}
