using JSH.Data;

namespace JSH.Gameplay
{
    /// <summary>
    /// Pure logic of the Reflex (퇴치) minigame. Ticked by MinigameDirector; UI reads IsGuarding / TimeLeft.
    /// The target alternates Guard/Open on random timers; Hit() resolves the game once.
    /// </summary>
    public class ReflexMinigame
    {
        private readonly ReflexSettings _settings;
        private readonly bool _targetIsVillain;
        private float _readyLeft;
        private float _timeLeft;
        private float _stateLeft;

        public bool IsGuarding { get; private set; }
        public bool IsReady => _readyLeft <= 0f;
        public float TimeLeft => _timeLeft;
        public bool IsFinished { get; private set; }
        public ReflexResult Result { get; private set; }
        /// <summary>True if the hit landed while the target was guarding (for feedback).</summary>
        public bool HitWhileGuarding { get; private set; }

        public ReflexMinigame(ReflexSettings settings, bool targetIsVillain)
        {
            _settings = settings;
            _targetIsVillain = targetIsVillain;
            _readyLeft = settings.ReadyDelay;
            _timeLeft = settings.TimeLimit;
            IsGuarding = settings.StartGuarding;
            _stateLeft = IsGuarding ? settings.RandomGuardDuration() : settings.RandomOpenDuration();
        }

        /// <returns>True if Guard/Open changed this tick.</returns>
        public bool Tick(float deltaTime)
        {
            if (IsFinished) return false;

            bool changed = false;
            _stateLeft -= deltaTime;
            if (_stateLeft <= 0f)
            {
                IsGuarding = !IsGuarding;
                _stateLeft = IsGuarding ? _settings.RandomGuardDuration() : _settings.RandomOpenDuration();
                changed = true;
            }

            if (_readyLeft > 0f) { _readyLeft -= deltaTime; return changed; }

            _timeLeft -= deltaTime;
            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                Finish(ReflexResult.TimedOut);
            }
            return changed;
        }

        /// <returns>True if the hit was accepted (ready and not finished).</returns>
        public bool Hit()
        {
            if (IsFinished || !IsReady) return false;
            HitWhileGuarding = IsGuarding;
            if (!_targetIsVillain) Finish(ReflexResult.PassengerHit);
            else Finish(IsGuarding ? ReflexResult.VillainDefeatedGuarded : ReflexResult.VillainDefeatedOpen);
            return true;
        }

        private void Finish(ReflexResult result)
        {
            IsFinished = true;
            Result = result;
        }
    }
}
