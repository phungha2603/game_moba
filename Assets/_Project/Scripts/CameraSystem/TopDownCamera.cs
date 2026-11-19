using UnityEngine;

namespace Project.CameraSystem
{
    public class TopDownCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 12f, -8f);
        [SerializeField] private float _smoothTime = 0.2f;

        private Vector3 _velocity;

        public void SetTarget(Transform target) => _target = target;

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPosition = _target.position + _offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _smoothTime);
            transform.LookAt(_target);
        }
    }
}
