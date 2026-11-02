using JSH.Core;
using UnityEngine;

namespace JSH.Juice
{
    /// <summary>
    /// Brief time freeze on impact. Pause-aware (never overrides GameManager pause).
    /// Usage: HitStop.Play(0.06f);
    /// For shake / flash / floating text, use Feel's MMF_Player instead.
    /// </summary>
    public sealed class HitStop : Singleton<HitStop>
    {
        [SerializeField, Range(0f, 1f)] private float _stopTimeScale;
        [SerializeField, Min(0f)] private float _maxDuration = 0.25f;

        private float _remaining;
        private bool _active;

        public static void Play(float duration)
        {
            if (HasInstance)
            {
                Instance.Trigger(duration);
            }
        }

        public void Trigger(float duration)
        {
            if (duration <= 0f || IsGamePaused())
            {
                return;
            }

            _remaining = Mathf.Min(Mathf.Max(_remaining, duration), _maxDuration);
            _active = true;
            Time.timeScale = _stopTimeScale;
        }

        private void Update()
        {
            if (!_active)
            {
                return;
            }

            if (IsGamePaused())
            {
                // Pause took over; GameManager already set timeScale to 0.
                _active = false;
                _remaining = 0f;
                return;
            }

            _remaining -= Time.unscaledDeltaTime;
            if (_remaining <= 0f)
            {
                _active = false;
                _remaining = 0f;
                RestoreTimeScale();
            }
        }

        private static bool IsGamePaused() => GameManager.HasInstance && GameManager.Instance.IsPaused;

        private static void RestoreTimeScale()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.RefreshTimeScale();
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }
}
