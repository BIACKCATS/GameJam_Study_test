using UnityEngine;

namespace JSH.Data
{
    /// <summary>Tuning for the Reflex (퇴치) minigame: target alternates Guard / Open; the player hits once.</summary>
    [CreateAssetMenu(fileName = "ReflexSettings", menuName = "JSH/Minigame/Reflex Settings")]
    public class ReflexSettings : ScriptableObject
    {
        [Tooltip("Seconds before input is accepted (prevents the dialogue click from leaking in). Not counted in the time limit.")]
        [SerializeField, Min(0f)] private float _readyDelay = 0.5f;
        [Tooltip("Seconds to hit after the ready delay. On timeout the game ends with no effect.")]
        [SerializeField, Min(0.1f)] private float _timeLimit = 5f;
        [SerializeField] private bool _startGuarding = true;
        [SerializeField] private Vector2 _guardDuration = new Vector2(0.6f, 1.5f);
        [SerializeField] private Vector2 _openDuration = new Vector2(0.3f, 0.8f);

        public float ReadyDelay => _readyDelay;
        public float TimeLimit => _timeLimit;
        public bool StartGuarding => _startGuarding;

        public float RandomGuardDuration() => Random.Range(_guardDuration.x, Mathf.Max(_guardDuration.x, _guardDuration.y));
        public float RandomOpenDuration() => Random.Range(_openDuration.x, Mathf.Max(_openDuration.x, _openDuration.y));
    }
}
