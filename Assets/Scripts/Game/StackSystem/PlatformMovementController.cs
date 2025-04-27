using UnityEngine;

namespace Game
{
    public class PlatformMovementController : MonoBehaviour
    {
        private bool _isMoving = false;
        private float _moveSpeed;
        private float _moveRange;
        private float _startX;
        private float _time;

        public void StartMovement(float moveSpeed, float moveRange)
        {
            _moveSpeed = moveSpeed;
            _moveRange = moveRange;
            _startX = transform.position.x;
            _time = Mathf.PI / 2f;
            _isMoving = true;
            transform.position = new Vector3(Mathf.Sin(_time) * _moveRange, transform.position.y, transform.position.z);
        }

        public void StopMovement()
        {
            _isMoving = false;
        }

        private void Update()
        {
            if (_isMoving)
            {
                _time += Time.deltaTime * _moveSpeed;
                float offsetX = Mathf.Sin(_time) * _moveRange;
                transform.position = new Vector3(_startX + offsetX, transform.position.y, transform.position.z);
            }
        }
    }
}