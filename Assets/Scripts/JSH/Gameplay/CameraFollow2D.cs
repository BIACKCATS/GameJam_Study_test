using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>Smoothly follows a target on X/Y, keeping the camera's Z.</summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector2 _offset = new Vector2(0f, 1f);
        [SerializeField] private float _smoothTime = 0.15f;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (_target == null) return;
            Vector3 goal = new Vector3(_target.position.x + _offset.x, _target.position.y + _offset.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, _smoothTime);
        }
    }
}
