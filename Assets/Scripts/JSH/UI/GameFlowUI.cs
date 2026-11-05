using JSH.Core;
using UnityEngine;

namespace JSH.UI
{
    /// <summary>
    /// Shows Pause / GameOver / Clear panels from GameManager state (EventBus).
    /// Wire Button.onClick to the public methods (Resume, Restart, NextStage, GoToTitle, Quit).
    /// Put one per scene on the gameplay Canvas.
    /// </summary>
    public sealed class GameFlowUI : MonoBehaviour
    {
        [SerializeField] private UIPanel _pausePanel;
        [SerializeField] private UIPanel _gameOverPanel;
        [SerializeField] private UIPanel _clearPanel;
        [Tooltip("Scene name for GoToTitle. Empty = build index 0.")]
        [SerializeField] private string _titleSceneName;

        private bool _started;

        private void OnEnable()
        {
            EventBus<GameStateChangedEvent>.Subscribe(OnGameStateChanged);
            if (_started)
            {
                ApplyCurrent();
            }
        }

        // Start (not OnEnable) so every UIPanel has run Awake first.
        private void Start()
        {
            _started = true;
            ApplyCurrent();
        }

        private void ApplyCurrent()
        {
            Apply(GameManager.HasInstance ? GameManager.Instance.State : GameState.Playing, true);
        }

        private void OnDisable()
        {
            EventBus<GameStateChangedEvent>.Unsubscribe(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt) => Apply(evt.Current, false);

        private void Apply(GameState state, bool instant)
        {
            SetPanel(_pausePanel, state == GameState.Paused, instant);
            SetPanel(_gameOverPanel, state == GameState.GameOver, instant);
            SetPanel(_clearPanel, state == GameState.Clear, instant);
        }

        private static void SetPanel(UIPanel panel, bool visible, bool instant)
        {
            if (panel != null && (panel.IsVisible != visible || instant))
            {
                panel.SetVisible(visible, instant);
            }
        }

        // ---- Button hooks ----

        public void Resume()
        {
            if (GameManager.HasInstance && GameManager.Instance.IsPaused)
            {
                GameManager.Instance.TogglePause();
            }
        }

        public void Restart()
        {
            if (SceneLoader.HasInstance)
            {
                SetPlaying();
                SceneLoader.Instance.Reload();
            }
        }

        public void NextStage()
        {
            if (SceneLoader.HasInstance)
            {
                SetPlaying();
                SceneLoader.Instance.LoadNext();
            }
        }

        public void GoToTitle()
        {
            if (!SceneLoader.HasInstance)
            {
                return;
            }

            if (GameManager.HasInstance)
            {
                GameManager.Instance.GoToTitle();
            }

            if (string.IsNullOrEmpty(_titleSceneName))
            {
                SceneLoader.Instance.Load(0);
            }
            else
            {
                SceneLoader.Instance.Load(_titleSceneName);
            }
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // GameManager persists across loads, so reset to Playing before the reload.
        private static void SetPlaying()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.StartGame();
            }
        }
    }
}
