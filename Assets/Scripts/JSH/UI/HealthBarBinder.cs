using JSH.Gameplay;
using MoreMountains.Tools;
using UnityEngine;

namespace JSH.UI
{
    /// <summary>
    /// Drives a Feel MMProgressBar from a JSH Health (HUD bar or world-space enemy bar).
    /// The bar's delayed/lerp/bump/color effects are all configured on MMProgressBar itself.
    /// </summary>
    public sealed class HealthBarBinder : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private MMProgressBar _bar;
        [Tooltip("Hide the bar GameObject when the target dies (enemy bars). Put this binder on a parent, not on the bar itself.")]
        [SerializeField] private bool _hideOnDeath;

        private void OnEnable()
        {
            if (_health == null || _bar == null)
            {
                return;
            }

            _health.Changed += OnHealthChanged;
            _bar.SetBar(_health.Current, 0f, _health.Max);
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.Changed -= OnHealthChanged;
            }
        }

        /// <summary>Rebind at runtime (e.g. pooled enemy, respawned player).</summary>
        public void SetTarget(Health health)
        {
            OnDisable();
            _health = health;
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void OnHealthChanged(Health health)
        {
            _bar.UpdateBar(health.Current, 0f, health.Max);

            if (_hideOnDeath && health.IsDead)
            {
                _bar.gameObject.SetActive(false);
            }
            else if (!_bar.gameObject.activeSelf && !health.IsDead)
            {
                _bar.gameObject.SetActive(true);
            }
        }
    }
}
