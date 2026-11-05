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
    /// Put on an always-active object of the gameplay Canvas (not on the panel it shows/hides).
    /// </summary>
    public sealed class DialogueView : MonoBehaviour
    {
        [Tooltip("Whole dialogue screen (portrait + ring + box). Faded in/out.")]
        [SerializeField] private UIPanel _panel;
        [SerializeField] private TMP_Text _speakerText;
        [SerializeField] private TMP_Text _bodyText;
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
            if (_speakerText != null)
                _speakerText.text = string.IsNullOrEmpty(node.Speaker) && _target != null ? _target.DisplayName : node.Speaker;
            if (_bodyText != null) _bodyText.text = node.Text;

            bool hasChoices = node.ChoiceCount > 0;
            if (_choices != null)
            {
                if (hasChoices) _choices.Show(node);
                else _choices.Hide();
            }
            if (_nextIndicator != null) _nextIndicator.SetActive(!hasChoices);
        }

        private void OnEnded(Talkable target)
        {
            _target = null;
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

            if (node.ChoiceCount > 0)
            {
                if (click && _choices != null && _choices.SelectedIndex >= 0) _runner.Choose(_choices.SelectedIndex);
                return;
            }

            Keyboard kb = Keyboard.current;
            bool key = kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame);
            if (click || key) _runner.Advance();
        }
    }
}
