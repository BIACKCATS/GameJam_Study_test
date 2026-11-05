using JSH.Core;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Player life count (default 3). Data flow only: UI/feedback subscribe to LivesChangedEvent / LivesDepletedEvent.
    /// </summary>
    public class LifeSystem : Singleton<LifeSystem>
    {
        [SerializeField, Min(1)] private int _maxLives = 3;

        private int _lives;

        public int Lives => _lives;
        public int MaxLives => _maxLives;
        public bool IsDepleted => _lives <= 0;

        protected override void OnSingletonAwake() => _lives = _maxLives;

        public void Damage(int amount = 1) => Change(-Mathf.Abs(amount));

        public void Heal(int amount = 1) => Change(Mathf.Abs(amount));

        public void ResetLives()
        {
            int delta = _maxLives - _lives;
            _lives = _maxLives;
            EventBus<LivesChangedEvent>.Raise(new LivesChangedEvent(_lives, _maxLives, delta));
        }

        private void Change(int delta)
        {
            if (IsDepleted || delta == 0) return;
            int before = _lives;
            _lives = Mathf.Clamp(_lives + delta, 0, _maxLives);
            if (_lives == before) return;

            EventBus<LivesChangedEvent>.Raise(new LivesChangedEvent(_lives, _maxLives, _lives - before));
            if (_lives == 0) EventBus<LivesDepletedEvent>.Raise(new LivesDepletedEvent());
        }
    }
}
