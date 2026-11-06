using JSH.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.DebugTools
{
    /// <summary>
    /// Editor / Development Build only cheat keys + FPS overlay.
    /// F1 God mode | F2 Time scale cycle | F3 Reload scene | F4 Next scene
    /// F5 Force Clear | F6 Force GameOver | F12 Toggle overlay
    /// </summary>
    public sealed class DebugCheats : MonoBehaviour
    {
        [SerializeField] private bool _showOverlay = false;
        [SerializeField] private float[] _timeScales = { 1f, 2f, 4f, 0.25f };
        [SerializeField, Min(0.1f)] private float _fpsRefreshInterval = 0.5f;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private int _timeScaleIndex;
        private int _frameCount;
        private float _fpsTimer;
        private string _overlayText = string.Empty;
        private GUIStyle _style;

        private void Update()
        {
            HandleKeys();
            UpdateFps();
        }

        private void HandleKeys()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null)
            {
                return;
            }

            if (kb.f1Key.wasPressedThisFrame)
            {
                DebugFlags.GodMode = !DebugFlags.GodMode;
                Debug.Log($"[Cheat] GodMode = {DebugFlags.GodMode}");
            }

            if (kb.f2Key.wasPressedThisFrame && _timeScales.Length > 0 && GameManager.HasInstance)
            {
                _timeScaleIndex = (_timeScaleIndex + 1) % _timeScales.Length;
                GameManager.Instance.SetTimeScale(_timeScales[_timeScaleIndex]);
                Debug.Log($"[Cheat] TimeScale = {_timeScales[_timeScaleIndex]}");
            }

            if (kb.f3Key.wasPressedThisFrame && SceneLoader.HasInstance)
            {
                SceneLoader.Instance.Reload();
            }

            if (kb.f4Key.wasPressedThisFrame && SceneLoader.HasInstance)
            {
                SceneLoader.Instance.LoadNext();
            }

            if (kb.f5Key.wasPressedThisFrame && GameManager.HasInstance)
            {
                GameManager.Instance.Clear();
            }

            if (kb.f6Key.wasPressedThisFrame && GameManager.HasInstance)
            {
                GameManager.Instance.GameOver();
            }

            if (kb.f12Key.wasPressedThisFrame)
            {
                _showOverlay = !_showOverlay;
            }
        }

        private void UpdateFps()
        {
            _frameCount++;
            _fpsTimer += Time.unscaledDeltaTime;
            if (_fpsTimer < _fpsRefreshInterval)
            {
                return;
            }

            float fps = _frameCount / _fpsTimer;
            string state = GameManager.HasInstance ? GameManager.Instance.State.ToString() : "-";
            _overlayText = $"FPS {fps:0}  |  State {state}  |  x{Time.timeScale:0.##}  |  God {(DebugFlags.GodMode ? "ON" : "off")}";
            _frameCount = 0;
            _fpsTimer = 0f;
        }

        private void OnGUI()
        {
            if (!_showOverlay)
            {
                return;
            }

            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 16, normal = { textColor = Color.yellow } };
            GUI.Label(new Rect(10f, 10f, 800f, 30f), _overlayText, _style);
        }
#endif
    }
}
