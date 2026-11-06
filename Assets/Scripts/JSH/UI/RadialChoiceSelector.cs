using JSH.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.UI
{
    /// <summary>
    /// Persona-style radial choice ring. Answer boxes (ChoiceSlotView) sit around a center (the portrait);
    /// the mouse must be inside the visible answer box plus a small margin to select it.
    /// DialogueView refreshes SelectedIndex before confirming on click.
    /// Works with any slot count; the design uses 2 (lower-left / upper-right).
    /// </summary>
    public sealed class RadialChoiceSelector : MonoBehaviour
    {
        [Tooltip("Ring center (usually the portrait). Directions are measured from here.")]
        [SerializeField] private RectTransform _center;
        [Tooltip("Answer boxes in choice order (index 0 = DialogueNode choice 0).")]
        [SerializeField] private ChoiceSlotView[] _slots = new ChoiceSlotView[0];
        [Tooltip("Shown/hidden with the choices. Empty = this GameObject's children are left as they are.")]
        [SerializeField] private GameObject _root;
        [Tooltip("Extra click margin outside each visible choice box (UI units per edge).")]
        [SerializeField, Min(0f)] private float _hitPadding = 10f;
        [Tooltip("Optional arrow/needle rotated toward the mouse (its right side = 0 degrees).")]
        [SerializeField] private RectTransform _pointer;

        private Canvas _canvas;
        private int _activeCount;

        public bool IsShown { get; private set; }
        /// <summary>-1 = nothing selected (dead zone or hidden).</summary>
        public int SelectedIndex { get; private set; } = -1;

        private void Awake()
        {
            CacheCanvas();
            if (!IsShown) SetRootActive(false);
        }

        public void Show(DialogueNode node)
        {
            CacheCanvas();
            _activeCount = Mathf.Min(node.ChoiceCount, _slots.Length);
            if (node.ChoiceCount > _slots.Length)
                Debug.LogWarning($"[RadialChoiceSelector] Node has {node.ChoiceCount} choices but only {_slots.Length} slots.", this);

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == null) continue;
                bool used = i < _activeCount;
                _slots[i].gameObject.SetActive(used);
                if (used)
                {
                    _slots[i].SetText(node.GetChoice(i).Text);
                    _slots[i].SetHighlighted(false);
                }
            }

            SelectedIndex = -1;
            IsShown = true;
            SetRootActive(true);
            UpdateSelection();
        }

        /// <summary>True while any shown choice label is still being typed.</summary>
        public bool IsTyping
        {
            get
            {
                if (!IsShown) return false;
                for (int i = 0; i < _activeCount; i++)
                {
                    if (_slots[i] != null && _slots[i].IsTyping) return true;
                }
                return false;
            }
        }

        /// <summary>Reveal every shown choice label now.</summary>
        public void SkipTyping()
        {
            for (int i = 0; i < _activeCount; i++)
            {
                if (_slots[i] != null) _slots[i].SkipTyping();
            }
        }

        public void Hide()
        {
            IsShown = false;
            Select(-1);
            SetRootActive(false);
        }

        private void Update()
        {
            if (IsShown) UpdateSelection();
        }

        public void UpdateSelection()
        {
            Mouse mouse = Mouse.current;
            if (!IsShown || mouse == null || _canvas == null)
            {
                Select(-1);
                return;
            }

            Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            Vector2 position = mouse.position.ReadValue();
            Vector2 center = _center != null ? RectTransformUtility.WorldToScreenPoint(cam, _center.position) : position;
            Vector2 toMouse = position - center;

            if (_pointer != null && toMouse.sqrMagnitude > 0.01f)
                _pointer.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg);

            int best = -1;
            float bestDistance = float.PositiveInfinity;
            for (int i = 0; i < _activeCount; i++)
            {
                if (_slots[i] == null || !_slots[i].gameObject.activeInHierarchy
                    || !_slots[i].ContainsScreenPoint(position, cam, _hitPadding)) continue;
                float distance = (RectTransformUtility.WorldToScreenPoint(cam, _slots[i].Rect.position) - position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }
            Select(best);
        }

        private void Select(int index)
        {
            if (SelectedIndex == index) return;
            SelectedIndex = index;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != null && _slots[i].gameObject.activeInHierarchy) _slots[i].SetHighlighted(i == index);
            }
        }

        private void SetRootActive(bool active)
        {
            if (_root != null && _root.activeSelf != active) _root.SetActive(active);
        }

        private void CacheCanvas()
        {
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>(true);
            if (_canvas != null) _canvas = _canvas.rootCanvas;
        }
    }
}
