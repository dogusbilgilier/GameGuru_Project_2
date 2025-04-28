using UnityEngine;
using Zenject;

namespace Game.GameElements
{
    public class FinishPlatform : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MeshRenderer _meshRenderer;

        public void Initialize()
        {
        }

        public float GetPlatformsLength()
        {
            return _meshRenderer.bounds.size.z;
        }
    }
}