using JSH.Core;
using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// One-screen train car repeated N times (default 9). Put on [SceneManagers].
    /// The player reaching the right exit (CarExit) judges the current car once
    /// (unresolved villains > 0 -> life -1, none -> life +1), then:
    ///  - last car         -> TrainEndReachedEvent (no curtain; EndingController decides good/bad)
    ///  - lives reached 0  -> nothing more (bad ending is pending)
    ///  - otherwise        -> lock input, play the curtain (SubwayCurtainPlayer). While the screen is black
    ///                        ("Black" phase): despawn NPCs, move the player to the start point, spawn the next car.
    ///                        When the curtain is fully open again ("Idle"): unlock input.
    /// The curtain is the teammate's component; it is only polled (Phase / IsPlaying), never modified.
    /// Without a curtain the next car is set up instantly.
    /// </summary>
    public class TrainProgress : Singleton<TrainProgress>
    {
        private enum State { Playing, WaitingForBlack, WaitingForOpen, Ended }

        private const string CurtainBlackPhase = "Black";

        [SerializeField] private TrainCar _car;
        [SerializeField, Min(1)] private int _carCount = 9;
        [SerializeField] private CarSpawnSettings _spawnSettings;
        [Tooltip("0 = random every run; any other value = same layout every run (testing).")]
        [SerializeField] private int _randomSeed;

        [Header("Player")]
        [Tooltip("Empty = the object with SideViewMover.")]
        [SerializeField] private SideViewMover _player;
        [Tooltip("Where the player is placed at the start of every new car.")]
        [SerializeField] private Transform _playerStart;

        [Header("Transition")]
        [Tooltip("Teammate's curtain (TempFolderStage). Empty = instant car change.")]
        [SerializeField] private SubwayCurtainPlayer _curtain;
        [Tooltip("Safety: set up the next car anyway if the curtain has not gone black after this many seconds (unscaled).")]
        [SerializeField, Min(0.5f)] private float _curtainTimeout = 5f;

        private State _state = State.Playing;
        private int _index;
        private float _waitTime;
        private Rigidbody2D _playerBody;
        private System.Random _rng;
        private int _lastUnresolved;
        private bool _lastLifeLost;

        public TrainCar CurrentCar => _car;
        public int CurrentIndex => _index;
        public int CarCount => _carCount;
        public bool IsLastCar => _index >= _carCount - 1;
        /// <summary>True from the exit until the curtain is open again (input is locked).</summary>
        public bool IsTransitioning => _state == State.WaitingForBlack || _state == State.WaitingForOpen;
        public bool IsEndReached => _state == State.Ended && IsLastCar;

        private void Start()
        {
            if (_player == null) _player = FindFirstObjectByType<SideViewMover>();
            if (_player != null) _player.TryGetComponent(out _playerBody);
            if (_car == null) Debug.LogError("[TrainProgress] TrainCar is not assigned.", this);
            if (_playerStart == null) Debug.LogWarning("[TrainProgress] Player Start is not assigned; the player stays where it is between cars.", this);

            _rng = new System.Random(_randomSeed != 0 ? _randomSeed : System.Environment.TickCount);
            _index = 0;
            if (_car != null) _car.Spawn(_spawnSettings, _rng, _index);
        }

        /// <summary>Called by CarExit when the player reaches the right exit. Runs once per car.</summary>
        public void RequestExit()
        {
            if (_state != State.Playing || _car == null) return;

            _lastUnresolved = _car.CountUnresolvedVillains();
            _lastLifeLost = _lastUnresolved > 0;
            if (LifeSystem.HasInstance)
            {
                if (_lastLifeLost) LifeSystem.Instance.Damage(1);
                else LifeSystem.Instance.Heal(1);
            }

            if (IsLastCar)
            {
                _state = State.Ended;
                EventBus<TrainEndReachedEvent>.Raise(new TrainEndReachedEvent(_car, _lastUnresolved, _lastLifeLost));
                return;
            }
            if (LifeSystem.HasInstance && LifeSystem.Instance.IsDepleted)
            {
                _state = State.Ended; // EndingController shows the bad ending
                return;
            }

            if (_player != null) _player.SetInputLocked(true);
            if (_curtain != null && _curtain.isActiveAndEnabled) _curtain.Play();

            if (_curtain != null && _curtain.IsPlaying)
            {
                _state = State.WaitingForBlack;
                _waitTime = 0f;
            }
            else
            {
                SetUpNextCar();
                FinishTransition();
            }
        }

        private void Update()
        {
            if (_state == State.WaitingForBlack)
            {
                _waitTime += Time.unscaledDeltaTime;
                bool black = _curtain == null || !_curtain.IsPlaying || _curtain.Phase == CurtainBlackPhase;
                if (!black && _waitTime < _curtainTimeout) return;
                if (!black) Debug.LogWarning("[TrainProgress] Curtain did not reach the black phase in time; changing car anyway.", this);
                SetUpNextCar();
                _state = State.WaitingForOpen;
            }
            else if (_state == State.WaitingForOpen)
            {
                if (_curtain != null && _curtain.IsPlaying) return;
                FinishTransition();
            }
        }

        private void SetUpNextCar()
        {
            int from = _index;
            _index++;

            if (_player != null && _playerStart != null)
            {
                Vector3 p = _playerStart.position;
                if (_playerBody != null)
                {
                    _playerBody.position = p;
                    _playerBody.linearVelocity = Vector2.zero;
                }
                _player.transform.position = p;
            }

            if (_car != null) _car.Spawn(_spawnSettings, _rng, _index);
            EventBus<CarChangedEvent>.Raise(new CarChangedEvent(from, _index, _lastUnresolved, _lastLifeLost, !_lastLifeLost));
        }

        private void FinishTransition()
        {
            _state = State.Playing;
            if (_player != null) _player.SetInputLocked(false);
        }
    }
}
