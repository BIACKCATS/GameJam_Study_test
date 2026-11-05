using UnityEngine;

namespace JSH.Data
{
    /// <summary>
    /// Tuning for the Timing (설득) minigame. The line ping-pongs over a normalized box [-1, 1];
    /// stopping inside [TargetCenter - TargetHalfWidth, TargetCenter + TargetHalfWidth] succeeds.
    /// </summary>
    [CreateAssetMenu(fileName = "TimingSettings", menuName = "JSH/Minigame/Timing Settings")]
    public class TimingSettings : ScriptableObject
    {
        [Tooltip("Seconds before input is accepted. Not counted in the time limit.")]
        [SerializeField, Min(0f)] private float _readyDelay = 0.5f;
        [Tooltip("Seconds to stop the line after the ready delay. Timeout = fail.")]
        [SerializeField, Min(0.1f)] private float _timeLimit = 5f;
        [Tooltip("Full left-to-right sweeps per second.")]
        [SerializeField, Min(0.01f)] private float _sweepsPerSecond = 1.2f;
        [SerializeField, Range(-1f, 1f)] private float _targetCenter = 0f;
        [SerializeField, Range(0.01f, 1f)] private float _targetHalfWidth = 0.15f;

        public float ReadyDelay => _readyDelay;
        public float TimeLimit => _timeLimit;
        public float SweepsPerSecond => _sweepsPerSecond;
        public float TargetCenter => _targetCenter;
        public float TargetHalfWidth => _targetHalfWidth;
    }
}
