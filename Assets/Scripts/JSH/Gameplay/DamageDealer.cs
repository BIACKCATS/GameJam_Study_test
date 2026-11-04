using JSH.Core;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Hitbox: damages any IDamageable it touches (trigger or collision).
    /// Use on bullets, spikes, enemy bodies, melee swing colliders.
    /// Target lookup: the collider's attached Rigidbody2D first, then the collider itself.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class DamageDealer : MonoBehaviour
    {
        [SerializeField, Min(1)] private int _damage = 1;
        [SerializeField] private LayerMask _targetLayers = ~0;
        [SerializeField, Min(0f)] private float _knockbackForce = 5f;
        [Tooltip("Extra upward push added to knockback (side-view games).")]
        [SerializeField, Min(0f)] private float _knockbackLift = 3f;
        [Tooltip("Keep damaging while overlapping (target i-frames limit the rate).")]
        [SerializeField] private bool _damageOnStay = true;

        [Header("After Hit")]
        [Tooltip("Return to pool / destroy after a successful hit (bullets).")]
        [SerializeField] private bool _despawnOnHit;

        [Tooltip("Who gets credit for the hit. Defaults to the root of this object.")]
        [SerializeField] private GameObject _owner;

        public int Damage
        {
            get => _damage;
            set => _damage = Mathf.Max(1, value);
        }

        private void Awake()
        {
            if (_owner == null)
            {
                _owner = transform.root.gameObject;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => TryHit(other);

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_damageOnStay)
            {
                TryHit(other);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) => TryHit(collision.collider);

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (_damageOnStay)
            {
                TryHit(collision.collider);
            }
        }

        private void TryHit(Collider2D other)
        {
            if ((_targetLayers.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            Component holder = other.attachedRigidbody != null ? other.attachedRigidbody : other;
            if (holder.transform.root == transform.root)
            {
                return; // never hit ourselves
            }

            if (!holder.TryGetComponent(out IDamageable target))
            {
                return;
            }

            Vector2 hitPoint = other.ClosestPoint(transform.position);
            float side = Mathf.Sign(holder.transform.position.x - transform.position.x);
            Vector2 knockback = new Vector2(side * _knockbackForce, _knockbackLift);

            if (target.TakeDamage(new DamageInfo(_damage, _owner, hitPoint, knockback)) && _despawnOnHit)
            {
                PoolReturn.ReleaseOrDestroy(gameObject);
            }
        }
    }
}
