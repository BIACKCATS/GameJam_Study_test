using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Drives the player's Animator from the actual horizontal movement (not the key input), so the walk cycle
    /// stops when the player is blocked, locked by dialogue/minigames, or carried by the curtain slide.
    /// Animator parameter: bool "IsWalking". Facing is handled by SideViewMover (SpriteRenderer.flipX).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        private static readonly int IsWalkingId = Animator.StringToHash("IsWalking");

        [Tooltip("Empty = Rigidbody2D on this object or a parent.")]
        [SerializeField] private Rigidbody2D _body;
        [Tooltip("Horizontal speed (units/s) above which the walk animation plays.")]
        [SerializeField, Min(0f)] private float _walkSpeedThreshold = 0.1f;

        private Animator _animator;
        private bool _isWalking;

        public bool IsWalking => _isWalking;

        private void Awake()
        {
            TryGetComponent(out _animator);
            if (_body == null) _body = GetComponentInParent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _isWalking = false;
            if (_animator != null) _animator.SetBool(IsWalkingId, false);
        }

        private void Update()
        {
            if (_animator == null || _body == null) return;
            bool walking = Mathf.Abs(_body.linearVelocity.x) > _walkSpeedThreshold;
            if (walking == _isWalking) return;
            _isWalking = walking;
            _animator.SetBool(IsWalkingId, walking);
        }
    }
}
