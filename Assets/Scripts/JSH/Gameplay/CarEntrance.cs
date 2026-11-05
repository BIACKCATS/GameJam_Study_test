using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Entrance trigger at a car's edge (Left_Entrance / Right_Entrance under a Platform with TrainCar).
    /// When the player touches an entrance that belongs to a different car, TrainProgress judges the car being left.
    /// The collider is forced to be a trigger so the player walks through it.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CarEntrance : MonoBehaviour
    {
        private TrainCar _car;

        private void Awake()
        {
            _car = GetComponentInParent<TrainCar>();
            if (_car == null) Debug.LogError($"[CarEntrance] {name}: no TrainCar on a parent.", this);
            if (TryGetComponent(out Collider2D col) && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning($"[CarEntrance] {_car?.name}/{name}: collider was not a trigger; set at runtime. Tick 'Is Trigger' in the scene.", this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_car == null || !TrainProgress.HasInstance) return;
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent(out SideViewMover _)) return; // player only
            TrainProgress.Instance.NotifyEntrance(_car);
        }
    }
}
