using System;
using Game.GameElements;
using UnityEngine;
using Zenject;

namespace Game
{
    public class MainPlayerController : MonoBehaviour
    {
        [Inject] private LevelManager _levelManager;
        [Header("References")]
        [SerializeField] PlayerAnimationController _animationController;

        private SignalBus _signalBus;
        public bool IsInitialized { get; private set; }

        private float _speed;
        private float _horizontalSpeed;
        private bool _isMoving;
        private float _centerX;
        private bool _checkForFall;

        public void Initialize(SignalBus signalBus)
        {
            _signalBus = signalBus;
            transform.position = new Vector3(0f, 0f, GameConfigs.Instance.PlayerStartDistance);
            _speed = GameConfigs.Instance.PlayerMovementSpeed;
            _horizontalSpeed = GameConfigs.Instance.PlayerHorizontalSpeed;
            IsInitialized = true;
            _signalBus.Subscribe<FirstPlatformPlacedInLevelSignal>(OnLevelStarted);
            _signalBus.Subscribe<PlatformCenterChangedSignal>(OnPlatformCenterChanged);
            _signalBus.Subscribe<LevelFinishedSignal>(OnLevelFinished);
        }

        private void OnLevelFinished(LevelFinishedSignal args)
        {
            if (args.IsSuccess == false)
            {
                _checkForFall = true;
            }
        }


        private void OnLevelStarted()
        {
            StartMoving();
        }

        private void Update()
        {
            if (_isMoving == false)
                return;

            Move();
            CheckPlatformBelow();
        }

        private void CheckPlatformBelow()
        {
            if (_checkForFall == false)
                return;

            Ray ray = new Ray(transform.position + Vector3.back + Vector3.up, Vector3.down);
            float rayDistance = 5f;
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance) == false)
            {
                _checkForFall = false;
                ReturnToLevelStartPosition();
            }
        }

        private void ReturnToLevelStartPosition()
        {
            StopMoving();

            _centerX = 0f;

            float zPos = _levelManager.CompletedLevelsInSession == 0 ? GameConfigs.Instance.PlayerStartDistance : _levelManager.GetLastLevelsStartDistance() + (_levelManager.GetFinishPlatformLength() * 2);

            transform.position = new Vector3(0f, 0f, zPos);

            _signalBus.Fire(new PlayerFallSignal());
        }

        private void Move()
        {
            float targetX = Mathf.Lerp(transform.position.x, _centerX, _horizontalSpeed * Time.deltaTime);
            float targetZ = transform.position.z + (_speed * Time.deltaTime);
            transform.position = new Vector3(targetX, transform.position.y, targetZ);
        }

        public void StartMoving()
        {
            _isMoving = true;
            _animationController.Run();
        }

        public void StopMoving()
        {
            _isMoving = false;
            _animationController.Dance();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<FinishPlatform>(out FinishPlatform finishPlatform))
            {
                other.enabled = false;
                _centerX = 0f;
                OnEnterFinishArea();
                _signalBus.Fire(new PlayerReachFinalPlatformSignal());
            }
        }

        private void OnEnterFinishArea()
        {
            StopMoving();
            _animationController.Dance();
        }

        private void OnPlatformCenterChanged(PlatformCenterChangedSignal args)
        {
            _centerX = args.NewCenterX;
        }
    }
}

public struct PlayerReachFinalPlatformSignal
{
}

public struct PlayerFallSignal
{
}