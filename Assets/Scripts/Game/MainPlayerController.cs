using Game.GameElements;
using UnityEngine;
using Zenject;

namespace Game
{
    public class MainPlayerController : MonoBehaviour
    {
        [Inject] private LevelManager _levelManager;
        private GameManager _gameManager;
        [Header("References")]
        [SerializeField] PlayerAnimationController _animationController;

        private SignalBus _signalBus;
        public bool IsInitialized { get; private set; }

        private float _speed;
        private float _horizontalSpeed;
        private bool _isMoving;
        private float _centerX;
        private bool _isLevelFinishedSuccessfully;
        private float _lastPlacedPlatformZPos;

        public void Initialize(GameManager gameManager, SignalBus signalBus)
        {
            _gameManager = gameManager;
            _signalBus = signalBus;
            
            transform.position = new Vector3(0f, 0f, _gameManager.GameConfigs.PlayerStartDistance);
            _speed = _gameManager.GameConfigs.PlayerMovementSpeed;
            _horizontalSpeed = _gameManager.GameConfigs.PlayerHorizontalSpeed;
            IsInitialized = true;

            _signalBus.Subscribe<FirstPlatformPlacedInLevelSignal>(OnLevelStarted);
            _signalBus.Subscribe<PlatformCenterChangedSignal>(OnPlatformCenterChanged);
            _signalBus.Subscribe<LevelFinishSuccessSignal>(OnLevelFinished);
            _signalBus.Subscribe<PlatformPlacedSignal>(OnPlatformPlaced);
        }

        private void OnLevelStarted()
        {
            _isLevelFinishedSuccessfully = false;
            StartMoving();
        }

        private void Update()
        {
            if (_isMoving == false)
                return;

            Move();

            if (_isLevelFinishedSuccessfully)
                return;

            CheckPlatformBelow();
        }

        private void CheckPlatformBelow()
        {
            if (transform.position.z > _lastPlacedPlatformZPos && _lastPlacedPlatformZPos > 0f)
            {
                Ray ray = new Ray(transform.position + Vector3.back + Vector3.up, Vector3.down);
                float rayDistance = 5f;
                if (Physics.Raycast(ray, out RaycastHit hit, rayDistance) == false)
                {
                    _signalBus.Fire(new PlayerFallSignal());
                    ReturnToLevelStartPosition();
                }
            }
        }

        private void ReturnToLevelStartPosition()
        {
            StopMoving();
            _centerX = 0f;
            float zPos = _levelManager.CompletedLevelsInSession == 0 ? _gameManager.GameConfigs.PlayerStartDistance : _levelManager.GetLastLevelsStartDistance() + (_levelManager.GetFinishPlatformLength() * 2);
            transform.position = new Vector3(0f, 0f, zPos);
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

            if (other.TryGetComponent<GameElementBase>(out GameElementBase gameElement))
            {
                gameElement.OnCollected();
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

        private void OnPlatformPlaced(PlatformPlacedSignal args)
        {
            if (args.PlacedPlatform != null)
            {
                _lastPlacedPlatformZPos = (_gameManager.GameConfigs.PlatformLength * 0.5f) + args.PlacedPlatform.transform.position.z;
            }
        }

        private void OnLevelFinished(LevelFinishSuccessSignal args)
        {
            _isLevelFinishedSuccessfully = true;
        }
    }
}

public struct PlayerReachFinalPlatformSignal
{
}

public struct PlayerFallSignal
{
}