using JSH.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.UI
{
    /// <summary>
    /// Persona-style radial choice ring. Answer boxes (ChoiceSlotView) sit around a center (the portrait);
    /// the mouse direction from the center picks the box whose direction is closest (no need to hover the box).
    /// Inside the dead zone nothing is selected. DialogueView reads SelectedIndex and confirms on click.
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
        [Tooltip("Radius around the center (canvas units) where no choice is selected.")]
        [SerializeField, Min(0f)] private float _deadZoneRadius = 80f;
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

        private void UpdateSelection()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || _center == null || _canvas == null) return;

            Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(cam, _center.position);
            Vector2 toMouse = mouse.position.ReadValue() - center;

            if (_pointer != null && toMouse.sqrMagnitude > 0.01f)
                _pointer.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg);

            float deadZone = _deadZoneRadius * _canvas.scaleFactor;
            if (toMouse.sqrMagnitude < deadZone * deadZone)
            {
                Select(-1);
                return;
            }

            Vector2 mouseDir = toMouse.normalized;
            int best = -1;
            float bestDot = float.NegativeInfinity;
            for (int i = 0; i < _activeCount; i++)
            {
                if (_slots[i] == null) continue;
                Vector2 slotDir = RectTransformUtility.WorldToScreenPoint(cam, _slots[i].Rect.position) - center;
                if (slotDir.sqrMagnitude < 0.01f) continue;
                float dot = Vector2.Dot(mouseDir, slotDir.normalized);
                if (dot > bestDot)
                {
                    bestDot = dot;
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
