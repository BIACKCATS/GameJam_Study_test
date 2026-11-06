using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    public sealed class QteMinigame
    {
        private readonly float _gain;
        private readonly float _drain;
        private float _readyLeft;
        public float Gauge { get; private set; }
        public int LastButton { get; private set; } = -1;
        public bool IsReady => _readyLeft <= 0f;
        public bool IsFinished { get; private set; }
        public bool Success { get; private set; }

        public QteMinigame(QteSettings settings)
        {
            Gauge = Mathf.Clamp(settings.startingGauge, 0.01f, 0.99f);
            _gain = Mathf.Max(0.001f, settings.gainPerClick);
            _drain = Mathf.Max(0.001f, settings.drainPerSecond);
            _readyLeft = Mathf.Max(0f, settings.readySeconds);
        }

        public void Tick(float deltaTime)
        {
            if (IsFinished || deltaTime <= 0f) return;
            if (!IsReady) { _readyLeft -= deltaTime; return; }
            Gauge = Mathf.Max(0f, Gauge - _drain * deltaTime);
            if (Gauge <= 0f) IsFinished = true;
        }

        // Both buttons in one frame are rejected; the first single click can be either side.
        public bool Click(bool left, bool right)
        {
            if (!IsReady || IsFinished || left == right) return false;
            int button = left ? 0 : 1;
            if (button == LastButton) return false;
            LastButton = button;
            Gauge = Mathf.Min(1f, Gauge + _gain);
            if (Gauge >= 1f) { Success = true; IsFinished = true; }
            return true;
        }
    }
}
