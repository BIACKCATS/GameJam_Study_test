using UnityEngine;

namespace JSH.Data
{
    /// <summary>
    /// Who sits in a car: prefab pools and head counts. Each NPC takes a random free seat from the car's
    /// SeatLayout (no duplicates); villains are seated first so they always fit.
    /// Prefabs must have a Talkable (and a Villain for villain prefabs).
    /// </summary>
    [CreateAssetMenu(fileName = "CarSpawnSettings", menuName = "JSH/Train/Car Spawn Settings")]
    public class CarSpawnSettings : ScriptableObject
    {
        [SerializeField] private GameObject[] _passengerPrefabs = new GameObject[0];
        [SerializeField] private GameObject[] _villainPrefabs = new GameObject[0];
        [Tooltip("Inclusive min/max passengers per car.")]
        [SerializeField] private Vector2Int _passengerCount = new Vector2Int(2, 4);
        [Tooltip("Inclusive min/max villains per car.")]
        [SerializeField] private Vector2Int _villainCount = new Vector2Int(1, 1);

        public int PassengerPrefabCount => _passengerPrefabs != null ? _passengerPrefabs.Length : 0;
        public int VillainPrefabCount => _villainPrefabs != null ? _villainPrefabs.Length : 0;

        public GameObject GetPassengerPrefab(int index) => _passengerPrefabs[index];
        public GameObject GetVillainPrefab(int index) => _villainPrefabs[index];

        public int RollPassengerCount(System.Random rng) => Roll(rng, _passengerCount);
        public int RollVillainCount(System.Random rng) => Roll(rng, _villainCount);

        private static int Roll(System.Random rng, Vector2Int range)
        {
            int min = Mathf.Max(0, range.x);
            int max = Mathf.Max(min, range.y);
            return rng.Next(min, max + 1);
        }
    }
}
