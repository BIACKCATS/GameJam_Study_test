using System.Collections.Generic;
using JSH.Core;
using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// The one on-screen train car. The game repeats it N times (TrainProgress owns the car number):
    /// each new car despawns the previous NPCs and spawns passengers/villains onto random free seats of the
    /// SeatLayout. NPCs are pooled per prefab (Villain resets its state on spawn via IPoolable).
    /// Select the car in the Scene view to see its seats (gizmos).
    /// </summary>
    public class TrainCar : MonoBehaviour
    {
        [Tooltip("Seat coordinates relative to this transform (match the car interior sprite).")]
        [SerializeField] private SeatLayout _seatLayout;
        [Tooltip("Parent for pooled NPCs. Empty = this transform.")]
        [SerializeField] private Transform _npcRoot;

        private readonly List<Villain> _villains = new List<Villain>(4);
        private readonly List<Talkable> _npcs = new List<Talkable>(8);
        private readonly List<int> _freeSeats = new List<int>(16);
        private readonly Dictionary<Talkable, ComponentPool<Talkable>> _pools = new Dictionary<Talkable, ComponentPool<Talkable>>();

        public IReadOnlyList<Villain> Villains => _villains;
        public IReadOnlyList<Talkable> Npcs => _npcs;

        private void Awake()
        {
            if (_npcRoot == null) _npcRoot = transform;
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

        /// <summary>Return every spawned NPC to its pool.</summary>
        public void Despawn()
        {
            for (int i = 0; i < _npcs.Count; i++)
            {
                if (_npcs[i] != null) PoolReturn.ReleaseOrDestroy(_npcs[i].gameObject);
            }
            _npcs.Clear();
            _villains.Clear();
        }

        /// <summary>Despawn the previous car's NPCs, then seat new ones (villains first so they always fit).</summary>
        public void Spawn(CarSpawnSettings settings, System.Random rng, int carIndex)
        {
            Despawn();
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

            for (int i = 0; i < villains; i++) SpawnOne(settings, rng, true, carIndex);
            for (int i = 0; i < passengers; i++) SpawnOne(settings, rng, false, carIndex);
        }

        private void SpawnOne(CarSpawnSettings settings, System.Random rng, bool villain, int carIndex)
        {
            int prefabCount = villain ? settings.VillainPrefabCount : settings.PassengerPrefabCount;
            if (_freeSeats.Count == 0 || prefabCount == 0) return;

            int pick = rng.Next(_freeSeats.Count);
            int seatIndex = _freeSeats[pick];
            _freeSeats.RemoveAt(pick);

            GameObject prefab = villain ? settings.GetVillainPrefab(rng.Next(prefabCount)) : settings.GetPassengerPrefab(rng.Next(prefabCount));
            if (prefab == null) return;
            if (!prefab.TryGetComponent(out Talkable prefabTalkable))
            {
                Debug.LogWarning($"[TrainCar] {prefab.name} has no Talkable; skipped.", prefab);
                return;
            }

            if (!_pools.TryGetValue(prefabTalkable, out ComponentPool<Talkable> pool))
            {
                pool = new ComponentPool<Talkable>(prefabTalkable, _npcRoot, 4, 16);
                _pools.Add(prefabTalkable, pool);
            }

            SeatPoint seat = _seatLayout.GetSeat(seatIndex);
            Talkable npc = pool.Get(transform.TransformPoint(seat.LocalPosition));
            npc.name = $"{prefab.name}_Car{carIndex}_Seat{seatIndex}";

            SpriteRenderer sr = npc.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.flipX = seat.FlipX;
            _npcs.Add(npc);

            bool hasVillain = npc.TryGetComponent(out Villain v);
            if (hasVillain) _villains.Add(v);
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
