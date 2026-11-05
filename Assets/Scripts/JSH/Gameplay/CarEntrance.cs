using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Boundary trigger at a car's edge (child of a Platform with TrainCar). One boundary per seam is enough:
    /// a Right_Entrance alone works (Left_Entrance optional). When the player LEAVES the trigger, the side it
    /// exits to decides which car the player is now in:
    ///  - entrance on the car's right edge: exit right = next car, exit left = this car
    ///  - entrance on the car's left edge:  exit right = this car, exit left = previous car
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
                Debug.LogWarning($"[CarEntrance] {name}: collider was not a trigger; set at runtime. Tick 'Is Trigger' in the scene.", this);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_car == null || !TrainProgress.HasInstance) return;
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent(out SideViewMover _)) return; // player only

            float x = transform.position.x;
            bool isRightEdge = x >= (_car.MinX + _car.MaxX) * 0.5f;
            bool exitedRight = body.position.x > x;
            int target = isRightEdge ? (exitedRight ? _car.Index + 1 : _car.Index)
                                     : (exitedRight ? _car.Index : _car.Index - 1);
            TrainProgress.Instance.NotifyCrossed(target);
        }
    }
}
