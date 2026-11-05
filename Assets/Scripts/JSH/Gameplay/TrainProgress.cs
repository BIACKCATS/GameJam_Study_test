using System;
using JSH.Core;
using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Tracks which train car the player is in (put on [Managers]). Cars (TrainCar) are found on Start and
    /// ordered left to right. Moving forward out of a car judges it once: unresolved villains > 0 -> life -1
    /// (regardless of how many). Moving backward never judges.
    /// Spawning: one car ahead. Start spawns the starting car + the next one; entering car N forward spawns N+1,
    /// so the player never sees NPCs pop in. (Same setup whether the villain was missed or not.)
    /// </summary>
    public class TrainProgress : Singleton<TrainProgress>
    {
        [Tooltip("Player transform used to find the starting car. Empty = the object with SideViewMover.")]
        [SerializeField] private Transform _player;
        [Tooltip("Default spawn rules for every car (TrainCar can override).")]
        [SerializeField] private CarSpawnSettings _defaultSpawn;
        [Tooltip("0 = random every run; any other value = same layout every run (testing).")]
        [SerializeField] private int _randomSeed;

        private TrainCar[] _cars = Array.Empty<TrainCar>();
        private TrainCar _current;
        private System.Random _rng;

        public TrainCar CurrentCar => _current;
        public int CarCount => _cars.Length;
        public bool IsLastCar => _current != null && _current.Index == _cars.Length - 1;

        public TrainCar GetCar(int index) => _cars[index];

        private void Start()
        {
            _cars = FindObjectsByType<TrainCar>(FindObjectsSortMode.None);
            Array.Sort(_cars, (a, b) => a.MinX.CompareTo(b.MinX));
            Villain[] villains = FindObjectsByType<Villain>(FindObjectsSortMode.None);
            for (int i = 0; i < _cars.Length; i++) _cars[i].Initialize(i, villains);

            if (_player == null)
            {
                SideViewMover mover = FindFirstObjectByType<SideViewMover>();
                if (mover != null) _player = mover.transform;
            }
            _current = _player != null ? FindCarAt(_player.position.x) : null;
            if (_current == null && _cars.Length > 0) _current = _cars[0];

            _rng = new System.Random(_randomSeed != 0 ? _randomSeed : Environment.TickCount);
            if (_current != null)
            {
                SpawnCar(_current.Index);
                SpawnCar(_current.Index + 1);
            }
        }

        private void SpawnCar(int index)
        {
            if (index >= 0 && index < _cars.Length) _cars[index].Spawn(_defaultSpawn, _rng);
        }

        /// <summary>
        /// Called by CarEntrance when the player leaves a boundary into car <paramref name="carIndex"/>.
        /// Index past the last car (end of the train) is ignored for now (ending hooks in later).
        /// </summary>
        public void NotifyCrossed(int carIndex)
        {
            if (carIndex < 0 || carIndex >= _cars.Length) return;
            TrainCar car = _cars[carIndex];
            if (car == _current) return;

            TrainCar from = _current;
            bool forward = from == null || car.Index > from.Index;
            int unresolved = 0;
            bool lifeLost = false;
            bool cleared = false;

            // Judge the car being left: forward only, once per car.
            if (forward && from != null && !from.IsJudged)
            {
                from.MarkJudged();
                unresolved = from.CountUnresolvedVillains();
                if (unresolved > 0)
                {
                    if (LifeSystem.HasInstance) LifeSystem.Instance.Damage(1);
                    lifeLost = true;
                }
                else
                {
                    if (LifeSystem.HasInstance) LifeSystem.Instance.Heal(1); // clean pass: life +1 (capped at max)
                    cleared = true;
                }
            }

            _current = car;
            if (forward)
            {
                SpawnCar(car.Index);     // safety (normally already spawned one car ahead)
                SpawnCar(car.Index + 1); // look-ahead
            }
            EventBus<CarChangedEvent>.Raise(new CarChangedEvent(from, car, forward, unresolved, lifeLost, cleared));
        }

        private TrainCar FindCarAt(float x)
        {
            for (int i = 0; i < _cars.Length; i++)
            {
                if (_cars[i].ContainsX(x)) return _cars[i];
            }
            return null;
        }
    }
}
