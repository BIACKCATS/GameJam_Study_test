using DG.Tweening;
using Febucci.TextAnimatorForUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JSH.UI
{
    /// <summary>
    /// One answer box of the radial choice ring (label + highlight). Driven by RadialChoiceSelector.
    /// Its RectTransform position (relative to the ring center) defines the direction that selects it.
    /// Auto size: when a label does not fit the base box, the box first widens (up to Max Extra Width), then grows
    /// in height - centered, so it grows left/right and up/down equally and the selection direction is unchanged.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class ChoiceSlotView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [Tooltip("Text Animator typewriter on the Label (optional). Empty = text appears instantly.")]
        [SerializeField] private TypewriterComponent _typewriter;
        [Tooltip("Box image tinted on highlight (optional).")]
        [SerializeField] private Graphic _background;
        [SerializeField] private Color _normalColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        [SerializeField] private Color _highlightColor = new Color(0.85f, 0.1f, 0.15f, 1f);
        [SerializeField] private Color _normalTextColor = Color.white;
        [SerializeField] private Color _highlightTextColor = Color.white;
        [SerializeField, Min(1f)] private float _highlightScale = 1.12f;
        [SerializeField, Min(0f)] private float _tweenDuration = 0.12f;

        [Header("Auto size (long labels)")]
        [SerializeField] private bool _autoResize = true;
        [Tooltip("Rect that grows. Empty = the Background's RectTransform, or this object if there is no Background.")]
        [SerializeField] private RectTransform _resizeTarget;
        [Tooltip("How much wider than the base box it may get before it starts growing in height (pixels).")]
        [SerializeField, Min(0f)] private float _maxExtraWidth = 120f;
        [Tooltip("Width search step (pixels). Smaller = tighter fit.")]
        [SerializeField, Min(1f)] private float _widthStep = 20f;
        [SerializeField, Min(0f)] private float _resizeDuration = 0.15f;
        [SerializeField] private Ease _resizeEase = Ease.OutBack;

        private RectTransform _rect;
        private Tween _scaleTween;
        private Tween _colorTween;
        private Tween _sizeTween;
        private RectTransform _target;
        private RectTransform _labelRect;
        private bool _labelFollowsTarget; // label stretches with the target on both axes -> only the target is resized
        private Vector2 _baseSize;
        private Vector2 _padding;         // target size - label size at base
        private Vector2 _currentSize;
        private bool _hasBase;

        public bool IsHighlighted { get; private set; }
        /// <summary>True while the typewriter is still revealing the label.</summary>
        public bool IsTyping => _typewriter != null && _typewriter.IsShowingText;
        public RectTransform Rect => _rect != null ? _rect : _rect = (RectTransform)transform;

        public bool ContainsScreenPoint(Vector2 position, Camera camera, float padding)
        {
            RectTransform bounds = _background != null ? _background.rectTransform
                : _resizeTarget != null ? _resizeTarget : Rect;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(bounds, position, camera, out Vector2 local))
                return false;
            Rect hitRect = bounds.rect;
            hitRect.min -= Vector2.one * padding;
            hitRect.max += Vector2.one * padding;
            return hitRect.Contains(local);
        }

        private void Awake()
        {
            _rect = (RectTransform)transform;
            CaptureBase();
            ApplyInstant(false);
        }

        private void OnDisable()
        {
            _scaleTween?.Kill();
            _colorTween?.Kill();
            _sizeTween?.Kill();
            ApplyInstant(false);
        }

        public void SetText(string text)
        {
            if (_label == null) return;
            string shown = DialogueTextTags.Expand(text);
            string measured = shown;
            if (_typewriter != null)
            {
                _typewriter.ShowText(shown);
                // Size the box with what TMP will actually lay out (Text Animator tags removed).
                string plain = _typewriter.TextAnimator != null ? _typewriter.TextAnimator.textWithoutTextAnimTags : null;
                if (!string.IsNullOrEmpty(plain)) measured = plain;
            }
            else
            {
                _label.text = shown;
            }
            if (_autoResize) ResizeToFit(measured);
        }

        /// <summary>Reveal the whole label now.</summary>
        public void SkipTyping()
        {
            if (IsTyping) _typewriter.SkipTypewriter();
        }

        private void CaptureBase()
        {
            if (_hasBase || _label == null) return;
            _target = _resizeTarget != null ? _resizeTarget
                : _background != null ? _background.rectTransform
                : Rect;
            _labelRect = _label.rectTransform;
            _labelFollowsTarget = _labelRect != _target && _labelRect.IsChildOf(_target)
                                  && _labelRect.anchorMin.x != _labelRect.anchorMax.x
                                  && _labelRect.anchorMin.y != _labelRect.anchorMax.y;
            _baseSize = _target.rect.size;
            _padding = _baseSize - _labelRect.rect.size;
            _currentSize = _baseSize;
            _hasBase = true;
        }

        private void ResizeToFit(string text)
        {
            CaptureBase();
            if (!_hasBase) return;

            // Widen first (up to the limit) until the text fits the base height, then grow in height.
            float maxWidth = _baseSize.x + _maxExtraWidth;
            Vector2 size = _baseSize;
            for (float w = _baseSize.x; ; w = Mathf.Min(w + _widthStep, maxWidth))
            {
                float textHeight = _label.GetPreferredValues(text, w - _padding.x, 0f).y;
                size = new Vector2(w, Mathf.Max(_baseSize.y, textHeight + _padding.y));
                if (size.y <= _baseSize.y || w >= maxWidth) break;
            }

            _sizeTween?.Kill();
            if (_resizeDuration <= 0f || !isActiveAndEnabled)
            {
                ApplySize(size);
                return;
            }
            _sizeTween = DOTween.To(() => _currentSize, ApplySize, size, _resizeDuration)
                .SetEase(_resizeEase).SetUpdate(true);
        }

        // Centered resize that works with any anchors (stretch or fixed).
        private void ApplySize(Vector2 size)
        {
            _currentSize = size;
            _target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            _target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            if (_labelFollowsTarget) return;
            _labelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(0f, size.x - _padding.x));
            _labelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(0f, size.y - _padding.y));
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
