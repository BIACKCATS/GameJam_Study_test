using System;
using JSH.Core;
using JSH.DebugTools;
using JSH.Juice;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// HP + i-frames + knockback + Feel feedbacks. Works for player, enemies, breakables.
    /// Juice: assign MMF_Player (shake/flash/floating text). With an MMF_FloatingText feedback set to
    /// "Use Intensity As Value", the damage amount is shown as the number.
    /// UI: HealthBarBinder (JSH.UI) drives a Feel MMProgressBar from the Changed event.
    /// Pool-friendly: resets on IPoolable.OnSpawned.
    /// </summary>
    public sealed class Health : MonoBehaviour, IDamageable, IPoolable
    {
        [Header("Stats")]
        [SerializeField, Min(1)] private int _maxHealth = 5;
        [SerializeField] private Countdown _invincibility = new Countdown(0.5f);
        [SerializeField] private bool _applyKnockback = true;

        [Header("Player")]
        [Tooltip("Respect DebugFlags.GodMode (F1 cheat). Enable on the player only.")]
        [SerializeField] private bool _useGodMode;
        [Tooltip("Call GameManager.GameOver() on death. Enable on the player only.")]
        [SerializeField] private bool _gameOverOnDeath;

        [Header("Juice (optional)")]
        [SerializeField, Min(0f)] private float _hitStopDuration = 0.05f;
        [SerializeField] private MMF_Player _damageFeedback;
        [SerializeField] private MMF_Player _healFeedback;
        [SerializeField] private MMF_Player _deathFeedback;

        [Header("Death")]
        [Tooltip("Release to pool (or Destroy if not pooled) after this delay. Negative = keep the object.")]
        [SerializeField] private float _despawnDelay = -1f;

        private Rigidbody2D _rb;
        private Countdown _despawnTimer;

        public int Current { get; private set; }
        public int Max => _maxHealth;
        public float Normalized => (float)Current / _maxHealth;
        public bool IsDead { get; private set; }
        public bool IsInvincible => _invincibility.IsRunning || (_useGodMode && DebugFlags.GodMode);

        /// <summary>Fired on any HP change (damage, heal, reset). Good for UI.</summary>
        public event Action<Health> Changed;
        public event Action<Health, DamageInfo> Damaged;
        public event Action<Health, DamageInfo> Died;

        private void Awake()
        {
            TryGetComponent(out _rb);
            ResetHealth();
        }

        private void Update()
        {
            _invincibility.Tick(Time.deltaTime);

            if (_despawnTimer.Tick(Time.deltaTime))
            {
                PoolReturn.ReleaseOrDestroy(gameObject);
            }
        }

        public bool TakeDamage(in DamageInfo info)
        {
            if (IsDead || IsInvincible || info.Amount <= 0)
            {
                return false;
            }

            Current = Mathf.Max(0, Current - info.Amount);
            _invincibility.Start();

            if (_applyKnockback && _rb != null && info.Knockback != Vector2.zero)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.AddForce(info.Knockback, ForceMode2D.Impulse);
            }

            HitStop.Play(_hitStopDuration);
            PlayFeedback(_damageFeedback, info.Amount);

            Damaged?.Invoke(this, info);
            Changed?.Invoke(this);

            if (Current == 0)
            {
                Die(info);
            }

            return true;
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0 || Current >= _maxHealth)
            {
                return;
            }

            Current = Mathf.Min(_maxHealth, Current + amount);
            PlayFeedback(_healFeedback, amount);
            Changed?.Invoke(this);
        }

        /// <summary>Instant death, ignores i-frames and god mode.</summary>
        public void Kill(GameObject source = null)
        {
            if (IsDead)
            {
                return;
            }

            Current = 0;
            Changed?.Invoke(this);
            Die(new DamageInfo(_maxHealth, source));
        }

        public void ResetHealth()
        {
            Current = _maxHealth;
            IsDead = false;
            _invincibility.Stop();
            _despawnTimer.Stop();
            Changed?.Invoke(this);
        }

        public void SetMaxHealth(int max, bool refill)
        {
            _maxHealth = Mathf.Max(1, max);
            Current = refill ? _maxHealth : Mathf.Min(Current, _maxHealth);
            Changed?.Invoke(this);
        }

        private void Die(in DamageInfo info)
        {
            IsDead = true;
            PlayFeedback(_deathFeedback, info.Amount);
            Died?.Invoke(this, info);
            EventBus<ActorDiedEvent>.Raise(new ActorDiedEvent(this, info.Source));

            if (_gameOverOnDeath && GameManager.HasInstance)
            {
                GameManager.Instance.GameOver();
            }

            if (_despawnDelay == 0f)
            {
                PoolReturn.ReleaseOrDestroy(gameObject);
            }
            else if (_despawnDelay > 0f)
            {
                _despawnTimer.Start(_despawnDelay);
            }
        }

        private void PlayFeedback(MMF_Player feedback, float intensity)
        {
            if (feedback != null)
            {
                feedback.PlayFeedbacks(transform.position, intensity);
            }
        }

        void IPoolable.OnSpawned() => ResetHealth();

        void IPoolable.OnDespawned() => _despawnTimer.Stop();
    }
}
