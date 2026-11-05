using System.Collections.Generic;
using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// One train car (= one platform). Put on each Platform root.
    /// Passengers/villains are spawned onto random free seats of the SeatLayout (TrainProgress decides when).
    /// Villains already standing inside the car's X range at Start are also counted (hand-placed specials).
    /// Select the car in the Scene view to see its seats (gizmos).
    /// </summary>
    public class TrainCar : MonoBehaviour
    {
        [Tooltip("Defines the car's X range. Empty = this GameObject's BoxCollider2D.")]
        [SerializeField] private Collider2D _bounds;
        [Tooltip("Seat coordinates for this car's sprite (shared asset).")]
        [SerializeField] private SeatLayout _seatLayout;
        [Tooltip("Optional per-car rules. Empty = TrainProgress default.")]
        [SerializeField] private CarSpawnSettings _spawnOverride;

        private readonly List<Villain> _villains = new List<Villain>(4);
        private readonly List<Talkable> _npcs = new List<Talkable>(8);
        private readonly List<int> _freeSeats = new List<int>(16);

        /// <summary>0-based order from left to right (assigned by TrainProgress).</summary>
        public int Index { get; private set; } = -1;
        /// <summary>True once the exit judgment ran for this car (it only runs once).</summary>
        public bool IsJudged { get; private set; }
        /// <summary>True once passengers/villains were spawned (runs once).</summary>
        public bool IsSpawned { get; private set; }
        public float MinX => _bounds != null ? _bounds.bounds.min.x : transform.position.x;
        public float MaxX => _bounds != null ? _bounds.bounds.max.x : transform.position.x;
        public IReadOnlyList<Villain> Villains => _villains;
        public IReadOnlyList<Talkable> Npcs => _npcs;

        private void Awake()
        {
            if (_bounds == null && TryGetComponent(out BoxCollider2D own)) _bounds = own;
        }

        internal void Initialize(int index, Villain[] placedVillains)
        {
            Index = index;
            for (int i = 0; i < placedVillains.Length; i++)
            {
                if (ContainsX(placedVillains[i].transform.position.x)) AddVillain(placedVillains[i]);
            }
        }

        internal void MarkJudged() => IsJudged = true;

        public bool ContainsX(float x) => x >= MinX && x <= MaxX;

        public void AddVillain(Villain villain)
        {
            if (villain != null && !_villains.Contains(villain)) _villains.Add(villain);
        }

        public int CountUnresolvedVillains()
        {
            int count = 0;
            for (int i = 0; i < _villains.Count; i++)
            {
                Villain v = _villains[i];
                if (v != null && v.isActiveAndEnabled && !v.IsResolved) count++;
            }
            return count;
        }

        /// <summary>
        /// Spawn passengers/villains onto random free seats (villains first). Runs once per car.
        /// NPCs are created with Instantiate (not pooled): spawned once per car and never despawned.
        /// </summary>
        public void Spawn(CarSpawnSettings defaults, System.Random rng)
        {
            if (IsSpawned) return;
            IsSpawned = true;

            CarSpawnSettings settings = _spawnOverride != null ? _spawnOverride : defaults;
            if (settings == null || _seatLayout == null || _seatLayout.SeatCount == 0)
            {
                Debug.LogWarning($"[TrainCar] {name}: missing CarSpawnSettings or SeatLayout (or 0 seats); nothing spawned.", this);
                return;
            }

            _freeSeats.Clear();
            for (int i = 0; i < _seatLayout.SeatCount; i++) _freeSeats.Add(i);

            int villains = settings.RollVillainCount(rng);
            int passengers = settings.RollPassengerCount(rng);
            if (villains + passengers > _freeSeats.Count)
                Debug.LogWarning($"[TrainCar] {name}: {villains + passengers} NPCs but {_freeSeats.Count} seats; extra NPCs skipped.", this);

            for (int i = 0; i < villains; i++) SpawnOne(settings, rng, true);
            for (int i = 0; i < passengers; i++) SpawnOne(settings, rng, false);
        }

        private void SpawnOne(CarSpawnSettings settings, System.Random rng, bool villain)
        {
            int prefabCount = villain ? settings.VillainPrefabCount : settings.PassengerPrefabCount;
            if (_freeSeats.Count == 0 || prefabCount == 0) return;

            int pick = rng.Next(_freeSeats.Count);
            int seatIndex = _freeSeats[pick];
            _freeSeats.RemoveAt(pick);

            GameObject prefab = villain ? settings.GetVillainPrefab(rng.Next(prefabCount)) : settings.GetPassengerPrefab(rng.Next(prefabCount));
            if (prefab == null) return;

            SeatPoint seat = _seatLayout.GetSeat(seatIndex);
            GameObject npc = Instantiate(prefab, transform.TransformPoint(seat.LocalPosition), Quaternion.identity, transform);
            npc.name = $"{prefab.name}_Car{Index}_Seat{seatIndex}";

            SpriteRenderer sr = npc.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.flipX = seat.FlipX;

            if (npc.TryGetComponent(out Talkable talkable)) _npcs.Add(talkable);
            else Debug.LogWarning($"[TrainCar] {prefab.name} has no Talkable.", prefab);

            bool hasVillain = npc.TryGetComponent(out Villain v);
            if (hasVillain) AddVillain(v);
            if (villain != hasVillain) Debug.LogWarning($"[TrainCar] {prefab.name}: listed as {(villain ? "villain" : "passenger")} but Villain component is {(hasVillain ? "present" : "missing")}.", prefab);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_seatLayout == null) return;
            Gizmos.color = new Color(1f, 0.8f, 0.1f, 0.9f);
            for (int i = 0; i < _seatLayout.SeatCount; i++)
            {
                Vector3 p = transform.TransformPoint(_seatLayout.GetSeat(i).LocalPosition);
                Gizmos.DrawWireCube(p, new Vector3(0.8f, 0.8f, 0f));
                UnityEditor.Handles.Label(p + Vector3.up * 0.6f, i.ToString());
            }
        }
#endif
    }
}
