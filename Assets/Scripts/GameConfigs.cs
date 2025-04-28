using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    [CreateAssetMenu(fileName = "GameConfigs", menuName = "GameConfigs")]
    public class GameConfigs : ScriptableObject
    {
#if UNITY_EDITOR
        //Editor Instance
        public static GameConfigs Instance => AssetDatabase.LoadAssetAtPath<GameConfigs>("Assets/GameConfigs.asset");
#endif

        [Header("Platform Settings")]
        public Material[] PlatformMaterials;
        public float PlatformWidth;
        public float PlatformLength;
        public float PlatformDepth;
        [Range(0f, 1f)] public float TolerancePercent;
        public float MovementOverflowAmount;
        public float MoveSpeed;

        [Header("Player Settings")]
        public float PlayerMovementSpeed;
        public float PlayerHorizontalSpeed;
        public float PlayerStartDistance;
    }
}