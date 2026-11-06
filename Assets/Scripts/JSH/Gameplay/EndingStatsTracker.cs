using JSH.Core;
using JSH.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    /// Counts reset when a gameplay scene is loaded, including reloads while already Playing.
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
        private int _lastCountedCar = -1; // highest car index whose unresolved villains are already counted
        private static EndingStatsTracker _owner;
        private bool _initialized;
        private int _runSceneHandle = -1;

        public EndingStats Current => Build();

        private void OnEnable()
        {
            // A new scene's [Managers] may be scheduled for destruction by Singleton.Awake.
            // Its tracker must not clear the surviving owner's data or subscribe a second time.
            if (_owner != null && _owner != this)
            {
                enabled = false;
                return;
            }
            _owner = this;
            if (!_initialized)
            {
                ResetRun();
                _initialized = true;
            }
            SceneManager.sceneLoaded += OnSceneLoaded;

            EventBus<VillainConvertedEvent>.Subscribe(OnConverted);
            EventBus<VillainDefeatedEvent>.Subscribe(OnDefeated);
            EventBus<CarChangedEvent>.Subscribe(OnCarChanged);
            EventBus<TrainEndReachedEvent>.Subscribe(OnTrainEnd);
            EventBus<LivesDepletedEvent>.Subscribe(OnLivesDepleted);
            EventBus<CounterattackEvent>.Subscribe(OnCounterattack);
        }

        private void ResetRun()
        {
            _embraced = 0;
            _notEmbraced = 0;
            _mistakes = 0;
            _lastCountedCar = -1;
            EmbracedVillains.Clear();
            Publish();

        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_owner != this || scene.handle == _runSceneHandle) return;
            // TrainProgress is scene-owned and identifies a fresh run without depending on
            // a scene name, Title route, or Playing/Paused state. sceneLoaded precedes Start.
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.GetComponentInChildren<TrainProgress>(true) == null) continue;
                _runSceneHandle = scene.handle;
                ResetRun();
                return;
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            EventBus<VillainConvertedEvent>.Unsubscribe(OnConverted);
            EventBus<VillainDefeatedEvent>.Unsubscribe(OnDefeated);
            EventBus<CarChangedEvent>.Unsubscribe(OnCarChanged);
            EventBus<TrainEndReachedEvent>.Unsubscribe(OnTrainEnd);
            EventBus<LivesDepletedEvent>.Unsubscribe(OnLivesDepleted);
            EventBus<CounterattackEvent>.Unsubscribe(OnCounterattack);
        }

        private void OnDestroy()
        {
            if (_owner == this) _owner = null;
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
            if (evt.Villain != null) EmbracedVillains.MarkDefeated(evt.Villain.Id);
            Publish();
        }

        // Each car's unresolved villains are counted once: CarChangedEvent (cars but the last), TrainEndReachedEvent
        // (last car), or LivesDepletedEvent (the car where the run ended - it never raises CarChangedEvent).
        private void OnCarChanged(CarChangedEvent evt) => AddUnresolvedForCar(evt.FromIndex, evt.UnresolvedVillains);

        private void OnTrainEnd(TrainEndReachedEvent evt) =>
            AddUnresolvedForCar(TrainProgress.HasInstance ? TrainProgress.Instance.CurrentIndex : -1, evt.UnresolvedVillains);

        private void OnLivesDepleted(LivesDepletedEvent evt)
        {
            if (!TrainProgress.HasInstance || TrainProgress.Instance.CurrentCar == null)
            {
                return;
            }

            TrainProgress train = TrainProgress.Instance;
            AddUnresolvedForCar(train.CurrentIndex, train.CurrentCar.CountUnresolvedVillains());
        }

        private void AddUnresolvedForCar(int carIndex, int count)
        {
            if (carIndex >= 0)
            {
                if (carIndex <= _lastCountedCar)
                {
                    return;
                }

                _lastCountedCar = carIndex;
            }

            AddUnresolved(count);
        }

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
