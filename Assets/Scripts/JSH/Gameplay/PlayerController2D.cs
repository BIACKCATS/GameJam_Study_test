using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JSH.Gameplay
{
    /// <summary>
    /// Terraria-style side-view controller.
    /// A/D or Left/Right: move, Space/W/Up: jump, S/Down: drop through one-way platform (or fast fall in air).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Move")]
        [SerializeField] private float _moveSpeed = 7f;
        [SerializeField] private float _groundAccel = 80f;
        [SerializeField] private float _airAccel = 40f;

        [Header("Jump")]
        [SerializeField] private float _jumpSpeed = 13f;
        [SerializeField] private float _coyoteTime = 0.1f;
        [SerializeField] private float _jumpBufferTime = 0.1f;
        [SerializeField, Range(0f, 1f)] private float _jumpCutMultiplier = 0.5f;

        [Header("Fall")]
        [SerializeField] private float _fastFallSpeed = 20f;
        [SerializeField] private float _maxFallSpeed = 25f;
        [SerializeField] private float _dropThroughDuration = 0.3f;

        [Header("Ground Check")]
        [SerializeField, Range(0f, 89f)] private float _groundNormalTolerance = 40f;

        private Rigidbody2D _rb;
        private Collider2D _collider;
        private ContactFilter2D _groundFilter;
        private readonly ContactPoint2D[] _contacts = new ContactPoint2D[8];

        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool _isGrounded;
        private bool _isDropping;

        public bool IsGrounded => _isGrounded;

        private void Awake()
        {
            TryGetComponent(out _rb);
            TryGetComponent(out _collider);
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            _groundFilter = new ContactFilter2D();
            _groundFilter.SetNormalAngle(90f - _groundNormalTolerance, 90f + _groundNormalTolerance);
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            bool jumpPressed = kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
            bool jumpReleased = kb.spaceKey.wasReleasedThisFrame || kb.wKey.wasReleasedThisFrame || kb.upArrowKey.wasReleasedThisFrame;
            bool downPressed = kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame;

            if (jumpPressed) _jumpBufferTimer = _jumpBufferTime;
            else _jumpBufferTimer -= Time.deltaTime;

            Vector2 v = _rb.linearVelocity;

            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
            {
                v.y = _jumpSpeed;
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
            }

            if (jumpReleased && v.y > 0f) v.y *= _jumpCutMultiplier;

            if (downPressed && _isGrounded && !_isDropping) TryDropThrough();

            _rb.linearVelocity = v;
        }

        private void FixedUpdate()
        {
            Keyboard kb = Keyboard.current;
            float input = 0f;
            bool holdDown = false;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input += 1f;
                holdDown = kb.sKey.isPressed || kb.downArrowKey.isPressed;
            }

            _isGrounded = _rb.IsTouching(_groundFilter) && _rb.linearVelocity.y <= 0.01f;
            if (_isGrounded) _coyoteTimer = _coyoteTime;
            else _coyoteTimer -= Time.fixedDeltaTime;

            Vector2 v = _rb.linearVelocity;
            float accel = _isGrounded ? _groundAccel : _airAccel;
            v.x = Mathf.MoveTowards(v.x, input * _moveSpeed, accel * Time.fixedDeltaTime);

            float minFall = (!_isGrounded && holdDown) ? -_fastFallSpeed : -_maxFallSpeed;
            if (!_isGrounded && holdDown && v.y > minFall) v.y = Mathf.MoveTowards(v.y, minFall, _fastFallSpeed * 4f * Time.fixedDeltaTime);
            if (v.y < -_maxFallSpeed) v.y = -_maxFallSpeed;

            _rb.linearVelocity = v;
        }

        // Only colliders with PlatformEffector2D (one-way platforms) can be dropped through.
        private void TryDropThrough()
        {
            int count = _rb.GetContacts(_groundFilter, _contacts);
            for (int i = 0; i < count; i++)
            {
                Collider2D platform = _contacts[i].collider;
                if (platform != null && platform.usedByEffector && platform.TryGetComponent(out PlatformEffector2D _))
                {
                    StartCoroutine(DropThroughRoutine(platform));
                    return;
                }
            }
        }

        private IEnumerator DropThroughRoutine(Collider2D platform)
        {
            _isDropping = true;
            Physics2D.IgnoreCollision(_collider, platform, true);
            yield return new WaitForSeconds(_dropThroughDuration);
            if (platform != null) Physics2D.IgnoreCollision(_collider, platform, false);
            _isDropping = false;
        }
    }
}
