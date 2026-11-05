using UnityEngine;

namespace JSH.Data
{
    /// <summary>
    /// Who sits in a car: prefab pools and head counts. Each NPC takes a random free seat from the car's
    /// SeatLayout (no duplicates); villains are seated first so they always fit.
    /// Head count is fixed per car: car n (1-based) has min(n, max) villains and (max - villains) passengers,
    /// so from car "max" on every NPC is a villain.
    /// Prefabs must have a Talkable (and a Villain for villain prefabs).
    /// </summary>
    [CreateAssetMenu(fileName = "CarSpawnSettings", menuName = "JSH/Train/Car Spawn Settings")]
    public class CarSpawnSettings : ScriptableObject
    {
        [SerializeField] private GameObject[] _passengerPrefabs = new GameObject[0];
        [SerializeField] private GameObject[] _villainPrefabs = new GameObject[0];
        [Tooltip("NPCs per car (villains + passengers). Car n has min(n, this) villains.")]
        [SerializeField, Min(1)] private int _maxNpcPerCar = 5;

        public int PassengerPrefabCount => _passengerPrefabs != null ? _passengerPrefabs.Length : 0;
        public int VillainPrefabCount => _villainPrefabs != null ? _villainPrefabs.Length : 0;

        public GameObject GetPassengerPrefab(int index) => _passengerPrefabs[index];
        public GameObject GetVillainPrefab(int index) => _villainPrefabs[index];

        /// <summary>Villains in the car at 0-based <paramref name="carIndex"/>: min(carIndex + 1, max).</summary>
        public int GetVillainCount(int carIndex) => Mathf.Clamp(carIndex + 1, 0, _maxNpcPerCar);

        /// <summary>Passengers in the car at 0-based <paramref name="carIndex"/>: max - villains.</summary>
        public int GetPassengerCount(int carIndex) => _maxNpcPerCar - GetVillainCount(carIndex);
    }
}
