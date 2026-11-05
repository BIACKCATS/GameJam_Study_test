using System.Collections.Generic;
using JSH.Core;
using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// The one on-screen train car. The game repeats it N times (TrainProgress owns the car number):
    /// each new car despawns the previous NPCs and spawns passengers/villains onto random free seats of the
    /// SeatLayout. A passenger prefab appears at most once per car (villain prefabs may repeat).
    /// NPCs with a SeatFootprint are seated first, where their neighbouring seats are free, and keep those seats empty.
    /// NPCs are pooled per prefab (Villain resets its state on spawn via IPoolable).
    /// Select the car in the Scene view to see its seats (gizmos).
    /// </summary>
    public class TrainCar : MonoBehaviour
    {
        [Tooltip("Seat coordinates relative to this transform (match the car interior sprite).")]
        [SerializeField] private SeatLayout _seatLayout;
        [Tooltip("Parent for pooled NPCs. Empty = this transform.")]
        [SerializeField] private Transform _npcRoot;
        [Tooltip("Seat points are where the feet go: lift each NPC so the bottom of its sprite sits on the seat point, " +
                 "whatever the sprite's pivot (feet pivot art and center pivot placeholders both line up).")]
        [SerializeField] private bool _alignFeetToSeat = true;

        private readonly List<Villain> _villains = new List<Villain>(4);
        private readonly List<Talkable> _npcs = new List<Talkable>(8);
        private readonly List<int> _freeSeats = new List<int>(16);
        private readonly List<int> _unusedPassengerPrefabs = new List<int>(8); // passengers never repeat within a car
        private readonly List<GameObject> _villainPicks = new List<GameObject>(8);
        private readonly List<int> _seatCandidates = new List<int>(16);
        private readonly List<int> _seatsByX = new List<int>(16);
        private int[] _seatRank = new int[0]; // seat index -> position in left-to-right order
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

        /// <summary>Return one NPC of this car to its pool (e.g. an ejected villain). False = not in this car.</summary>
        public bool RemoveNpc(Talkable npc)
        {
            if (npc == null || !_npcs.Remove(npc)) return false;
            if (npc.TryGetComponent(out Villain v)) _villains.Remove(v);
            PoolReturn.ReleaseOrDestroy(npc.gameObject);
            return true;
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

            int villains = settings.GetVillainCount(carIndex);
            int passengers = settings.GetPassengerCount(carIndex);
            if (villains + passengers > _freeSeats.Count)
                Debug.LogWarning($"[TrainCar] {name}: {villains + passengers} NPCs but {_freeSeats.Count} seats; extra NPCs skipped.", this);

            _unusedPassengerPrefabs.Clear();
            for (int i = 0; i < settings.PassengerPrefabCount; i++) _unusedPassengerPrefabs.Add(i);
            if (passengers > _unusedPassengerPrefabs.Count)
                Debug.LogWarning($"[TrainCar] {name}: {passengers} passengers but {_unusedPassengerPrefabs.Count} passenger prefabs; passengers never repeat in a car, extra seats stay empty.", this);

            BuildSeatOrder();

            // Pick villains first, then seat the wide ones (SeatFootprint) before the rest so their neighbours are free.
            _villainPicks.Clear();
            if (settings.VillainPrefabCount > 0)
            {
                for (int i = 0; i < villains; i++) _villainPicks.Add(settings.GetVillainPrefab(rng.Next(settings.VillainPrefabCount)));
            }
            for (int i = 0; i < _villainPicks.Count; i++)
            {
                if (GetExtraSeats(_villainPicks[i]) > 0) SpawnPrefab(_villainPicks[i], rng, true, carIndex);
            }
            for (int i = 0; i < _villainPicks.Count; i++)
            {
                if (GetExtraSeats(_villainPicks[i]) == 0) SpawnPrefab(_villainPicks[i], rng, true, carIndex);
            }

            for (int i = 0; i < passengers; i++)
            {
                if (_unusedPassengerPrefabs.Count == 0) break;
                int prefabPick = rng.Next(_unusedPassengerPrefabs.Count);
                GameObject prefab = settings.GetPassengerPrefab(_unusedPassengerPrefabs[prefabPick]);
                _unusedPassengerPrefabs.RemoveAt(prefabPick);
                SpawnPrefab(prefab, rng, false, carIndex);
            }
        }

        private static int GetExtraSeats(GameObject prefab) =>
            prefab != null && prefab.TryGetComponent(out SeatFootprint footprint) ? footprint.ExtraSeatsEachSide : 0;

        // Seat indices sorted by x (SeatLayout order is not guaranteed to be left-to-right).
        private void BuildSeatOrder()
        {
            int count = _seatLayout.SeatCount;
            _seatsByX.Clear();
            for (int i = 0; i < count; i++)
            {
                float x = _seatLayout.GetSeat(i).LocalPosition.x;
                int at = _seatsByX.Count;
                while (at > 0 && _seatLayout.GetSeat(_seatsByX[at - 1]).LocalPosition.x > x) at--;
                _seatsByX.Insert(at, i);
            }
            if (_seatRank.Length != count) _seatRank = new int[count];
            for (int r = 0; r < count; r++) _seatRank[_seatsByX[r]] = r;
        }

        // True if every seat within `extra` ranks of `seatIndex` is free (seats past the ends don't matter).
        private bool NeighboursFree(int seatIndex, int extra)
        {
            int rank = _seatRank[seatIndex];
            for (int d = -extra; d <= extra; d++)
            {
                int r = rank + d;
                if (d == 0 || r < 0 || r >= _seatsByX.Count) continue;
                if (!_freeSeats.Contains(_seatsByX[r])) return false;
            }
            return true;
        }

        private int TakeSeat(System.Random rng, int extra, GameObject prefab)
        {
            List<int> pool = _freeSeats;
            if (extra > 0)
            {
                _seatCandidates.Clear();
                for (int i = 0; i < _freeSeats.Count; i++)
                {
                    if (NeighboursFree(_freeSeats[i], extra)) _seatCandidates.Add(_freeSeats[i]);
                }
                if (_seatCandidates.Count > 0) pool = _seatCandidates;
                else Debug.LogWarning($"[TrainCar] {name}: no seat with {extra} free seat(s) on each side for {prefab.name}; seated without spacing.", this);
            }

            int seatIndex = pool[rng.Next(pool.Count)];
            _freeSeats.Remove(seatIndex);
            if (extra > 0)
            {
                int rank = _seatRank[seatIndex];
                for (int d = -extra; d <= extra; d++)
                {
                    int r = rank + d;
                    if (d != 0 && r >= 0 && r < _seatsByX.Count) _freeSeats.Remove(_seatsByX[r]); // keep neighbours empty
                }
            }
            return seatIndex;
        }

        private void SpawnPrefab(GameObject prefab, System.Random rng, bool villain, int carIndex)
        {
            if (_freeSeats.Count == 0 || prefab == null) return;
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

            int seatIndex = TakeSeat(rng, GetExtraSeats(prefab), prefab);
            SeatPoint seat = _seatLayout.GetSeat(seatIndex);
            Vector3 seatPosition = transform.TransformPoint(seat.LocalPosition);
            Talkable npc = pool.Get(seatPosition);
            npc.name = $"{prefab.name}_Car{carIndex}_Seat{seatIndex}";

            SpriteRenderer sr = npc.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.flipX = seat.FlipX;
                if (_alignFeetToSeat) npc.transform.position += new Vector3(0f, seatPosition.y - sr.bounds.min.y, 0f);
            }
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
                if (_alignFeetToSeat) Gizmos.DrawLine(p + Vector3.left * 0.6f, p + Vector3.right * 0.6f); // feet line
                UnityEditor.Handles.Label(p + Vector3.up * 0.6f, i.ToString());
            }
        }
#endif
    }
}
