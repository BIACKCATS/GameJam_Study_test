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

        [Tooltip("Who gets credit for the hit and is never hit by it. Defaults to this collider's Rigidbody2D object (or this object).")]
        [SerializeField] private GameObject _owner;

        private Transform _ownerTransform;

        public int Damage
        {
            get => _damage;
            set => _damage = Mathf.Max(1, value);
        }

        private void Awake()
        {
            if (_owner == null)
            {
                _owner = TryGetComponent(out Collider2D self) && self.attachedRigidbody != null
                    ? self.attachedRigidbody.gameObject
                    : gameObject;
            }

            _ownerTransform = _owner.transform;
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

            if (other.transform.IsChildOf(_ownerTransform))
            {
                return; // never hit the owner or its own parts
            }

            // IDamageable on the Rigidbody2D object first, then on the collider object.
            Component holder = other.attachedRigidbody;
            IDamageable target = null;
            if (holder == null || !holder.TryGetComponent(out target))
            {
                holder = other;
                if (!other.TryGetComponent(out target))
                {
                    return;
                }
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
