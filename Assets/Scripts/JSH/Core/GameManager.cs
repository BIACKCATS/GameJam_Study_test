using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.Core
{
    /// <summary>
    /// Owns the game flow state and Time.timeScale.
    /// Listen via EventBus <GameStateChangedEvent>
    /// </summary>
    public sealed class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameState _initialState = GameState.Playing;
        [SerializeField] private bool _enablePauseKey = true;

        private float _normalTimeScale = 1f;

        public GameState State { get; private set; } = GameState.Boot;
        public GameState PreviousState { get; private set; } = GameState.Boot;
        public bool IsPaused => State == GameState.Paused;
        public bool IsPlaying => State == GameState.Playing;
        public float NormalTimeScale => _normalTimeScale;

        private void Start()
        {
            // Start (not Awake) so listeners subscribed in OnEnable receive the first state.
            ChangeState(_initialState);
        }

        private void Update()
        {
            if (!_enablePauseKey || !WasPausePressed())
            {
                return;
            }

            TogglePause();
        }

        public void ChangeState(GameState next)
        {
            if (State == next)
            {
                return;
            }

            PreviousState = State;
            State = next;
            RefreshTimeScale();
            EventBus<GameStateChangedEvent>.Raise(new GameStateChangedEvent(PreviousState, State));
        }

        public void StartGame() => ChangeState(GameState.Playing);
        public void GameOver() => ChangeState(GameState.GameOver);
        public void Clear() => ChangeState(GameState.Clear);
        public void GoToTitle() => ChangeState(GameState.Title);

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
            else if (State == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }

        /// <summary>Base time scale while not paused (debug speed-up, slow-mo, etc.).</summary>
        public void SetTimeScale(float scale)
        {
            _normalTimeScale = Mathf.Max(0f, scale);
            RefreshTimeScale();
        }

        public void RefreshTimeScale()
        {
            Time.timeScale = IsPaused ? 0f : _normalTimeScale;
        }

        private static bool WasPausePressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.startButton.wasPressedThisFrame;
        }
    }
}
