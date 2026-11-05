using System;
using UnityEngine;

namespace JSH.Core
{
    /// <summary>
    /// Zero-allocation countdown for cooldowns, i-frames, durations, wave timers.
    /// Keep it as a non-readonly FIELD (it is a struct; properties/locals get copied).
    /// Usage: if (_attackCd.IsReady) { Attack(); _attackCd.Start(); }  ...  _attackCd.Tick(Time.deltaTime);
    /// For consume-then-refill resources (stamina, dash charge) use Feel's MMCooldown instead.
    /// </summary>
    [Serializable]
    public struct Countdown
    {
        [SerializeField, Min(0f)] private float _duration;

        private float _remaining;
        private bool _running;

        public Countdown(float duration)
        {
            _duration = Mathf.Max(0f, duration);
            _remaining = 0f;
            _running = false;
        }

        public float Duration => _duration;
        public float Remaining => _remaining;
        public bool IsRunning => _running;
        public bool IsReady => !_running;

        /// <summary>0 at start → 1 when finished (1 while idle).</summary>
        public float Progress => _running && _duration > 0f ? 1f - (_remaining / _duration) : 1f;

        public void Start()
        {
            _remaining = _duration;
            _running = _duration > 0f;
        }

        public void Start(float duration)
        {
            _duration = Mathf.Max(0f, duration);
            Start();
        }

        public void Stop()
        {
            _remaining = 0f;
            _running = false;
        }

        /// <summary>Advance by deltaTime. Returns true only on the tick it finishes.</summary>
        public bool Tick(float deltaTime)
        {
            if (!_running)
            {
                return false;
            }

            _remaining -= deltaTime;
            if (_remaining > 0f)
            {
                return false;
            }

            _remaining = 0f;
            _running = false;
            return true;
        }
    }
}
