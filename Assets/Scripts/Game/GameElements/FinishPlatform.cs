using UnityEngine;

namespace Game.GameElements
{
    public class FinishPlatform : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MeshRenderer _meshRenderer;

        public void Initialize()
        {
            SetWidth();
        }

        private void SetWidth()
        {
            
            float targetWidth = GameManager.Instance.GameConfigs.PlatformWidth;
            float currentWidth = _meshRenderer.bounds.size.x;

            float scaleMultiplier = targetWidth / currentWidth;
            transform.localScale = new Vector3(transform.localScale.x * scaleMultiplier, transform.localScale.y, transform.localScale.z);
        }

        public float GetPlatformsLength()
        {
            return _meshRenderer.bounds.size.z;
        }
    }
}