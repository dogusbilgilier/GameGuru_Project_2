using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class Platform : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlatformMovementController _movementController;
        [SerializeField] private MeshRenderer _meshRenderer;
        public MeshRenderer MeshRenderer => _meshRenderer;
        public float Width => transform.localScale.x;

        private IObjectPool<Platform> _assignedPool;

        public void Initialize()
        {
        }

        public void Prepare(Vector3 position, Material material)
        {
            transform.position = position;

            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            _meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", material.color);
            _meshRenderer.SetPropertyBlock(propertyBlock);
        }

        public void StartMovement(float speed, float moveRange)
        {
            if (_movementController != null)
            {
                _movementController.StartMovement(speed, moveRange);
            }
        }

        public void StopMovement()
        {
            if (_movementController != null)
            {
                _movementController.StopMovement();
            }
        }

        public void AssignToPool(IObjectPool<Platform> assignedPool)
        {
            _assignedPool = assignedPool;
        }

        public void ReleaseFromPool()
        {
            Debug.Assert(_assignedPool != null);
            _assignedPool.Release(this);
        }

        private void OnDestroy()
        {
        }
    }
}