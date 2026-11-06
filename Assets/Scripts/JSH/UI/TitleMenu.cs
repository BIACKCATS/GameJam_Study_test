using JSH.Core;
using BC.Core;
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

        private void Start()
        {
            // The title scene can also be launched directly from the Editor.
            if (!SoundManager.Instance) new GameObject("SoundManager").AddComponent<SoundManager>();
            SoundManager.Instance.PlayPresentationMusic(SoundNames.StartMusic);
        }

        private void OnDestroy()
        {
            if (SoundManager.Instance) SoundManager.Instance.StopSound(SoundNames.StartMusic);
        }
        public void StartGame()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            if (SoundManager.Instance) SoundManager.Instance.StopSound(SoundNames.StartMusic);
            if (GameManager.HasInstance)
            {
                GameManager.Instance.StartGame();
            }

            bool useName = !string.IsNullOrEmpty(_gameSceneName);
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (SceneLoader.HasInstance && nextIndex >= SceneManager.sceneCountInBuildSettings) nextIndex = 0;
            if (SoundManager.Instance) SoundManager.Instance.QueueTrainForScene(useName ? _gameSceneName : null, nextIndex);
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
