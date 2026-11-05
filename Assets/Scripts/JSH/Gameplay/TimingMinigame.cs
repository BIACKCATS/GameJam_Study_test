using JSH.Data;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Pure logic of the Timing (설득) minigame. Ticked by MinigameDirector; UI reads Position (-1..1),
    /// TargetMin/TargetMax and TimeLeft. Stop() resolves the game once.
    /// </summary>
    public class TimingMinigame
    {
        private readonly TimingSettings _settings;
        private float _readyLeft;
        private float _timeLeft;
        private float _elapsed;

        public float Position { get; private set; } = -1f;
        public float TargetMin => _settings.TargetCenter - _settings.TargetHalfWidth;
        public float TargetMax => _settings.TargetCenter + _settings.TargetHalfWidth;
        public bool IsReady => _readyLeft <= 0f;
        public float TimeLeft => _timeLeft;
        public bool IsFinished { get; private set; }
        public bool Success { get; private set; }
        public bool TimedOut { get; private set; }

        public TimingMinigame(TimingSettings settings)
        {
            _settings = settings;
            _readyLeft = settings.ReadyDelay;
            _timeLeft = settings.TimeLimit;
        }

        public void Tick(float deltaTime)
        {
            if (IsFinished) return;

            _elapsed += deltaTime;
            // One sweep = -1 -> +1 (distance 2). PingPong over [0, 2] then shift to [-1, 1].
            Position = Mathf.PingPong(_elapsed * _settings.SweepsPerSecond * 2f, 2f) - 1f;

            if (_readyLeft > 0f) { _readyLeft -= deltaTime; return; }

            _timeLeft -= deltaTime;
            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                TimedOut = true;
                Finish(false);
            }
        }

        /// <returns>True if the stop was accepted (ready and not finished).</returns>
        public bool Stop()
        {
            if (IsFinished || !IsReady) return false;
            Finish(Position >= TargetMin && Position <= TargetMax);
            return true;
        }

        private void Finish(bool success)
        {
            IsFinished = true;
            Success = success;
        }
    }
}
