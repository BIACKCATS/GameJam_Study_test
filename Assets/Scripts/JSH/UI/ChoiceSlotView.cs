using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JSH.UI
{
    /// <summary>
    /// One answer box of the radial choice ring (label + highlight). Driven by RadialChoiceSelector.
    /// Its RectTransform position (relative to the ring center) defines the direction that selects it.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class ChoiceSlotView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [Tooltip("Box image tinted on highlight (optional).")]
        [SerializeField] private Graphic _background;
        [SerializeField] private Color _normalColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        [SerializeField] private Color _highlightColor = new Color(0.85f, 0.1f, 0.15f, 1f);
        [SerializeField] private Color _normalTextColor = Color.white;
        [SerializeField] private Color _highlightTextColor = Color.white;
        [SerializeField, Min(1f)] private float _highlightScale = 1.12f;
        [SerializeField, Min(0f)] private float _tweenDuration = 0.12f;

        private RectTransform _rect;
        private Tween _scaleTween;
        private Tween _colorTween;

        public bool IsHighlighted { get; private set; }
        public RectTransform Rect => _rect != null ? _rect : _rect = (RectTransform)transform;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            ApplyInstant(false);
        }

        private void OnDisable()
        {
            _scaleTween?.Kill();
            _colorTween?.Kill();
            ApplyInstant(false);
        }

        public void SetText(string text)
        {
            if (_label != null) _label.text = text;
        }

        public void SetHighlighted(bool highlighted)
        {
            if (IsHighlighted == highlighted) return;
            IsHighlighted = highlighted;

            if (_rect == null || !isActiveAndEnabled || _tweenDuration <= 0f)
            {
                ApplyInstant(highlighted);
                return;
            }

            _scaleTween?.Kill();
            _scaleTween = _rect.DOScale(highlighted ? _highlightScale : 1f, _tweenDuration).SetUpdate(true);
            if (_background != null)
            {
                _colorTween?.Kill();
                _colorTween = _background.DOColor(highlighted ? _highlightColor : _normalColor, _tweenDuration).SetUpdate(true);
            }
            if (_label != null) _label.color = highlighted ? _highlightTextColor : _normalTextColor;
        }

        private void ApplyInstant(bool highlighted)
        {
            IsHighlighted = highlighted;
            transform.localScale = Vector3.one * (highlighted ? _highlightScale : 1f);
            if (_background != null) _background.color = highlighted ? _highlightColor : _normalColor;
            if (_label != null) _label.color = highlighted ? _highlightTextColor : _normalTextColor;
        }
    }
}
