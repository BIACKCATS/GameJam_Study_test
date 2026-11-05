using JSH.Core;
using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Counts the ending numbers during one run (put on the gameplay scene's [Managers]) and keeps
    /// EndingStats.Last up to date, so the ending scene can read it after any scene load.
    ///  - Embraced     : VillainConvertedEvent
    ///  - Not embraced : VillainDefeatedEvent + villains left unresolved when a car was judged
    ///  - Reports      : embraced + not embraced (every villain dealt with or passed)
    ///  - Mistakes     : counterattack because a normal passenger was accused
    ///  - Grade        : embrace ratio and mistakes (thresholds below)
    /// Counts reset when this component is enabled (new run / scene reload).
    /// </summary>
    public sealed class EndingStatsTracker : MonoBehaviour
    {
        [Header("Peace grade")]
        [Tooltip("S: embrace ratio at least this AND mistakes at most Max Mistakes For S.")]
        [SerializeField, Range(0f, 1f)] private float _sRatio = 0.8f;
        [SerializeField, Min(0)] private int _maxMistakesForS = 0;
        [Tooltip("A: embrace ratio at least this. Otherwise B.")]
        [SerializeField, Range(0f, 1f)] private float _aRatio = 0.5f;

        private int _embraced;
        private int _notEmbraced;
        private int _mistakes;

        public EndingStats Current => Build();

        private void OnEnable()
        {
            _embraced = 0;
            _notEmbraced = 0;
            _mistakes = 0;
            EmbracedVillains.Clear();
            Publish();

            EventBus<VillainConvertedEvent>.Subscribe(OnConverted);
            EventBus<VillainDefeatedEvent>.Subscribe(OnDefeated);
            EventBus<CarChangedEvent>.Subscribe(OnCarChanged);
            EventBus<TrainEndReachedEvent>.Subscribe(OnTrainEnd);
            EventBus<CounterattackEvent>.Subscribe(OnCounterattack);
        }

        private void OnDisable()
        {
            EventBus<VillainConvertedEvent>.Unsubscribe(OnConverted);
            EventBus<VillainDefeatedEvent>.Unsubscribe(OnDefeated);
            EventBus<CarChangedEvent>.Unsubscribe(OnCarChanged);
            EventBus<TrainEndReachedEvent>.Unsubscribe(OnTrainEnd);
            EventBus<CounterattackEvent>.Unsubscribe(OnCounterattack);
        }

        private void OnConverted(VillainConvertedEvent evt)
        {
            _embraced++;
            if (evt.Villain != null) EmbracedVillains.Mark(evt.Villain.Id);
            Publish();
        }

        private void OnDefeated(VillainDefeatedEvent evt)
        {
            _notEmbraced++;
            Publish();
        }

        // Each car is judged exactly once: CarChangedEvent for every car but the last, TrainEndReachedEvent for the last.
        private void OnCarChanged(CarChangedEvent evt) => AddUnresolved(evt.UnresolvedVillains);

        private void OnTrainEnd(TrainEndReachedEvent evt) => AddUnresolved(evt.UnresolvedVillains);

        private void OnCounterattack(CounterattackEvent evt)
        {
            if (evt.Reason != CounterattackReason.AccusedPassenger)
            {
                return;
            }

            _mistakes++;
            Publish();
        }

        private void AddUnresolved(int count)
        {
            if (count <= 0)
            {
                return;
            }

            _notEmbraced += count;
            Publish();
        }

        /// <summary>Inspector context menu (Play Mode): print the current numbers to the Console.</summary>
        [ContextMenu("Log Current Stats")]
        private void LogCurrent()
        {
            EndingStats s = Build();
            Debug.Log($"[EndingStatsTracker] reports={s.ReportCount} embraced={s.EmbracedCount} " +
                      $"notEmbraced={s.NotEmbracedCount} mistakes={s.MistakeCount} grade={s.Grade} " +
                      $"embracedVillains: danso={EmbracedVillains.IsEmbraced(VillainId.Danso)} " +
                      $"sleepyMan={EmbracedVillains.IsEmbraced(VillainId.SleepyMan)} " +
                      $"hipHop={EmbracedVillains.IsEmbraced(VillainId.HipHop)} " +
                      $"jarvan={EmbracedVillains.IsEmbraced(VillainId.Jarvan)} " +
                      $"knight={EmbracedVillains.IsEmbraced(VillainId.Knight)}", this);
        }

        private void Publish() => EndingStats.SetLast(Build());

        private EndingStats Build()
        {
            int reports = _embraced + _notEmbraced;
            float ratio = reports > 0 ? (float)_embraced / reports : 1f;
            PeaceGrade grade;
            if (ratio >= _sRatio && _mistakes <= _maxMistakesForS)
            {
                grade = PeaceGrade.S;
            }
            else if (ratio >= _aRatio)
            {
                grade = PeaceGrade.A;
            }
            else
            {
                grade = PeaceGrade.B;
            }

            return new EndingStats(reports, _embraced, _notEmbraced, _mistakes, grade);
        }
    }
}
