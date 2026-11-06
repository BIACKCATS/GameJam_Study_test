using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JSH.Core
{
    /// <summary>
    /// Async scene loading with optional fade. Uses unscaled time (works while paused).
    /// Fade: assign a full-screen black Image's CanvasGroup on a child Canvas of [Managers].
    /// </summary>
    public sealed class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField] private CanvasGroup _fadeGroup;
        [SerializeField, Min(0f)] private float _fadeDuration = 0.3f;

        public bool IsLoading { get; private set; }
        public float FadeDuration { get => _fadeDuration; set => _fadeDuration = Mathf.Max(0f, value); }

        protected override void OnDuplicateInstance()
        {
            // Title's fade loader persists into SideView. Its duplicate lives on
            // [Managers] alongside GameManager and EndingStatsTracker: keep those alive.
            enabled = false;
            Destroy(this);
        }

        protected override void OnSingletonAwake()
        {
            EnsureFadeOverlay();
            SetFade(0f);
        }

        private void EnsureFadeOverlay()
        {
            if (_fadeGroup != null) return;
            var overlay = new GameObject("SceneFadeOverlay", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.GraphicRaycaster), typeof(CanvasGroup));
            overlay.transform.SetParent(transform, false);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;
            _fadeGroup = overlay.GetComponent<CanvasGroup>();
            var black = new GameObject("Black", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            var rect = (RectTransform)black.transform;
            rect.SetParent(overlay.transform, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            black.GetComponent<UnityEngine.UI.Image>().color = Color.black;
        }
        public void Load(string sceneName)
        {
            if (!IsLoading)
            {
                StartCoroutine(LoadRoutine(sceneName, -1));
            }
        }

        public void Load(int buildIndex)
        {
            if (!IsLoading)
            {
                StartCoroutine(LoadRoutine(null, buildIndex));
            }
        }

        public void Reload() => Load(SceneManager.GetActiveScene().buildIndex);

        /// <summary>Loads next scene in Build Settings, wrapping to index 0.</summary>
        public void LoadNext()
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            Load(next < SceneManager.sceneCountInBuildSettings ? next : 0);
        }

        private IEnumerator LoadRoutine(string sceneName, int buildIndex)
        {
            IsLoading = true;
            yield return Fade(0f, 1f);

            AsyncOperation op = sceneName != null
                ? SceneManager.LoadSceneAsync(sceneName)
                : SceneManager.LoadSceneAsync(buildIndex);

            if (op != null)
            {
                while (!op.isDone)
                {
                    yield return null;
                }
            }
            else
            {
                Debug.LogError($"[SceneLoader] Failed to load scene '{sceneName ?? buildIndex.ToString()}'. Is it in Build Settings?");
            }

            yield return Fade(1f, 0f);
            IsLoading = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            if (_fadeGroup == null || _fadeDuration <= 0f)
            {
                SetFade(to);
                yield break;
            }

            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                SetFade(Mathf.Lerp(from, to, t / _fadeDuration));
                yield return null;
            }

            SetFade(to);
        }

        private void SetFade(float alpha)
        {
            if (_fadeGroup == null)
            {
                return;
            }

            _fadeGroup.alpha = alpha;
            _fadeGroup.blocksRaycasts = alpha > 0.001f;
        }
    }
}
