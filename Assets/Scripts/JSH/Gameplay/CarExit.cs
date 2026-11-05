using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Right exit of the on-screen car. When the player touches it, TrainProgress judges the car and moves on
    /// (curtain -> next car, or ending on the last car). The collider is forced to be a trigger.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CarExit : MonoBehaviour
    {
        private void Awake()
        {
            if (TryGetComponent(out Collider2D col) && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning($"[CarExit] {name}: collider was not a trigger; set at runtime. Tick 'Is Trigger' in the scene.", this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!TrainProgress.HasInstance) return;
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent(out SideViewMover _)) return; // player only
            TrainProgress.Instance.RequestExit();
        }
    }
}
