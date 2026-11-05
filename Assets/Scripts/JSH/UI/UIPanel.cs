using DG.Tweening;
using UnityEngine;

namespace JSH.UI
{
    /// <summary>
    /// Show/hide a UI panel with a DOTween CanvasGroup fade.
    /// Uses unscaled time, so it works while paused (timeScale 0).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIPanel : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _fadeDuration = 0.2f;
        [SerializeField] private bool _visibleOnStart;

        private CanvasGroup _group;
        private Tween _tween;

        private bool _hasState;

        public bool IsVisible { get; private set; }

        private void Awake()
        {
            // Apply the default only if nobody set a state before Awake (GameFlowUI may have).
            if (!_hasState)
            {
                SetVisible(_visibleOnStart, true);
            }
        }

        private void OnDisable() => KillTween();

        public void Show() => SetVisible(true, false);
        public void Hide() => SetVisible(false, false);
        public void Toggle() => SetVisible(!IsVisible, false);

        public void SetVisible(bool visible, bool instant)
        {
            // Safe even if called before Awake (e.g. panel GameObject starts inactive).
            if (_group == null && !TryGetComponent(out _group))
            {
                return;
            }

            _hasState = true;
            IsVisible = visible;
            if (visible && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            _group.interactable = visible;
            _group.blocksRaycasts = visible;

            KillTween();

            float target = visible ? 1f : 0f;
            if (instant || _fadeDuration <= 0f || !isActiveAndEnabled)
            {
                _group.alpha = target;
                return;
            }

            _tween = _group.DOFade(target, _fadeDuration).SetUpdate(true);
        }

        private void KillTween()
        {
            if (_tween != null && _tween.IsActive())
            {
                _tween.Kill();
            }

            _tween = null;
        }
    }
}
