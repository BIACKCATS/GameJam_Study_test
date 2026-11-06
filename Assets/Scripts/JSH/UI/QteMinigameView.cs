using JSH.Core;
using JSH.Data;
using JSH.Gameplay;
using TMPro;
using UnityEngine;

namespace JSH.UI
{
    public sealed class QteMinigameView : MonoBehaviour
    {
        private GameObject _root;
        private UnityEngine.UI.Slider _gauge;
        private UnityEngine.UI.RawImage _left;
        private UnityEngine.UI.RawImage _right;
        private TMP_Text _prompt;
        private MinigameDirector _director;
        private QteMinigame _round;
        private float _animationTime;
        private float _resultLeft;

        private void Awake() => _director = GetComponent<MinigameDirector>();
        private void OnEnable() => EventBus<MinigameFinishedEvent>.Subscribe(OnFinished);
        private void OnDisable()
        {
            EventBus<MinigameFinishedEvent>.Unsubscribe(OnFinished);
            if (_root) _root.SetActive(false);
            _round = null;
            _resultLeft = 0f;
        }

        private void Update()
        {
            if (GameOverSequenceController.IsPlaying || TrueEndingController.IsPlaying)
            {
                if (_root) _root.SetActive(false);
                _round = null;
                _resultLeft = 0f;
                return;
            }
            if (_director.ActiveQte != null && _director.ActiveQte != _round)
            {
                _round = _director.ActiveQte;
                if (!_root) Build();
                _root.SetActive(true);
                _animationTime = _resultLeft = 0f;
            }
            if (!_root || !_root.activeSelf) return;
            if (GameManager.HasInstance && GameManager.Instance.IsPaused) return;
            if (_resultLeft > 0f)
            {
                _resultLeft -= Time.deltaTime;
                if (_resultLeft <= 0f) { _root.SetActive(false); _round = null; }
                return;
            }
            if (_director.ActiveQte == null)
            {
                _root.SetActive(false);
                _round = null;
                return;
            }
            _gauge.value = _round.Gauge;
            _animationTime += Time.deltaTime;
            int frame = (int)(_animationTime * _director.QteConfig.framesPerSecond) % 24;
            Animate(_left, frame);
            Animate(_right, (frame + 12) % 24);
            _left.color = _round.LastButton == 0 ? new Color(0.55f, 0.55f, 0.55f) : Color.white;
            _right.color = _round.LastButton == 1 ? new Color(0.55f, 0.55f, 0.55f) : Color.white;
            _prompt.text = !_round.IsReady ? "준비!" : _round.LastButton < 0 ? "좌클릭 · 우클릭을 번갈아 누르세요!"
                : _round.LastButton == 0 ? "다음: 우클릭!" : "다음: 좌클릭!";
        }

        private void OnFinished(MinigameFinishedEvent evt)
        {
            if (evt.Type != MinigameType.Qte) return;
            if (!_root) Build();
            _root.SetActive(true);
            _gauge.value = evt.Success ? 1f : 0f;
            _prompt.text = evt.Success ? "성공!" : "실패...";
            _resultLeft = 0.6f;
        }

        private static void Animate(UnityEngine.UI.RawImage image, int frame)
        {
            image.uvRect = new Rect((frame % 6) / 6f, 1f - (frame / 6 + 1) / 4f, 1f / 6f, 1f / 4f);
        }

        private void Build()
        {
            _root = new GameObject("QteMinigameCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            _root.transform.SetParent(transform, false);
            var canvas = _root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;
            var scaler = _root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var backdrop = Element("Backdrop", _root.transform, Vector2.zero, Vector2.zero).gameObject.AddComponent<UnityEngine.UI.Image>();
            backdrop.color = new Color(0, 0, 0, 0.65f);
            backdrop.rectTransform.anchorMin = Vector2.zero;
            backdrop.rectTransform.anchorMax = Vector2.one;
            backdrop.rectTransform.offsetMin = backdrop.rectTransform.offsetMax = Vector2.zero;

            var track = Element("Gauge", _root.transform, new Vector2(0, 190), new Vector2(620, 40));
            track.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0.12f, 0.12f, 0.15f);
            var fill = Element("Fill", track, Vector2.zero, Vector2.zero).gameObject.AddComponent<UnityEngine.UI.Image>();
            fill.color = new Color(0.35f, 0.9f, 0.55f);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
            _gauge = track.gameObject.AddComponent<UnityEngine.UI.Slider>();
            _gauge.fillRect = fill.rectTransform;
            _gauge.interactable = false;
            _gauge.transition = UnityEngine.UI.Selectable.Transition.None;
            _gauge.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            _left = MouseImage("LeftMouse", -165, _director.QteConfig.leftSheet);
            _right = MouseImage("RightMouse", 165, _director.QteConfig.rightSheet);
            _prompt = Element("Prompt", _root.transform, new Vector2(0, -200), new Vector2(1000, 70)).gameObject.AddComponent<TextMeshProUGUI>();
            _prompt.font = _director.QteConfig.font;
            _prompt.fontSize = 36;
            _prompt.alignment = TextAlignmentOptions.Center;
            _prompt.raycastTarget = false;
        }

        private UnityEngine.UI.RawImage MouseImage(string name, float x, Texture texture)
        {
            var image = Element(name, _root.transform, new Vector2(x, -10), new Vector2(256, 256)).gameObject.AddComponent<UnityEngine.UI.RawImage>();
            image.texture = texture;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform Element(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
    }
}
