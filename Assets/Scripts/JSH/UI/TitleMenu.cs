using JSH.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JSH.UI
{
    /// <summary>
    /// Title screen buttons. Wire Button.onClick to StartGame / Quit.
    /// Uses SceneLoader (fade) if present in the scene, otherwise loads directly.
    /// </summary>
    public sealed class TitleMenu : MonoBehaviour
    {
        [Tooltip("Scene to load on Start. Empty = next scene in Build Settings.")]
        [SerializeField] private string _gameSceneName;

        private bool _started;

        public void StartGame()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            if (GameManager.HasInstance)
            {
                GameManager.Instance.StartGame();
            }

            bool useName = !string.IsNullOrEmpty(_gameSceneName);
            if (SceneLoader.HasInstance)
            {
                if (useName)
                {
                    SceneLoader.Instance.Load(_gameSceneName);
                }
                else
                {
                    SceneLoader.Instance.LoadNext();
                }

                return;
            }

            if (useName)
            {
                SceneManager.LoadScene(_gameSceneName);
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
    }
}
