using Cinemachine;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Game
{
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera _playerFollowCamera;
        [SerializeField] private CinemachineFreeLook _freeLookCamera;

        [SerializeField] private float _freeLookTurnSpeed;
        private SignalBus _signalBus;

        public void Initialize(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _signalBus.Subscribe<MainPlayerCreatedSignal>(OnPlayerCreated);
            _signalBus.Subscribe<GameplayStateChangedSignal>(OnGameplayStateChanged);

            _freeLookCamera.Priority = 0;
            _playerFollowCamera.Priority = 1;
        }

        private void OnGameplayStateChanged(GameplayStateChangedSignal args)
        {
            _freeLookCamera.Priority = args.CurrenyGameplayState == GameplayState.Win ? 2 : 0;
        }

        private void Update()
        {
            if (_freeLookCamera.Priority > _playerFollowCamera.Priority)
                _freeLookCamera.m_XAxis.Value += Time.deltaTime * _freeLookTurnSpeed;
        }

        public void OnPlayerCreated(MainPlayerCreatedSignal args)
        {
            _playerFollowCamera.Follow = args.MainPlayerController.transform;
            _playerFollowCamera.LookAt = args.MainPlayerController.transform;

            _freeLookCamera.Follow = args.MainPlayerController.transform;
            _freeLookCamera.LookAt = args.MainPlayerController.transform;
        }
    }
}