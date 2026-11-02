using UnityEngine;

namespace JSH.Core
{
    /// <summary>Returns the object to its pool (or destroys it) after a lifetime. For VFX, bullets, popups.</summary>
    public sealed class AutoRelease : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _lifetime = 1f;
        [SerializeField] private bool _useUnscaledTime;

        private float _remaining;

        private void OnEnable()
        {
            _remaining = _lifetime;
        }

        private void Update()
        {
            _remaining -= _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (_remaining <= 0f)
            {
                PoolReturn.ReleaseOrDestroy(gameObject);
            }
        }
    }
}
