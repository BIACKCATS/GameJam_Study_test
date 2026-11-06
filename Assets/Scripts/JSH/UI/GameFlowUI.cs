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
        private bool _pauseCursorSaved;
        private bool _cursorVisible;
        private CursorLockMode _cursorLock;

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
            ConfigurePauseMenu();
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
            RestoreCursor();
        }

        private void OnGameStateChanged(GameStateChangedEvent evt) => Apply(evt.Current, false);

        private void Apply(GameState state, bool instant)
        {
            if (state == GameState.Paused && !_pauseCursorSaved)
            {
                _cursorVisible = Cursor.visible;
                _cursorLock = Cursor.lockState;
                _pauseCursorSaved = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (state != GameState.Paused)
            {
                RestoreCursor();
            }
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

        private void RestoreCursor()
        {
            if (!_pauseCursorSaved) return;
            Cursor.lockState = _cursorLock;
            Cursor.visible = _cursorVisible;
            _pauseCursorSaved = false;
        }

        private void ConfigurePauseMenu()
        {
            if (_pausePanel == null) return;
            var panel = (RectTransform)_pausePanel.transform;
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.offsetMin = panel.offsetMax = Vector2.zero;
            panel.localScale = Vector3.one;
            panel.localRotation = Quaternion.identity;
            panel.SetAsLastSibling();
            var canvas = _pausePanel.GetComponent<Canvas>();
            if (canvas == null) canvas = _pausePanel.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 32000;
            if (!_pausePanel.TryGetComponent<UnityEngine.UI.GraphicRaycaster>(out _))
                _pausePanel.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var resume = panel.Find("BtnResume")?.GetComponent<UnityEngine.UI.Button>();
            if (resume == null) return;
            var quit = Instantiate(resume, panel);
            quit.name = "BtnQuit";
            // Replace copied Inspector callbacks rather than adding a second action.
            resume.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            quit.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            resume.onClick.AddListener(Resume);
            quit.onClick.AddListener(Quit);

            var card = new GameObject("PauseCard", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            card.transform.SetParent(panel, false);
            card.transform.SetAsFirstSibling();
            var cardRect = (RectTransform)card.transform;
            Place(cardRect, Vector2.zero, new Vector2(420f, 360f));
            card.GetComponent<UnityEngine.UI.Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.98f);
            var backdrop = panel.GetComponent<UnityEngine.UI.Image>();
            if (backdrop != null) backdrop.color = new Color(0f, 0f, 0f, 0.65f);
            resume.transform.SetParent(cardRect, false);
            quit.transform.SetParent(cardRect, false);
            Place((RectTransform)resume.transform, new Vector2(0f, 20f), new Vector2(320f, 64f));
            Place((RectTransform)quit.transform, new Vector2(0f, -110f), new Vector2(320f, 64f));
            resume.GetComponentInChildren<TMPro.TMP_Text>(true).text = "돌아가기";
            quit.GetComponentInChildren<TMPro.TMP_Text>(true).text = "종료하기";
            var appearance = Resources.Load<PauseMenuAppearance>("PauseMenuAppearance");
            if (appearance != null)
            {
                ApplyButtonSprites(resume, appearance.returnNormal, appearance.returnPressed);
                ApplyButtonSprites(quit, appearance.exitNormal, appearance.exitPressed);
            }
            var title = panel.Find("Title")?.GetComponent<TMPro.TMP_Text>();
            if (title != null)
            {
                title.transform.SetParent(cardRect, false);
                title.alignment = TMPro.TextAlignmentOptions.Center;
                title.text = "일시정지";
                Place(title.rectTransform, new Vector2(0f, 130f), new Vector2(360f, 50f));
            }
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void ApplyButtonSprites(UnityEngine.UI.Button button, Sprite normal, Sprite pressed)
        {
            if (normal == null || pressed == null) return;
            var image = button.GetComponent<UnityEngine.UI.Image>();
            image.sprite = normal;
            image.type = UnityEngine.UI.Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            button.targetGraphic = image;
            button.transition = UnityEngine.UI.Selectable.Transition.SpriteSwap;
            button.spriteState = new UnityEngine.UI.SpriteState
            {
                highlightedSprite = normal,
                selectedSprite = normal,
                pressedSprite = pressed,
                disabledSprite = normal
            };
            foreach (var label in button.GetComponentsInChildren<TMPro.TMP_Text>(true))
                label.gameObject.SetActive(false); // The supplied PNG already contains the label.
            var rect = (RectTransform)button.transform;
            rect.sizeDelta = new Vector2(320f, 320f * normal.rect.height / normal.rect.width);
        }

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
