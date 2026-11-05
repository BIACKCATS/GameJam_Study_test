using JSH.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.Gameplay
{
    /// <summary>
    /// Shift + Left Click on a visible Talkable starts a dialogue (no distance limit — anything the mouse
    /// can click on screen). Put on the Player (or any scene object).
    /// While Shift is held and talking is possible, the teammate's crosshair (CrosshairAimController, hty) is shown;
    /// releasing Shift or anything that blocks talking (dialogue, minigame, pause, curtain) plays its dismiss
    /// ("release") animation. The click recoil is played by the crosshair itself.
    /// </summary>
    public class TalkTargetSelector : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _talkableLayers = ~0;
        [Tooltip("Show the teammate's crosshair while Shift is held. Off = old behaviour (system cursor only).")]
        [SerializeField] private bool _useCrosshair = true;

        private readonly Collider2D[] _hits = new Collider2D[8];
        private ContactFilter2D _filter;
        private bool _hasCrosshair;
        private bool _aimRequested;

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
            _filter = new ContactFilter2D();
            _filter.SetLayerMask(_talkableLayers);
            _filter.useTriggers = true;
        }

        private void Start()
        {
            // StartAim logs an error when no controller exists, so only use it if one is in the scene.
            _hasCrosshair = _useCrosshair && FindFirstObjectByType<CrosshairAimController>() != null;
        }

        private void OnDisable() => SetAim(false);

        private void Update()
        {
            Mouse mouse = Mouse.current;
            Keyboard kb = Keyboard.current;
            if (mouse == null || kb == null || _camera == null) { SetAim(false); return; }

            bool shift = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
            bool canTalk = CanStartTalk();
            SetAim(shift && canTalk);

            if (!shift || !canTalk || !mouse.leftButton.wasPressedThisFrame) return;

            Vector2 screen = mouse.position.ReadValue();
            Vector3 world = _camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -_camera.transform.position.z));

            int count = Physics2D.OverlapPoint(world, _filter, _hits);
            for (int i = 0; i < count; i++)
            {
                if (_hits[i].TryGetComponent(out Talkable talkable) && talkable.CanTalk)
                {
                    DialogueRunner.Instance.StartDialogue(talkable);
                    SetAim(false); // click recoil finishes first, then the dismiss animation
                    return;
                }
            }
        }

        private static bool CanStartTalk()
        {
            if (!DialogueRunner.HasInstance || DialogueRunner.Instance.IsActive) return false;
            if (MinigameDirector.HasInstance && MinigameDirector.Instance.IsActive) return false;
            if (GameManager.HasInstance && !GameManager.Instance.IsPlaying) return false; // paused / ending
            if (TrainProgress.HasInstance && TrainProgress.Instance.IsTransitioning) return false; // curtain / car change
            return true;
        }

        /// <summary>Start/stop the crosshair only on change (Esc inside the crosshair can stop it on its own).</summary>
        private void SetAim(bool aim)
        {
            if (!_hasCrosshair) return;
            if (aim)
            {
                // Re-start if the crosshair stopped itself (Esc / focus loss) while Shift is still held.
                if (!CrosshairAimController.IsAiming) CrosshairAimController.StartAim();
            }
            else if (_aimRequested)
            {
                CrosshairAimController.StopAim();
            }
            _aimRequested = aim;
        }
    }
}
