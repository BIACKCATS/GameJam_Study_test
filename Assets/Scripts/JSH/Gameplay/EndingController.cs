using JSH.Core;
using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Decides the ending (put on the scene's [Managers]).
    ///  - Lives reach 0                  -> bad ending  (GameManager.GameOver)
    ///  - End of the train with lives > 0 -> good ending (GameManager.Clear)
    /// The ending is applied after a short delay so feedback (hit, counterattack) can play, and never inside
    /// another system's event callback. It closes any open dialogue / minigame first. Panels: GameFlowUI.
    /// </summary>
    public class EndingController : MonoBehaviour
    {
        private enum Ending { None, Bad, Good }

        [Tooltip("Seconds between the trigger and the ending screen (scaled time; pausing delays it).")]
        [SerializeField, Min(0f)] private float _badEndingDelay = 1f;
        [SerializeField, Min(0f)] private float _goodEndingDelay = 0.5f;
        [Tooltip("Play the game-over / true-ending presentations (TempFolderStage) after the state change.")]
        [SerializeField] private bool _playPresentations = true;

        private Ending _pending = Ending.None;
        private float _timer;
        private bool _applied;

        public bool IsEndingPending => _pending != Ending.None;

        private void OnEnable()
        {
            EventBus<LivesDepletedEvent>.Subscribe(OnLivesDepleted);
            EventBus<TrainEndReachedEvent>.Subscribe(OnTrainEndReached);
        }

        private void OnDisable()
        {
            EventBus<LivesDepletedEvent>.Unsubscribe(OnLivesDepleted);
            EventBus<TrainEndReachedEvent>.Unsubscribe(OnTrainEndReached);
        }

        private void OnLivesDepleted(LivesDepletedEvent evt) => Schedule(Ending.Bad, _badEndingDelay);

        private void OnTrainEndReached(TrainEndReachedEvent evt)
        {
            // The last car was judged first; if that cost the last life, the bad ending is already pending.
            if (LifeSystem.HasInstance && LifeSystem.Instance.IsDepleted) return;
            Schedule(Ending.Good, _goodEndingDelay);
        }

        private void Schedule(Ending ending, float delay)
        {
            if (_applied) return;
            if (_pending == Ending.Bad) return; // bad ending wins
            _pending = ending;
            _timer = delay;
        }

        private void Update()
        {
            if (_pending == Ending.None || _applied) return;
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            Apply(_pending);
        }

        private void Apply(Ending ending)
        {
            _applied = true;
            _pending = Ending.None;

            if (MinigameDirector.HasInstance && MinigameDirector.Instance.IsActive) MinigameDirector.Instance.Cancel();
            if (DialogueRunner.HasInstance && DialogueRunner.Instance.IsActive) DialogueRunner.Instance.EndDialogue();

            if (!GameManager.HasInstance)
            {
                Debug.LogWarning($"[EndingController] {ending} ending, but no GameManager in the scene.", this);
                return;
            }
            if (ending == Ending.Bad) GameManager.Instance.GameOver();
            else GameManager.Instance.Clear();

            if (_playPresentations) PlayPresentation(ending);
        }

        // Teammate presentations (hty TempFolderStage: GameOverPresentation / TrueEndingPresentation).
        // Their "Re" buttons return to the title scene. If a presentation is missing, GameFlowUI's panel stays the fallback.
        private static void PlayPresentation(Ending ending)
        {
            if (ending == Ending.Bad)
            {
                GameOverSequenceController.PlayGameOver();
                return;
            }

            TrueEndingController.PlayTrueEnding(
                EmbracedVillains.IsEmbraced(VillainId.Danso),
                EmbracedVillains.IsEmbraced(VillainId.SleepyMan),
                EmbracedVillains.IsEmbraced(VillainId.HipHop),
                EmbracedVillains.IsEmbraced(VillainId.Jarvan),
                EmbracedVillains.IsEmbraced(VillainId.Knight));
        }
    }
}
