using Cinemachine;
using UnityEngine;
using Zenject;

namespace Game
{
    public class CameraController:MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera _playerFollowCamera;

        private SignalBus _signalBus;
        public void Initialize(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _signalBus.Subscribe<MainPlayerCreatedSignal>(OnPlayerCreated);
        }

        public void OnPlayerCreated(MainPlayerCreatedSignal args)
        {
            _playerFollowCamera.Follow = args.MainPlayerController.transform;
        }
    }
}