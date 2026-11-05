using System;
using JSH.Core;
using JSH.Data;
using JSH.Gameplay;
using TMPro;
using UnityEngine;

namespace JSH.UI
{
    /// <summary>
    /// Tutorial shown only in the first car (TrainProgress index 0). Put on the tutorial Canvas (or [UI]).
    ///  1. Controls  - hint panels ("A D To move", "Shift To Aim", "Click To Talk"): visible while the player
    ///                 roams the first car; hidden during dialogue / minigame / pause / car change; gone for good
    ///                 once the player leaves the first car.
    ///  2. Dialogue  - shown the first time a dialogue starts in the first car, while it runs. Done when it ends.
    ///  3. Minigame  - shown the first time a Timing minigame starts in the first car, while the Timing round runs.
    ///                 Done when the round ends (success, fail -> counterattack, or session end).
    /// Overlay only: never pauses or blocks input. Each panel needs a UIPanel (CanvasGroup fade).
    /// A tip's Message (if not empty) is written into its Label at Awake, so texts are edited here in the Inspector.
    /// </summary>
    public sealed class FirstCarTutorial : MonoBehaviour
    {
        [Serializable]
        private sealed class Tip
        {
            [SerializeField] private UIPanel _panel;
            [Tooltip("Optional. Receives Message at Awake.")]
            [SerializeField] private TMP_Text _label;
            [Tooltip("Empty = keep the label's own text.")]
            [SerializeField, TextArea(2, 5)] private string _message;

            private bool _visible;

            public Tip() { }
            public Tip(string message) => _message = message;

            public void Init()
            {
                if (_label != null && !string.IsNullOrEmpty(_message)) _label.text = _message;
                _visible = false;
                if (_panel != null) _panel.SetVisible(false, true);
            }

            /// <summary>Fades only on change (calling every frame is fine).</summary>
            public void SetVisible(bool visible)
            {
                if (_visible == visible || _panel == null) return;
                _visible = visible;
                _panel.SetVisible(visible, false);
            }
        }

        private enum Progress { NotShown, Showing, Done }

        [Header("1. Controls (first car, free roaming)")]
        [SerializeField] private Tip[] _controlTips = Array.Empty<Tip>();

        [Header("2. First dialogue")]
        [SerializeField] private Tip _dialogueTip = new Tip(
            "대화 씬에 도달하셨군요!\n여러분들은 다양한 사람들과\n대화를 할 것입니다.\n대화를 나눠보세요.");

        [Header("3. First minigame")]
        [SerializeField] private Tip _minigameTip = new Tip(
            "대화 도중 특정 조건에 도달할 경우\n미니게임이 성사됩니다.\n대화의 결과를 확인해보세요.");

        [Tooltip("Off = skip the whole tutorial (testing).")]
        [SerializeField] private bool _tutorialEnabled = true;

        private Progress _dialogueProgress;
        private Progress _minigameProgress;
        private bool _finished;

        private void Awake()
        {
            for (int i = 0; i < _controlTips.Length; i++) _controlTips[i].Init();
            _dialogueTip.Init();
            _minigameTip.Init();
            if (!_tutorialEnabled) _finished = true;
        }

        private void Update()
        {
            if (_finished) return;

            bool firstCar = !TrainProgress.HasInstance || TrainProgress.Instance.CurrentIndex == 0;
            if (!firstCar)
            {
                Finish();
                return;
            }

            bool playing = !GameManager.HasInstance || GameManager.Instance.IsPlaying;
            bool transitioning = TrainProgress.HasInstance && TrainProgress.Instance.IsTransitioning;
            bool canShow = playing && !transitioning;

            bool dialogue = DialogueRunner.HasInstance && DialogueRunner.Instance.IsActive;
            bool minigame = MinigameDirector.HasInstance && MinigameDirector.Instance.IsActive;
            bool timing = minigame && MinigameDirector.Instance.CurrentType == MinigameType.Timing;

            bool roaming = canShow && !dialogue && !minigame;
            for (int i = 0; i < _controlTips.Length; i++) _controlTips[i].SetVisible(roaming);

            Step(ref _dialogueProgress, _dialogueTip, dialogue && !minigame, canShow);
            Step(ref _minigameProgress, _minigameTip, timing, canShow);
        }

        /// <summary>Shows the tip the first time its condition holds; done once the condition ends.</summary>
        private static void Step(ref Progress progress, Tip tip, bool condition, bool canShow)
        {
            if (progress == Progress.Done) return;
            if (condition)
            {
                progress = Progress.Showing;
                tip.SetVisible(canShow); // pause hides it temporarily
            }
            else if (progress == Progress.Showing)
            {
                progress = Progress.Done;
                tip.SetVisible(false);
            }
        }

        private void Finish()
        {
            _finished = true;
            for (int i = 0; i < _controlTips.Length; i++) _controlTips[i].SetVisible(false);
            _dialogueTip.SetVisible(false);
            _minigameTip.SetVisible(false);
            _dialogueProgress = Progress.Done;
            _minigameProgress = Progress.Done;
        }
    }
}
