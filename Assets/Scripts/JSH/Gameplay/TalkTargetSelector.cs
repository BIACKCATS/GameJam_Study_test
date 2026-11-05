using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.Gameplay
{
    /// <summary>
    /// Shift + Left Click on a visible Talkable starts a dialogue (no distance limit — anything the mouse
    /// can click on screen). Put on the Player (or any scene object).
    /// </summary>
    public class TalkTargetSelector : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _talkableLayers = ~0;

        private readonly Collider2D[] _hits = new Collider2D[8];
        private ContactFilter2D _filter;

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
            _filter = new ContactFilter2D();
            _filter.SetLayerMask(_talkableLayers);
            _filter.useTriggers = true;
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            Keyboard kb = Keyboard.current;
            if (mouse == null || kb == null || _camera == null) return;
            if (!mouse.leftButton.wasPressedThisFrame) return;
            if (!kb.leftShiftKey.isPressed && !kb.rightShiftKey.isPressed) return;
            if (!DialogueRunner.HasInstance || DialogueRunner.Instance.IsActive) return;
            if (MinigameDirector.HasInstance && MinigameDirector.Instance.IsActive) return;

            Vector2 screen = mouse.position.ReadValue();
            Vector3 world = _camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -_camera.transform.position.z));

            int count = Physics2D.OverlapPoint(world, _filter, _hits);
            for (int i = 0; i < count; i++)
            {
                if (_hits[i].TryGetComponent(out Talkable talkable) && talkable.CanTalk)
                {
                    DialogueRunner.Instance.StartDialogue(talkable);
                    return;
                }
            }
        }
    }
}
