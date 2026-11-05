using System.Collections.Generic;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// One train car (= one platform). Put on each Platform root. Villains standing inside the car's X range
    /// are collected automatically on Start; villains spawned later are added with AddVillain.
    /// </summary>
    public class TrainCar : MonoBehaviour
    {
        [Tooltip("Defines the car's X range. Empty = this GameObject's BoxCollider2D.")]
        [SerializeField] private Collider2D _bounds;

        private readonly List<Villain> _villains = new List<Villain>(4);

        /// <summary>0-based order from left to right (assigned by TrainProgress).</summary>
        public int Index { get; private set; } = -1;
        /// <summary>True once the exit judgment ran for this car (it only runs once).</summary>
        public bool IsJudged { get; private set; }
        public float MinX => _bounds != null ? _bounds.bounds.min.x : transform.position.x;
        public float MaxX => _bounds != null ? _bounds.bounds.max.x : transform.position.x;
        public IReadOnlyList<Villain> Villains => _villains;

        private void Awake()
        {
            if (_bounds == null && TryGetComponent(out BoxCollider2D own)) _bounds = own;
        }

        internal void Initialize(int index, Villain[] allVillains)
        {
            Index = index;
            _villains.Clear();
            for (int i = 0; i < allVillains.Length; i++)
            {
                if (ContainsX(allVillains[i].transform.position.x)) _villains.Add(allVillains[i]);
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
    }
}
