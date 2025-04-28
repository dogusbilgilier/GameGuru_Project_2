using UnityEngine;

namespace Game
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _animator;

        private static readonly int RunHash = Animator.StringToHash("Run");
        private static readonly int DanceHash = Animator.StringToHash("Dance");


        public void Run()
        {
            _animator.gameObject.transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            _animator.gameObject.transform.rotation = Quaternion.identity;
            _animator.SetTrigger(RunHash);
        }

        public void Dance()
        {
            _animator.SetTrigger(DanceHash);
        }
    }
}