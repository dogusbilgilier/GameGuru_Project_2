using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class FallingPiece : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MeshRenderer _meshRenderer;

        IObjectPool<FallingPiece> _assignedPool;

        private Rigidbody _rigidbody;
        private Rigidbody Rigidbody
        {
            get
            {
                if (_rigidbody == null)
                    _rigidbody = GetComponent<Rigidbody>();

                return _rigidbody;
            }
        }

        public void Initialize()
        {
        }

        public void Prepare(Vector3 position, Vector3 scale, Material material)
        {
            transform.position = position;
            transform.localScale = scale;

            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            _meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", material.color);
            _meshRenderer.SetPropertyBlock(propertyBlock);
            Drop();
        }

        public void AssignToPool(IObjectPool<FallingPiece> pool)
        {
            _assignedPool = pool;
        }

        public void ReleaseFromPool()
        {
            Debug.Assert(_assignedPool != null);

            _assignedPool.Release(this);
        }

        private void Drop()
        {
            Rigidbody.isKinematic = true;
            Rigidbody.isKinematic = false;

            Rigidbody.AddTorque((transform.position.x > 0 ? Vector3.forward : Vector3.back) * 150f, ForceMode.Force);
            Invoke(nameof(ReleaseFromPool), 3);
        }
    }
}