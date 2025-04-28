using Game;
using Game.GameElements;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace DefaultNamespace
{
    public class Level : MonoBehaviour
    {
        [Inject] GameManager _gameManager;
        [Inject] LevelManager _levelManager;

        [Header("References")]
        [SerializeField] FinishPlatform _finishPlatform;
        public int platformCount;

        public void Initialize()
        {
            bool isFirstLevelInSession = _levelManager.CompletedLevelsInSession == 0;
            Vector3 finishPos = Vector3.forward * _gameManager.GameConfigs.PlatformLength * (platformCount + 1);
            _finishPlatform.transform.localPosition = _finishPlatform.GetPlatformsLength() * 0.5f * Vector3.forward + finishPos;
        }

        public float GetLevelLength()
        {
            return GameManager.Instance.GameConfigs.PlatformLength * platformCount + _finishPlatform.GetPlatformsLength();
        }

        public float GetFinishPlatformLength()
        {
            return _finishPlatform.GetPlatformsLength();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;
            Vector3 finishPos = Vector3.forward * GameConfigs.Instance.PlatformLength * platformCount;
            _finishPlatform.transform.position = _finishPlatform.GetPlatformsLength() * 0.5f * Vector3.forward + finishPos;
            Gizmos.DrawLine(finishPos, Vector3.zero);
        }
#endif
    }
}