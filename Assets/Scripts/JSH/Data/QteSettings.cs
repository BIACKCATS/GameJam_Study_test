using UnityEngine;

namespace JSH.Data
{
    [CreateAssetMenu(menuName = "JSH/Minigames/QTE Settings")]
    public sealed class QteSettings : ScriptableObject
    {
        [Range(0.01f, 0.99f)] public float startingGauge = 0.5f;
        [Min(0.001f)] public float gainPerClick = 0.10f;
        [Min(0.001f)] public float drainPerSecond = 0.16f;
        [Min(0f)] public float readySeconds = 0.6f;
        [Min(0f)] public float resultHoldSeconds = 0.6f;
        [Min(1f)] public float framesPerSecond = 16f;
        public Texture2D leftSheet;
        public Texture2D rightSheet;
        public TMPro.TMP_FontAsset font;
    }
}
