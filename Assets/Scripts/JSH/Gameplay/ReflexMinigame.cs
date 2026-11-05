using JSH.Data;

namespace JSH.Gameplay
{
    /// <summary>
    /// Pure logic of the Reflex (퇴치) minigame. Ticked by MinigameDirector; UI reads IsGuarding / IsReady.
    /// The target alternates Guard/Open on random timers; Hit() resolves the game once. No time limit.
    /// </summary>
    public class ReflexMinigame
    {
        private readonly ReflexSettings _settings;
        private readonly bool _targetIsVillain;
        private float _readyLeft;
        private float _stateLeft;

        public bool IsGuarding { get; private set; }
        public bool IsReady => _readyLeft <= 0f;
        public bool IsFinished { get; private set; }
        public ReflexResult Result { get; private set; }
        /// <summary>True if the hit landed while the target was guarding (for feedback).</summary>
        public bool HitWhileGuarding { get; private set; }

        public ReflexMinigame(ReflexSettings settings, bool targetIsVillain)
        {
            _settings = settings;
            _targetIsVillain = targetIsVillain;
            _readyLeft = settings.ReadyDelay;
            IsGuarding = settings.StartGuarding;
            _stateLeft = IsGuarding ? settings.RandomGuardDuration() : settings.RandomOpenDuration();
        }

        /// <returns>True if Guard/Open changed this tick.</returns>
        public bool Tick(float deltaTime)
        {
            if (IsFinished) return false;
            if (_readyLeft > 0f) _readyLeft -= deltaTime;

            _stateLeft -= deltaTime;
            if (_stateLeft > 0f) return false;

            IsGuarding = !IsGuarding;
            _stateLeft = IsGuarding ? _settings.RandomGuardDuration() : _settings.RandomOpenDuration();
            return true;
        }

        /// <returns>True if the hit was accepted (ready and not finished).</returns>
        public bool Hit()
        {
            if (IsFinished || !IsReady) return false;
            HitWhileGuarding = IsGuarding;
            if (!_targetIsVillain) Result = ReflexResult.PassengerHit;
            else Result = IsGuarding ? ReflexResult.VillainDefeatedGuarded : ReflexResult.VillainDefeatedOpen;
            IsFinished = true;
            return true;
        }
    }
}
