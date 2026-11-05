using JSH.Core;
using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Smoothly follows a target, keeping the camera's Z. Follow can be paused for cutscenes:
    ///  - direct: SetFollowing(false) / SetFollowing(true, snap)
    ///  - decoupled: EventBus&lt;CameraFollowRequest&gt;.Raise(new CameraFollowRequest(false))
    /// While paused, other code may move the camera freely; resuming eases back to the target (or snaps).
    /// Pause with SetFollowing, not by disabling the component (a disabled component stops receiving requests).
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector2 _offset = new Vector2(0f, 1f);
        [SerializeField] private float _smoothTime = 0.15f;
        [SerializeField] private bool _followX = true;
        [SerializeField] private bool _followY = false;
        [SerializeField] private bool _followOnStart = true;

        private Vector3 _velocity;
        private bool _isFollowing;

        public bool IsFollowing => _isFollowing;
        public Transform Target => _target;

        private void Awake() => _isFollowing = _followOnStart;

        private void OnEnable() => EventBus<CameraFollowRequest>.Subscribe(OnFollowRequest);

        private void OnDisable() => EventBus<CameraFollowRequest>.Unsubscribe(OnFollowRequest);

        private void LateUpdate()
        {
            if (!_isFollowing || _target == null) return;
            transform.position = Vector3.SmoothDamp(transform.position, GetGoal(), ref _velocity, _smoothTime);
        }

        /// <summary>Turn follow on/off. snap = jump to the target immediately when turning on.</summary>
        public void SetFollowing(bool enabled, bool snap = false)
        {
            _isFollowing = enabled;
            _velocity = Vector3.zero; // no leftover momentum when pausing or resuming
            if (enabled && snap) SnapToTarget();
        }

        public void SetTarget(Transform target, bool snap = false)
        {
            _target = target;
            _velocity = Vector3.zero;
            if (snap) SnapToTarget();
        }

        public void SnapToTarget()
        {
            if (_target == null) return;
            transform.position = GetGoal();
            _velocity = Vector3.zero;
        }

        private void OnFollowRequest(CameraFollowRequest request) => SetFollowing(request.Enabled, request.Snap);

        private Vector3 GetGoal()
        {
            Vector3 current = transform.position;
            Vector3 targetPos = _target.position;
            float x = _followX ? targetPos.x + _offset.x : current.x;
            float y = _followY ? targetPos.y + _offset.y : current.y;
            return new Vector3(x, y, current.z);
        }
    }
}
