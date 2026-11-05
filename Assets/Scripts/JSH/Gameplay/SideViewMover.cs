using JSH.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.Gameplay
{
    /// <summary>
    /// Horizontal-only side-view movement (no jump).
    /// A/D or Left/Right arrows move the actor; gravity keeps it on the floor.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class SideViewMover : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _acceleration = 60f;
        [SerializeField] private float _deceleration = 80f;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Rigidbody2D _rb;
        private float _input = 0;
        private bool _manualLock;
        private bool _dialogueLock;
        private bool _minigameLock;

        /// <summary>-1 = left, 0 = idle, 1 = right.</summary>
        public float MoveInput => _input;
        public bool InputLocked => _manualLock || _dialogueLock || _minigameLock;

        private void Awake()
        {
            TryGetComponent(out _rb);
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            if (_spriteRenderer == null) TryGetComponent(out _spriteRenderer);
        }

        private void OnEnable()
        {
            EventBus<DialogueStateChangedEvent>.Subscribe(OnDialogueStateChanged);
            EventBus<MinigameStateChangedEvent>.Subscribe(OnMinigameStateChanged);
        }

        private void OnDisable()
        {
            EventBus<DialogueStateChangedEvent>.Unsubscribe(OnDialogueStateChanged);
            EventBus<MinigameStateChangedEvent>.Unsubscribe(OnMinigameStateChanged);
        }

        /// <summary>
        /// Manual lock (cutscenes etc.). Locked = no movement input (actor decelerates to a stop).
        /// Dialogue and minigames lock/unlock automatically and independently of this.
        /// </summary>
        public void SetInputLocked(bool locked) => _manualLock = locked;

        private void OnDialogueStateChanged(DialogueStateChangedEvent evt) => _dialogueLock = evt.IsActive;

        private void OnMinigameStateChanged(MinigameStateChangedEvent evt) => _minigameLock = evt.IsActive;

        private void Update()
        {
            _input = 0f;
            if (InputLocked) return;
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) _input -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) _input += 1f;

            if (_spriteRenderer != null && _input != 0f) _spriteRenderer.flipX = _input < 0f;
        }

        private void FixedUpdate()
        {
            Vector2 v = _rb.linearVelocity;
            float rate = _input != 0f ? _acceleration : _deceleration;
            v.x = Mathf.MoveTowards(v.x, _input * _moveSpeed, rate * Time.fixedDeltaTime);
            _rb.linearVelocity = v;
        }
    }
}
